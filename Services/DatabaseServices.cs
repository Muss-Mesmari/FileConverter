using FileConverter.Data;
using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
    public class DatabaseServices : IDatabaseServices
    {
        private readonly DocumentFileDbContext _documentFileDbContext;

        public DatabaseServices(DocumentFileDbContext documentFileDbContext)
        {
            _documentFileDbContext = documentFileDbContext;
        }

        public string GetConfigString(DocumentFileViewModel documentFileViewModel)
        {
            var server = documentFileViewModel.SQLServerConfig.Server;
            var database = documentFileViewModel.SQLServerConfig.Database;
            var userId = documentFileViewModel.SQLServerConfig.UserId;
            var password = documentFileViewModel.SQLServerConfig.Password;

            var conString = $"Server={server};Database={database};User Id={userId};password={password};Trusted_Connection=True;MultipleActiveResultSets=true";
            return conString;
        }
        public async Task<List<KeyValuePair<string, List<string>>>> GetAllAttributesSortedByTableAsync(string conString, string tableName)
        {
            var allAttributesByTable = new List<KeyValuePair<string, List<string>>>();

            var tables = new List<string>();
            if (!string.IsNullOrEmpty(tableName))
            {
                var attributes = await GetAttributesByTableNameAsync(conString, tableName);
                allAttributesByTable.Insert(0, new KeyValuePair<string, List<string>>(tableName, attributes));
            }
            else
            {
                tables = await GetAllTablesAsync(conString);

                for (int i = 0; i < tables.Count(); i++)
                {
                    var attributes = await GetAttributesByTableNameAsync(conString, tables[i]);
                    allAttributesByTable.Add(new KeyValuePair<string, List<string>>(tables[i], attributes));
                }
            }

            return allAttributesByTable;
        }
        public async Task<List<string>> GetAllTablesAsync(string conString)
        {
            using (SqlConnection connection = new SqlConnection(conString))
            {
                await connection.OpenAsync();

                DataTable schema = await connection.GetSchemaAsync("Tables");
                List<string> TableNames = new List<string>();

                foreach (DataRow row in schema.Rows)
                {
                    TableNames.Add(row[2].ToString());
                }
                return TableNames;
            }
        }
        private async Task<List<string>> GetAttributesByTableNameAsync(string conString, string table)
        {
            var sql = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{table}'";

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                var attributeTypes = new List<string>();
                var attributeValues = new List<string>();

                while (await reader.ReadAsync())
                {
                    var attributeValue = reader.GetSqlString(0).Value;
                    attributeValues.Add(attributeValue);
                }
                return attributeValues;
            }
        }
        private async Task<List<KeyValuePair<string, int>>> GetObjectsTypesByTableNameAsync(string conString, string table)
        {
            var sql = $@"SELECT DISTINCT [className], [classId] FROM [UDGAHBAS].[dbo].[{table}] ORDER BY [className] ";

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                var attributeTypes = new List<string>();
                var attributeValues = new List<KeyValuePair<string, int>>();

                while (await reader.ReadAsync())
                {
                    var attributeValueOne = reader.GetSqlString(0).Value;
                    var attributeValueTwo = reader.GetSqlInt32(1).Value;
                    var attributeKeyValuePairValues = new KeyValuePair<string, int>(attributeValueOne, attributeValueTwo);
                    attributeValues.Add(attributeKeyValuePairValues);
                }
                return attributeValues;
            }
        }
        public async Task<List<KeyValuePair<string, int>>> GetAllObjectsTypesNamesAsync(List<string> tables, string conString)
        {
            List<KeyValuePair<string, int>> row = new List<KeyValuePair<string, int>>();
            foreach (var table in tables)
            {
                if (table == "ObjectCount")
                {
                    row = await GetObjectsTypesByTableNameAsync(conString, table);
                }
            }
            return row;
        }
        public async Task<string> GetObjectNameByClassIdAsync(int classId, string conString)
        {
            var tableName = string.Empty;
            var tables = await GetAllTablesAsync(conString);
            var objectsNamesAndIds = await GetAllObjectsTypesNamesAsync(tables, conString);
            foreach (var o in objectsNamesAndIds)
            {
                if (o.Value == classId)
                {
                    tableName = o.Key;
                }
            }
            return tableName;
        }



        private async Task<List<KeyValuePair<string, int>>> GetPropertiesIdsByClassIdAsync(string conString, int classId)
        {
            var sql = $@"  SELECT [classId] ,[propertyId] FROM [UDGAHBAS].[dbo].[Property_Class] WHERE [classId] = {classId} ORDER BY [propertyId]";

            var rows = new List<KeyValuePair<string, int>>();

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                int count = 0;
                while (await reader.ReadAsync())
                {
                    var intAttributeValue = 0;
                    var stringAttributeValue = string.Empty;

                    var attributeOneType = reader.GetValue(count).GetType();
                    if (attributeOneType == typeof(int))
                    {
                        intAttributeValue = reader.GetSqlInt32(count).Value;
                    }
                    else
                    {
                        stringAttributeValue = reader.GetSqlString(count).Value;
                    }


                    var attributeTwoType = reader.GetValue(count + 1).GetType();
                    if (attributeTwoType == typeof(string))
                    {
                        stringAttributeValue = reader.GetSqlString(count + 1).Value;
                    }
                    else
                    {
                        intAttributeValue = reader.GetSqlInt32(count + 1).Value;
                    }

                    rows.Add(new KeyValuePair<string, int>(classId.ToString(), intAttributeValue));

                }
                return rows;
            }
        }
        public async Task<List<KeyValuePair<string, int>>> GetPropertiesNamesByPropertiesIdsAsync(string conString, int classId)
        {
            var properties = await GetPropertiesIdsByClassIdAsync(conString, classId);

            var sql = $@" SELECT [propertyId], [propertyName] FROM [UDGAHBAS].[dbo].[Property] WHERE ";

            var whereClause = string.Empty;
            for (int i = 0; i < properties.Count(); i++)
            {
                if (i == 0)
                {
                    whereClause = $"[propertyId] = {properties[i].Value} ";
                    sql += whereClause;
                }
                else
                {
                    whereClause = $"OR [propertyId] = {properties[i].Value} ";
                    sql += whereClause;
                }
            }


            var rows = new List<KeyValuePair<string, int>>();

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                int count = 0;
                while (await reader.ReadAsync())
                {
                    var intAttributeValue = 0;
                    var stringAttributeValue = string.Empty;

                    var attributeOneType = reader.GetValue(count).GetType();
                    if (attributeOneType == typeof(int))
                    {
                        intAttributeValue = reader.GetSqlInt32(count).Value;
                    }
                    else
                    {
                        stringAttributeValue = reader.GetSqlString(count).Value;
                    }


                    var attributeTwoType = reader.GetValue(count + 1).GetType();
                    if (attributeTwoType == typeof(string))
                    {
                        stringAttributeValue = reader.GetSqlString(count + 1).Value;
                    }
                    else
                    {
                        intAttributeValue = reader.GetSqlInt32(count + 1).Value;
                    }

                    rows.Add(new KeyValuePair<string, int>(stringAttributeValue.Trim(), intAttributeValue));

                }
                return rows;
            }
        }
        public async Task<List<KeyValuePair<int, KeyValuePair<int, string>>>> GetPropertiesValuesByObjectIdsSortedByObjectIdsAsync(string conString, int classId)
        {
            var properties = await GetPropertiesIdsByClassIdAsync(conString, classId);
            var objects = await GetObjectsNamesAndObjectIdsByClassIdAsync(conString, classId);

            var sql = $@" SELECT [objectId], [propertyId], [value] FROM [UDGAHBAS].[dbo].[PropertyValue] WHERE ";

            var whereClause = string.Empty;
            for (int i = 0; i < objects.Count(); i++)
            {
                if (i == 0)
                {
                    whereClause = $"[objectId] = {objects[i].Value} ";
                    sql += whereClause;
                }
                else
                {
                    whereClause = $"OR [objectId] = {objects[i].Value} ";
                    sql += whereClause;
                }
            }

            sql += "ORDER BY [objectId], [propertyId]";

            
            var rows = new List<KeyValuePair<int, KeyValuePair<int, string>>>();

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                int count = 0;
                while (await reader.ReadAsync())
                {
                    var objectId = 0;
                    var propertyId = 0;
                    var value = string.Empty;

                    var attributeOneType = reader.GetValue(count).GetType();
                    if (attributeOneType == typeof(int))
                    {
                        objectId = reader.GetSqlInt32(count).Value;
                    }
                    else
                    {
                        objectId = int.Parse(reader.GetSqlString(count).Value);
                    }

                    var attributeTwoType = reader.GetValue(count + 1).GetType();
                    if (attributeTwoType == typeof(int))
                    {
                        propertyId = reader.GetSqlInt32(count + 1).Value;
                    }
                    else
                    {
                        value = reader.GetSqlString(count + 1).Value;
                    }


                    var attributeThreeType = reader.GetValue(count + 2).GetType();
                    if (attributeThreeType == typeof(string))
                    {
                        value = reader.GetSqlString(count + 2).Value;
                    }
                    else
                    {
                        propertyId = reader.GetSqlInt32(count + 2).Value;
                    }

                    var propertyIdPropertyValue = new KeyValuePair<int, string>(propertyId, value.Trim());
                    var objectIdPropertyIdPropertyValue = new KeyValuePair<int, KeyValuePair<int, string>>(objectId, propertyIdPropertyValue);
                    rows.Add(objectIdPropertyIdPropertyValue);

                }
                return rows;
            }
        }

        public async Task<List<KeyValuePair<string, int>>> GetObjectsNamesAndObjectIdsByClassIdAsync(string conString, int classId)
        {
            var sql = $@"  SELECT [objectId] ,[name] FROM [UDGAHBAS].[dbo].[Object] WHERE [classId] = {classId} AND [name] like 'fut%' ORDER BY [name]";

            var rows = new List<KeyValuePair<string, int>>();

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                int count = 0;
                while (await reader.ReadAsync())
                {
                    var intAttributeValue = 0;
                    var stringAttributeValue = string.Empty;

                    var attributeOneType = reader.GetValue(count).GetType();
                    if (attributeOneType == typeof(int))
                    {
                        intAttributeValue = reader.GetSqlInt32(count).Value;
                    }
                    else
                    {
                        stringAttributeValue = reader.GetSqlString(count).Value;
                    }


                    var attributeTwoType = reader.GetValue(count + 1).GetType();
                    if (attributeTwoType == typeof(string))
                    {
                        stringAttributeValue = reader.GetSqlString(count + 1).Value;
                    }
                    else
                    {
                        intAttributeValue = reader.GetSqlInt32(count + 1).Value;
                    }

                    rows.Add(new KeyValuePair<string, int>(stringAttributeValue.Trim(), intAttributeValue));

                }
                return rows;
            }
        }

    }

}


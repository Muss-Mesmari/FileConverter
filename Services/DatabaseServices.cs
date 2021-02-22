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
        public DatabaseServices()
        {
        }

        // Handles connection to the server and the database
        public string GetConfigString(DocumentFileViewModel documentFileViewModel)
        {
            var server = documentFileViewModel.SQLServerConfig.Server;
            var database = documentFileViewModel.SQLServerConfig.Database;
            var userId = documentFileViewModel.SQLServerConfig.UserId;
            var password = documentFileViewModel.SQLServerConfig.Password;

            var conString = $"Server={server};Database={database};User Id={userId};password={password};Trusted_Connection=True;MultipleActiveResultSets=true";
            return conString;
        }
        //---------------------


        // Retrieve models names
        public async Task<List<string>> GetModelsNamesAsync(string conString)
        {
            var voId = 576;
            var objectsNamesAndIds = await GetObjectsNamesAndObjectIdsByClassIdAsync(conString, voId, null);

            var modelsNames = new List<string>();
            foreach (var objectsNameAndId in objectsNamesAndIds)
            {
                modelsNames.Add(objectsNameAndId.Value[0].Substring(0, 3));
            }
            return modelsNames;
        }
        //---------------------


        // Retrieve tables and columns
        public async Task<List<KeyValuePair<string, List<string>>>> GetAllColumnsSortedByTableAsync(string conString, string tableName)
        {
            var allColumnsByTable = new List<KeyValuePair<string, List<string>>>();

            if (!string.IsNullOrEmpty(tableName))
            {
                var attributes = await GetColumnsByTableNameAsync(conString, tableName);
                allColumnsByTable.Insert(0, new KeyValuePair<string, List<string>>(tableName, attributes));
            }
            else
            {
                var tables = await GetAllTablesAsync(conString);

                for (int i = 0; i < tables.Count; i++)
                {
                    var attributes = await GetColumnsByTableNameAsync(conString, tables[i]);
                    allColumnsByTable.Add(new KeyValuePair<string, List<string>>(tables[i], attributes));
                }
            }

            return allColumnsByTable;
        }
        public async Task<List<string>> GetAllTablesAsync(string conString)
        {
            using SqlConnection connection = new SqlConnection(conString);
            await connection.OpenAsync();

            DataTable schema = await connection.GetSchemaAsync("Tables");
            List<string> TableNames = new List<string>();

            foreach (DataRow row in schema.Rows)
            {
                TableNames.Add(row[2].ToString());
            }
            return TableNames;
        }
        static async Task<List<string>> GetColumnsByTableNameAsync(string conString, string table)
        {
            var sql = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{table}'";
            var resultSet = await ExcuteSQLAsync(conString, sql);
            var columns = new List<string>();
            foreach (var row in resultSet)
            {
                foreach (var value in row.Value)
                {
                    columns.Add(value);
                }
            }
            return columns;
        }
        //---------------------


        // Retrieve types of objects
        public async Task<List<KeyValuePair<int, string>>> GetAllObjectsTypesNamesAsync(List<string> tables, string conString)
        {
            var row = new List<KeyValuePair<int, string>>();
            foreach (var table in tables)
            {
                if (table == "ObjectCount")
                {
                    row = await GetObjectsTypesByTableNameAsync(conString, table);
                }
            }
            return row;
        }
        static async Task<List<KeyValuePair<int, string>>> GetObjectsTypesByTableNameAsync(string conString, string table)
        {
            var sql = $@"SELECT DISTINCT [className], [classId] FROM [UDGAHBAS].[dbo].[{table}] ORDER BY [className] ";
            var objectsTypes = new List<KeyValuePair<int, string>>();

            var resultSet = await ExcuteSQLAsync(conString, sql);
            foreach (var row in resultSet)
            {
                foreach (var value in row.Value)
                {
                    var intValue = row.Key;
                    var stringValue = value;
                    var values = new KeyValuePair<int, string>(intValue, stringValue);
                    objectsTypes.Add(values);
                }
            }
            return objectsTypes;
        }

        public async Task<string> GetObjectTypeNameByClassIdAsync(int classId, string conString)
        {
            var tableName = string.Empty;

            var tables = await GetAllTablesAsync(conString);
            var objectsTypesName = await GetAllObjectsTypesNamesAsync(tables, conString);

            foreach (var o in objectsTypesName)
            {
                if (o.Key == classId)
                {
                    tableName = o.Value;
                }
            }
            return tableName;
        }
        //---------------------


        // Retrieve properties of objects
        public async Task<List<KeyValuePair<int, List<string>>>> GetPropertiesAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelName, string inputOrOutput)
        {
            var sql = $@"SELECT [objectId], property.[propertyId], property.[propertyName], PropertyValue.[value] FROM [UDGAHBAS].[dbo].[Property] property JOIN [UDGAHBAS].[dbo].[PropertyValue] PropertyValue on PropertyValue.propertyId = property.propertyId WHERE ";

            var objects = new List<KeyValuePair<int, string>>();
            var objectstmp = new List<KeyValuePair<int, List<string>>>();
            if (serviceClassIdOne == 12)
            {
                objects = await GetAttributesOrAttributesGroupsNamesAndIdsThatAreUsedInsideAttributesGroupsByClassIdsAsync(conString, serviceClassIdOne, serviceClassIdTwo, modelName, inputOrOutput);
            }
            else
            {
                objectstmp = await GetObjectsNamesAndObjectIdsByClassIdAsync(conString, serviceClassIdOne, modelName);

                var objectsNamesAndIds = new List<KeyValuePair<int, string>>();
                foreach (var row in objectstmp)
                {
                    foreach (var value in row.Value)
                    {
                        objectsNamesAndIds.Add(new KeyValuePair<int, string>(row.Key, value.Trim()));
                    }
                }
                objects = objectsNamesAndIds;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (i == 0)
                {
                    var whereClause = $"[objectId] = {objects[i].Key} ";
                    sql += whereClause;
                }
                else
                {
                    var whereClause = $" OR [objectId] = {objects[i].Key} ";
                    sql += whereClause;
                }
            }
            sql += " ORDER BY [objectId], property.[propertyId]";
            var properties = await ExcuteSQLAsync(conString, sql);
            return properties;
        }

        public async Task<List<KeyValuePair<int, List<string>>>> GetPropertiesNamesByPropertiesIdsAsync(string conString, int classId)
        {
            var propertiesIds = await GetPropertiesIdsByClassIdAsync(conString, classId);
            var sql = string.Empty;
            if (propertiesIds.Count != 0)
            {
                sql = $@" SELECT [propertyId], [propertyName] FROM [UDGAHBAS].[dbo].[Property] WHERE ";

                for (int i = 0; i < propertiesIds.Count; i++)
                {
                    if (i == 0)
                    {
                        var whereClause = $"[propertyId] = {propertiesIds[i].Value} ";
                        sql += whereClause;
                    }
                    else
                    {
                        var whereClause = $"OR [propertyId] = {propertiesIds[i].Value} ";
                        sql += whereClause;
                    }
                }
            }
            else
            {
                sql = $@" SELECT [propertyId], [propertyName] FROM [UDGAHBAS].[dbo].[Property] WHERE [propertyId] = 0";
            }

            var properties = await ExcuteSQLAsync(conString, sql);
            return properties;
        }
        public async Task<List<KeyValuePair<int, string>>> GetPropertiesIdsByClassIdAsync(string conString, int classId)
        {
            var sql = $@"  SELECT [classId] ,[propertyId] FROM [UDGAHBAS].[dbo].[Property_Class] WHERE [classId] = {classId} ORDER BY [propertyId]";

            var propertiesIds = new List<KeyValuePair<int, string>>();

            var resultSet = await ExcuteSQLAsync(conString, sql);
            foreach (var row in resultSet)
            {
                var intValue = row.Key;
                var stringValue = row.Value[0];
                var values = new KeyValuePair<int, string>(intValue, stringValue);
                propertiesIds.Add(values);
            }
            return propertiesIds;
        }

        public async Task<List<KeyValuePair<int, List<string>>>> GetPropertiesValuesByObjectIdsSortedByObjectIdsAsync(string conString, int classId, string modelName)
        {
            var objects = await GetObjectsNamesAndObjectIdsByClassIdAsync(conString, classId, modelName);
            var sql = $@" SELECT [objectId], [propertyId], [value] FROM [UDGAHBAS].[dbo].[PropertyValue] WHERE ";
            for (int i = 0; i < objects.Count; i++)
            {
                if (i == 0)
                {
                    var whereClause = $"[objectId] = {objects[i].Key} ";
                    sql += whereClause;
                }
                else
                {
                    var whereClause = $" OR [objectId] = {objects[i].Key} ";
                    sql += whereClause;
                }
            }
            sql += " ORDER BY [objectId], [propertyId]";

            var propertiesValuesByObjectIds = await ExcuteSQLAsync(conString, sql);
            return propertiesValuesByObjectIds;
        }
        public async Task<List<KeyValuePair<int, List<string>>>> GetObjectsNamesAndObjectIdsByClassIdAsync(string conString, int classId, string modelName)  // Retrieve objects names and ids
        {
            var sql = $@"  SELECT [objectId] ,[name], [description] FROM [UDGAHBAS].[dbo].[Object] WHERE [classId] = {classId} AND [name] like '{modelName}%' ORDER BY [name]";
            var objectsNamesAndIds = new List<KeyValuePair<int, string>>();

            var resultSet = await ExcuteSQLAsync(conString, sql);
            return resultSet;
        }
        //---------------------


        // Retrieve attributes groups and aattributes
        public async Task<List<KeyValuePair<int, string>>> GetAttributesOrAttributesGroupsNamesAndIdsThatAreUsedInsideAttributesGroupsByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelName, string inputOrOutput)
        {
            var sql = $@" SELECT related.objectId AS [Related objectId] ,related.[name] AS [Relation till] FROM [UDGAHBAS].[dbo].[Object] [Object] JOIN [UDGAHBAS].[dbo].[ObjectRelship] rel ON [Object].objectId = rel.sourceObjectId JOIN [UDGAHBAS].[dbo].[Object] related ON related.objectId = rel.targetObjectId WHERE [Object].[classId] = 634 AND [Object].[name] like '{modelName}%{inputOrOutput}' AND related.classId = {serviceClassIdOne} ORDER BY [Object].objectId ";

            var objectsNamesAndIds = new List<KeyValuePair<int, string>>();

            var resultSet = await ExcuteSQLAsync(conString, sql);
            foreach (var row in resultSet)
            {
                foreach (var value in row.Value)
                {
                    objectsNamesAndIds.Add(new KeyValuePair<int, string>(row.Key, value.Trim()));
                }
            }
            return objectsNamesAndIds;
        }
        //---------------------


        // Retrieve relationships between objects
        public async Task<List<KeyValuePair<int, int>>> GetRelationshipsByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelNameOne, string modelNameTwo)
        {
            var sql = $@"SELECT tjanst.[objectId] AS [ObjectId nr.1], related.[objectId] AS [ObjectId nr.2] FROM [UDGAHBAS].[dbo].[Object] tjanst JOIN [UDGAHBAS].[dbo].[ObjectRelship] rel ON tjanst.objectId = rel.sourceObjectId  JOIN [UDGAHBAS].[dbo].[Object] related ON related.objectId = rel.targetObjectId WHERE tjanst.[name] LIKE '{modelNameOne}%' AND tjanst.[classId] = {serviceClassIdOne} AND related.[classId] = {serviceClassIdTwo} AND related.[name] LIKE '{modelNameTwo}%' ORDER BY tjanst.[objectId]";

            var objectsIds = new List<KeyValuePair<int, int>>();

            var resultSet = await ExcuteSQLAsync(conString, sql);
            foreach (var row in resultSet)
            {
                foreach (var value in row.Value)
                {
                    objectsIds.Add(new KeyValuePair<int, int>(row.Key, int.Parse(value)));
                }
            }
            return objectsIds;
        }
        //---------------------


        // Retrieve relationships between attributes or attributesGroup objects
        public async Task<List<KeyValuePair<int, int>>> GetRelationshipsBetweenAttributesOrAttributesGroupAndOtherObjectssByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelNameOne, string inputOrOutput)
        {
            var sql = $@" SELECT related.objectId AS [objectId] ,[Object].objectId AS [Related objectId] FROM [UDGAHBAS].[dbo].[Object] [Object] JOIN [UDGAHBAS].[dbo].[ObjectRelship] rel ON [Object].objectId = rel.sourceObjectId JOIN [UDGAHBAS].[dbo].[Object] related ON related.objectId = rel.targetObjectId WHERE [Object].[classId] = 634 AND [Object].[name] like '{modelNameOne}%{inputOrOutput}' AND related.classId = {serviceClassIdOne} ORDER BY [Object].objectId ";

            var objectsIds = new List<KeyValuePair<int, int>>();

            var resultSet = await ExcuteSQLAsync(conString, sql);
            foreach (var row in resultSet)
            {
                foreach (var value in row.Value)
                {
                    objectsIds.Add(new KeyValuePair<int, int>(row.Key, int.Parse(value)));
                }
            }
            return objectsIds;
        }
        //---------------------

        // Handles sql execution
        static async Task<List<KeyValuePair<int, List<string>>>> ExcuteSQLAsync(string conString, string sql)
        {
            var rows = new List<KeyValuePair<int, List<string>>>();

            using SqlConnection connection = new SqlConnection(conString);
            using SqlCommand command = new SqlCommand(sql, connection);
            await connection.OpenAsync();

            SqlDataReader reader = await command.ExecuteReaderAsync();
            if (sql.Contains("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA"))
            {
                var intValue = new int();
                var stringValues = new List<string>();
                var attributeValues = new List<string>();

                while (await reader.ReadAsync())
                {
                    var stringValue = reader.GetSqlString(0).Value;
                    stringValues.Add(stringValue);
                }

                rows.Add(new KeyValuePair<int, List<string>>(intValue, stringValues));
            }
            else
            {
                if (sql.Contains("ORDER BY"))
                {
                    int index = sql.IndexOf("ORDER BY");
                    if (index >= 0) sql = sql.Substring(0, index);
                }

                var numberOfColumns = sql.Count(s => (s.ToString() == ",")) + 1;
                while (await reader.ReadAsync())
                {
                    var intValue = new int();
                    var stringValues = new List<string>();

                    int countIntType = 0;
                    for (int i = 0; i < numberOfColumns; i++)
                    {
                        var valueType = reader.GetValue(i).GetType();
                        if (valueType == typeof(int))
                        {
                            if (countIntType == 0)
                            {
                                intValue = reader.GetSqlInt32(i).Value;
                                countIntType += 1;
                            }
                            else
                            {
                                var stringValue = reader.GetSqlInt32(i).ToString();
                                stringValues.Add(stringValue);
                            }
                        }
                        else
                        {
                            var stringValue = string.Empty;
                            var isValueNull = reader.IsDBNull(i);
                            if (isValueNull)
                            {
                                stringValue = string.Empty;
                            }
                            else
                            {
                                stringValue = reader.GetSqlString(i).Value;
                            }
                            stringValues.Add(stringValue);
                        }
                    }

                    rows.Add(new KeyValuePair<int, List<string>>(intValue, stringValues));

                }
            }

            return rows;
        }
        //---------------------
    }
}


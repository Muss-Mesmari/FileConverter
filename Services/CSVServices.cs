using ExcelDataReader;
using FileConverter.Data;
using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace FileConverter.Services
{
    public class CSVServices : ICSVServices
    {
        private readonly DocumentFileDbContext _documentFileDbContext;
        private readonly IXlsxServices _xlsxServices;
        private readonly IDatabaseServices _databaseServices;
        public CSVServices(
            DocumentFileDbContext documentFileDbContext,
            IXlsxServices xlsxServices,
            IDatabaseServices databaseServices
            )
        {
            _documentFileDbContext = documentFileDbContext;
            _xlsxServices = xlsxServices;
            _databaseServices = databaseServices;
        }

        public CSV ConvertXlsxToCSV(string filePath)
        {
            var excelSheetHeaders = _xlsxServices.GetDataFromXlsxFile(filePath).Headers;
            var excelSheetRows = _xlsxServices.GetDataFromXlsxFile(filePath).Rows;
            var excelSheet = _xlsxServices.GetDataFromXlsxFile(filePath);

            var csv = new CSV
            {
                HeadersFromXlsxFile = ConvertXlsxHeadersToCSV(excelSheetHeaders),
                RowsFromXlsxFile = ConvertXlsxRowsToCSV(excelSheetRows),
                NumberOfRows = CountCsvRows(excelSheet),
                NumberOfHeaders = CountCsvHeaders(excelSheet)
            };
            return csv;
        }
        private string ConvertXlsxHeadersToCSV(List<string> excelSheetHeaders)
        {
            var csvHeaders = String.Join(",", excelSheetHeaders);
            return csvHeaders;
        }
        private List<string> ConvertXlsxRowsToCSV(List<List<string>> excelSheetTable)
        {
            List<string> csvRows = new List<string>();

            foreach (var row in excelSheetTable)
            {
                var doubleQuote = "\"";
                var validCsvRow = ValidateStrings(row);

                var csvRow = doubleQuote + string.Join(@""",""", validCsvRow) + doubleQuote;
                csvRows.Add(csvRow);
            }

            return csvRows;
        }
        private List<string> ValidateStrings(List<string> row)
        {
            List<string> csvRows = new List<string>();

            foreach (var value in row)
            {
                if (value.GetType() == typeof(string))
                {
                    if (value.Contains("\""))
                    {
                        var newValue = value.Replace("\"", "\"\"");
                        csvRows.Add(newValue);
                    }
                    else
                    {
                        csvRows.Add(value);
                    }
                }
                else
                {
                    csvRows.Add(value);
                }

            }
            return csvRows;
        }
        private List<string> CreateValidCSVRowString(List<string> row)
        {
            List<string> csvRows = new List<string>();

            foreach (var value in row)
            {
                if (value.GetType() == typeof(string))
                {
                    var validValue = WebUtility.HtmlDecode(Regex.Replace(value, "<[^>]*(>|$)", string.Empty)).Replace("\n", " ").Replace("\"", "\"\"");
                    csvRows.Add(validValue);
                }
                else
                {
                    csvRows.Add(value);
                }
            }
            return csvRows;
        }
        private int CreateValidCSVRowInt(int value)
        {
            if (value.GetType() == typeof(int))
            {
                var validValue = Convert.ToInt32(value);
                return validValue;
            }
            else
            {
                return value;
            }
        }
        private int CountCsvRows(ExcelSheet excelSheet)
        {
            var numberOfRows = excelSheet.Rows.Count();
            var numberOfHeaders = 1;
            var totalRows = numberOfRows + numberOfHeaders;

            return totalRows;
        }
        private int CountCsvHeaders(ExcelSheet excelSheet)
        {
            var numberOfHeaders = excelSheet.Headers.Count();
            return numberOfHeaders;
        }
        public string BuildCsvStringFromXlsxFile(CSV csv)
        {
            var doubleQuote = "\"";
            var builder = new StringBuilder();

            builder.AppendLine(csv.HeadersFromXlsxFile);

            if (csv.RowsFromXlsxFile != null)
            {
                foreach (var row in csv.RowsFromXlsxFile)
                {
                    builder.AppendLine(doubleQuote + doubleQuote + row);
                }
            }

            return builder.ToString();
        }



        public async Task<CSV> ConvertSQLServerToCSVAsync(string conString, string tableName, int objectIdOne, int objectIdTwo)
        {
            var sqlServerAttributesByTable = new List<KeyValuePair<string, List<string>>>();
            var sqlServerRowsString = new List<string>();
            var sqlServerRowsInt = new List<string>();

            if (objectIdTwo != 0)
            {
                var sqlServerRelationsshipsValues = await _databaseServices.GetRelationshipsByClassIdsAsync(conString, objectIdOne, objectIdTwo);
                sqlServerRowsInt = ConvertSqlServerToCSVInt(sqlServerRelationsshipsValues);
            }
            else
            {
                if (objectIdOne == 0)
                {
                    sqlServerAttributesByTable = await _databaseServices.GetAllAttributesSortedByTableAsync(conString, tableName);
                }
                else
                {
                    var sqlServerObjects = await _databaseServices.GetObjectsNamesAndObjectIdsByClassIdAsync(conString, objectIdOne);
                    var sqlServerPropertiesNames = await _databaseServices.GetPropertiesNamesByPropertiesIdsAsync(conString, objectIdOne);
                    var sqlServerPropertiesValues = await _databaseServices.GetPropertiesValuesByObjectIdsSortedByObjectIdsAsync(conString, objectIdOne);
                    sqlServerRowsString = ConvertSqlServerToCSVString(sqlServerObjects, sqlServerPropertiesNames, sqlServerPropertiesValues);
                }
            }


            var sqlServerHeaders = new List<string>();
            if (!string.IsNullOrEmpty(tableName))
            {
                sqlServerHeaders = MatchSqlServerHeadersByTable(tableName, sqlServerAttributesByTable);
            }
            else
            {
                sqlServerHeaders = GetSqlServerHeaders(sqlServerAttributesByTable, null);
            }

            var sqlServerRows = new List<string>();
            if (sqlServerRowsInt.Count() != 0)
            {
                sqlServerRows = sqlServerRowsInt;
            }
            else
            {
                sqlServerRows = sqlServerRowsString;
            }

            var csv = new CSV
            {
                HeadersFromSqlServer = sqlServerHeaders,
                RowsFromSqlServer = sqlServerRows
            };
            return csv;
        }
        private List<string> MatchSqlServerHeadersByTable(string tableName, List<KeyValuePair<string, List<string>>> sqlServerAttributesByTable)
        {
            List<string> csvHeaders = new List<string>();

            for (int i = 0; i < sqlServerAttributesByTable.Count(); i++)
            {
                if (sqlServerAttributesByTable[i].Key == tableName)
                {
                    var header = String.Join(",", sqlServerAttributesByTable[i].Value);
                    csvHeaders.Add(header);
                }
            }
            return csvHeaders;
        }
        private List<string> GetSqlServerHeaders(List<KeyValuePair<string, List<string>>> sqlServerAttributesByTable, List<KeyValuePair<int, List<KeyValuePair<string, string>>>> sortedPropertiesByObjectId)
        {
            List<string> csvHeaders = new List<string>();
            if (sqlServerAttributesByTable != null)
            {
                for (int i = 0; i < sqlServerAttributesByTable.Count(); i++)
                {
                    var header = String.Join(",", sqlServerAttributesByTable[i].Value);
                    csvHeaders.Add(header);
                }
                return csvHeaders;
            }
            else
            {

                for (int i = 0; i < sortedPropertiesByObjectId.Count(); i++)
                {
                    if (i == 1)
                    {
                        for (int j = 0; j < sortedPropertiesByObjectId[i].Value.Count(); j++)
                        {
                            var header = String.Join(",", sortedPropertiesByObjectId[j].Value[j].Key);
                            csvHeaders.Add(header);
                        }
                    }
                }

                return csvHeaders;
            }
        }
        private List<List<string>> GetSqlServerRowsString(List<KeyValuePair<int, List<KeyValuePair<string, string>>>> sortedPropertiesByObjectId)
        {           
            var rows = new List<List<string>>();
            foreach (var sortedProperties in sortedPropertiesByObjectId)
            {
                var row = new List<string>();
                foreach (var value in sortedProperties.Value)
                {                    
                    row.Add(value.Value);
                }
                rows.Add(row);
            }
            return rows;
        }
        private List<KeyValuePair<int, List<KeyValuePair<string, string>>>> SortSqlServerDataByObjectIdsAndPropertiesNames(List<KeyValuePair<string, int>> ObjectsNamesAndIds, List<KeyValuePair<string, int>> propertiesNames, List<KeyValuePair<int, KeyValuePair<int, string>>> propertiesValues)
        {
            var propertiesSortedByObjectIdList = new List<KeyValuePair<int, List<KeyValuePair<string, string>>>>();
          
            foreach (var o in ObjectsNamesAndIds)
            {
                var properties = new List<KeyValuePair<string, string>>();
                foreach (var propertyName in propertiesNames)
                {
                    foreach (var propertyValue in propertiesValues)
                    {
                        if (o.Value == propertyValue.Key) // if the objectId == propertyId of the property keyValuePair
                        {
                            if (propertyName.Value == propertyValue.Value.Key)
                            {
                                properties.Add(new KeyValuePair<string, string>(propertyName.Key, propertyValue.Value.Value));

                            }
                        }
                    }
                }

                // Get the unused properties names
                var p1 = propertiesNames.Select(s1 => s1.Key).ToList();
                var p2 = properties.Select(s2 => s2.Key).ToList();
                var emptyProperties = new List<string>();
                emptyProperties.AddRange(p1.Except(p2));
                emptyProperties.AddRange(p2.Except(p1));

                // Add the object name and object id to the properties list
                properties.Insert(0,new KeyValuePair<string, string>("Object Id", o.Value.ToString()));
                properties.Insert(1,new KeyValuePair<string, string>("Object Name", o.Key.ToString()));

                // Assign an empty value to the unused properties and add them to the list
                foreach (var property in emptyProperties)
                {
                    properties.Add(new KeyValuePair<string, string>(property, string.Empty));
                }

                //// order properties by name
                //var propertiesOrderedByName = properties.OrderBy(p => p.Key).ToList();

                var propertiesSortedByObjectId = new KeyValuePair<int, List<KeyValuePair<string, string>>>(o.Value, properties);
                propertiesSortedByObjectIdList.Add(propertiesSortedByObjectId);
            }

            return propertiesSortedByObjectIdList;
        }
        private List<string> ConvertSqlServerToCSVString(List<KeyValuePair<string, int>> ObjectsNamesAndIds, List<KeyValuePair<string, int>> propertiesNames, List<KeyValuePair<int, KeyValuePair<int, string>>> propertiesValues)
        {
            var sortedPropertiesByObjectId = SortSqlServerDataByObjectIdsAndPropertiesNames(ObjectsNamesAndIds, propertiesNames, propertiesValues);
            var csv = new List<string>();

            var headers = GetSqlServerHeaders(null, sortedPropertiesByObjectId);
            if (headers.Count() != 0)
            {
                var validHeader = CreateValidCSVRowString(headers);
                var csvHeader = String.Join(" \" , \" ", validHeader);
                csv.Add(csvHeader);
            }

            var rows = GetSqlServerRowsString(sortedPropertiesByObjectId);
            foreach (var row in rows)
            {
                var validRow = CreateValidCSVRowString(row);
                var csvRow = String.Join(" \" , \" ", validRow);
                csv.Add(csvRow);
            }

            return csv;
        }
        private List<string> ConvertSqlServerToCSVInt(List<KeyValuePair<int, int>> relationshipsValues)
        {
            var rows = new List<string>();
            rows.Add("ObjectId\", \"RelatedObjectId");
            foreach (var relationship in relationshipsValues)
            {
                var row = string.Empty;
                var objectIdOne = CreateValidCSVRowInt(relationship.Key);
                var objectIdTwo = CreateValidCSVRowInt(relationship.Value);
                var value = objectIdOne.ToString() + "," + objectIdTwo.ToString();
                row += value;

                rows.Add(row);
            }
            return rows;
        }
        public string BuildCsvStringFromSQLServer(CSV csv)
        {
            var doubleQuote = "\"";
            var builder = new StringBuilder();

            if (csv.HeadersFromSqlServer != null)
            {
                foreach (var header in csv.HeadersFromSqlServer)
                {
                    builder.AppendLine(header);
                }
            }

            if (csv.RowsFromSqlServer != null)
            {
                foreach (var row in csv.RowsFromSqlServer)
                {
                    builder.AppendLine(doubleQuote + doubleQuote + doubleQuote + doubleQuote + row + doubleQuote);
                }
            }

            return builder.ToString();
        }
        public List<string> BuildMultipleCsvStringsFromSQLServer(CSV csv)
        {
            var builder = new List<string>();

            if (csv.HeadersFromSqlServer.Count() != 0)
            {
                var headers = new StringBuilder();
                foreach (var header in csv.HeadersFromSqlServer)
                {
                    headers.AppendLine(header);
                }
                builder.Add(headers.ToString());
            }

            if (csv.RowsFromSqlServer.Count() != 0)
            {
                var doubleQuote = "\"";
                foreach (var row in csv.RowsFromSqlServer)
                {
                    builder.Add(doubleQuote + doubleQuote + row);
                }
            }

            return builder;
        }

    }
}

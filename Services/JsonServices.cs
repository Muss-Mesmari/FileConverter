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
using System.Json;

namespace FileConverter.Services
{
    public class JsonServices : IJsonServices
    {
        private readonly IXlsxServices _xlsxServices;
        private readonly IDatabaseServices _databaseServices;
        public JsonServices(
            IXlsxServices xlsxServices,
            IDatabaseServices databaseServices
            )
        {
            _xlsxServices = xlsxServices;
            _databaseServices = databaseServices;
        }


        public async Task<List<string>> ConvertSQLServerToJSONAsync(string conString, string tableName, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo, string inputOutputMessage)
        {
            var rows = new List<string>();
            var headers = new List<string>();

            var cSVDownloadingOption = ChooseCSVDownloadingOptions(tableName, objectIdOne, objectIdTwo, inputOutputMessage);
            if (cSVDownloadingOption == CSVDownloadingOptions.Relationships)
            {
                // to retrieve relationsships - ex relationsship between tjänster and vo
                rows = await ConvertSQLServerRelationshipToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne, modelNameTwo);
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.Objects)
            {
                // to retrieve a specific type of objects - ex tjänster or VO 
                rows = await ConvertSQLServerObjectsToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne);
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.Tables)
            {
                // to retrieve all columns names of a table 
                headers = await ConvertSQLServerTablesHeadersToCSVAsync(conString, tableName); // retrieving columns values will be implemented in the soon future                
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.AttributesGroupsInInputMessage)
            {
                // to retrieve all attributes in the input message
                rows = await ConvertSQLServerAttributesGroupToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOutputMessage);
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.AttributesGroupsOutInputMessage)
            {
                // to retrieve all attributes in the output message
                rows = await ConvertSQLServerAttributesGroupToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOutputMessage);
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.AttributesInInputMessage)
            {
                // to retrieve all attributes groups in the input message
                rows = await ConvertSQLServerAttributesToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOutputMessage);
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.AttributesInOutputMessage)
            {
                // to retrieve all attributes groups in the output message
                rows = await ConvertSQLServerAttributesToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOutputMessage);
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.RelationshipsToAttributesOrAttributesGroupsInputMessage)
            {
                // to retrieve relationships between attributes or attributesGroup objects
                rows = await ConvertSQLServerRelationshipToAttributesOrAttributesGroupsToCSVAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOutputMessage);
            }

            return rows;
        }
        public CSVDownloadingOptions ChooseCSVDownloadingOptions(string tableName, int objectIdOne, int objectIdTwo, string inputOutputMessage)
        {
            if (objectIdOne == 12 && objectIdTwo == 0 && inputOutputMessage != null)
            {
                return CSVDownloadingOptions.AttributesInInputMessage;
            }
            else if (objectIdOne == 634 && objectIdTwo == 0 && inputOutputMessage != null)
            {
                return CSVDownloadingOptions.AttributesGroupsInInputMessage;
            }
            else if (objectIdOne == 12 && objectIdTwo != 0 && inputOutputMessage != null)
            {
                return CSVDownloadingOptions.RelationshipsToAttributesOrAttributesGroupsInputMessage;
            }
            else if (tableName == "Download all tables")
            {
                return CSVDownloadingOptions.DownloadAllTables;
            }
            else
            {
                if (objectIdTwo != 0)
                {
                    return CSVDownloadingOptions.Relationships;
                }
                else
                {
                    if (objectIdOne == 0)
                    {
                        return CSVDownloadingOptions.Tables;
                    }
                    else
                    {
                        return CSVDownloadingOptions.Objects;
                    }
                }
            }

        }

        // Attributes between objects
        private async Task<List<string>> ConvertSQLServerAttributesToCSVAsync(string conString, int objectIdOne, int objectIdTwo, string modelName, string inputOrOutput)
        {
            // Input or output attributes
            var attributesRows = await CreateJsonObjectRowAsync(conString, objectIdOne, objectIdTwo, modelName, inputOrOutput);
            return attributesRows;
        }
        //---------------------


        // Attributes groups between objects
        private async Task<List<string>> ConvertSQLServerAttributesGroupToCSVAsync(string conString, int objectIdOne, int objectIdTwo, string modelName, string inputOrOutput)
        {
            // Input or output attributes
            var attributesGroupsRows = await CreateJsonObjectRowAsync(conString, objectIdOne, objectIdTwo, modelName, inputOrOutput);
            return attributesGroupsRows;
        }
        //---------------------


        // Retrieve relationships between attributes or attributesGroup objects
        private async Task<List<string>> ConvertSQLServerRelationshipToAttributesOrAttributesGroupsToCSVAsync(string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string inputOrOutput)
        {
            var relationshipsRows = await CreateCSVRelationshipToAttributesOrAttributesGroupsRowsAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOrOutput);
            return relationshipsRows;
        }
        private async Task<List<string>> CreateCSVRelationshipToAttributesOrAttributesGroupsRowsAsync(string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string inputOrOutput)
        {
            // retrieve necessary data from database
            var relationshipsIdsRows = await _databaseServices.GetRelationshipsBetweenAttributesOrAttributesGroupAndOtherObjectssByClassIdsAsync(conString, objectIdOne, objectIdTwo, modelNameOne, inputOrOutput);

            // Convert database data to CSV
            var rows = new List<string>
            {
                "ObjectId,RelatedObjectId"  // add the header
            };
            foreach (var relationshipIdsRow in relationshipsIdsRows)
            {
                var objectId = CreateValidCSVIntValue(relationshipIdsRow.Key);
                var RelatedObjectId = CreateValidCSVIntValue(relationshipIdsRow.Value);
                var relationshipName = "related to";

                var row = objectId.ToString().Trim() + "," + RelatedObjectId.ToString().Trim() + "," + relationshipName.Trim();
                rows.Add(row);
            }
            return rows;
        }
        //---------------------





        // Relationship between objects
        private async Task<List<string>> ConvertSQLServerRelationshipToCSVAsync(string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo)
        {
            var relationshipsRows = await CreateCSVRelationshipRowsAsync(conString, objectIdOne, objectIdTwo, modelNameOne, modelNameTwo);
            return relationshipsRows;
        }
        private async Task<List<string>> CreateCSVRelationshipRowsAsync(string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo)
        {
            // retrieve necessary data from database
            var relationshipsIdsRows = await _databaseServices.GetRelationshipsByClassIdsAsync(conString, objectIdOne, objectIdTwo, modelNameOne, modelNameTwo);

            // Convert database data to CSV
            var rows = new List<string>
            {
                "ObjectId,RelatedObjectId"  // add the header
            };
            foreach (var relationshipIdsRow in relationshipsIdsRows)
            {
                var objectId = CreateValidCSVIntValue(relationshipIdsRow.Key);
                var RelatedObjectId = CreateValidCSVIntValue(relationshipIdsRow.Value);
                var relationshipName = "related to";

                var row = objectId.ToString().Trim() + "," + RelatedObjectId.ToString().Trim() + "," + relationshipName.Trim();
                rows.Add(row);
            }
            return rows;
        }
        //---------------------

        // Rows of objects: values of properties
        private async Task<List<string>> ConvertSQLServerObjectsToCSVAsync(string conString, int objectIdOne, int classIdTwo, string modelName)
        {
            var objectsRows = await CreateJsonObjectRowAsync(conString, objectIdOne, classIdTwo, modelName, null);
            return objectsRows;
        }
        public async Task<List<string>> CreateJsonObjectRowAsync(string conString, int classId, int classIdTwo, string modelName, string inputOrOutput)
        {
            // retrieve necessary data from database
            var propertiesIds = await _databaseServices.GetPropertiesIdsByClassIdAsync(conString, classId);
            var sortedPropertiesByObjectIds = await _databaseServices.GetPropertiesAsync(conString, classId, classIdTwo, modelName, inputOrOutput);

            // prepare the data to fit CSV converting
            var propertiesSortedByObjectIds = new List<KeyValuePair<int, List<KeyValuePair<int, string>>>>();
            for (int i = 0; i < sortedPropertiesByObjectIds.Count; i++)
            {
                var mergedPropertiesThatHaveTheSameObjectIds = MergePropertiesThatHaveTheSameObjectIds(sortedPropertiesByObjectIds, i);
                var addedMissingPropertiesForEveryObjectId = AddMissingPropertiesForEveryObjectId(mergedPropertiesThatHaveTheSameObjectIds, propertiesIds);
                var propertiesWithoutDuplicates = RemoveDuplicatedProperties(addedMissingPropertiesForEveryObjectId, propertiesIds);
                var orderedPropertiesById = OrderPropertiesById(propertiesWithoutDuplicates);

                var propertiesSortedByObjectId = new KeyValuePair<int, List<KeyValuePair<int, string>>>(sortedPropertiesByObjectIds[i].Key, orderedPropertiesById);
                propertiesSortedByObjectIds.Add(propertiesSortedByObjectId);
            }

            // Add properties names in the top of the list
            var propertiesNamesAndDescriptionAddedForEveryObjectId = await AddPropertiesNamesAndDescriptionForEveryObjectIdAsync(conString, classId, classIdTwo, modelName, inputOrOutput, propertiesSortedByObjectIds);

            // Add headers in the top of the list
            var propertiesSortedByObjectIdsWithHeader = await AddPropertiesNamesASHeaderAsync(conString, classId, propertiesNamesAndDescriptionAddedForEveryObjectId);

            // Convert database data to CSV
            var headingsAndRows = SeparateHeadingsAndRows(propertiesSortedByObjectIdsWithHeader);
            var Json = GenerateJsonForAllTheRows(headingsAndRows);
            return Json;
        }
        static List<KeyValuePair<int, string>> MergePropertiesThatHaveTheSameObjectIds(List<KeyValuePair<int, List<string>>> sortedPropertiesByObjectIds, int i)
        {
            var properties = new List<KeyValuePair<int, string>>();
            for (int j = 0; j < sortedPropertiesByObjectIds.Count; j++)
            {
                var _objectIdOne = sortedPropertiesByObjectIds[i].Key;
                var _objectIdTwo = sortedPropertiesByObjectIds[j].Key;
                if (_objectIdOne == _objectIdTwo)
                {
                    var propertyId = sortedPropertiesByObjectIds[j].Value[0];
                    var propertyValue = sortedPropertiesByObjectIds[j].Value[2];

                    var property = new KeyValuePair<int, string>(int.Parse(propertyId), propertyValue);
                    properties.Add(property);
                }
            }

            return properties;
        }
        static List<KeyValuePair<int, string>> AddMissingPropertiesForEveryObjectId(List<KeyValuePair<int, string>> properties, List<KeyValuePair<int, string>> propertiesIds)
        {
            int numberOfUsedProperties = properties.Count;
            for (int j = 0; j < numberOfUsedProperties; j++)
            {
                foreach (var propertyId in propertiesIds)
                {
                    if (int.Parse(propertyId.Value) != properties[j].Key)
                    {
                        var propertyName = string.Empty;
                        var unusedPoperties = new KeyValuePair<int, string>(int.Parse(propertyId.Value), propertyName);
                        properties.Insert(j, unusedPoperties);
                    }
                }
            }
            return properties;
        }
        private async Task<List<KeyValuePair<int, List<KeyValuePair<int, string>>>>> AddPropertiesNamesAndDescriptionForEveryObjectIdAsync(string conString, int classId, int classIdTwo, string modelName, string inputOrOutput, List<KeyValuePair<int, List<KeyValuePair<int, string>>>> propertiesSortedByObjectIds)
        {
            var propertiesNamesAndIds = new List<KeyValuePair<int, List<KeyValuePair<int, string>>>>();

            if (classId == 12)
            {
                var attributesPropertiesNamesAndIds = await _databaseServices.GetAttributesOrAttributesGroupsNamesAndIdsThatAreUsedInsideAttributesGroupsByClassIdsAsync(conString, classId, classIdTwo, modelName, inputOrOutput);
                for (int j = 0; j < propertiesSortedByObjectIds.Count; j++)
                {
                    var propertyNameAndId = new List<KeyValuePair<int, string>>();
                    for (int i = 0; i < attributesPropertiesNamesAndIds.Count; i++)
                    {
                        if (propertiesSortedByObjectIds[j].Key == attributesPropertiesNamesAndIds[i].Key)
                        {
                            var propertyIdOne = 0;
                            var propertyValueOne = attributesPropertiesNamesAndIds[i].Key.ToString();
                            var propertyOne = new KeyValuePair<int, string>(propertyIdOne, propertyValueOne);
                            propertyNameAndId.Add(propertyOne);

                            var propertyIdTwo = 1;
                            var propertyValueTwo = attributesPropertiesNamesAndIds[i].Value;
                            var propertyTwo = new KeyValuePair<int, string>(propertyIdTwo, propertyValueTwo);
                            propertyNameAndId.Add(propertyTwo);
                            break;
                        }
                    }
                    var propertiesSortedByObjectId = propertyNameAndId.Concat(propertiesSortedByObjectIds[j].Value).ToList();
                    var objectId = propertiesSortedByObjectIds[j].Key;

                    propertiesNamesAndIds.Add(new KeyValuePair<int, List<KeyValuePair<int, string>>>(objectId, propertiesSortedByObjectId));
                }
            }
            else
            {
                var objectsNamesAndIds = await _databaseServices.GetObjectsNamesAndObjectIdsByClassIdAsync(conString, classId, modelName);
                for (int j = 0; j < propertiesSortedByObjectIds.Count; j++)
                {
                    var propertyNameAndId = new List<KeyValuePair<int, string>>();
                    for (int i = 0; i < objectsNamesAndIds.Count; i++)
                    {
                        if (propertiesSortedByObjectIds[j].Key == objectsNamesAndIds[i].Key)
                        {
                            var propertyIdOne = 0;
                            var propertyValueOne = objectsNamesAndIds[i].Key.ToString();
                            var propertyOne = new KeyValuePair<int, string>(propertyIdOne, propertyValueOne);
                            propertyNameAndId.Add(propertyOne);

                            var propertyIdTwo = 1;
                            var propertyValueTwo = objectsNamesAndIds[i].Value[0];
                            var propertyTwo = new KeyValuePair<int, string>(propertyIdTwo, propertyValueTwo);
                            propertyNameAndId.Add(propertyTwo);

                            break;
                        }
                    }
                    var propertiesSortedByObjectId = propertyNameAndId.Concat(propertiesSortedByObjectIds[j].Value).ToList();
                    var objectId = propertiesSortedByObjectIds[j].Key;

                    propertiesNamesAndIds.Add(new KeyValuePair<int, List<KeyValuePair<int, string>>>(objectId, propertiesSortedByObjectId));
                }
            }
            return propertiesNamesAndIds;
        }
        static List<KeyValuePair<int, string>> RemoveDuplicatedProperties(List<KeyValuePair<int, string>> properties, List<KeyValuePair<int, string>> propertiesIds)
        {
            foreach (var propertyId in propertiesIds)
            {
                var doblicatedproperties = properties.Where(p => p.Key == int.Parse(propertyId.Value)).ToList();
                if (doblicatedproperties.Count >= 2)
                {
                    for (int j = 1; j < doblicatedproperties.Count; j++)
                    {
                        var propertyName = string.Empty;
                        var unusedPoperties = new KeyValuePair<int, string>(int.Parse(propertyId.Value), propertyName);
                        properties.Remove(unusedPoperties);
                    }
                }
            }
            return properties;
        }
        private async Task<List<KeyValuePair<int, List<KeyValuePair<int, string>>>>> AddPropertiesNamesASHeaderAsync(string conString, int classId, List<KeyValuePair<int, List<KeyValuePair<int, string>>>> propertiesSortedByObjectIds)
        {
            var propertiesNames = await _databaseServices.GetPropertiesNamesByPropertiesIdsAsync(conString, classId);

            var propertiesNamesAndTheirIds = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < propertiesNames.Count; i++)
            {
                var propertyName = propertiesNames[i].Value[0];
                var propertyNameAndItsId = new KeyValuePair<int, string>(propertiesNames[i].Key, propertyName);
                propertiesNamesAndTheirIds.Add(propertyNameAndItsId);
            }

            var propertyNameAndItsIdOne = new KeyValuePair<int, string>(0, "Id");
            propertiesNamesAndTheirIds.Add(propertyNameAndItsIdOne);
            var propertyNameAndItsIdTwo = new KeyValuePair<int, string>(1, "Namn");
            propertiesNamesAndTheirIds.Add(propertyNameAndItsIdTwo);

            var orderedPropertiesById = OrderPropertiesById(propertiesNamesAndTheirIds);
            var header = new KeyValuePair<int, List<KeyValuePair<int, string>>>(0, orderedPropertiesById);
            propertiesSortedByObjectIds.Insert(0, header);

            return propertiesSortedByObjectIds;
        }
        static List<KeyValuePair<int, string>> OrderPropertiesById(List<KeyValuePair<int, string>> properties)
        {
            var orderedPropertiesById = properties.OrderBy(id => id.Key).ToList();
            return orderedPropertiesById;
        }
        static KeyValuePair<List<List<string>>, List<List<string>>> SeparateHeadingsAndRows(List<KeyValuePair<int, List<KeyValuePair<int, string>>>> propertiesSortedByObjectIds)
        {
            var headings = new List<List<string>>();
            var rows = new List<List<string>>();
            int count = 0;
            for (int j = 0; j < propertiesSortedByObjectIds.Count(); j++)
            {
                var heading = new List<string>();
                var row = new List<string>();
                if (count == 0)
                {
                    for (int i = 0; i < propertiesSortedByObjectIds[j].Value.Count; i++)
                    {
                        if ((i + 1) != propertiesSortedByObjectIds[j].Value.Count)
                        {
                            var value = CreateValidCSVValue(propertiesSortedByObjectIds[j].Value[i].Value);
                            heading.Add(value);
                        }
                        else
                        {
                            var value = CreateValidCSVValue(propertiesSortedByObjectIds[j].Value[i].Value);
                            heading.Add(value);
                        }
                    }
                    headings.Add(heading);
                }
                else
                {
                    for (int i = 0; i < propertiesSortedByObjectIds[j].Value.Count; i++)
                    {
                        if ((i + 1) != propertiesSortedByObjectIds[j].Value.Count)
                        {
                            var value = CreateValidCSVValue(propertiesSortedByObjectIds[j].Value[i].Value);
                            row.Add(value);
                        }
                        else
                        {
                            var value = CreateValidCSVValue(propertiesSortedByObjectIds[j].Value[i].Value);
                            row.Add(value);
                        }
                    }
                    rows.Add(row);
                }

                count++;
            }
            var headingsAndRows = new KeyValuePair<List<List<string>>, List<List<string>>>(headings, rows);
            return headingsAndRows;
        }
        private string MarchTheDesiredRowWithHeadings(KeyValuePair<List<List<string>>, List<List<string>>> headingsAndRows, int rowNumber)
        {
            var headings = headingsAndRows.Key[0];
            var rows = headingsAndRows.Value[rowNumber];

            string message;
            JsonObject json = new JsonObject();

            for (int j = 0; j < headings.Count; j++)
            {
                json.Add(headings[j], rows[j]);
            }

            message = json.ToString();

            return message;
        }

        private List<string> GenerateJsonForAllTheRows(KeyValuePair<List<List<string>>, List<List<string>>> headingsAndRows)
        {
            var jsons = new List<string>();
            for (int i = 0; i < headingsAndRows.Value.Count; i++)
            {
                var Json = MarchTheDesiredRowWithHeadings(headingsAndRows, i);
                jsons.Add(Json);
            }            

            return jsons;
        }
        //---------------------

        // columns of objects: names of properties
        private async Task<List<string>> ConvertSQLServerTablesHeadersToCSVAsync(string conString, string tableName)
        {
            var columnsNamesSortedByTable = await _databaseServices.GetAllColumnsSortedByTableAsync(conString, tableName);
            // Convert to CSV headers of a table
            if (!string.IsNullOrEmpty(tableName))
            {
                var headers = GreateCSVHeaderForOneTable(tableName, columnsNamesSortedByTable);
                return headers;
            }
            else
            {
                var headers = CreateCSVHeadersForAllTheTables(columnsNamesSortedByTable);
                return headers;
            }
        }
        static List<string> GreateCSVHeaderForOneTable(string tableName, List<KeyValuePair<string, List<string>>> columnsNamesSortedByTable)
        {
            List<string> headers = new List<string>();

            for (int i = 0; i < columnsNamesSortedByTable.Count; i++)
            {
                if (columnsNamesSortedByTable[i].Key == tableName)
                {
                    var header = String.Join(",", columnsNamesSortedByTable[i].Value);
                    headers.Add(header);
                }
            }
            return headers;
        }
        static List<string> CreateCSVHeadersForAllTheTables(List<KeyValuePair<string, List<string>>> columnsNamesSortedByTable)
        {
            List<string> headers = new List<string>();
            for (int i = 0; i < columnsNamesSortedByTable.Count; i++)
            {
                var header = String.Join(",", columnsNamesSortedByTable[i].Value);
                headers.Add(header);
            }
            return headers;
        }
        //---------------------

        // validation of rows and headers
        static string CreateValidCSVValue(string value)
        {
            if (value.GetType() == typeof(string))
            {
                if (Regex.IsMatch(value, "<[^>]*(>|$)")) // temprory to skip downloading long texts
                {
                    var validValue = string.Empty;
                    value = validValue;
                }
                else
                {
                    var validValue = WebUtility.HtmlDecode(Regex.Replace(value, "<[^>]*(>|$)", string.Empty)).Replace("\n", String.Empty).Replace("\"", "\"\"").Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Replace("\r\n", string.Empty).Trim();
                    value = validValue;
                }
            }
            else
            {
                var validValue = value.Trim();
                value = validValue;
            }

            return value;
        }
        static int CreateValidCSVIntValue(int value)
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
        //---------------------

        // Build string for csv exporting
        public string BuildCsvStringFromSQLServer(CSV csv)
        {
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
                    builder.AppendLine(row);
                }
            }

            return builder.ToString();
        }
        public List<string> BuildMultipleCsvStringsFromSQLServer(CSV csv)
        {
            var builder = new List<string>();

            if (csv.HeadersFromSqlServer.Count != 0)
            {
                var headers = new StringBuilder();
                foreach (var header in csv.HeadersFromSqlServer)
                {
                    headers.AppendLine(header);
                }
                builder.Add(headers.ToString());
            }

            if (csv.RowsFromSqlServer.Count != 0)
            {
                var doubleQuote = "\"";
                foreach (var row in csv.RowsFromSqlServer)
                {
                    builder.Add(doubleQuote + doubleQuote + row);
                }
            }

            return builder;
        }
        //---------------------
    }
}

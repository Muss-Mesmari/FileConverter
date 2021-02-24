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
    public class CypherServices : ICypherServices
    {
        private readonly ICSVServices _cSVServices;
        private readonly IDatabaseServices _databaseServices;
        private readonly IFileServices _fileServices;
        public CypherServices(
            ICSVServices cSVServices,
            IDatabaseServices databaseServices,
              IFileServices fileServices
            )
        {
            _cSVServices = cSVServices;
            _databaseServices = databaseServices;
            _fileServices = fileServices;
        }


        private async Task<string> CreateNodesAsync(string tableName, string conString, int objectIdOne, int objectIdTwo, string modelName, string inputOrOutput)
        {

            var fileName = await _fileServices.CreateFileNameAsync(tableName, conString, objectIdOne, objectIdTwo, modelName);
            var attributesLists = await _cSVServices.CreateCSVObjectsRowsAsync(conString, objectIdOne, objectIdTwo, modelName, inputOrOutput);
            var variableName = CreateVariableNameForNodes(fileName);
            var attributesNames = attributesLists[0].Split(",").ToList();

            var cypherPartOne = $" LOAD CSV WITH HEADERS FROM 'file:////{fileName}.csv' AS row " +
                         $" CREATE ({variableName}:{fileName}) " +
                         $" SET {variableName} = row, ";

            var cypherPartTwo = string.Empty;
            for (int i = 1; i < attributesNames.Count; i++)
            {
                var attributeName = attributesNames[i];
                if (attributeName.Contains("Id"))
                {
                    if ((i + 1) == attributesNames.Count)
                    {
                        cypherPartTwo += $" {variableName}.{attributeName} = toInteger(row.{attributeName}) ";
                    }
                    else
                    {
                        cypherPartTwo += $" {variableName}.{attributeName} = toInteger(row.{attributeName}), ";
                    }

                }
                else
                {
                    if ((i + 1) == attributesNames.Count)
                    {
                        cypherPartTwo += $" {variableName}.{attributeName} = row.{attributeName} ";
                    }
                    else
                    {
                        cypherPartTwo += $" {variableName}.{attributeName} = row.{attributeName}, ";
                    }
                }

            }

            var cypher = cypherPartOne + cypherPartTwo + ";";
            return cypher;
        }
        private async Task<string> CreateRelationshipsAsync(string tableName, string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo, string inputOrOutput)
        {

            // :auto USING PERIODIC COMMIT 500  
            // LOAD CSV WITH HEADERS FROM 'file:////TjänstAndTjänst.csv' AS row 
            // WITH toInteger(row.ObjectId) AS ObjectId, toInteger(row.RelatedObjectId) AS RelatedObjectId  
            // MATCH(t: Tjänst { ObjectId: ObjectId})  
            // MATCH(t: Tjänst { RelatedObjectId: RelatedObjectId}) 
            // MERGE(t) -[rel: INGAR_I]->(a)  
            // RETURN count(rel); 

            var fileName = await _fileServices.CreateFileNameAsync(tableName, conString, objectIdOne, objectIdTwo, modelNameOne);
            var relationshipsRows = await _cSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectIdOne, objectIdTwo, modelNameOne, modelNameTwo, modelNameTwo);
            var attributesNames = relationshipsRows.RowsFromSqlServer[0].Split(",").ToList();
           
            var nodeNames = fileName.Split("And").ToList();

            var cypherPartOne = ":auto USING PERIODIC COMMIT 500 " +
                            $" LOAD CSV WITH HEADERS FROM 'file:////{fileName}.csv' AS row ";

            var cypherPartTwo = "WITH ";
            for (int i = 0; i < attributesNames.Count; i++)
            {
                var attributeName = attributesNames[i];
                if (attributeName.Contains("Id"))
                {
                    if ((i + 1) == attributesNames.Count)
                    {
                        cypherPartTwo += $" toInteger(row.{attributeName}) AS {attributeName} ";
                    }
                    else
                    {
                        cypherPartTwo += $" toInteger(row.{attributeName}) AS {attributeName}, ";
                    }

                }
                else
                {
                    if ((i + 1) == attributesNames.Count)
                    {
                        cypherPartTwo += $" toInteger(row.{attributeName}) AS {attributeName} ";
                    }
                    else
                    {
                        cypherPartTwo += $" toInteger(row.{attributeName}) AS {attributeName}, ";
                    }
                }

            }

            var cypherPartThree = string.Empty;
            for (int i = 0; i < nodeNames.Count; i++)
            {
                var variableName = CreateVariableNameForNodes(nodeNames[i]);
                var attributeName = attributesNames[i];

                cypherPartThree += $" MATCH({variableName}: {nodeNames[i]}" + " {" + $" {attributeName}: {attributeName}" + "}) ";

            }

            var variableNameOne = CreateVariableNameForNodes(nodeNames[0]);
            var variableNameTwo = CreateVariableNameForNodes(nodeNames[1]);
            var cypherPartFour = $" MERGE({variableNameOne}) -[rel: INGAR_I]->({variableNameTwo}) " +
              " RETURN count(rel) ";


            var cypher = cypherPartOne + cypherPartTwo + cypherPartThree + cypherPartFour + ";";
            return cypher;
        }
        static string CreateVariableNameForNodes(string fileName)
        {
            var variable = fileName.Substring(0, 1).ToLower();
            return variable;
        }

        public async Task<string> GenerateCypherCodeAsync(string tableName, string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo, string inputOrOutput)
        {
            if (objectIdTwo == 0)
            {
                var cypher = await CreateNodesAsync( tableName, conString, objectIdOne, objectIdTwo, modelNameOne, inputOrOutput);
                return cypher;
            }
            else
            {
                var cypher = await CreateRelationshipsAsync(tableName, conString, objectIdOne, objectIdTwo, modelNameOne, modelNameTwo, inputOrOutput);
                return cypher;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileConverter.Data;
using FileConverter.Models;
using FileConverter.Services;
using FileConverter.ViewModels;
using ExcelDataReader;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.IO.Compression;

namespace FileConverter.Controllers
{
    public class SqlServerToCSVController : Controller
    {
        private readonly IDatabaseServices _databaseServices;
        private readonly ICSVServices _CSVServices;
        private readonly IFileServices _fileServices;
        private readonly ICypherServices _cypherServices;

        public SqlServerToCSVController
            (
            IDatabaseServices databaseServices,
             ICypherServices cypherServices,
            ICSVServices csvServices,
            IFileServices fileServices)
        {
            _databaseServices = databaseServices;
            _cypherServices = cypherServices;
            _CSVServices = csvServices;
            _fileServices = fileServices;
        }
        [HttpGet]
        public IActionResult Fetch()
        {
            var sQLServer = new SQLServer();

            return View(new DocumentFileViewModel
            {
                SQLServer = sQLServer
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fetch(DocumentFileViewModel newDocumentFile)
        {
            var conString = _databaseServices.GetConfigString(newDocumentFile);
            var sQLServerConfig = new SQLServerConfig
            {
                ConString = conString,
                Database = newDocumentFile.SQLServerConfig.Database
            };

            var attributesByTable = await _databaseServices.GetAllColumnsSortedByTableAsync(conString, null);
            var tables = await _databaseServices.GetAllTablesAsync(conString);
            var numberOfTables = attributesByTable.Count;
            var objectsTypesNames = await _databaseServices.GetAllObjectsTypesNamesAsync(tables, conString);
            var modelsNames = await _databaseServices.GetModelsNamesAsync(conString);
            var numberOfColumns = attributesByTable.Count;

            var sQLServer = new SQLServer
            {
                NumberOfTables = numberOfTables,
                Tables = tables,
                NumberOfColumns = numberOfColumns
            };

            return View(new DocumentFileViewModel
            {
                AttributesByTable = attributesByTable,
                SQLServer = sQLServer,
                SQLServerConfig = sQLServerConfig,
                ObjectsTypesNames = objectsTypesNames,
                ModelsNames = modelsNames
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(string tableName, string conString, int objectIdOne, int ObjectIdTwo, string modelNameOne, string modelNameTwo, bool zipDownloadingFormat, string inputOutputMessage, bool isCypher)
        {
            var csv = await _CSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectIdOne, ObjectIdTwo, modelNameOne, modelNameTwo, inputOutputMessage);
            var csvFileName = await _fileServices.CreateFileNameAsync(tableName, conString, objectIdOne, ObjectIdTwo, modelNameOne);



            if (!zipDownloadingFormat && !isCypher)
            {
                var csvDownloadFormat = _CSVServices.BuildCsvStringFromSQLServer(csv);
                var fileContents = _fileServices.GetFileContents(csvDownloadFormat);
                return File(fileContents, "text/csv", $"{csvFileName.Trim()}.csv");
            }
            else if (isCypher)
            {
                //var fileContents = _fileServices.GetFileContents(cypher);
                //return File(fileContents, "text/txt", $"{fileName.Trim() +"-Cypher"}.txt");
                var cypherAndCsvDownloadFormat = new List<string>();

                var csvDownloadFormat = _CSVServices.BuildCsvStringFromSQLServer(csv);
                cypherAndCsvDownloadFormat.Add(csvDownloadFormat);           

                var cypher = await _cypherServices.GenerateCypherCodeAsync(tableName, conString, objectIdOne, ObjectIdTwo, modelNameOne, modelNameTwo, inputOutputMessage);
                cypherAndCsvDownloadFormat.Add(cypher);

                var fileContents = _fileServices.GetZipFileMultipleContentsFormat(cypherAndCsvDownloadFormat, csvFileName);
                return File(fileContents, "application/zip", $"{csvFileName.Trim()}.zip", true);
            }
            else
            {
                var multipleCsvDownloadFormat = _CSVServices.BuildMultipleCsvStringsFromSQLServer(csv);
                var fileContents = _fileServices.GetZipFileContents(multipleCsvDownloadFormat);
                return File(fileContents, "application/zip", $"{csvFileName.Trim()}.zip", true);
            }
        }

    }
}

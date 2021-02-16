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
        private readonly DocumentFileDbContext _context;
        private readonly IDatabaseServices _databaseServices;
        private readonly IXlsxServices _xlsxServices;
        private readonly ICSVServices _CSVServices;
        private readonly IFileServices _fileServices;

        public SqlServerToCSVController
            (DocumentFileDbContext context,
            IDatabaseServices databaseServices,
            IXlsxServices xlsxServices,
            ICSVServices csvServices,
            IFileServices fileServices)
        {
            _context = context;
            _databaseServices = databaseServices;
            _xlsxServices = xlsxServices;
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

            var attributesByTable = await _databaseServices.GetAllAttributesSortedByTableAsync(conString, null);
            var tables = await _databaseServices.GetAllTablesAsync(conString);
            var numberOfTables = attributesByTable.Count();
            var objectsTypesNames = await _databaseServices.GetAllObjectsTypesNamesAsync(tables, conString);
            var modelsNames = await _databaseServices.GetModelsNamesAsync(conString);

            var sQLServer = new SQLServer
            {
                NumberOfTables = numberOfTables,
                Tables = tables
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
        public async Task<IActionResult> Download(string tableName, string conString, int objectIdOne, int ObjectIdTwo, string modelName, bool zipDownloadingFormat)
        {
            var csv = new CSV();
            if (tableName == null && objectIdOne > 0)
            {
                csv = await _CSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectIdOne, ObjectIdTwo, modelName);
                tableName = await _databaseServices.GetObjectTypeNameByClassIdAsync(objectIdOne, conString);
            }
            else if (tableName != null && objectIdOne == 0)
            {
                tableName = "ObjectCount";
                csv = await _CSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectIdOne, ObjectIdTwo, modelName);
            }
            else
            {
                if (tableName == "Download all")
                {
                    tableName = "ObjectCount";
                    csv = await _CSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectIdOne, ObjectIdTwo, modelName);
                    tableName = DateTime.Now.ToShortDateString() + "- Converted SQLServer file";
                }
                else
                {
                    csv = await _CSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectIdOne, ObjectIdTwo, modelName);
                    tableName = DateTime.Now.ToShortDateString() + "- Converted SQLServer file";
                }

            }

            if (!zipDownloadingFormat)
            {
                var csvDownloadFormat = _CSVServices.BuildCsvStringFromSQLServer(csv);
                var fileContents = _fileServices.GetFileContents(csvDownloadFormat);
                return File(fileContents, "text/csv", $"{tableName.Trim()}.csv");
            }
            else
            {
                var multipleCsvDownloadFormat = _CSVServices.BuildMultipleCsvStringsFromSQLServer(csv);
                var fileContents = _fileServices.GetZipFileContents(multipleCsvDownloadFormat);
                return File(fileContents, "application/zip", $"{tableName.Trim()}.zip", true);
            }
        }

    }
}

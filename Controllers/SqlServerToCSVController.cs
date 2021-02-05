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
            var database = new Database();        

            return View(new DocumentFileViewModel
            {
                Database = database
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fetch(DocumentFileViewModel newDocumentFile)
        {
            var fileName = newDocumentFile.TableName;

            var conString = _databaseServices.GetConfigString(newDocumentFile);
            var sQLServerConfig = new SQLServerConfig
            {
                ConString = conString
            };            

            var _attributesByTable = await _databaseServices.GetAllAttributesAsync(conString, fileName, null);
            var tables = await _databaseServices.GetAllDatabaseTablesAsync(conString);
            var numberOfTables = _attributesByTable.Count();
            var servicesNames = await _databaseServices.GetAllDataTypesNamesAsync(tables, conString);
            //var services = await _databaseServices.GetAllDataTypesNamesAsync(tables, conString);
            //var servicesNames = new List<string>();
            //foreach (var service in services)
            //{
            //    servicesNames.Add(service.Key);
            //}
            var databases = new Database
            {
                NumberOfTables = numberOfTables,
                Tables = tables
            };

            return View(new DocumentFileViewModel
            {
                AttributesByTable = _attributesByTable,
                Database = databases,
                SQLServerConfig = sQLServerConfig,
                ServicesNames = servicesNames
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(string tableName, string conString, int objectId)
        {
            var csv = await _CSVServices.ConvertSQLServerToCSVAsync(conString, tableName, objectId);
            var csvDownloadFormat = _CSVServices.BuildCsvStringFromSQLServer(csv);
            var fileContents = _fileServices.GetFileContents(csvDownloadFormat);
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = DateTime.Now.ToShortDateString() + "- Converted SQLServer file";
            }            
            return File(fileContents, "text/csv", $"{tableName}.csv");
        }

    }
}

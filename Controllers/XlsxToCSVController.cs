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

namespace FileConverter.Controllers
{
    public class XlsxToCSVController : Controller
    {
        private readonly DocumentFileDbContext _context;
        private readonly IDatabaseServices _databaseServices;
        private readonly IXlsxServices _xlsxServices;
        private readonly ICSVServices _CSVServices;
        private readonly IFileServices _fileServices;
     
        public XlsxToCSVController
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
        public IActionResult Index(string path, string fileName)
        {
            var csv = new CSV();
            var filePath = string.Empty;

            if (path != null)
            {
                filePath = path;
                csv = _CSVServices.ConvertXlsxToCSV(filePath);
            }

            return View(new DocumentFileViewModel
            {
                CSV = csv,
                FilePath = filePath,
                TableName = fileName
            });
        }

        [HttpPost("FileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            var _uploadedFile = await _fileServices.UploadAsync(files);
            var _path = _uploadedFile.Path;
            var _fileName = _uploadedFile.Name;

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
           return RedirectToAction(nameof(Index), new { path = _path, fileName = _fileName});
        }

        public IActionResult Download(string filePath, string fileName)
        {           
            var csv = _CSVServices.ConvertXlsxToCSV(filePath);
            var csvDownloadFormat = _CSVServices.BuildCsvString(csv);

            var fileContents = _fileServices.GetFileContents(csvDownloadFormat);
            return File(fileContents, "text/csv", $"{fileName}.csv");
        }

    }
}

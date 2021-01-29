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
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;

namespace FileConverter.Controllers
{
    public class XlsxToJsonController : Controller
    {
        private readonly DocumentFileDbContext _context;
		private readonly IDatabaseServices _databaseServices;
		private readonly IXlsxServices _xlsxServices;
        private readonly IWebHostEnvironment _webHostEnvironment;        

        public XlsxToJsonController
			(DocumentFileDbContext context, 
			IDatabaseServices databaseServices,
			IXlsxServices xlsxServices,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
			_databaseServices = databaseServices;
			_xlsxServices = xlsxServices;
            _webHostEnvironment = webHostEnvironment;

        }


		[HttpGet]
		public IActionResult ViewXLSX()
		{
			var excelSheet = new ExcelSheet();
			return View(new DocumentFileViewModel
			{
				ExcelSheet = excelSheet,
			});
		}

		[HttpPost]
		public IActionResult ViewXLSX(IFormCollection form)
		{	
            var fileName = "./wwwroot/ExcelTest.xlsx";

			var excelSheet = _xlsxServices.GetDataFromXlsxFile(fileName);

			return View(new DocumentFileViewModel
			{
				ExcelSheet = excelSheet,
            });
		}




        //[HttpPost("FileUpload")]
        //public async Task<IActionResult> Upload(List<IFormFile> files)
        //{
        //    long size = files.Sum(f => f.Length);

        //    var filePaths = new List<string>();
        //    foreach (var formFile in files)
        //    {
        //        if (formFile.Length > 0)
        //        {
        //            // full path to file in temp location
        //            var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
        //            filePaths.Add(filePath);
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await formFile.CopyToAsync(stream);
        //            }
        //        }
        //    }
        //    // process uploaded files
        //    // Don't rely on or trust the FileName property without validation.
        //    return Ok(new { count = files.Count, size, filePaths });
        //}

       
    }
}

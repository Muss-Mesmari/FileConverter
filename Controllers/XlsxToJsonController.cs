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
		  
    }
}

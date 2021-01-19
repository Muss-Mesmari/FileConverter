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

namespace FileConverter.Controllers
{
    public class XlsxToCSVController : Controller
    {
        private readonly DocumentFileDbContext _context;
		private readonly IDatabaseServices _databaseServices;
		private readonly IXlsxServices _xlsxServices;

		public XlsxToCSVController
			(DocumentFileDbContext context, 
			IDatabaseServices databaseServices,
			IXlsxServices xlsxServices)
        {
            _context = context;
			_databaseServices = databaseServices;
			_xlsxServices = xlsxServices;
		}

        // GET: XlsxToJson
        public async Task<IActionResult> Index()
        {
			return View(new DocumentFileViewModel
			{
			});
		}


		[HttpGet]
		public IActionResult Convert()
		{
			var excelSheet = new ExcelSheet();
			return View(new DocumentFileViewModel
			{
				ExcelSheet = excelSheet,
			});
		}

		[HttpPost]
		public IActionResult Fetch(IFormCollection form)
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

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
		private readonly ICSVServices _CSVServices;

		public XlsxToCSVController
			(DocumentFileDbContext context, 
			IDatabaseServices databaseServices,
			IXlsxServices xlsxServices,
			ICSVServices csvServices)
        {
            _context = context;
			_databaseServices = databaseServices;
			_xlsxServices = xlsxServices;
			_CSVServices = csvServices;
		}

		// GET: XlsxToJson
		[HttpGet]
		public IActionResult Convert()
        {
			var csv = new CSV();
			return View(new DocumentFileViewModel
			{
				CSV = csv
			});
		}


		[HttpPost]
		public IActionResult Convert(IFormCollection form)
		{	
            var fileName = "./wwwroot/ExcelTest.xlsx";

			var csv = _CSVServices.ConvertXlsxToCSV(fileName);

			return View(new DocumentFileViewModel
			{
				CSV = csv,
            });
		}

	}
}

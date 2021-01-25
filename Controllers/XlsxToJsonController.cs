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

        // GET: XlsxToJson
        public async Task<IActionResult> Index()
        {
			var documentFiles = await _databaseServices.GetAllDocumentFilesAsync();
			return View(new DocumentFileViewModel
			{
				DocumentFiles = documentFiles,
			});
		}

        // GET: XlsxToJson/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
			var documentFile = await _databaseServices.GetDocumentFileByIdAsync(id);
			if (documentFile == null)
            {
                return NotFound();
            }
			return View(new DocumentFileViewModel
			{
				DocumentFile = documentFile,
			});
        }

        // GET: XlsxToJson/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: XlsxToJson/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentFileViewModel newDocumentFile)
        {
            if (ModelState.IsValid)
            {
			     await _databaseServices.CreateDocumentFileAsync(newDocumentFile);
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // GET: XlsxToJson/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
			var documentViewModel = new DocumentFileViewModel
			{
				DocumentFile = await _databaseServices.GetDocumentFileByIdAsync(id),
			};
            if (documentViewModel == null)
            {
                return NotFound();
            }
            return View(documentViewModel);
        }

        // POST: XlsxToJson/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentFileViewModel newDocumentFile)
        {
			var documentFile = await _databaseServices.GetDocumentFileByIdAsync(id);
			if (id != documentFile.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
            {
                try
                {
					await _databaseServices.UpdateDocumentFileAsync(newDocumentFile);
                }
                catch (DbUpdateConcurrencyException)
                {
					var updateDocumentFile = await _databaseServices.UpdateDocumentFileAsync(newDocumentFile);
					if (!_databaseServices.DocumentFileExists(updateDocumentFile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(newDocumentFile);
        }

        // GET: XlsxToJson/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
			var documentViewModel = new DocumentFileViewModel
			{
				DocumentFile = await _databaseServices.GetDocumentFileByIdAsync(id),
			};
			if (documentViewModel == null)
            {
                return NotFound();
            }
            return View(documentViewModel);
        }

        // POST: XlsxToJson/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
			await _databaseServices.DeleteDocumentFile(id);
			return RedirectToAction(nameof(Index));
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

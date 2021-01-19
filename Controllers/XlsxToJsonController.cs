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
    public class XlsxToJsonController : Controller
    {
        private readonly DocumentFileDbContext _context;
		private readonly IDatabaseServices _databaseServices;

		public XlsxToJsonController(DocumentFileDbContext context, IDatabaseServices databaseServices)
        {
            _context = context;
			_databaseServices = databaseServices;
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
		public IActionResult Fetch()
		{
			//return View(new List<ExcelSheet>());

			var excelSheet = new ExcelSheet();
			return View(new DocumentFileViewModel
			{
				ExcelSheet = excelSheet,
			});
		}

		[HttpPost]
		public IActionResult Fetch(IFormCollection form)
		{
			List<string> columns = new List<string>();

			var fileName = "./wwwroot/ExcelTest.xlsx";
			// For .net core, the next line requires the NuGet package, 
			// System.Text.Encoding.CodePages
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
			{
				using (var reader = ExcelReaderFactory.CreateReader(stream))
				{
					var columnsCount = reader.FieldCount;
					var rowsCount = reader.RowCount;

					while (reader.Read()) //Each row of the file
					{
						for (int i = 0; i < columnsCount; i++)
						{
							for (int j = 0; j < rowsCount; j++)
							{
								var column = reader.GetValue(i).ToString();
								columns.Add(column);								
							}

						}
					}
				}
			}

			var excelSheet = new ExcelSheet
			{
				Column = columns,
			};

			return View(new DocumentFileViewModel
			{
				ExcelSheet = excelSheet,
			});
		}
	}
}

using ExcelDataReader;
using FileConverter.Data;
using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FileConverter.Services
{
	public class XlsxServices : IXlsxServices
	{
		private readonly DocumentFileDbContext _documentFileDbContext;

		public XlsxServices(DocumentFileDbContext documentFileDbContext)
		{
			_documentFileDbContext = documentFileDbContext;
		}

		public ExcelSheet GetDataFromXlsxFile(string fileLink)
		{
			List<List<string>> table = new List<List<string>>();
			List<string> headers = new List<string>();
			var numberOfColumns = 0;
			var numberOfRows = 0;
			var sheetName = string.Empty;
			var FileName = string.Empty;

			// For .net core, the next line requires the NuGet package, 
			// System.Text.Encoding.CodePages
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			using (var stream = System.IO.File.Open(fileLink, FileMode.Open, FileAccess.Read))
			{
				using (var reader = ExcelReaderFactory.CreateReader(stream))
				{
					numberOfColumns = reader.FieldCount;
					numberOfRows = reader.RowCount;
					sheetName = reader.Name;				

					int rowsCount = 0;
					while (reader.Read()) //Each row of the file
					{
						List<string> row = new List<string>();
						for (int i = 0; i < numberOfColumns; i++)
						{							
							var cell = reader.GetValue(i).ToString();
							if (rowsCount == 0)
							{
								headers.Add(cell);
							}
							else
							{
								row.Add(cell);
							}							
						}
						if (rowsCount != 0)
						{
							table.Add(row);
						}
						
						rowsCount += 1;
					}
				}
			}

			var excelSheet = new ExcelSheet
			{
				Table = table,
				NumberOfColumns = numberOfColumns,
				NumberOfRows = numberOfRows,
				Headers = headers,
				SheetName = sheetName
			};

			return excelSheet;
		}
	}
}

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
	public class CSVServices : ICSVServices
	{
		private readonly DocumentFileDbContext _documentFileDbContext;
		private readonly IXlsxServices _xlsxServices;

		public CSVServices(
			DocumentFileDbContext documentFileDbContext, 
			IXlsxServices xlsxServices
			)
		{
			_documentFileDbContext = documentFileDbContext;
			_xlsxServices = xlsxServices;
		}

		public CSV ConvertXlsxToCSV(string fileLink)
		{

			var excelSheetHeaders = _xlsxServices.GetDataFromXlsxFile(fileLink).Headers;
			var excelSheetTable = _xlsxServices.GetDataFromXlsxFile(fileLink).Table;

			var excelSheet = _xlsxServices.GetDataFromXlsxFile(fileLink);

			var csv = new CSV
			{
				Headers = ConvertXlsxHeadersToCSV(excelSheetHeaders),
				Rows = ConvertXlsxTableToCSV(excelSheetTable),
				NumberOfRows = CountCsvRows(excelSheet)
		};
			return csv;
		}

		private string ConvertXlsxHeadersToCSV(List<string> excelSheetHeaders)
		{
			var csvHeaders = String.Join(",", excelSheetHeaders);
			return csvHeaders;
		}

		private List<string> ConvertXlsxTableToCSV(List<List<string>> excelSheetTable)
		{
			List<string> csvRows = new List<string>();

			foreach (var row in excelSheetTable)
			{
				var csvRow = String.Join(",", row);
				csvRows.Add(csvRow);
			}

			return csvRows;
		}

		private int CountCsvRows(ExcelSheet excelSheet)
		{
			var totalTableRows = excelSheet.Table.Count();
			var totalHeadersRows = 1;
			var totalRows = totalTableRows + totalHeadersRows;

			return totalRows;
		}
	}
}

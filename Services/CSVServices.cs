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
using System.Text;

namespace FileConverter.Services
{
	public class CSVServices : ICSVServices
	{
		private readonly DocumentFileDbContext _documentFileDbContext;
		private readonly IXlsxServices _xlsxServices;
		private readonly IDatabaseServices _databaseServices;
		public CSVServices(
			DocumentFileDbContext documentFileDbContext, 
			IXlsxServices xlsxServices,
			IDatabaseServices databaseServices
			)
		{
			_documentFileDbContext = documentFileDbContext;
			_xlsxServices = xlsxServices;
			_databaseServices = databaseServices;
		}

		public CSV ConvertXlsxToCSV(string filePath)
		{
			var excelSheetHeaders = _xlsxServices.GetDataFromXlsxFile(filePath).Headers;
			var excelSheetRows = _xlsxServices.GetDataFromXlsxFile(filePath).Rows;
			var excelSheet = _xlsxServices.GetDataFromXlsxFile(filePath);

			var csv = new CSV
			{
				HeadersFromXlsxFile = ConvertXlsxHeadersToCSV(excelSheetHeaders),
				RowsFromXlsxFile = ConvertXlsxRowsToCSV(excelSheetRows),
				NumberOfRows = CountCsvRows(excelSheet),
				NumberOfHeaders = CountCsvHeaders(excelSheet)
			};
			return csv;
		}
		private string ConvertXlsxHeadersToCSV(List<string> excelSheetHeaders)
		{
			var csvHeaders = String.Join(",", excelSheetHeaders);
			return csvHeaders;
		}
		private List<string> ConvertXlsxRowsToCSV(List<List<string>> excelSheetTable)
		{
			List<string> csvRows = new List<string>();

			foreach (var row in excelSheetTable)
			{
				var doubleQuote = "\"";
				var validCsvRow = ValidateStrings(row);

				var csvRow = doubleQuote + string.Join(@""",""", validCsvRow) + doubleQuote;
				csvRows.Add(csvRow);
			}

			return csvRows;
		}
		private List<string> ValidateStrings(List<string> row)
		{
			List<string> csvRows = new List<string>();

			foreach (var value in row)
			{
				if (value.GetType() == typeof(string))
				{
					if (value.Contains("\""))
					{
						var newValue = value.Replace("\"", "\"\"");
						csvRows.Add(newValue);
					}
					else
					{
						csvRows.Add(value);
					}
				}
				else
				{
					csvRows.Add(value);
				}

			}
			return csvRows;
		}

		//private List<string> CreateValidCSVRow(List<string> row)
		//{
		//	List<string> csvRows = new List<string>();

		//	foreach (var value in row)
		//	{
		//              if (value.GetType() == typeof(int))
		//              {
		//			if (value.Contains(","))
		//			{
		//				var newValue = value.Replace(",", ".");
		//				csvRows.Add(newValue);
		//			}
		//			else
		//			{
		//				csvRows.Add(value);
		//			}
		//		}
		//		else
		//		{
		//			csvRows.Add(value);
		//		}

		//	}
		//	return csvRows;
		//}       
		private int CountCsvRows(ExcelSheet excelSheet)
		{
			var numberOfRows = excelSheet.Rows.Count();
			var numberOfHeaders = 1;
			var totalRows = numberOfRows + numberOfHeaders;

			return totalRows;
		}
		private int CountCsvHeaders(ExcelSheet excelSheet)
		{
			var numberOfHeaders = excelSheet.Headers.Count();
			return numberOfHeaders;
		}
		public string BuildCsvStringFromXlsxFile(CSV csv)
		{
			var doubleQuote = "\"";
			var builder = new StringBuilder();

			builder.AppendLine(csv.HeadersFromXlsxFile);

			if (csv.RowsFromXlsxFile != null)
			{
				foreach (var row in csv.RowsFromXlsxFile)
				{
					builder.AppendLine(doubleQuote + doubleQuote + row);
				}
			}

			return builder.ToString();
		}


		public async Task<CSV> ConvertSQLServerToCSVAsync(string conString, string tableName, int objectId)
		{
			var sqlServerAttributesByTable = new List<KeyValuePair<string, List<string>>>();
			var sqlServerRows = new List<string>();

			if (objectId == 0)
            {
				sqlServerAttributesByTable = await _databaseServices.GetAllAttributesByTableAsync(conString, tableName, null);
			}
            else
            {
				var sqlServerRowsByTables = await _databaseServices.GetDataFromTableByIdAsync(conString, objectId);
				sqlServerRows = ConvertSqlServerRowToCSV(sqlServerRowsByTables);
			}

			var sqlServerHeaders = new List<string>();
			if (!string.IsNullOrEmpty(tableName))
            {
				sqlServerHeaders = MatchSqlServerHeadersByTable(tableName, sqlServerAttributesByTable);
			}
            else
            {
				sqlServerHeaders = ConvertSqlServerHeadersToCSV(sqlServerAttributesByTable);
			}
			
			var csv = new CSV
			{
				HeadersFromSqlServer = sqlServerHeaders,
				RowsFromSqlServer = sqlServerRows
			};
			return csv;
		}
		private List<string> MatchSqlServerHeadersByTable(string tableName, List<KeyValuePair<string, List<string>>> sqlServerAttributesByTable)
		{
			List<string> csvHeaders = new List<string>();

			for (int i = 0; i < sqlServerAttributesByTable.Count(); i++)
			{
				if (sqlServerAttributesByTable[i].Key == tableName)
				{
					var header = String.Join(",", sqlServerAttributesByTable[i].Value);
					csvHeaders.Add(header);
				}
			}
			return csvHeaders;
		}
		private List<string> ConvertSqlServerHeadersToCSV(List<KeyValuePair<string, List<string>>> sqlServerAttributesByTable)
		{
			List<string> csvHeaders = new List<string>();
            for (int i = 0; i < sqlServerAttributesByTable.Count(); i++)
            {				
				var header = String.Join(",", sqlServerAttributesByTable[i].Value);
				csvHeaders.Add(header);
			}		
			return csvHeaders;
		}
		private List<string> ConvertSqlServerRowToCSV(List<KeyValuePair<string, int>> sqlServerRowsByTables)
		{
			List<string> csvRows = new List<string>();
			for (int i = 0; i < sqlServerRowsByTables.Count(); i++)
			{
				var doubleQuote = "\"";
				var csvRow = doubleQuote + sqlServerRowsByTables[i].Value.ToString() + doubleQuote + doubleQuote + "," + doubleQuote + doubleQuote + sqlServerRowsByTables[i].Key.ToString() + doubleQuote + doubleQuote;
				csvRows.Add(csvRow);
			}
			return csvRows;
		}
		public string BuildCsvStringFromSQLServer(CSV csv)
		{
			var doubleQuote = "\"";
            var builder = new StringBuilder();

			if (csv.HeadersFromSqlServer != null)
			{
				foreach (var header in csv.HeadersFromSqlServer)
				{
					builder.AppendLine(header);
				}
			}

			if (csv.RowsFromSqlServer != null)
			{
				foreach (var row in csv.RowsFromSqlServer)
				{
					builder.AppendLine(doubleQuote + doubleQuote + row);
				}
			}

			return builder.ToString();
		}
	}
}

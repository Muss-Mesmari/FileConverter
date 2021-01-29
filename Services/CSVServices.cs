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

		public CSV ConvertXlsxToCSV(string fileLink)
		{

			var excelSheetHeaders = _xlsxServices.GetDataFromXlsxFile(fileLink).Headers;
			var excelSheetTable = _xlsxServices.GetDataFromXlsxFile(fileLink).Table;

			var excelSheet = _xlsxServices.GetDataFromXlsxFile(fileLink);

			var csv = new CSV
			{
				Headers = ConvertXlsxHeadersToCSV(excelSheetHeaders),
				Rows = ConvertXlsxTableToCSV(excelSheetTable),
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

		private int CountCsvHeaders(ExcelSheet excelSheet)
		{
			var totalHeaders = excelSheet.Headers.Count();
			return totalHeaders;
		}

		public string BuildCsvString(CSV csv)
		{
			var builder = new StringBuilder();
			builder.AppendLine(csv.Headers);

            if (csv.Rows != null)
            {
				foreach (var row in csv.Rows)
				{
					builder.AppendLine(row);
				}
			}
            
			return builder.ToString();
		}

		public string BuildCsvStringFromSQLServer(CSV csv)
		{
			var builder = new StringBuilder();

			if (csv.HeadersFromSqlServer != null)
			{
				foreach (var header in csv.HeadersFromSqlServer)
				{
					builder.AppendLine(header);
				}
			}

			return builder.ToString();
		}


		public async Task<CSV> ConvertSQLServerToCSVAsync(string conString, string tableName)
		{
			var sqlServerHeadersAndTables = await _databaseServices.GetAllAttributesAsync(conString, tableName);

			var sqlServerHeaders = new List<string>();

			if (!string.IsNullOrEmpty(tableName))
            {
				sqlServerHeaders =MatchTable(tableName, sqlServerHeadersAndTables);
			}
            else
            {
				sqlServerHeaders = ConvertSqlServerHeadersToCSV(sqlServerHeadersAndTables);
			}
			
			var csv = new CSV
			{
				HeadersFromSqlServer = sqlServerHeaders,
			};
			return csv;
		}

		private List<string> ConvertSqlServerHeadersToCSV(List<KeyValuePair<string, List<string>>> sqlServerHeaders)
		{
			List<string> csvHeaders = new List<string>();
            for (int i = 0; i < sqlServerHeaders.Count(); i++)
            {				
				var header = String.Join(",", sqlServerHeaders[i].Value);
				csvHeaders.Add(header);
			}		
			return csvHeaders;
		}

		private List<string> MatchTable(string tableName, List<KeyValuePair<string, List<string>>> sqlServerHeaders)
		{
			List<string> csvHeaders = new List<string>();

			for (int i = 0; i < sqlServerHeaders.Count(); i++)
			{
                if (sqlServerHeaders[i].Key == tableName)
                {
					var header = String.Join(",", sqlServerHeaders[i].Value);
					csvHeaders.Add(header);
				}
			}
			return csvHeaders;
		}
	}
}

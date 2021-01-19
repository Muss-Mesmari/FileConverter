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
	public class CSVServices 
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
			var excelSheet =_xlsxServices.GetDataFromXlsxFile(fileLink);


			var csv = new CSV
			{
			};
			return csv;
		}
	}
}

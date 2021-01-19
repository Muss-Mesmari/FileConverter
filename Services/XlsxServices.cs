using ExcelDataReader;
using FileConverter.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public class XlsxServices
	{
		private readonly DocumentFileDbContext _documentFileDbContext;

		public XlsxServices(DocumentFileDbContext documentFileDbContext)
		{
			_documentFileDbContext = documentFileDbContext;
		}


		private List<string> GetDataFromCSVFile(Stream stream)
		{
			var empList = new List<string>();
			try
			{
				using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
				{
					var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
					{
						ConfigureDataTable = _ => new ExcelDataTableConfiguration
						{
							UseHeaderRow = true // To set First Row As Column Names    
						}
					});

					if (dataSet.Tables.Count > 0)
					{
						var dataTable = dataSet.Tables[0];
						foreach (DataRow objDataRow in dataTable.Rows)
						{
							if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
							var name = objDataRow["Name"].ToString();
							empList.Add(name);
						}
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return empList;
		}
	}
}

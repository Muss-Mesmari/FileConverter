using FileConverter.Data;
using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public class DatabaseServices : IDatabaseServices
	{
		private readonly DocumentFileDbContext _documentFileDbContext;

		public DatabaseServices(DocumentFileDbContext documentFileDbContext)
		{
			_documentFileDbContext = documentFileDbContext;
		}

		public string GetConfigString(DocumentFileViewModel documentFileViewModel)
		{
			var server = documentFileViewModel.SQLServerConfig.Server;
			var database = documentFileViewModel.SQLServerConfig.Database;
			var userId = documentFileViewModel.SQLServerConfig.UserId;
			var password = documentFileViewModel.SQLServerConfig.Password;

			var conString = $"Server={server};Database={database};User Id={userId};password={password};Trusted_Connection=True;MultipleActiveResultSets=true";
			return conString;
		}
		public async Task<List<KeyValuePair<string, List<string>>>> GetAllAttributesAsync(string conString, string tableName, string modelName)
		{
			var allAttributesByTable = new List<KeyValuePair<string, List<string>>>();

			var tables = new List<string>();

			if (!string.IsNullOrEmpty(tableName))
			{
				var attributes = await GetAttributesByTableAsync(conString, tableName);
				allAttributesByTable.Insert(0, new KeyValuePair<string, List<string>>(tableName, attributes));
			}
			else
			{
				tables = await GetAllDatabaseTablesAsync(conString);

				for (int i = 0; i < tables.Count(); i++)
				{
					if (modelName != null)
					{
						if (tables[i].Contains(modelName))
						{
							var attributes = await GetAttributesByTableAsync(conString, tables[i]);
							allAttributesByTable.Insert(i, new KeyValuePair<string, List<string>>(tables[i], attributes));
						}
					}
					else
					{
						var attributes = await GetAttributesByTableAsync(conString, tables[i]);
						allAttributesByTable.Insert(i, new KeyValuePair<string, List<string>>(tables[i], attributes));
					}

				}
			}

			return allAttributesByTable;
		}
		public async Task<List<string>> GetAllDatabaseTablesAsync(string conString)
		{
			using (SqlConnection connection = new SqlConnection(conString))
			{
				await connection.OpenAsync();

				DataTable schema = await connection.GetSchemaAsync("Tables");
				List<string> TableNames = new List<string>();

				foreach (DataRow row in schema.Rows)
				{
					TableNames.Add(row[2].ToString());
				}
				return TableNames;
			}
		}
		private async Task<List<string>> GetAttributesByTableAsync(string conString, string table)
		{
			var sql = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{table}'";

			using (SqlConnection connection = new SqlConnection(conString))
			using (SqlCommand command = new SqlCommand(sql, connection))
			{
				await connection.OpenAsync();

				SqlDataReader reader = await command.ExecuteReaderAsync();

				var attributeTypes = new List<string>();
				var attributeValues = new List<string>();

				while (await reader.ReadAsync())
				{
					var attributeValue = reader.GetSqlString(0).Value;
					attributeValues.Add(attributeValue);
				}
				return attributeValues;
			}
		}

		public List<string> GetAllModelsNames(List<string> tables)
		{
			List<string> models = new List<string>();
			foreach (var table in tables)
			{
				var model = table.Substring(0, 3);
				models.Add(model);
			}
			return models;
		}

	}

}


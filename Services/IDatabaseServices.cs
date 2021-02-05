using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IDatabaseServices
	{
		Task<List<KeyValuePair<string, List<string>>>> GetAllAttributesAsync(string conString, string fileName, string servicesNames);
		Task<List<string>> GetAllDatabaseTablesAsync(string conString);
		string GetConfigString(DocumentFileViewModel documentFileViewModel);
		Task<List<KeyValuePair<string, int>>> GetAllDataTypesNamesAsync(List<string> tables, string conString);
		Task<List<KeyValuePair<string, int>>> GetDataFromTableAsync(string conString, int objectId);
	}
}
using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IDatabaseServices
	{
		Task<List<KeyValuePair<string, List<string>>>> GetAllAttributesByTableAsync(string conString, string fileName, string servicesNames);
		Task<List<string>> GetAllTablesAsync(string conString);
		string GetConfigString(DocumentFileViewModel documentFileViewModel);
		Task<List<KeyValuePair<string, int>>> GetAllObjectsTypesNamesAsync(List<string> tables, string conString);
		Task<List<KeyValuePair<string, int>>> GetDataFromTableByIdAsync(string conString, int objectId);
		Task<string> GetObjectNameByIdAsync(int objectId, string conString);
	}
}
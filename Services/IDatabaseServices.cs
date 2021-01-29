using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IDatabaseServices
	{
		Task<List<KeyValuePair<string, List<string>>>> GetAllAttributesAsync(string conString, string fileName);
		Task<List<string>> GetAllDatabaseTablesAsync(string conString);
		string GetConfigString(DocumentFileViewModel documentFileViewModel);
	}
}
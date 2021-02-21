using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IDatabaseServices
	{
		Task<List<string>> GetModelsNamesAsync(string conString);
		Task<List<KeyValuePair<int, int>>> GetRelationshipsBetweenAttributesOrAttributesGroupAndOtherObjectssByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelNameOne, string inputOrOutput);
		Task<List<KeyValuePair<int, int>>> GetRelationshipsByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelNameOne, string modelNameTwo);
		Task<List<KeyValuePair<string, List<string>>>> GetAllColumnsSortedByTableAsync(string conString, string fileName);
		Task<List<string>> GetAllTablesAsync(string conString);
		string GetConfigString(DocumentFileViewModel documentFileViewModel);
		Task<List<KeyValuePair<int, string>>> GetAllObjectsTypesNamesAsync(List<string> tables, string conString);
		Task<List<KeyValuePair<int, string>>> GetObjectsNamesAndObjectIdsByClassIdAsync(string conString, int objectId, string modelName);
		Task<List<KeyValuePair<int, List<string>>>> GetPropertiesNamesByPropertiesIdsAsync(string conString, int classId);
		Task<List<KeyValuePair<int, List<string>>>> GetPropertiesValuesByObjectIdsSortedByObjectIdsAsync(string conString, int classId, string modelName);
		Task<string> GetObjectTypeNameByClassIdAsync(int objectId, string conString);
		Task<List<KeyValuePair<int, List<string>>>> GetPropertiesAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelName, string inputOrOutput);
		Task<List<KeyValuePair<int, string>>> GetPropertiesIdsByClassIdAsync(string conString, int classId);
		Task<List<KeyValuePair<int, string>>> GetAttributesOrAttributesGroupsNamesAndIdsThatAreUsedInsideAttributesGroupsByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelName, string inputOrOutput);
	}
}
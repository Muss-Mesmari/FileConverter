﻿using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IDatabaseServices
	{
		Task<List<string>> GetModelsNamesAsync(string conString);
		Task<List<KeyValuePair<int, int>>> GetRelationshipsByClassIdsAsync(string conString, int serviceClassIdOne, int serviceClassIdTwo, string modelName);
		Task<List<KeyValuePair<string, List<string>>>> GetAllAttributesSortedByTableAsync(string conString, string fileName);
		Task<List<string>> GetAllTablesAsync(string conString);
		string GetConfigString(DocumentFileViewModel documentFileViewModel);
		Task<List<KeyValuePair<string, int>>> GetAllObjectsTypesNamesAsync(List<string> tables, string conString);
		Task<List<KeyValuePair<string, int>>> GetObjectsNamesAndObjectIdsByClassIdAsync(string conString, int objectId, string modelName);
		Task<List<KeyValuePair<string, int>>> GetPropertiesNamesByPropertiesIdsAsync(string conString, int classId);
		Task<List<KeyValuePair<int, KeyValuePair<int, string>>>> GetPropertiesValuesByObjectIdsSortedByObjectIdsAsync(string conString, int classId, string modelName);
		Task<string> GetObjectTypeNameByClassIdAsync(int objectId, string conString);
	}
}
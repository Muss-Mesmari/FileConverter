using FileConverter.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
    public interface IJsonServices
    {
        string BuildCsvStringFromSQLServer(CSV csv);
        List<string> BuildMultipleCsvStringsFromSQLServer(CSV csv);
        CSVDownloadingOptions ChooseCSVDownloadingOptions(string tableName, int objectIdOne, int objectIdTwo, string inputOutputMessage);
        Task<List<string>> ConvertSQLServerToJSONAsync(string conString, string tableName, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo, string inputOutputMessage);
    }
}
using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
    public interface ICSVServices
    {
        Task<CSV> ConvertSQLServerToCSVAsync(string conString, string tableName, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo, string inputOutputMessage);
        CSV ConvertXlsxToCSV(string fileName);
        string BuildCsvStringFromXlsxFile(CSV csv);
        string BuildCsvStringFromSQLServer(CSV csv);
        List<string> BuildMultipleCsvStringsFromSQLServer(CSV csv);
        CSVDownloadingOptions ChooseCSVDownloadingOptions(string tableName, int objectIdOne, int objectIdTwo, string inputOutputMessage);
        Task<List<string>> CreateCSVObjectsRowsAsync(string conString, int classId, int classIdTwo, string modelName, string inputOrOutput);
    }
}

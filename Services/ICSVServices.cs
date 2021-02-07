using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
    public interface ICSVServices
    {
        Task<CSV> ConvertSQLServerToCSVAsync(string conString, string tableName, int objectId);
        CSV ConvertXlsxToCSV(string fileName);
        string BuildCsvStringFromXlsxFile(CSV csv);
        string BuildCsvStringFromSQLServer(CSV csv);
        List<string> BuildMultipleCsvStringsFromSQLServer(CSV csv);
    }
}

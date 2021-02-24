using System.Threading.Tasks;

namespace FileConverter.Services
{
    public interface ICypherServices
    {
        Task<string> GenerateCypherCodeAsync(string tableName, string conString, int objectIdOne, int objectIdTwo, string modelNameOne, string modelNameTwo, string inputOrOutput);
    }
}
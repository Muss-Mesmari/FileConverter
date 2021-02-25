using FileConverter.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
    public interface IFileServices
    {
        Byte[] GetZipFileContents(List<string> files);
        Byte[] GetZipFileMultipleContentsFormatCypherIncluded(List<string> files, string csvFileName);
        Byte[] GetZipFileMultipleContentsFormat(List<string> files, string csvFileName);
        Byte[] GetFileContents(string file);
        Task<Files> UploadAsync(List<IFormFile> file);
        Task<string> CreateFileNameAsync(string tableName, string conString, int objectIdOne, int ObjectIdTwo, string modelName, bool isJson);
    }
}
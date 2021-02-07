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
        Byte[] GetFileContents(string file);
        Task<Files> UploadAsync(List<IFormFile> file);
    }
}
using FileConverter.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
    public interface IFileServices
    {
        Byte[] GetFileContents(string file);
        Task<Files> UploadAsync(List<IFormFile> file);
    }
}
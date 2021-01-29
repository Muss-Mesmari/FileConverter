using ExcelDataReader;
using FileConverter.Data;
using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace FileConverter.Services
{
    public class FileServices : IFileServices
    {
        private readonly DocumentFileDbContext _documentFileDbContext;
        private readonly IXlsxServices _xlsxServices;

        public FileServices(
            DocumentFileDbContext documentFileDbContext,
            IXlsxServices xlsxServices
            )
        {
            _documentFileDbContext = documentFileDbContext;
            _xlsxServices = xlsxServices;
        }


        public async Task<Files> UploadAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            var name = files.FirstOrDefault().FileName;
            var type = files.FirstOrDefault().ContentType;

            var paths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    var filePath = Path.GetTempFileName(); // GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                    paths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }                   
                }
            }
            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            var uploadedFile = new Files
            {
                Type = type,
                Size = size,
                Path = paths.FirstOrDefault(),
                Name = name
            };

            return uploadedFile;
        }
        public Byte[] GetFileContents(string file)
        {
            var fileContents = Encoding.UTF8.GetBytes(file);
            return fileContents;
        }

    }
}

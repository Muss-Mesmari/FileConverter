﻿using ExcelDataReader;
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
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace FileConverter.Services
{
    public class FileServices : IFileServices
    {
        private readonly ICSVServices _CSVServices;
        private readonly IDatabaseServices _databaseServices;

        public FileServices(
             ICSVServices csvServices,
              IDatabaseServices databaseServices
            )
        {
            _CSVServices = csvServices;
            _databaseServices = databaseServices;
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

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await formFile.CopyToAsync(stream);
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
            var data = Encoding.UTF8.GetBytes(file);
            var fileContents = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
            return fileContents;
        }
        public Byte[] GetZipFileContents(List<string> files)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var name = string.Empty;
                    if (files[i].ToString().Contains("\""))
                    {
                        name = files[i].Split(",")[1];
                    }
                    else
                    {
                        name = files[i].Split(",").LastOrDefault();
                    }

                    var newName = string.Empty;
                    if (name.Contains("\""))
                    {
                        newName = name.Replace("\"", "").Trim();
                    }
                    else if (string.IsNullOrEmpty(name))
                    {
                        newName = $"Converted SQLServer file No. {i}";
                    }

                    var file1 = archive.CreateEntry($"{newName}.csv", CompressionLevel.Optimal);
                    using var streamWriter = new StreamWriter(file1.Open(), Encoding.UTF8);
                    streamWriter.Write(files[i]);
                }
            }

            return memoryStream.ToArray();
        }
        public async Task<string> CreateFileNameAsync(string tableName, string conString, int objectIdOne, int ObjectIdTwo, string modelName)
        {
            var cSVDownloadingOption = _CSVServices.ChooseCSVDownloadingOptions(tableName, objectIdOne, ObjectIdTwo);
            if (cSVDownloadingOption == CSVDownloadingOptions.Tables)
            {
                var fileName = tableName;
                return fileName;
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.Objects || cSVDownloadingOption == CSVDownloadingOptions.Relationships)
            {
                var fileName = await _databaseServices.GetObjectTypeNameByClassIdAsync(objectIdOne, conString);
                return fileName;
            }
            else if (cSVDownloadingOption == CSVDownloadingOptions.DownloadAllTables)
            {
                var fileName = DateTime.Now.ToShortDateString() + "- Converted SQLServer file";
                return fileName;
            }
            else
            {
                var fileName = DateTime.Now.ToShortDateString() + "- Converted SQLServer file";
                return fileName;
            }            
        }
    }
}

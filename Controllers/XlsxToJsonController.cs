using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileConverter.Data;
using FileConverter.Models;
using FileConverter.Services;
using FileConverter.ViewModels;
using ExcelDataReader;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileConverter.Controllers
{
    public class XlsxToJsonController : Controller
    {
        private readonly IDatabaseServices _databaseServices;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public XlsxToJsonController
            (
            IDatabaseServices databaseServices,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _databaseServices = databaseServices;
            _webHostEnvironment = webHostEnvironment;

        }


        [HttpGet]
        public IActionResult ViewXLSX()
        {
            return View(new DocumentFileViewModel
            {

            });
        }

    }
}

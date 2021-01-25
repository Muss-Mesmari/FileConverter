using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
    public class Files
    {
        public string Type { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
    }
}

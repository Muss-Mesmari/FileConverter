using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IXlsxServices
	{
		ExcelSheet GetDataFromXlsxFile(string fileName);
	}
}

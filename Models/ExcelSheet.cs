using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
	public class ExcelSheet
	{
		public List<List<string>> Table { get; set; }
		public List<string> Headers { get; set; }
		public int NumberOfRows { get; set; }
		public int NumberOfColumns { get; set; }
		public string SheetName { get; set; }
	}
}

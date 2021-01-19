using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
	public class ExcelSheet
	{
		public List<List<string>> Table { get; set; }
		public int NumberOfRows { get; set; }
		public int NumberOfColumns { get; set; }
	}
}

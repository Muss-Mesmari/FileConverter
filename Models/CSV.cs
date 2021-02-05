using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
	public class CSV
	{
		public List<string> RowsFromXlsxFile { get; set; }
		public string HeadersFromXlsxFile { get; set; }
		public int NumberOfRows { get; set; }
		public int NumberOfHeaders { get; set; }
		public List<string> HeadersFromSqlServer { get; set; }
		public List<string> RowsFromSqlServer { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
	public class CSV
	{
		public List<string> Rows { get; set; }
		public string Headers { get; set; }
		public int NumberOfRows { get; set; }
		public int NumberOfHeaders { get; set; }
	}
}

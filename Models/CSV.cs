using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
	public class CSV
	{
		public List<List<string>> Rows { get; set; }
		public List<string> Headers { get; set; }
		public int NumberOfRows { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
	public class SQLServer
	{
		public List<string> Tables { get; set; }
		public int NumberOfTables { get; set; }
		public List<string> Attributes { get; set; }
	}
}


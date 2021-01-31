using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.ViewModels
{
	public class DocumentFileViewModel
	{
		public SQLServerConfig SQLServerConfig { get; set; }
		public DocumentFile DocumentFile { get; set; }
		public IEnumerable<DocumentFile> DocumentFiles { get; set; } 
		public ExcelSheet ExcelSheet { get; set; }
		public CSV CSV { get; set; }
		public Database Database { get; set; }
		public string FilePath { get; set; }
		public string FileName { get; set; }
		public List<string> ModelsNames { get; set; }
		public List<KeyValuePair<string, List<string>>> AttributesByTable { get; set; }
	}
}

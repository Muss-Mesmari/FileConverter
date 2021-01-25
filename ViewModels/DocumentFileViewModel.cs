using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.ViewModels
{
	public class DocumentFileViewModel
	{
		public DocumentFile DocumentFile { get; set; }
		public IEnumerable<DocumentFile> DocumentFiles { get; set; }
		public ExcelSheet ExcelSheet { get; set; }
		public CSV CSV { get; set; }
		public string FilePath { get; set; }
		public string FileName { get; set; }
	}
}

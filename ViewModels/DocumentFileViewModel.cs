﻿using FileConverter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.ViewModels
{
	public class DocumentFileViewModel
	{
		public SQLServerConfig SQLServerConfig { get; set; }
	//	public DocumentFile DocumentFile { get; set; }
	//	public IEnumerable<DocumentFile> DocumentFiles { get; set; } 
		public ExcelSheet ExcelSheet { get; set; }
		public CSV CSV { get; set; }
		public SQLServer SQLServer { get; set; }
		public string FilePath { get; set; }
		public List<KeyValuePair<string, int>> ObjectsTypesNames { get; set; }

		[Display(Name = "Table name")]
		public string TableName { get; set; }

		[Display(Name = "Object type")]
		public int ObjectIdOne { get; set; }

		[Display(Name = "Related object type (Optional)")]
		public int ObjectIdTwo { get; set; }

		public List<KeyValuePair<string, List<string>>> AttributesByTable { get; set; }
		public bool IsType { get; set; }
		public bool IsTable { get; set; }
	}
}

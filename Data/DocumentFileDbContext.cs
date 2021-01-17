using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileConverter.Models;
using Microsoft.EntityFrameworkCore;


namespace FileConverter.Data
{
	public class DocumentFileDbContext : DbContext
	{
		public DocumentFileDbContext(DbContextOptions<DocumentFileDbContext> options) : base(options)
		{
		}
		public DbSet<DocumentFile> DocumentFile { get; set; }
	}
}

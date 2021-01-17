using FileConverter.Data;
using FileConverter.Models;
using FileConverter.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public class DatabaseServices : IDatabaseServices
	{
		private readonly DocumentFileDbContext _documentFileDbContext;

		public DatabaseServices(DocumentFileDbContext documentFileDbContext)
		{
			_documentFileDbContext = documentFileDbContext;
		}

		public async Task<IEnumerable<DocumentFile>> GetAllDocumentFilesAsync()
		{
			return await _documentFileDbContext.DocumentFile.ToListAsync();
		}
		public async Task<DocumentFile> CreateDocumentFileAsync(DocumentFileViewModel newDocumentFile)
		{
			var _newDocumentFile = new DocumentFile()
			{
				Name = newDocumentFile.DocumentFile.Name,
				Format = newDocumentFile.DocumentFile.Format
			};

			_documentFileDbContext.DocumentFile.Add(_newDocumentFile);
			await _documentFileDbContext.SaveChangesAsync();

			return _newDocumentFile;
		}
		public async Task<DocumentFile> UpdateDocumentFileAsync(DocumentFileViewModel documentFile)
		{
			if (documentFile != null)
			{
				documentFile.DocumentFile.Name = documentFile.DocumentFile.Name;
				documentFile.DocumentFile.Format = documentFile.DocumentFile.Format;
			}

			var updatedDocumentFile = documentFile.DocumentFile;
			var entity = _documentFileDbContext.Entry(updatedDocumentFile);
			entity.State = EntityState.Modified;
			await _documentFileDbContext.SaveChangesAsync();

			return updatedDocumentFile;
		}
		public bool DocumentFileExists(int id)
		{
			return _documentFileDbContext.DocumentFile.Any(df => df.Id == id);
		}
		public async Task<DocumentFile> GetDocumentFileByIdAsync(int? id)
		{
			var documentFileById = await _documentFileDbContext.DocumentFile.FindAsync(id);
			// var documentFileById = _documentFileDbContext.DocumentFile.FirstOrDefault(e => e.Id == id);

			var entity = _documentFileDbContext.Entry(documentFileById);
			entity.State = EntityState.Detached;

			return documentFileById;
		}
		public async Task<bool> DeleteDocumentFile(int id)
		{
			var removedDocumentFile = await GetDocumentFileByIdAsync(id);
			if (removedDocumentFile != null)
			{
				_documentFileDbContext.Remove(removedDocumentFile);
				await _documentFileDbContext.SaveChangesAsync();

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}

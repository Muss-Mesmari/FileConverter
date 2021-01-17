using FileConverter.Models;
using FileConverter.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileConverter.Services
{
	public interface IDatabaseServices
	{
		Task<IEnumerable<DocumentFile>> GetAllDocumentFilesAsync();
		Task<DocumentFile> CreateDocumentFileAsync(DocumentFileViewModel newDocumentFile);
		Task<DocumentFile> UpdateDocumentFileAsync(DocumentFileViewModel documentFile);
		bool DocumentFileExists(int id);
		Task<DocumentFile> GetDocumentFileByIdAsync(int? id);
		Task<bool> DeleteDocumentFile(int id);
	}
}
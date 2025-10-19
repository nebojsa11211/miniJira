using MiniJira.Models;

namespace MiniJira.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Save a file to storage and create a database record
    /// </summary>
    Task<TaskAttachment> SaveFileAsync(Guid taskId, Guid columnId, Stream fileStream, string fileName, string? contentType = null, string? uploadedBy = null);

    /// <summary>
    /// Get file stream for download
    /// </summary>
    Task<(Stream fileStream, string fileName, string? contentType)> GetFileAsync(Guid attachmentId);

    /// <summary>
    /// Delete a file from storage and database
    /// </summary>
    Task<bool> DeleteFileAsync(Guid attachmentId);

    /// <summary>
    /// Get all attachments for a task
    /// </summary>
    Task<List<TaskAttachment>> GetTaskAttachmentsAsync(Guid taskId);

    /// <summary>
    /// Get attachments by column (phase)
    /// </summary>
    Task<List<TaskAttachment>> GetAttachmentsByColumnAsync(Guid taskId, Guid columnId);
}

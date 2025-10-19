using Microsoft.EntityFrameworkCore;
using MiniJira.Data;
using MiniJira.Models;

namespace MiniJira.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ApplicationDbContext _context;
    private readonly string _storagePath;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public FileStorageService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _storagePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<TaskAttachment> SaveFileAsync(
        Guid taskId,
        Guid columnId,
        Stream fileStream,
        string fileName,
        string? contentType = null,
        string? uploadedBy = null)
    {
        // Validate file size
        if (fileStream.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024}MB");
        }

        // Generate unique filename to prevent collisions
        var fileExtension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_storagePath, storedFileName);

        // Save file to disk
        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // Create database record
        var attachment = new TaskAttachment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ColumnId = columnId,
            FileName = fileName,
            StoredFileName = storedFileName,
            FileSizeBytes = fileStream.Length,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };

        _context.TaskAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return attachment;
    }

    public async Task<(Stream fileStream, string fileName, string? contentType)> GetFileAsync(Guid attachmentId)
    {
        var attachment = await _context.TaskAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            throw new FileNotFoundException("Attachment not found");
        }

        var filePath = Path.Combine(_storagePath, attachment.StoredFileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found on disk");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return (fileStream, attachment.FileName, attachment.ContentType);
    }

    public async Task<bool> DeleteFileAsync(Guid attachmentId)
    {
        var attachment = await _context.TaskAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            return false;
        }

        // Delete file from disk
        var filePath = Path.Combine(_storagePath, attachment.StoredFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Delete database record
        _context.TaskAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TaskAttachment>> GetTaskAttachmentsAsync(Guid taskId)
    {
        return await _context.TaskAttachments
            .Include(a => a.Column)
            .ThenInclude(c => c.Translations)
            .Where(a => a.TaskId == taskId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<TaskAttachment>> GetAttachmentsByColumnAsync(Guid taskId, Guid columnId)
    {
        return await _context.TaskAttachments
            .Include(a => a.Column)
            .ThenInclude(c => c.Translations)
            .Where(a => a.TaskId == taskId && a.ColumnId == columnId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync();
    }
}

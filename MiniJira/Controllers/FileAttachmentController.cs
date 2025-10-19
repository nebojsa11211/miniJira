using Microsoft.AspNetCore.Mvc;
using MiniJira.Services;

namespace MiniJira.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileAttachmentController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileAttachmentController> _logger;

    public FileAttachmentController(
        IFileStorageService fileStorageService,
        ILogger<FileAttachmentController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file for a task
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        [FromForm] Guid taskId,
        [FromForm] Guid columnId,
        [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            using var stream = file.OpenReadStream();
            var attachment = await _fileStorageService.SaveFileAsync(
                taskId,
                columnId,
                stream,
                file.FileName,
                file.ContentType);

            _logger.LogInformation(
                "File uploaded successfully: {FileName} for Task {TaskId} in Column {ColumnId}",
                file.FileName,
                taskId,
                columnId);

            return Ok(new
            {
                id = attachment.Id,
                fileName = attachment.FileName,
                fileSizeBytes = attachment.FileSizeBytes,
                uploadedAt = attachment.UploadedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "File upload failed for Task {TaskId}", taskId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file for Task {TaskId}", taskId);
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("download/{attachmentId}")]
    public async Task<IActionResult> DownloadFile(Guid attachmentId)
    {
        try
        {
            var (fileStream, fileName, contentType) = await _fileStorageService.GetFileAsync(attachmentId);

            return File(fileStream, contentType ?? "application/octet-stream", fileName);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning("File not found: {AttachmentId}", attachmentId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "An error occurred while downloading the file" });
        }
    }

    /// <summary>
    /// View an image inline (for thumbnails and lightbox)
    /// </summary>
    [HttpGet("image/{attachmentId}")]
    public async Task<IActionResult> ViewImage(Guid attachmentId)
    {
        try
        {
            var (fileStream, fileName, contentType) = await _fileStorageService.GetFileAsync(attachmentId);

            // Only serve images
            if (contentType == null || !contentType.StartsWith("image/"))
            {
                return BadRequest(new { error = "The requested file is not an image" });
            }

            return File(fileStream, contentType, enableRangeProcessing: true);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning("Image not found: {AttachmentId}", attachmentId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing image {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "An error occurred while loading the image" });
        }
    }

    /// <summary>
    /// Delete a file attachment
    /// </summary>
    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> DeleteFile(Guid attachmentId)
    {
        try
        {
            var result = await _fileStorageService.DeleteFileAsync(attachmentId);

            if (!result)
            {
                return NotFound(new { error = "Attachment not found" });
            }

            _logger.LogInformation("File deleted successfully: {AttachmentId}", attachmentId);
            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "An error occurred while deleting the file" });
        }
    }

    /// <summary>
    /// Get all attachments for a task
    /// </summary>
    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetTaskAttachments(Guid taskId)
    {
        try
        {
            var attachments = await _fileStorageService.GetTaskAttachmentsAsync(taskId);

            return Ok(attachments.Select(a => new
            {
                id = a.Id,
                fileName = a.FileName,
                fileSizeBytes = a.FileSizeBytes,
                contentType = a.ContentType,
                uploadedAt = a.UploadedAt,
                uploadedBy = a.UploadedBy,
                columnId = a.ColumnId,
                columnName = a.Column?.Translations?.FirstOrDefault()?.Name ?? "Unknown"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for Task {TaskId}", taskId);
            return StatusCode(500, new { error = "An error occurred while retrieving attachments" });
        }
    }

    /// <summary>
    /// Get attachments for a task by column (phase)
    /// </summary>
    [HttpGet("task/{taskId}/column/{columnId}")]
    public async Task<IActionResult> GetAttachmentsByColumn(Guid taskId, Guid columnId)
    {
        try
        {
            var attachments = await _fileStorageService.GetAttachmentsByColumnAsync(taskId, columnId);

            return Ok(attachments.Select(a => new
            {
                id = a.Id,
                fileName = a.FileName,
                fileSizeBytes = a.FileSizeBytes,
                contentType = a.ContentType,
                uploadedAt = a.UploadedAt,
                uploadedBy = a.UploadedBy
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for Task {TaskId} in Column {ColumnId}", taskId, columnId);
            return StatusCode(500, new { error = "An error occurred while retrieving attachments" });
        }
    }
}

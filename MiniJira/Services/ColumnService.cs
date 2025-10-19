using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MiniJira.Services;

public class ColumnService : IColumnService
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<ColumnService> _logger;

    public ColumnService(
        ApplicationDbContext context,
        ILocalizationService localizationService,
        ILogger<ColumnService> logger)
    {
        _context = context;
        _localizationService = localizationService;
        _logger = logger;
    }

    public async Task<List<ColumnDto>> GetAllColumnsAsync(string? culture = null)
    {
        var cultureCode = culture ?? _localizationService.CurrentCulture.Name;
        _logger.LogInformation("Retrieving all columns for culture {Culture}", cultureCode);

        var columns = await _context.Columns
            .AsNoTracking()
            .Include(c => c.Translations)
            .Include(c => c.Tasks)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Order)
            .ToListAsync();

        return columns.Select(c => MapToDto(c, cultureCode)).ToList();
    }

    public async Task<ColumnDto?> GetColumnByIdAsync(Guid id, string? culture = null)
    {
        var cultureCode = culture ?? _localizationService.CurrentCulture.Name;
        _logger.LogInformation("Retrieving column {ColumnId} for culture {Culture}", id, cultureCode);

        var column = await _context.Columns
            .AsNoTracking()
            .Include(c => c.Translations)
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (column == null)
        {
            _logger.LogWarning("Column with ID {ColumnId} not found", id);
            return null;
        }

        return MapToDto(column, cultureCode);
    }

    public async Task<Result<ColumnDto>> CreateColumnAsync(CreateColumnRequest request, string culture)
    {
        try
        {
            _logger.LogInformation("Creating new column");

            // Validate the request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning("Column creation validation failed: {ErrorMessage}", errorMessage);
                return Result<ColumnDto>.Failure(errorMessage);
            }

            // Validate that translations exist for all supported cultures
            var supportedCultures = _localizationService.SupportedCultures.Select(c => c.Name).ToList();
            var missingCultures = supportedCultures.Where(c => !request.Translations.ContainsKey(c)).ToList();

            if (missingCultures.Any())
            {
                var errorMessage = $"Translations missing for cultures: {string.Join(", ", missingCultures)}";
                _logger.LogWarning("Column creation failed: {ErrorMessage}", errorMessage);
                return Result<ColumnDto>.Failure(errorMessage);
            }

            // Validate translation content
            foreach (var translation in request.Translations)
            {
                if (string.IsNullOrWhiteSpace(translation.Value.Name))
                {
                    return Result<ColumnDto>.Failure($"Name is required for culture {translation.Key}");
                }
            }

            // Get the "Done" column (last system column) to insert new columns before it
            var doneColumnId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var doneColumn = await _context.Columns.FirstOrDefaultAsync(c => c.Id == doneColumnId);

            int newOrder;
            if (doneColumn != null)
            {
                // Insert before "Done" column
                newOrder = doneColumn.Order;

                // Shift "Done" and any columns after it to the right
                var columnsToShift = await _context.Columns
                    .Where(c => c.Order >= newOrder)
                    .ToListAsync();

                foreach (var col in columnsToShift)
                {
                    col.Order++;
                    col.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Fallback: add at the end if "Done" column doesn't exist
                var maxOrder = await _context.Columns.MaxAsync(c => (int?)c.Order) ?? -1;
                newOrder = maxOrder + 1;
            }

            var column = new Column
            {
                Id = Guid.NewGuid(),
                Order = newOrder,
                Color = request.Color.Trim(),
                IsSystem = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create translations
            foreach (var translation in request.Translations)
            {
                column.Translations.Add(new ColumnTranslation
                {
                    Id = Guid.NewGuid(),
                    ColumnId = column.Id,
                    Culture = translation.Key,
                    Name = translation.Value.Name.Trim(),
                    Description = translation.Value.Description?.Trim() ?? string.Empty
                });
            }

            _context.Columns.Add(column);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Column created successfully with ID {ColumnId}", column.Id);

            // Reload with relationships
            var createdColumn = await GetColumnByIdAsync(column.Id, culture);
            return Result<ColumnDto>.Success(createdColumn!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create column");
            return Result<ColumnDto>.Failure($"Failed to create column: {ex.Message}");
        }
    }

    public async Task<Result<ColumnDto>> UpdateColumnAsync(Guid id, UpdateColumnRequest request, string culture)
    {
        try
        {
            _logger.LogInformation("Updating column {ColumnId}", id);

            // Validate the request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning("Column update validation failed for column {ColumnId}: {ErrorMessage}", id, errorMessage);
                return Result<ColumnDto>.Failure(errorMessage);
            }

            var column = await _context.Columns
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (column == null)
            {
                _logger.LogWarning("Column with ID {ColumnId} not found for update", id);
                return Result<ColumnDto>.Failure($"Column with ID {id} not found");
            }

            // Update color if provided
            if (!string.IsNullOrWhiteSpace(request.Color))
            {
                column.Color = request.Color.Trim();
            }

            // Update translations if provided
            if (request.Translations != null && request.Translations.Any())
            {
                foreach (var translationDto in request.Translations)
                {
                    var translation = column.Translations.FirstOrDefault(t => t.Culture == translationDto.Key);

                    if (translation != null)
                    {
                        // Update existing translation
                        if (!string.IsNullOrWhiteSpace(translationDto.Value.Name))
                        {
                            translation.Name = translationDto.Value.Name.Trim();
                        }
                        if (translationDto.Value.Description != null)
                        {
                            translation.Description = translationDto.Value.Description.Trim();
                        }
                    }
                    else
                    {
                        // Add new translation
                        if (string.IsNullOrWhiteSpace(translationDto.Value.Name))
                        {
                            return Result<ColumnDto>.Failure($"Name is required for new translation in culture {translationDto.Key}");
                        }

                        column.Translations.Add(new ColumnTranslation
                        {
                            Id = Guid.NewGuid(),
                            ColumnId = column.Id,
                            Culture = translationDto.Key,
                            Name = translationDto.Value.Name.Trim(),
                            Description = translationDto.Value.Description?.Trim() ?? string.Empty
                        });
                    }
                }
            }

            column.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Column {ColumnId} updated successfully", id);

            var updatedColumn = await GetColumnByIdAsync(id, culture);
            return Result<ColumnDto>.Success(updatedColumn!);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating column {ColumnId}", id);
            return Result<ColumnDto>.Failure("The column was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update column {ColumnId}", id);
            return Result<ColumnDto>.Failure($"Failed to update column: {ex.Message}");
        }
    }

    public async Task<Result> DeleteColumnAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting column {ColumnId}", id);

            var column = await _context.Columns
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (column == null)
            {
                _logger.LogWarning("Column with ID {ColumnId} not found for deletion", id);
                return Result.Failure($"Column with ID {id} not found");
            }

            if (column.IsSystem)
            {
                _logger.LogWarning("Attempt to delete system column {ColumnId}", id);
                return Result.Failure("Cannot delete system columns");
            }

            if (column.Tasks.Any())
            {
                _logger.LogWarning("Attempt to delete column {ColumnId} with {TaskCount} tasks", id, column.Tasks.Count);
                return Result.Failure("Cannot delete column with existing tasks. Please move or delete the tasks first.");
            }

            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Column {ColumnId} deleted successfully", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete column {ColumnId}", id);
            return Result.Failure($"Failed to delete column: {ex.Message}");
        }
    }

    public async Task<Result> ReorderColumnsAsync(ReorderColumnsRequest request)
    {
        try
        {
            _logger.LogInformation("Reordering columns");

            // Validate the request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning("Column reorder validation failed: {ErrorMessage}", errorMessage);
                return Result.Failure(errorMessage);
            }

            // Get all columns
            var columns = await _context.Columns
                .Where(c => request.ColumnIds.Contains(c.Id))
                .ToListAsync();

            if (columns.Count != request.ColumnIds.Count)
            {
                var missingIds = request.ColumnIds.Except(columns.Select(c => c.Id)).ToList();
                _logger.LogWarning("Invalid column IDs in reorder request: {MissingIds}", string.Join(", ", missingIds));
                return Result.Failure("One or more column IDs are invalid");
            }

            // Update order based on position in the list
            for (int i = 0; i < request.ColumnIds.Count; i++)
            {
                var column = columns.First(c => c.Id == request.ColumnIds[i]);
                column.Order = i;
                column.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Columns reordered successfully");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reorder columns");
            return Result.Failure($"Failed to reorder columns: {ex.Message}");
        }
    }

    private ColumnDto MapToDto(Column column, string cultureCode)
    {
        // Get the translation for the requested culture, fallback to en-US if not found
        var translation = column.Translations.FirstOrDefault(t => t.Culture == cultureCode)
            ?? column.Translations.FirstOrDefault(t => t.Culture == "en-US");

        var dto = new ColumnDto
        {
            Id = column.Id,
            Order = column.Order,
            Color = column.Color,
            IsSystem = column.IsSystem,
            IsActive = column.IsActive,
            Name = translation?.Name ?? "Unnamed",
            Description = translation?.Description ?? string.Empty,
            TaskCount = column.Tasks?.Count ?? 0,
            Translations = column.Translations.ToDictionary(
                t => t.Culture,
                t => new ColumnTranslationDto
                {
                    Name = t.Name,
                    Description = t.Description
                })
        };

        return dto;
    }
}

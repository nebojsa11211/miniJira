using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Services.DTOs;
using MiniJira.Resources;
using System.Text.RegularExpressions;

namespace MiniJira.Components.Shared.Dialogs;

public partial class ColumnDialog
{
    [Parameter]
    public ColumnDto? Column { get; set; }

    [Parameter]
    public EventCallback<ColumnDto> OnSave { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private ColumnDto model = new();
    private string activeTab = "en";
    private bool showValidation = false;
    private bool isSaving = false;
    private string? validationError = null;

    // Form fields
    private string nameEn = string.Empty;
    private string descriptionEn = string.Empty;
    private string nameHr = string.Empty;
    private string descriptionHr = string.Empty;
    private string color = "#DFE1E6";

    protected override void OnParametersSet()
    {
        if (Column != null)
        {
            // Edit mode - populate from existing column
            model = Column;
            color = Column.Color;

            if (Column.Translations.TryGetValue("en-US", out var enTranslation))
            {
                nameEn = enTranslation.Name;
                descriptionEn = enTranslation.Description;
            }

            if (Column.Translations.TryGetValue("hr-HR", out var hrTranslation))
            {
                nameHr = hrTranslation.Name;
                descriptionHr = hrTranslation.Description;
            }
        }
        else
        {
            // Create mode - initialize defaults
            model = new ColumnDto
            {
                Color = color,
                Translations = new Dictionary<string, ColumnTranslationDto>()
            };
        }
    }

    private void SetActiveTab(string tab)
    {
        activeTab = tab;
    }

    private bool IsValidColor(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
            return false;

        var regex = new Regex(@"^#[0-9A-Fa-f]{6}$");
        return regex.IsMatch(colorValue);
    }

    private async Task HandleSave()
    {
        showValidation = true;
        validationError = null;

        // Validate required fields
        if (string.IsNullOrWhiteSpace(nameEn))
        {
            validationError = Localizer["ColumnDialog.EnglishNameRequired"];
            activeTab = "en";
            return;
        }

        if (string.IsNullOrWhiteSpace(nameHr))
        {
            validationError = Localizer["ColumnDialog.CroatianNameRequired"];
            activeTab = "hr";
            return;
        }

        if (!IsValidColor(color))
        {
            validationError = Localizer["ColumnDialog.ColorInvalid"];
            return;
        }

        try
        {
            isSaving = true;

            // Build the column DTO
            var columnToSave = new ColumnDto
            {
                Id = Column?.Id ?? Guid.Empty,
                Color = color,
                IsSystem = Column?.IsSystem ?? false,
                Translations = new Dictionary<string, ColumnTranslationDto>
                {
                    ["en-US"] = new ColumnTranslationDto
                    {
                        Name = nameEn.Trim(),
                        Description = descriptionEn?.Trim() ?? string.Empty
                    },
                    ["hr-HR"] = new ColumnTranslationDto
                    {
                        Name = nameHr.Trim(),
                        Description = descriptionHr?.Trim() ?? string.Empty
                    }
                }
            };

            await OnSave.InvokeAsync(columnToSave);
        }
        catch (Exception ex)
        {
            validationError = string.Format(Localizer["ColumnDialog.SaveError"], ex.Message);
            isSaving = false;
        }
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }
}

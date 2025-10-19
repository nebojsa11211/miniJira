# Croatian (hr-HR) Localization Implementation

This document describes the localization system implemented for MiniJira, with Croatian translation support.

## Overview

The application now supports both English (en-US) and Croatian (hr-HR) languages. Users can switch between languages using the language selector in the navigation menu.

## Architecture

### Localization Service

**Files:**
- `Services/ILocalizationService.cs` - Interface for localization service
- `Services/LocalizationService.cs` - Implementation managing culture switching

**Features:**
- Culture management (English and Croatian)
- Culture change event notification
- Thread-safe culture switching

### Resource Files

**Location:** `Resources/`

**Files:**
- `Localization.cs` - Dummy class for resource typing
- `Localization.resx` - English (default) translations
- `Localization.hr-HR.resx` - Croatian translations

**Resource Keys Structure:**
- `Nav.*` - Navigation menu items
- `Common.*` - Common UI elements (buttons, labels)
- `Board.*` - Board page specific strings
- `TaskStatus.*` - Task status translations
- `TaskPriority.*` - Priority level translations
- `CreateTask.*` - Create task page strings
- `TaskDetail.*` - Task detail page strings
- `Validation.*` - Validation messages
- `Category.*` - Task category labels
- `Language.*` - Language selector strings

## Components Updated

### Pages
1. **Index.razor** (Board Page)
   - Column titles (TO DO, IN PROGRESS, DONE)
   - Loading messages
   - Task count labels
   - Error messages

2. **CreateTask.razor**
   - Form labels and placeholders
   - Button text
   - Tips section
   - Validation messages

3. **TaskDetail.razor**
   - Task metadata labels
   - Edit mode form
   - Delete confirmation modal
   - Status change buttons
   - Error messages

### Shared Components
1. **NavMenu.razor**
   - Navigation links
   - Includes LanguageSelector component

2. **LanguageSelector.razor** (New)
   - Dropdown to switch between languages
   - Updates all components on culture change

## Configuration

### Program.cs

Configured localization middleware:
```csharp
// Supported cultures
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("hr-HR")
};

// Localization configuration
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

// Middleware
app.UseRequestLocalization(...);
```

## Croatian Translations

All Croatian translations have been carefully crafted to:
- Use proper Croatian grammar and terminology
- Sound natural to native speakers
- Use appropriate project management terms
- Maintain consistency across the application

### Key Croatian Terms

| English | Croatian |
|---------|----------|
| Board | Ploča |
| Task | Zadatak |
| To Do | Za napraviti |
| In Progress | U tijeku |
| Done | Završeno |
| Priority | Prioritet |
| High | Visok |
| Medium | Srednji |
| Low | Nizak |
| Create Task | Kreiraj zadatak |
| Description | Opis |
| Status | Status |
| Settings | Postavke |

## Usage

### For Users

1. Click on the language selector dropdown in the navigation menu
2. Select "Hrvatski" (Croatian) or "English"
3. The entire application will update to the selected language

### For Developers

**To add a new translatable string:**

1. Add the English version to `Resources/Localization.resx`:
```xml
<data name="YourKey.Name" xml:space="preserve">
    <value>Your English text</value>
</data>
```

2. Add the Croatian version to `Resources/Localization.hr-HR.resx`:
```xml
<data name="YourKey.Name" xml:space="preserve">
    <value>Vaš hrvatski tekst</value>
</data>
```

3. Use in Razor components:
```razor
@inject IStringLocalizer<Localization> Localizer

<div>@Localizer["YourKey.Name"]</div>
```

**To add a new supported language:**

1. Create a new resource file: `Resources/Localization.{culture}.resx`
2. Add translations for all existing keys
3. Update `LocalizationService.cs` to include the new culture in `_supportedCultures`
4. Update `LanguageSelector.razor` to display the new language option

## Validation Messages

Validation attributes in DTOs now use resource-based error messages:

```csharp
[Required(
    ErrorMessageResourceName = "Validation.TitleRequired",
    ErrorMessageResourceType = typeof(Resources.Localization)
)]
public string Title { get; set; }
```

This ensures validation messages are also localized.

## Testing

To test the localization:

1. Run the application
2. Navigate to different pages
3. Switch between English and Croatian using the language selector
4. Verify all text updates correctly
5. Test validation messages by submitting invalid forms
6. Check that priority and status labels are translated

## Future Enhancements

- Add more languages (German, Spanish, French, etc.)
- Implement browser language detection for default culture
- Add culture-specific date/time formatting
- Implement RTL language support if needed
- Add localized error pages

## Notes

- Language preference is currently session-based (not persisted)
- To persist language preference, implement browser localStorage or user preferences in database
- All numeric and date formats use the current culture settings
- Resource files are compiled into satellite assemblies for efficient loading

## Files Modified

### New Files
- `Services/ILocalizationService.cs`
- `Services/LocalizationService.cs`
- `Resources/Localization.cs`
- `Resources/Localization.resx`
- `Resources/Localization.hr-HR.resx`
- `Components/Shared/LanguageSelector.razor`

### Modified Files
- `Program.cs`
- `MiniJira.csproj`
- `wwwroot/css/site.css`
- `Components/Pages/Index.razor`
- `Components/Pages/CreateTask.razor`
- `Components/Pages/TaskDetail.razor`
- `Shared/NavMenu.razor`
- `Services/DTOs/CreateTaskRequest.cs`
- `Services/DTOs/UpdateTaskRequest.cs`

## Support

For questions or issues with localization:
1. Check resource keys exist in both `.resx` files
2. Verify `IStringLocalizer<Localization>` is injected
3. Ensure `@using MiniJira.Resources` directive is present
4. Check browser console for any runtime errors

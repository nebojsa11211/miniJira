# Color Palette Settings Implementation Plan

## Overview
Adding customizable color palette options to MiniJira settings, allowing users to choose from multiple preset themes for both light and dark modes.

## Goals
- Provide at least 3 color palette options for light mode
- Provide at least 3 color palette options for dark mode
- Seamless integration with existing theme system (light/dark toggle)
- Persistent user preference via localStorage
- Professional, accessible color combinations
- Follow existing service-based architecture pattern

## Color Palette Definitions

### Light Mode Palettes

#### 1. Classic Red (Current/Default)
**Brand Identity:** Professional, Action-Oriented
- Primary: `#E41E26` (Jira Red)
- Accent: Teal, Amber
- Use Case: Default professional look matching logo

#### 2. Ocean Blue
**Brand Identity:** Calm, Trustworthy, Corporate
- Primary: `#0052CC` (Deep Blue)
- Secondary: `#00B8D9` (Cyan)
- Accent: `#36B37E` (Green), `#FFAB00` (Amber)
- Use Case: Corporate environments, finance, healthcare

#### 3. Forest Green
**Brand Identity:** Growth, Nature, Sustainability
- Primary: `#10B981` (Emerald)
- Secondary: `#059669` (Deep Green)
- Accent: `#3B82F6` (Blue), `#8B5CF6` (Purple)
- Use Case: Environmental, wellness, growth-focused teams

### Dark Mode Palettes

#### 1. Classic Dark (Current/Default)
**Brand Identity:** Professional Dark Theme
- Based on Classic Red palette
- Dark backgrounds with red accents
- Use Case: Default dark mode

#### 2. Midnight Blue
**Brand Identity:** Deep, Focused, Professional
- Primary: `#1E3A8A` (Deep Blue)
- Secondary: `#3B82F6` (Bright Blue)
- Background: `#0F172A` (Slate 900)
- Use Case: Extended night work, developer preference

#### 3. Forest Night
**Brand Identity:** Calm, Natural Dark
- Primary: `#065F46` (Deep Emerald)
- Secondary: `#10B981` (Emerald)
- Background: `#064E3B` (Deep Green-tinted)
- Use Case: Reduced eye strain, nature-themed preference

## Technical Architecture

### 1. Service Layer (`ColorPaletteService.cs`)
```csharp
public interface IColorPaletteService
{
    Task<string> GetColorPaletteAsync();
    Task SetColorPaletteAsync(string palette);
    event EventHandler? ColorPaletteChanged;
    List<ColorPaletteOption> GetAvailablePalettes();
}

public class ColorPaletteOption
{
    public string Id { get; set; }          // "classic-red", "ocean-blue", etc.
    public string NameKey { get; set; }     // Localization key
    public string DescriptionKey { get; set; }
    public string[] SupportedThemes { get; set; } // ["light", "dark"]
}
```

**Responsibilities:**
- Manage color palette state
- Persist to localStorage via JSInterop
- Notify components of palette changes
- Provide available palette options

**Storage Key:** `"colorPalette"`
**Default Value:** `"classic-red"`

### 2. JavaScript Interop (`colorPaletteManager.js`)
```javascript
window.colorPaletteManager = {
    setColorPalette: function(palette, theme) {
        // Apply CSS classes to document root
        // Trigger CSS custom property updates
    },

    getColorPalette: function() {
        return localStorage.getItem('colorPalette') || 'classic-red';
    }
};
```

**Responsibilities:**
- Update `data-color-palette` attribute on document root
- Store/retrieve from localStorage
- Coordinate with existing theme system

### 3. CSS Architecture (`color-palettes.css`)

#### Structure:
```css
/* Base palette definitions */
[data-color-palette="classic-red"][data-theme="light"] {
    --color-primary: #E41E26;
    --color-primary-hover: #C11119;
    /* ... */
}

[data-color-palette="ocean-blue"][data-theme="light"] {
    --color-primary: #0052CC;
    --color-primary-hover: #0043A8;
    /* ... */
}

/* Dark mode variants */
[data-color-palette="classic-red"][data-theme="dark"] {
    /* Dark theme colors */
}
```

**CSS Custom Properties to Define:**
- `--color-primary` - Main brand color
- `--color-primary-hover` - Hover state
- `--color-primary-light` - Light variant
- `--color-secondary` - Secondary accent
- `--color-accent-1` - First accent color
- `--color-accent-2` - Second accent color
- `--color-success` - Success states
- `--color-warning` - Warning states
- `--color-error` - Error states
- `--color-info` - Info states

### 4. UI Component (`ColorPaletteSelector.razor`)

**Features:**
- Dropdown selector with palette previews
- Visual color swatches for each option
- Localized palette names and descriptions
- Real-time preview on selection
- Groups palettes by theme compatibility

**Component Structure:**
```razor
<div class="color-palette-selector">
    <label>@Localizer["ColorPalette"]</label>
    <select @bind="selectedPalette" @bind:event="onchange">
        @foreach (var palette in availablePalettes)
        {
            <option value="@palette.Id">
                @Localizer[palette.NameKey]
            </option>
        }
    </select>
    <p class="description">@Localizer[currentDescription]</p>
    <div class="palette-preview">
        <!-- Color swatches -->
    </div>
</div>
```

### 5. Settings Page Integration

**Update `Settings.razor`:**
- Add ColorPaletteSelector component
- Add save logic for palette preference
- Add success/error messaging
- Maintain consistent UI with existing settings

**Layout:**
```
Settings Page
├── Language Selection
├── Font Size Selection
├── Color Palette Selection ← NEW
└── Save Button
```

## Implementation Steps

### Phase 1: Service & Core Infrastructure
1. Create `IColorPaletteService` interface
2. Implement `ColorPaletteService` class
3. Create `colorPaletteManager.js` JavaScript module
4. Register service in `Program.cs`

### Phase 2: Styling
5. Create `color-palettes.css` with all palette definitions
6. Define CSS custom properties for each palette/theme combination
7. Ensure accessibility (WCAG AA contrast ratios)
8. Include file in `_Layout.cshtml`

### Phase 3: UI Components
9. Create `ColorPaletteSelector.razor` component
10. Add localization strings for all palettes
11. Update `Settings.razor` to include selector
12. Add palette preview visuals

### Phase 4: Integration
13. Update `MainLayout.razor` to initialize palette on load
14. Coordinate with existing `ThemeService`
15. Ensure proper initialization order
16. Add event handling for palette changes

### Phase 5: Testing
17. Test each palette in light mode
18. Test each palette in dark mode
19. Test palette switching
20. Test persistence across page refreshes
21. Test with different languages
22. Test accessibility (contrast, keyboard navigation)

## Localization Keys

### English (Localization.resx)
```
ColorPalette = "Color Palette"
ColorPaletteDescription = "Choose a color scheme for the application"
Palette_ClassicRed = "Classic Red"
Palette_ClassicRed_Desc = "Professional red theme matching MiniJira branding"
Palette_OceanBlue = "Ocean Blue"
Palette_OceanBlue_Desc = "Calm blue tones for focused work"
Palette_ForestGreen = "Forest Green"
Palette_ForestGreen_Desc = "Natural green palette for sustainable teams"
Palette_ClassicDark = "Classic Dark"
Palette_ClassicDark_Desc = "Dark theme with red accents"
Palette_MidnightBlue = "Midnight Blue"
Palette_MidnightBlue_Desc = "Deep blue tones for night work"
Palette_ForestNight = "Forest Night"
Palette_ForestNight_Desc = "Dark green theme for reduced eye strain"
```

### Croatian (Localization.hr-HR.resx)
```
ColorPalette = "Paleta Boja"
ColorPaletteDescription = "Odaberite shemu boja za aplikaciju"
Palette_ClassicRed = "Klasična Crvena"
Palette_ClassicRed_Desc = "Profesionalna crvena tema koja odgovara MiniJira brendu"
Palette_OceanBlue = "Ocean Plava"
Palette_OceanBlue_Desc = "Mirni plavi tonovi za fokusiran rad"
Palette_ForestGreen = "Šumska Zelena"
Palette_ForestGreen_Desc = "Prirodna zelena paleta za održive timove"
Palette_ClassicDark = "Klasična Tamna"
Palette_ClassicDark_Desc = "Tamna tema s crvenim akcentima"
Palette_MidnightBlue = "Ponoćno Plava"
Palette_MidnightBlue_Desc = "Duboki plavi tonovi za noćni rad"
Palette_ForestNight = "Šumska Noć"
Palette_ForestNight_Desc = "Tamno zelena tema za smanjenje naprezanja očiju"
```

## Accessibility Considerations

### Contrast Ratios (WCAG AA)
- Normal text: 4.5:1 minimum
- Large text: 3:1 minimum
- UI components: 3:1 minimum

### Testing Tools
- Chrome DevTools Lighthouse
- WAVE Web Accessibility Evaluation Tool
- Manual keyboard navigation testing

### Color Blindness Support
- Avoid red/green as sole differentiators
- Use patterns/icons alongside colors
- Test with color blindness simulators

## File Structure

```
MiniJira/
├── Services/
│   ├── IColorPaletteService.cs         ← NEW
│   └── ColorPaletteService.cs          ← NEW
├── Components/
│   ├── Shared/
│   │   └── ColorPaletteSelector.razor  ← NEW
│   │   └── ColorPaletteSelector.razor.css ← NEW (optional)
│   └── Pages/
│       └── Settings.razor              ← MODIFIED
├── wwwroot/
│   ├── css/
│   │   └── color-palettes.css          ← NEW
│   └── js/
│       └── colorPaletteManager.js      ← NEW
├── Resources/
│   ├── Localization.resx               ← MODIFIED (add keys)
│   └── Localization.hr-HR.resx         ← MODIFIED (add keys)
├── Pages/
│   └── _Layout.cshtml                  ← MODIFIED (include new files)
├── Shared/
│   └── MainLayout.razor                ← MODIFIED (init palette)
└── Program.cs                          ← MODIFIED (register service)
```

## Integration with Existing Systems

### Theme System Integration
```csharp
// In MainLayout.razor.cs
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var theme = await ThemeService.GetThemeAsync();
        var palette = await ColorPaletteService.GetColorPaletteAsync();

        // Initialize both theme and palette
        await JS.InvokeVoidAsync("themeManager.setTheme", theme);
        await JS.InvokeVoidAsync("colorPaletteManager.setColorPalette", palette, theme);
    }
}
```

### Event Coordination
```csharp
// Subscribe to theme changes to update palette
ThemeService.OnThemeChanged += async (s, e) =>
{
    var palette = await ColorPaletteService.GetColorPaletteAsync();
    var theme = await ThemeService.GetThemeAsync();
    await JS.InvokeVoidAsync("colorPaletteManager.setColorPalette", palette, theme);
};
```

## Future Enhancements (Out of Scope)

1. **Custom Color Palettes**
   - User-defined color picker
   - Save custom palettes
   - Share palettes across team

2. **Additional Preset Palettes**
   - Sunset Orange (warm tones)
   - Royal Purple (premium feel)
   - Monochrome (minimal distraction)

3. **Advanced Features**
   - Per-project color palettes
   - Automatic dark mode scheduling
   - Color palette import/export
   - A11y mode (high contrast)

## Success Metrics

- User can select from 6 color palettes (3 light, 3 dark)
- Palette persists across sessions
- Palette works with theme toggle
- All palettes meet WCAG AA standards
- No visual glitches during palette switching
- Settings UI is intuitive and localized

## Timeline Estimate

- **Phase 1:** 1-2 hours (Service implementation)
- **Phase 2:** 2-3 hours (CSS palette definitions)
- **Phase 3:** 1-2 hours (UI components)
- **Phase 4:** 1 hour (Integration)
- **Phase 5:** 1-2 hours (Testing)

**Total:** 6-10 hours

## Dependencies

- Existing ThemeService
- Existing FontSizeService (pattern reference)
- JSInterop
- localStorage API
- CSS custom properties support (all modern browsers)

## Risks & Mitigation

| Risk | Mitigation |
|------|-----------|
| CSS variable browser support | Use modern browsers only (already required) |
| Color contrast failures | Test all combinations with accessibility tools |
| Palette switching performance | Use CSS classes, avoid inline styles |
| localStorage quota | Minimal data stored (one string value) |
| Palette conflicts with custom CSS | Use scoped CSS isolation where possible |

## Notes

- Follow existing code style and patterns
- Reuse existing components where possible
- Maintain consistency with Jira design system
- Ensure mobile responsiveness
- Keep bundle size minimal (CSS only loaded once)

---

**Document Version:** 1.0
**Last Updated:** 2025-10-16
**Status:** Ready for Implementation

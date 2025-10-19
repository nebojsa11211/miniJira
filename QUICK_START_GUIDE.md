# MiniJira UI Quick Start Guide

## For Developers: Using the Modern Design System

This guide provides quick references for using MiniJira's modern design system in your components.

---

## Color Usage

### Use Semantic Tokens (Not Raw Colors)

**Good:**
```css
color: var(--jira-text-primary);
background: var(--jira-bg-secondary);
border: 1px solid var(--jira-border);
```

**Bad:**
```css
color: #172B4D;  /* Don't hardcode colors */
background: #F4F5F7;
border: 1px solid #DFE1E6;
```

### Common Color Tokens

```css
/* Text */
--jira-text-primary    /* Main text (#172B4D in light mode) */
--jira-text-secondary  /* Subtitles, labels (#5E6C84) */
--jira-text-subtle     /* Muted text (#7A869A) */

/* Backgrounds */
--jira-bg-page        /* Page background (#FAFBFC) */
--jira-bg-primary     /* Card backgrounds (#FFFFFF) */
--jira-bg-secondary   /* Section backgrounds (#F4F5F7) */
--jira-bg-tertiary    /* Hover states (#EBECF0) */

/* Borders */
--jira-border         /* Standard borders (#DFE1E6) */
--jira-border-focus   /* Focus outlines (#E41E26) */

/* Brand */
--jira-primary        /* Brand red (#E41E26) */
--jira-primary-hover  /* Hover state (#C81820) */
--jira-primary-active /* Active state (#A61419) */

/* Status */
--jira-green          /* Success (#10B981) */
--jira-red            /* Error (#E41E26) */
--jira-yellow         /* Warning (#F59E0B) */
```

---

## Spacing

Use the 4px grid system for all spacing:

```css
/* Spacing Tokens */
--jira-space-025: 2px   /* Minimal spacing */
--jira-space-050: 4px   /* Tight spacing */
--jira-space-075: 6px   /* Small spacing */
--jira-space-100: 8px   /* Base unit */
--jira-space-150: 12px  /* Small-medium spacing */
--jira-space-200: 16px  /* Medium spacing */
--jira-space-250: 20px  /* Medium-large spacing */
--jira-space-300: 24px  /* Large spacing */
--jira-space-400: 32px  /* Extra large */
--jira-space-500: 40px  /* Section spacing */
--jira-space-600: 48px  /* Page-level spacing */
```

**Example:**
```css
.my-component {
    padding: var(--jira-space-200);      /* 16px */
    margin-bottom: var(--jira-space-300); /* 24px */
    gap: var(--jira-space-100);          /* 8px */
}
```

---

## Typography

Use the defined font size multiplier for accessibility:

```css
/* Font Sizes */
font-size: calc(14px * var(--font-size-multiplier)); /* Body text */
font-size: calc(12px * var(--font-size-multiplier)); /* Small text */
font-size: calc(16px * var(--font-size-multiplier)); /* Large text */
font-size: calc(20px * var(--font-size-multiplier)); /* Headings */
```

**Font Weights:**
- 400: Regular (body text)
- 500: Medium (buttons, nav items)
- 600: Semi-bold (headings)
- 700: Bold (labels, emphasis)

---

## Creating a Modern Button

```html
<!-- Primary Button -->
<button class="btn btn-primary">
    Save Changes
</button>

<!-- Secondary Button -->
<button class="btn btn-secondary">
    Cancel
</button>

<!-- Outline Button -->
<button class="btn btn-outline-primary">
    Edit
</button>

<!-- Button with Ripple Effect -->
<button class="btn btn-primary btn-ripple">
    Create Task
</button>

<!-- Pill-shaped Button -->
<button class="btn btn-primary btn-pill">
    New Feature
</button>

<!-- Icon Button -->
<button class="btn btn-icon btn-secondary" aria-label="Settings">
    <svg width="20" height="20"><!-- Icon SVG --></svg>
</button>
```

---

## Creating a Card Component

```html
<!-- Basic Card -->
<div class="card">
    <div class="card-body">
        Content here
    </div>
</div>

<!-- Card with Elevation on Hover -->
<div class="card card-elevated">
    <div class="card-body">
        Hover me for lift effect
    </div>
</div>

<!-- Glass Effect Card -->
<div class="card glass-effect">
    <div class="card-body">
        Modern glass morphism
    </div>
</div>

<!-- Card with Gradient Border -->
<div class="card card-gradient-border">
    <div class="card-body">
        Stylish gradient outline
    </div>
</div>
```

---

## Form Elements

```html
<!-- Text Input -->
<div class="form-group">
    <label for="taskTitle" class="form-label">Task Title</label>
    <input type="text" id="taskTitle" class="form-control" placeholder="Enter title">
</div>

<!-- Input with Icon -->
<div class="form-group">
    <label for="search" class="form-label">Search</label>
    <div class="input-with-icon">
        <svg class="icon" width="20" height="20"><!-- Search icon --></svg>
        <input type="text" id="search" class="form-control" placeholder="Search tasks">
    </div>
</div>

<!-- Floating Label Input -->
<div class="form-floating-modern">
    <input type="text" class="form-control" id="floatingInput" placeholder=" ">
    <label for="floatingInput">Email address</label>
</div>

<!-- Select Dropdown -->
<div class="form-group">
    <label for="priority" class="form-label">Priority</label>
    <select id="priority" class="form-select">
        <option>Low</option>
        <option>Medium</option>
        <option selected>High</option>
    </select>
</div>
```

---

## Loading States

```html
<!-- Spinner -->
<div class="spinner-border" role="status">
    <span class="visually-hidden">Loading...</span>
</div>

<!-- Modern Spinner -->
<div class="spinner-modern"></div>

<!-- Skeleton Loaders -->
<div class="skeleton skeleton-title"></div>
<div class="skeleton skeleton-text"></div>
<div class="skeleton skeleton-text"></div>
<div class="skeleton skeleton-card"></div>

<!-- Progress Bar -->
<div class="progress-modern">
    <div class="progress-bar-modern" style="width: 65%;"></div>
</div>
```

---

## Badges and Tags

```html
<!-- Status Badge (from StatusBadge.razor component) -->
<StatusBadge Status="@TaskStatus.InProgress" />

<!-- Manual Badge -->
<span class="badge badge-pill-modern" style="background: var(--jira-green-bg); color: var(--jira-green-dark);">
    Completed
</span>

<!-- Category Badge -->
<span class="task-card-category billing">
    BILLING
</span>

<!-- Chip/Tag with Remove -->
<span class="chip-modern">
    JavaScript
    <button class="chip-remove" aria-label="Remove">×</button>
</span>

<!-- Pulsing Notification Badge -->
<span class="badge badge-pulse" style="background: var(--jira-red);">
    5
</span>
```

---

## Utility Classes

### Layout

```html
<!-- Flexbox -->
<div class="flex items-center justify-between gap-2">
    <div>Left content</div>
    <div>Right content</div>
</div>

<!-- Full Width -->
<div class="w-full">Full width element</div>

<!-- Centered Container -->
<div class="flex items-center justify-center h-full">
    Centered content
</div>
```

### Spacing

```html
<!-- Margins -->
<div class="mt-auto">Margin top auto</div>
<div class="mb-auto">Margin bottom auto</div>

<!-- Gaps -->
<div class="flex gap-1">Small gap (8px)</div>
<div class="flex gap-2">Medium gap (16px)</div>
<div class="flex gap-3">Large gap (24px)</div>
```

### Text

```html
<p class="text-center font-bold">Centered bold text</p>
<p class="text-muted">Muted text</p>
<h1 class="text-gradient">Gradient heading</h1>
```

### Shadows

```html
<div class="shadow">Card shadow</div>
<div class="shadow-lg">Raised shadow</div>
<div class="shadow-xl">Overlay shadow</div>
```

### Borders

```html
<div class="rounded">Small border radius (3px)</div>
<div class="rounded-lg">Large border radius (8px)</div>
<div class="rounded-full">Circular (50%)</div>
```

---

## Responsive Design

### Breakpoint-Specific Classes

```html
<!-- Hide on mobile -->
<div class="hide-mobile">Only visible on tablet and desktop</div>

<!-- Show only on mobile -->
<div class="show-mobile">Only visible on mobile</div>

<!-- Hide on tablet -->
<div class="hide-tablet">Hidden on tablet sizes</div>

<!-- Hide on desktop -->
<div class="hide-desktop">Hidden on large screens</div>
```

### Media Query Breakpoints

```css
/* Mobile (default - no media query needed) */

/* Tablet */
@media (min-width: 768px) and (max-width: 1023px) {
    /* Tablet styles */
}

/* Desktop */
@media (min-width: 1024px) {
    /* Desktop styles */
}

/* Large Desktop */
@media (min-width: 1440px) {
    /* Large screen optimizations */
}
```

---

## Animations

### Built-in Animations

```html
<!-- Fade In -->
<div class="fade-in">Fades in on load</div>

<!-- Slide In from Right -->
<div class="slide-in-right">Slides in from right</div>

<!-- Scale In -->
<div class="scale-in">Scales up on load</div>

<!-- Bounce on Click -->
<button class="btn bounce-click">Click me</button>

<!-- Shake on Error -->
<input class="form-control shake-error" />
```

### Custom Transitions

```css
.my-element {
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.my-element:hover {
    transform: translateY(-2px);
    box-shadow: var(--jira-shadow-raised);
}
```

---

## Accessibility Best Practices

### Always Include ARIA Labels

```html
<!-- Icon-only button -->
<button class="btn btn-icon" aria-label="Delete task">
    <svg><!-- Delete icon --></svg>
</button>

<!-- Status indicator -->
<div role="status" aria-live="polite">
    Loading complete
</div>
```

### Semantic HTML

```html
<!-- Good -->
<nav aria-label="Main navigation">
    <NavLink>...</NavLink>
</nav>

<!-- Bad -->
<div onclick="navigate()">...</div>
```

### Form Labels

```html
<!-- Always associate labels with inputs -->
<label for="taskName" class="form-label">Task Name</label>
<input type="text" id="taskName" class="form-control">

<!-- Include validation messages -->
<ValidationMessage For="@(() => model.Title)" class="validation-message" />
```

### Focus States

```css
/* Always have visible focus states */
.my-button:focus-visible {
    outline: 2px solid var(--jira-border-focus);
    outline-offset: 2px;
}

/* Or use the modern focus ring class */
.my-button {
    /* ... other styles ... */
}
.my-button:focus-visible {
    @extend .focus-ring-modern;
}
```

---

## Dark Mode Support

All components automatically support dark mode through CSS custom properties. When creating new components:

```css
/* Good - uses design tokens */
.my-component {
    background: var(--jira-bg-primary);
    color: var(--jira-text-primary);
    border: 1px solid var(--jira-border);
}
/* This automatically works in dark mode! */

/* Bad - hardcoded colors */
.my-component {
    background: #FFFFFF;  /* Won't change in dark mode */
    color: #172B4D;
    border: 1px solid #DFE1E6;
}
```

### Testing Dark Mode

Toggle dark mode with the ThemeToggle component in the top navigation, or set `data-theme="dark"` on the `<body>` element.

---

## Common Patterns

### Modal Dialog

```html
<div class="modal show d-block" tabindex="-1" style="background-color: rgba(9, 30, 66, 0.54);">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Confirm Action</h5>
                <button type="button" class="btn-close" @onclick="CloseModal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <p>Are you sure you want to proceed?</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @onclick="CloseModal">Cancel</button>
                <button type="button" class="btn btn-primary" @onclick="Confirm">Confirm</button>
            </div>
        </div>
    </div>
</div>
```

### Empty State

```html
<div class="empty-state-modern">
    <svg class="empty-state-icon" width="120" height="120">
        <!-- Empty state illustration -->
    </svg>
    <h3 class="empty-state-title">No tasks yet</h3>
    <p class="empty-state-description">
        Create your first task to get started with your project.
    </p>
    <button class="btn btn-primary">Create First Task</button>
</div>
```

### Table

```html
<table class="table-modern">
    <thead>
        <tr>
            <th>Task ID</th>
            <th>Title</th>
            <th>Status</th>
            <th>Priority</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>TASK-001</td>
            <td>Fix login bug</td>
            <td><StatusBadge Status="@TaskStatus.InProgress" /></td>
            <td><PriorityIcon Priority="@TaskPriority.High" /></td>
        </tr>
    </tbody>
</table>
```

### Divider

```html
<!-- Gradient Divider -->
<div class="divider-gradient"></div>

<!-- Divider with Text -->
<div class="divider-text">or</div>
```

---

## File Structure for New Components

When creating a new component:

```
Components/
└── MyNewComponent.razor         # Component markup
    └── MyNewComponent.razor.cs  # Code-behind (optional)
    └── MyNewComponent.razor.css # Scoped styles (optional)
```

### Example Component with Scoped Styles

**MyCard.razor:**
```html
<div class="my-card">
    <h3>@Title</h3>
    <p>@Content</p>
</div>

@code {
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Content { get; set; }
}
```

**MyCard.razor.css:**
```css
.my-card {
    background: var(--jira-bg-primary);
    border-radius: var(--jira-radius-small);
    padding: var(--jira-space-200);
    box-shadow: var(--jira-shadow-card);
    transition: box-shadow 0.08s linear;
}

.my-card:hover {
    box-shadow: var(--jira-shadow-raised);
}

.my-card h3 {
    color: var(--jira-text-primary);
    font-size: calc(16px * var(--font-size-multiplier));
    margin-bottom: var(--jira-space-100);
}

.my-card p {
    color: var(--jira-text-secondary);
    font-size: calc(14px * var(--font-size-multiplier));
}
```

---

## Performance Tips

1. **Use CSS transforms for animations** (GPU accelerated):
   ```css
   /* Good */
   transform: translateY(-2px);

   /* Avoid */
   margin-top: -2px;
   ```

2. **Minimize specificity**:
   ```css
   /* Good */
   .btn-primary { }

   /* Avoid */
   div.container .buttons .btn.btn-primary { }
   ```

3. **Use CSS custom properties** for theming:
   ```css
   /* Good */
   color: var(--jira-primary);

   /* Avoid */
   color: #E41E26;
   ```

4. **Lazy load images**:
   ```html
   <img src="placeholder.jpg" loading="lazy" alt="Task image">
   ```

---

## Troubleshooting

### Styles not applying?
- Check that `modern-enhancements.css` is included in `_Layout.cshtml`
- Verify CSS custom properties are defined in `:root`
- Check browser DevTools for CSS conflicts

### Dark mode not working?
- Ensure `data-theme="dark"` is set on a parent element (usually `<body>`)
- Use `var(--jira-xxx)` tokens, not hardcoded colors
- Check dark mode overrides in `[data-theme="dark"]` section

### Responsive not working?
- Use mobile-first approach (default styles are mobile)
- Test actual breakpoints: 768px, 1024px, 1440px
- Use browser DevTools device emulation

### Accessibility issues?
- Run Lighthouse audit in Chrome DevTools
- Test keyboard navigation (Tab, Enter, Escape)
- Verify ARIA labels on icon-only buttons
- Check color contrast ratios

---

## Quick Reference Card

```
COLORS:     var(--jira-text-primary), var(--jira-bg-primary), var(--jira-border)
SPACING:    var(--jira-space-100) to var(--jira-space-600)
SHADOWS:    var(--jira-shadow-card), var(--jira-shadow-raised), var(--jira-shadow-overlay)
RADIUS:     var(--jira-radius-small), var(--jira-radius-large), var(--jira-radius-circle)
FONT SIZE:  calc(14px * var(--font-size-multiplier))

BREAKPOINTS:
  Mobile:     < 768px
  Tablet:     768px - 1023px
  Desktop:    1024px - 1439px
  Large:      1440px+

BUTTONS:    .btn .btn-primary .btn-secondary .btn-outline-primary .btn-ripple .btn-pill .btn-icon
CARDS:      .card .card-elevated .glass-effect .card-gradient-border
FORMS:      .form-control .form-select .form-label .input-with-icon
LOADING:    .spinner-border .spinner-modern .skeleton .progress-modern
BADGES:     .badge .badge-pill-modern .chip-modern
UTILITIES:  .flex .gap-2 .items-center .justify-between .shadow .rounded .text-center
```

---

## Getting Help

- **Design System Docs**: `D:\KimiAi\MiniJira\MiniJira\DESIGN_SYSTEM.md`
- **Full Summary**: `D:\KimiAi\MiniJira\MiniJira\UI_REDESIGN_SUMMARY.md`
- **CSS Files**:
  - `wwwroot/css/site.css` (core design tokens)
  - `wwwroot/css/modern-enhancements.css` (modern components)
- **Reference**: Atlassian Design System - https://atlassian.design

---

Happy coding!

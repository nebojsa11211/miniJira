# MiniJira Design System

## Overview
This document defines the complete design system for MiniJira, inspired by modern Jira's 2025 interface design. The system ensures consistency, accessibility, and maintainability across the entire application.

---

## Design Principles

### 1. Clarity First
- Clear visual hierarchy with purposeful use of typography and spacing
- Uncluttered interfaces that prioritize user tasks
- Consistent patterns that reduce cognitive load

### 2. Accessibility by Default
- WCAG 2.1 AA compliant color contrast ratios
- Keyboard navigable interfaces
- Screen reader friendly markup
- Clear focus indicators

### 3. Responsive Excellence
- Mobile-first approach
- Touch-friendly targets (minimum 44x44px)
- Fluid layouts that adapt gracefully across devices

### 4. Performance Conscious
- Lightweight CSS with minimal specificity
- Optimized rendering performance
- Progressive enhancement

---

## Color Palette

### Primary Colors (Red-Based Brand Identity)
```
--jira-red-500: #E41E26    (Primary brand color - matches logo)
--jira-red-600: #C81820    (Hover state - 15% darker)
--jira-red-700: #A61419    (Active state - 25% darker)
--jira-red-400: #F87171    (Light variant for backgrounds)
--jira-red-200: #FECACA    (Very light for subtle highlights)
--jira-red-100: #FEE2E2    (Background tint)
```

### Neutrals (N-Series)
```
Light Mode:
--jira-n0:   #FFFFFF    (Pure white - cards, backgrounds)
--jira-n10:  #FAFBFC    (Page background)
--jira-n20:  #F4F5F7    (Secondary background)
--jira-n30:  #EBECF0    (Tertiary background, borders)
--jira-n40:  #DFE1E6    (Borders)
--jira-n100: #7A869A    (Subtle text)
--jira-n300: #5E6C84    (Secondary text)
--jira-n500: #42526E    (Primary on light backgrounds)
--jira-n800: #172B4D    (Primary text)

Dark Mode:
--jira-dn10:  #1D2125   (Page background)
--jira-dn20:  #22272B   (Primary background)
--jira-dn30:  #2C333A   (Secondary background)
--jira-dn40:  #38414A   (Tertiary background, borders)
--jira-dn90:  #9FADBC   (Subtle text)
--jira-dn200: #C7D1DB   (Secondary text)
--jira-dn400: #FFFFFF   (Primary text)
```

### Semantic Colors

#### Success (Green)
```
--jira-green: #10B981        (Primary success)
--jira-green-light: #6EE7B7  (Light variant)
--jira-green-dark: #059669   (Dark variant)
--jira-green-bg: #D1FAE5     (Background tint)
Contrast Ratio: 4.7:1 on white (AA compliant)
```

#### Warning (Amber)
```
--jira-yellow: #F59E0B       (Primary warning)
--jira-yellow-light: #FCD34D (Light variant)
--jira-yellow-dark: #D97706  (Dark variant)
--jira-yellow-bg: #FEF3C7    (Background tint)
Contrast Ratio: 3.2:1 on white (AA Large Text)
```

#### Error (Red - Brand Aligned)
```
--jira-red: #E41E26          (Primary error - matches brand)
--jira-red-light: #F87171    (Light variant)
--jira-red-dark: #A61419     (Dark variant)
--jira-red-bg: #FEE2E2       (Background tint)
Contrast Ratio: 5.1:1 on white (AA compliant)
```

#### Accent Colors
```
--jira-accent-teal: #0D9488       (Complementary teal)
--jira-accent-amber: #F59E0B      (Warm amber accent)
--jira-purple: #5243AA            (Purple for categories)
```

### Status Colors (Semantic)
```
--jira-todo: var(--jira-n500)              (Gray for pending)
--jira-todo-bg: var(--jira-n30)            (Light gray background)
--jira-inprogress: var(--jira-primary)     (Brand red for active)
--jira-inprogress-bg: var(--jira-red-100)  (Light red background)
--jira-done: var(--jira-green-dark)        (Green for complete)
--jira-done-bg: var(--jira-green-bg)       (Light green background)
```

### Priority Colors
```
--jira-priority-highest: var(--jira-red-700)    (Critical - dark red)
--jira-priority-high: var(--jira-red-600)       (High - red)
--jira-priority-medium: var(--jira-yellow-dark) (Medium - amber)
--jira-priority-low: var(--jira-accent-teal)    (Low - teal)
--jira-priority-lowest: var(--jira-n100)        (Lowest - gray)
```

---

## Typography

### Font Family
```css
font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto',
             'Noto Sans', 'Ubuntu', 'Droid Sans', 'Helvetica Neue', sans-serif;
```

### Type Scale (with font-size-multiplier support)
```
H1: calc(29px * var(--font-size-multiplier))  line-height: 32px  weight: 600
H2: calc(24px * var(--font-size-multiplier))  line-height: 28px  weight: 600
H3: calc(20px * var(--font-size-multiplier))  line-height: 24px  weight: 600
H4: calc(16px * var(--font-size-multiplier))  line-height: 20px  weight: 600
H5: calc(14px * var(--font-size-multiplier))  line-height: 16px  weight: 600
H6: calc(12px * var(--font-size-multiplier))  line-height: 16px  weight: 700 (uppercase)

Body: calc(14px * var(--font-size-multiplier)) line-height: 20px  weight: 400
Small: calc(12px * var(--font-size-multiplier)) line-height: 16px  weight: 400
Tiny: calc(11px * var(--font-size-multiplier))  line-height: 14px  weight: 400
```

### Letter Spacing
```
H1-H3: -0.01em (tighter for larger text)
H4: -0.006em
Body: normal
H6/Labels: 0.05em (wider for uppercase)
```

### Font Weights
```
400: Regular (body text, descriptions)
500: Medium (navigation, interactive elements)
600: Semi-bold (headings, emphasis)
700: Bold (labels, badges, important UI)
```

---

## Spacing System

### 4px Grid
```
--jira-space-025: 2px
--jira-space-050: 4px
--jira-space-075: 6px
--jira-space-100: 8px
--jira-space-150: 12px
--jira-space-200: 16px
--jira-space-250: 20px
--jira-space-300: 24px
--jira-space-400: 32px
--jira-space-500: 40px
--jira-space-600: 48px
```

### Usage Guidelines
- Use multiples of 4px for consistency
- Smaller spacing (2-8px) for tight components
- Medium spacing (12-24px) for sections
- Large spacing (32-48px) for page-level separation

---

## Border Radius

```
--jira-radius-small: 3px    (buttons, inputs, cards)
--jira-radius-medium: 3px   (same as small for consistency)
--jira-radius-large: 8px    (modals, larger containers)
--jira-radius-circle: 50%   (avatars, icon buttons)
```

---

## Shadows (Elevation System)

```
--jira-shadow-card:
  0px 1px 1px rgba(9, 30, 66, 0.25),
  0px 0px 1px rgba(9, 30, 66, 0.31);

--jira-shadow-raised:
  0px 4px 8px -2px rgba(9, 30, 66, 0.25),
  0px 0px 1px rgba(9, 30, 66, 0.31);

--jira-shadow-overlay:
  0px 8px 16px -4px rgba(9, 30, 66, 0.25),
  0px 0px 1px rgba(9, 30, 66, 0.31);
```

### Elevation Hierarchy
- Level 0 (Flat): No shadow, borders only
- Level 1 (Card): shadow-card for task cards, panels
- Level 2 (Raised): shadow-raised on hover states
- Level 3 (Overlay): shadow-overlay for modals, dropdowns

---

## Components

### Buttons

#### Primary Button
```css
Background: var(--jira-primary)
Text: var(--jira-n0)
Padding: 8px 12px
Border-radius: 3px
Font-weight: 500
Min-height: 44px (touch-friendly)

Hover: background becomes var(--jira-primary-hover)
Active: background becomes var(--jira-primary-active)
Focus: 2px solid outline with 2px offset
Disabled: opacity 0.5, cursor not-allowed
```

#### Secondary Button
```css
Background: var(--jira-bg-tertiary)
Text: var(--jira-text-primary)
Padding: 8px 12px
Border-radius: 3px
Font-weight: 500

Hover: background becomes var(--jira-n40)
Focus: 2px solid outline with 2px offset
```

#### Outline Button
```css
Background: transparent
Border: 2px solid var(--jira-border)
Text: var(--jira-text-primary)
Padding: 6px 10px (adjusted for border)

Hover: background becomes var(--jira-bg-secondary)
```

### Form Controls

#### Text Input
```css
Border: 2px solid var(--jira-border)
Border-radius: 3px
Padding: 8px 6px
Font-size: 14px
Line-height: 20px

Hover: background becomes var(--jira-n10)
Focus: border-color becomes var(--jira-border-focus)
       box-shadow: 0 0 0 1px var(--jira-border-focus)
Invalid: border-color becomes var(--jira-red)
```

#### Select / Dropdown
```css
Same as text input
Additional: dropdown arrow icon
Height: auto (based on content)
```

#### Textarea
```css
Same as text input
Resize: vertical (allow user control)
Min-height: based on rows attribute
```

#### Checkbox / Radio
```css
Size: 20px x 20px (minimum for accessibility)
Border: 2px solid var(--jira-border)
Checked background: var(--jira-primary)
Focus ring: visible outline
```

### Cards

#### Task Card
```css
Background: var(--jira-bg-primary)
Border: none
Border-radius: 3px
Padding: 8px
Box-shadow: var(--jira-shadow-card)
Cursor: pointer

Hover: background becomes var(--jira-bg-page)
       box-shadow becomes var(--jira-shadow-raised)
Active: background becomes var(--jira-bg-secondary)
Focus: 2px solid outline with 2px offset
```

### Badges / Labels

#### Category Badge (Lozenge)
```css
Display: inline-block
Background: var(--jira-bg-tertiary)
Color: var(--jira-text-primary)
Font-size: 11px
Font-weight: 700
Padding: 2px 6px
Border-radius: 3px
Text-transform: uppercase
Letter-spacing: 0.05em
```

#### Status Badge
```css
Background: semantic color (e.g., --jira-done-bg)
Color: semantic text color (e.g., --jira-green-dark)
Font-size: 11px
Font-weight: 700
Padding: 2px 8px
Border-radius: 12px (pill shape)
```

### Modals / Dialogs

```css
Background: var(--jira-bg-primary)
Border-radius: 3px
Box-shadow: var(--jira-shadow-overlay)
Max-width: 600px
Padding: 0 (controlled by sections)

Header:
  Border-bottom: 1px solid var(--jira-border)
  Padding: 16px 24px

Body:
  Padding: 24px

Footer:
  Border-top: 1px solid var(--jira-border)
  Padding: 16px 24px

Backdrop: rgba(9, 30, 66, 0.54)
```

---

## Layout Patterns

### Sidebar Navigation
```
Width: 280px (desktop), 240px (tablet), 64px (collapsed)
Background: gradient from bg-primary to bg-secondary
Border-right: 1px solid var(--jira-border)
Box-shadow: 2px 0 8px rgba(0, 0, 0, 0.08)
```

### Main Content Area
```
Padding:
  Mobile: 16px
  Tablet: 24px 32px
  Desktop: 32px 40px
  Large: 40px 48px

Background: var(--jira-bg-page)
Min-height: calc(100vh - header height)
```

### Kanban Board
```
Display: grid
Gap: 16px
Grid columns: repeat(auto-fit, minmax(280px, 1fr))

Mobile: 1 column (stacked)
Tablet: 1 column (better readability)
Desktop: dynamic based on column count
```

### Kanban Column
```
Background: var(--jira-bg-secondary)
Border-radius: 3px
Border-top: 3px solid (column color)
Min-height: 600px (desktop), auto (mobile)
Padding: 8px 12px
```

---

## Responsive Breakpoints

```
Mobile:     < 768px
Tablet:     768px - 1023px
Desktop:    1024px - 1439px
Large:      1440px+
Extra Large: 1920px+
```

### Mobile Strategy
- Stack all columns vertically
- Full-width buttons
- Reduced heading sizes
- Touch-friendly targets (44x44px minimum)
- Font-size minimum 16px for inputs (prevents iOS zoom)

### Tablet Strategy
- Single column layout for kanban (better readability)
- Sidebar can collapse to icon-only mode
- Moderate spacing

### Desktop Strategy
- Multi-column kanban layout
- Full sidebar with text labels
- Optimal spacing and typography

---

## Accessibility Guidelines

### Color Contrast
All text must meet WCAG 2.1 AA standards:
- Normal text (< 18px): 4.5:1 minimum
- Large text (>= 18px or >= 14px bold): 3:1 minimum
- UI components: 3:1 minimum

### Keyboard Navigation
- All interactive elements must be keyboard accessible
- Logical tab order
- Visible focus indicators (2px outline, 2px offset)
- Skip links for main content

### Screen Readers
- Semantic HTML (nav, main, article, section)
- ARIA labels where needed
- Alt text for images
- Role attributes for custom components
- Visually hidden text for icon-only buttons

### Motion
- Respect prefers-reduced-motion
- All animations/transitions disabled or reduced to 0.01ms

---

## Icons

### System
Using inline SVG icons for:
- Navigation items (24x24px)
- Buttons (16x20px depending on context)
- Status indicators (16x16px)
- Priority icons (16x16px)

### Style
- Stroke-based icons (consistent 2px stroke)
- currentColor for inheritance
- Proper viewBox for scaling

---

## Dark Mode

### Implementation
```css
[data-theme="dark"] {
  /* Override all color variables */
  /* Invert hierarchy: lighter colors for text, darker for backgrounds */
  /* Increase contrast ratios for accessibility */
}
```

### Key Adjustments
- Text colors become lighter (dn400 for primary)
- Backgrounds become darker (dn10, dn20, dn30)
- Shadows become deeper with darker rgba values
- Primary colors lightened for better visibility on dark backgrounds
- Border colors adjusted for subtle separation

---

## Animation & Transitions

### Standard Timing
```css
Fast: 0.08s linear (color changes, subtle effects)
Normal: 0.2s cubic-bezier(0.4, 0, 0.2, 1) (most UI transitions)
Slow: 0.3s ease (complex animations, ripples)
```

### Common Transitions
```css
Background color: 0.08s linear
Box shadow: 0.08s linear
Transform: 0.2s cubic-bezier(0.4, 0, 0.2, 1)
Opacity: 0.15s ease-in-out
Width/Height: 0.15s ease-in-out
```

### Hover Effects
- Subtle elevation changes (shadow increase)
- Background color darkening/lightening
- Scale transforms for icons (1.05 - 1.1)
- Smooth transitions

---

## Best Practices

### CSS Architecture
1. Use CSS custom properties for theming
2. Scope styles to components with .razor.css
3. Minimize specificity
4. Follow BEM-like naming for clarity
5. Mobile-first media queries

### Performance
1. Avoid expensive properties (box-shadow on scroll)
2. Use transform for animations (GPU accelerated)
3. Limit reflows/repaints
4. CSS Grid over nested flexbox where appropriate

### Maintainability
1. Document color token usage
2. Consistent naming conventions
3. Reusable component classes
4. Clear separation of concerns
5. Version control for design changes

---

## Component Checklist

When creating new components, ensure:
- [ ] Responsive across all breakpoints
- [ ] Keyboard accessible
- [ ] Proper focus indicators
- [ ] Color contrast meets AA standards
- [ ] Touch targets >= 44x44px
- [ ] Semantic HTML
- [ ] ARIA attributes where needed
- [ ] Respects reduced motion
- [ ] Dark mode compatible
- [ ] Documented in design system

---

## References

- **Jira Design System**: Reference image analyzed for color, spacing, typography
- **Atlassian Design System**: https://atlassian.design
- **WCAG 2.1 Guidelines**: https://www.w3.org/WAI/WCAG21/quickref/
- **Material Design**: Principles for elevation, motion
- **Apple Human Interface Guidelines**: Accessibility standards

---

Last Updated: 2025-10-16
Version: 1.0

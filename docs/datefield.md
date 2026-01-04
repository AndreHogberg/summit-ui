# DateField

A segmented date/time input component with culture-aware formatting and full keyboard navigation. Supports both `DateOnly` and `DateTime` values.

## Features

- Segmented editing (each part is independently editable)
- Culture-aware date formatting
- Support for both `DateOnly` and `DateTime` types
- Configurable granularity (day, hour, minute, second)
- 12-hour and 24-hour time formats
- Custom date patterns
- Min/max validation
- Disabled and read-only states
- EditForm integration
- Full keyboard navigation
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using SummitUI
```

## Anatomy

```razor
<DateFieldRoot @bind-Value="date">
    <DateFieldLabel>Select a date</DateFieldLabel>
    <DateFieldInput />
</DateFieldRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `DateFieldRoot` | Root container managing date field state |
| `DateFieldLabel` | Accessible label for the date field |
| `DateFieldInput` | Container that renders date/time segments |
| `DateFieldSegment` | Individual editable segment (auto-generated) |

## API Reference

### DateFieldRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `DateOnly?` | - | Bound DateOnly value |
| `ValueChanged` | `EventCallback<DateOnly?>` | - | DateOnly value change callback |
| `Placeholder` | `DateOnly` | `DateOnly.FromDateTime(DateTime.Now)` | Placeholder date when value is null |
| `DateTimeValue` | `DateTime?` | - | Bound DateTime value |
| `DateTimeValueChanged` | `EventCallback<DateTime?>` | - | DateTime value change callback |
| `DateTimePlaceholder` | `DateTime` | `DateTime.Now` | Placeholder DateTime when value is null |
| `Granularity` | `DateFieldGranularity` | `Day` | Controls which segments are displayed |
| `HourCycle` | `HourCycle` | `Auto` | 12-hour or 24-hour time format |
| `Locale` | `CultureInfo?` | Current culture | Culture for date formatting |
| `DatePattern` | `string?` | - | Custom date format pattern (e.g., "yyyy-MM-dd") |
| `MinValue` | `DateOnly?` | - | Minimum allowed DateOnly value |
| `MaxValue` | `DateOnly?` | - | Maximum allowed DateOnly value |
| `MinDateTime` | `DateTime?` | - | Minimum allowed DateTime value |
| `MaxDateTime` | `DateTime?` | - | Maximum allowed DateTime value |
| `Disabled` | `bool` | `false` | Disables the entire date field |
| `ReadOnly` | `bool` | `false` | Makes the field read-only |
| `Invalid` | `bool` | `false` | Indicates validation error state |
| `Name` | `string?` | - | Form field name for hidden input |
| `Required` | `bool` | `false` | Marks the field as required |
| `ChildContent` | `RenderFragment?` | - | Child content |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DateFieldLabel

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Label text content |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DateFieldInput

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment<IReadOnlyList<DateFieldSegmentState>>?` | - | Custom template for segments |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DateFieldSegment

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Segment` | `DateFieldSegmentState` | **Required** | Segment state object |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

## Enums

### DateFieldGranularity

```csharp
public enum DateFieldGranularity
{
    Day,    // Show only date segments (Year, Month, Day)
    Hour,   // Show date and hour segments
    Minute, // Show date, hour, and minute segments
    Second  // Show date, hour, minute, and second segments
}
```

### HourCycle

```csharp
public enum HourCycle
{
    Auto, // Use the locale's default hour cycle
    H12,  // 12-hour cycle with AM/PM (1-12)
    H23,  // 24-hour cycle (0-23)
    H24,  // 24-hour cycle (1-24)
    H11   // 12-hour cycle (0-11)
}
```

### DateFieldSegmentType

```csharp
public enum DateFieldSegmentType
{
    Day,
    Month,
    Year,
    Hour,
    Minute,
    Second,
    DayPeriod,
    Literal
}
```

## Examples

### Basic DateOnly Field

```razor
@code {
    private DateOnly? selectedDate = DateOnly.FromDateTime(DateTime.Today);
}

<DateFieldRoot @bind-Value="selectedDate">
    <DateFieldLabel class="datefield-label">Select a date</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

<p>Selected: @(selectedDate?.ToString("yyyy-MM-dd") ?? "None")</p>
```

### DateTime with Time

```razor
@code {
    private DateTime? selectedDateTime = DateTime.Now;
}

<DateFieldRoot @bind-DateTimeValue="selectedDateTime" Granularity="DateFieldGranularity.Minute">
    <DateFieldLabel class="datefield-label">Select date and time</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

<p>Selected: @(selectedDateTime?.ToString("yyyy-MM-dd HH:mm") ?? "None")</p>
```

### Full DateTime with Seconds

```razor
@code {
    private DateTime? fullDateTime = DateTime.Now;
}

<DateFieldRoot @bind-DateTimeValue="fullDateTime" Granularity="DateFieldGranularity.Second">
    <DateFieldLabel class="datefield-label">Full date and time</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

<p>Selected: @(fullDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None")</p>
```

### With Culture/Locale

```razor
@using System.Globalization

@code {
    private DateOnly? swedishDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly? usDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly? japaneseDate = DateOnly.FromDateTime(DateTime.Today);
    
    private CultureInfo swedishCulture = new CultureInfo("sv-SE");
    private CultureInfo usCulture = new CultureInfo("en-US");
    private CultureInfo japaneseCulture = new CultureInfo("ja-JP");
}

@* Swedish format: yyyy-MM-dd *@
<DateFieldRoot @bind-Value="swedishDate" Locale="@swedishCulture">
    <DateFieldLabel>Välj ett datum</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

@* US format: M/d/yyyy *@
<DateFieldRoot @bind-Value="usDate" Locale="@usCulture">
    <DateFieldLabel>Select a date</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

@* Japanese format: yyyy/MM/dd *@
<DateFieldRoot @bind-Value="japaneseDate" Locale="@japaneseCulture">
    <DateFieldLabel>日付を選択</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>
```

### Custom Date Pattern

```razor
@code {
    private DateOnly? customDate = DateOnly.FromDateTime(DateTime.Today);
}

<DateFieldRoot @bind-Value="customDate" DatePattern="dd.MM.yyyy">
    <DateFieldLabel>Date (dd.MM.yyyy format)</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>
```

### Min/Max Validation

```razor
@code {
    private DateOnly? constrainedDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly minDate = new DateOnly(2025, 1, 1);
    private DateOnly maxDate = new DateOnly(2025, 12, 31);
}

<DateFieldRoot @bind-Value="constrainedDate" MinValue="@minDate" MaxValue="@maxDate">
    <DateFieldLabel>Select a date (Jan 1, 2025 - Dec 31, 2025)</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

@if (constrainedDate.HasValue && (constrainedDate < minDate || constrainedDate > maxDate))
{
    <p style="color: #dc3545;">
        Date must be between @minDate.ToString("yyyy-MM-dd") and @maxDate.ToString("yyyy-MM-dd")
    </p>
}
```

### With Placeholder

```razor
@code {
    private DateOnly? nullableDate = null;
    private DateOnly placeholderDate = new DateOnly(2025, 1, 1);
}

<DateFieldRoot Value="@nullableDate" 
               ValueChanged="v => nullableDate = v" 
               Placeholder="@placeholderDate">
    <DateFieldLabel>Date with placeholder</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>

<p>Selected: @(nullableDate?.ToString("yyyy-MM-dd") ?? "None (showing placeholder)")</p>
```

### Disabled State

```razor
@code {
    private DateOnly? disabledDate = DateOnly.FromDateTime(DateTime.Today);
}

<DateFieldRoot Value="@disabledDate" Disabled="true">
    <DateFieldLabel>Disabled date field</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>
```

### Read-Only State

```razor
@code {
    private DateOnly? readOnlyDate = DateOnly.FromDateTime(DateTime.Today);
}

<DateFieldRoot Value="@readOnlyDate" ReadOnly="true">
    <DateFieldLabel>Read-only date field</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>
```

### Invalid State

```razor
@code {
    private DateOnly? invalidDate = null;
}

<DateFieldRoot @bind-Value="invalidDate" Invalid="true">
    <DateFieldLabel>Date with error</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>
<p style="color: #dc3545; font-size: 0.875rem;">Please enter a valid date</p>
```

### EditForm Integration

```razor
@using System.ComponentModel.DataAnnotations

@code {
    public class DateFieldFormModel
    {
        [Required(ErrorMessage = "Birth date is required")]
        public DateOnly? BirthDate { get; set; }

        public DateTime? AppointmentTime { get; set; }
    }
    
    private DateFieldFormModel formModel = new();
    private EditContext? editContext;
    private bool editFormSubmitted;
    
    protected override void OnInitialized()
    {
        editContext = new EditContext(formModel);
    }
    
    private void HandleSubmit()
    {
        editFormSubmitted = true;
        if (editContext!.Validate())
        {
            // Process form
        }
    }
}

<EditForm EditContext="editContext" OnSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <div style="margin-bottom: 1rem;">
        <DateFieldRoot @bind-Value="formModel.BirthDate"
                       Name="birthDate"
                       Required="true"
                       Invalid="@(editFormSubmitted && formModel.BirthDate == null)">
            <DateFieldLabel>Birth Date (Required)</DateFieldLabel>
            <DateFieldInput class="datefield-input" />
        </DateFieldRoot>
        <ValidationMessage For="@(() => formModel.BirthDate)" />
    </div>

    <div style="margin-bottom: 1rem;">
        <DateFieldRoot @bind-DateTimeValue="formModel.AppointmentTime"
                       Granularity="DateFieldGranularity.Minute"
                       Name="appointmentTime"
                       MinDateTime="@DateTime.Now"
                       MaxDateTime="@DateTime.Now.AddYears(1)">
            <DateFieldLabel>Appointment (Optional, future dates only)</DateFieldLabel>
            <DateFieldInput class="datefield-input" />
        </DateFieldRoot>
    </div>

    <button type="submit">Submit</button>
</EditForm>
```

### With Styling

```razor
<DateFieldRoot @bind-Value="selectedDate">
    <DateFieldLabel class="datefield-label">Select a date</DateFieldLabel>
    <DateFieldInput class="datefield-input" />
</DateFieldRoot>
```

```css
.datefield-label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #333;
}

.datefield-input {
    display: inline-flex;
    align-items: center;
    gap: 0;
    padding: 0.5rem 0.75rem;
    background: white;
    border: 1px solid #ccc;
    border-radius: 6px;
    font-size: 0.875rem;
    font-family: inherit;
}

.datefield-input:focus-within {
    border-color: #0d6efd;
    box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.25);
}

[data-disabled] .datefield-input {
    background: #f5f5f5;
    color: #999;
    cursor: not-allowed;
}

[data-readonly] .datefield-input {
    background: #f9f9f9;
}

[data-invalid] .datefield-input {
    border-color: #dc3545;
}

[data-invalid] .datefield-input:focus-within {
    box-shadow: 0 0 0 3px rgba(220, 53, 69, 0.25);
}

/* Segment styling */
[data-segment]:not([data-segment="literal"]) {
    padding: 0.125rem 0.25rem;
    border-radius: 4px;
    outline: none;
    min-width: 1.5em;
    text-align: center;
}

[data-segment]:not([data-segment="literal"]):focus {
    background: #e7f1ff;
    color: #0d6efd;
}

[data-segment="literal"] {
    color: #666;
    padding: 0 0.125rem;
}

[data-segment][data-placeholder] {
    color: #999;
}
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `ArrowUp` | Increment the focused segment value |
| `ArrowDown` | Decrement the focused segment value |
| `ArrowLeft` | Move focus to previous segment |
| `ArrowRight` | Move focus to next segment |
| `Tab` | Move focus to next segment (or next element) |
| `Shift+Tab` | Move focus to previous segment (or previous element) |
| `0-9` | Type numeric value directly into segment |
| `Backspace` / `Delete` | Clear the value (sets to null) |
| `A` / `P` | Toggle AM/PM on DayPeriod segment |

### ARIA Attributes

- Root element has `role="group"` with `aria-labelledby` pointing to label
- Each editable segment has `role="spinbutton"`
- Segments include `aria-valuemin`, `aria-valuemax`, `aria-valuenow`, and `aria-valuetext`
- Literal segments (separators) have `aria-hidden="true"`
- Disabled segments have `tabindex="-1"`

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-disabled` | Present when disabled | Date field is disabled |
| `data-readonly` | Present when read-only | Date field is read-only |
| `data-invalid` | Present when invalid | Validation error state |
| `data-placeholder` | Present when no value | Showing placeholder |
| `data-segment` | Segment type | Identifies segment type for styling |

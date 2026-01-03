# Select

A custom dropdown select component with full keyboard navigation and accessibility support. Supports strings, enums, and complex objects.

## Features

- Generic type support (`string`, enums, objects)
- Full keyboard navigation with typeahead
- Grouped items with labels
- Flexible positioning with collision detection
- EditForm integration with validation
- Controlled and uncontrolled modes
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using SummitUI.Components.Select
```

## Anatomy

```razor
<SelectRoot TValue="string">
    <SelectTrigger TValue="string">
        <SelectValue TValue="string" Placeholder="Select an option..." />
    </SelectTrigger>
    <SelectPortal TValue="string">
        <SelectContent TValue="string">
            <SelectViewport TValue="string">
                <SelectItem TValue="string" Value="@("option-1")">
                    <SelectItemText>Option 1</SelectItemText>
                </SelectItem>
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `SelectRoot<TValue>` | Generic root container managing select state |
| `SelectTrigger<TValue>` | Button that opens the dropdown (combobox role) |
| `SelectValue<TValue>` | Displays selected value or placeholder |
| `SelectPortal<TValue>` | Renders content in fixed-position container |
| `SelectContent<TValue>` | Floating listbox panel with positioning |
| `SelectViewport<TValue>` | Scrollable container for items |
| `SelectItem<TValue>` | Selectable option (option role) |
| `SelectItemText` | Text content wrapper for items |
| `SelectGroup<TValue>` | Groups related items |
| `SelectGroupLabel` | Label for a group |

## API Reference

### SelectRoot<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Child content |
| `Value` | `TValue?` | - | Controlled selected value |
| `DefaultValue` | `TValue?` | - | Default value (uncontrolled) |
| `ValueChanged` | `EventCallback<TValue?>` | - | Value change callback |
| `ValueExpression` | `Expression<Func<TValue?>>?` | - | For EditForm validation |
| `OnValueChange` | `EventCallback<TValue?>` | - | Alternative value change callback |
| `Open` | `bool?` | - | Controlled open state |
| `DefaultOpen` | `bool` | `false` | Default open state |
| `OpenChanged` | `EventCallback<bool>` | - | Open state change callback |
| `Disabled` | `bool` | `false` | Disable entire select |
| `Required` | `bool` | `false` | For form validation |
| `Invalid` | `bool` | `false` | For error styling |
| `Name` | `string?` | - | Form field name for hidden input |

### SelectTrigger<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Typically SelectValue |
| `As` | `string` | `"button"` | HTML element |
| `AriaLabel` | `string?` | - | Direct aria-label |
| `AriaLabelledBy` | `string?` | - | ID of external label |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### SelectValue<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Placeholder` | `string?` | - | Placeholder when no value |
| `ChildContent` | `RenderFragment?` | - | Custom content |

### SelectContent<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Typically SelectViewport |
| `As` | `string` | `"div"` | HTML element |
| `Side` | `Side` | `Bottom` | Placement side |
| `SideOffset` | `int` | `4` | Offset from trigger (px) |
| `Align` | `Align` | `Start` | Alignment along side axis |
| `AlignOffset` | `int` | `0` | Alignment offset (px) |
| `AvoidCollisions` | `bool` | `true` | Avoid viewport boundaries |
| `CollisionPadding` | `int` | `8` | Viewport padding (px) |
| `EscapeKeyBehavior` | `EscapeKeyBehavior` | `Close` | Escape key behavior |
| `OutsideClickBehavior` | `OutsideClickBehavior` | `Close` | Outside click behavior |
| `OnInteractOutside` | `EventCallback` | - | Outside click callback |
| `OnEscapeKeyDown` | `EventCallback` | - | Escape key callback |

### SelectViewport<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | SelectItem components |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### SelectItem<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `TValue` | **Required** | Value of this item |
| `Key` | `string?` | - | Optional string key for JS interop |
| `Label` | `string?` | - | Label for typeahead and display |
| `Disabled` | `bool` | `false` | Disable item |
| `OnSelect` | `EventCallback` | - | Selection callback |
| `ChildContent` | `RenderFragment?` | - | Item content |

### SelectItemText

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Text content |

### SelectGroup<TValue>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Group content |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### SelectGroupLabel

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Label content |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

## Enums

### Side

```csharp
public enum Side
{
    Top,
    Right,
    Bottom,
    Left
}
```

### Align

```csharp
public enum Align
{
    Start,
    Center,
    End
}
```

### EscapeKeyBehavior

```csharp
public enum EscapeKeyBehavior
{
    Close,
    Ignore
}
```

### OutsideClickBehavior

```csharp
public enum OutsideClickBehavior
{
    Close,
    Ignore
}
```

## Examples

### Basic String Select

```razor
@code {
    private string? selectedFruit;
}

<SelectRoot TValue="string" @bind-Value="selectedFruit">
    <SelectTrigger TValue="string" class="select-trigger">
        <SelectValue TValue="string" Placeholder="Select a fruit..." />
        <span class="select-icon">▼</span>
    </SelectTrigger>
    <SelectPortal TValue="string">
        <SelectContent TValue="string" class="select-content" SideOffset="4">
            <SelectViewport TValue="string" class="select-viewport">
                <SelectItem TValue="string" Value="@("apple")" Label="Apple">
                    <SelectItemText>Apple</SelectItemText>
                </SelectItem>
                <SelectItem TValue="string" Value="@("banana")" Label="Banana">
                    <SelectItemText>Banana</SelectItemText>
                </SelectItem>
                <SelectItem TValue="string" Value="@("cherry")" Label="Cherry">
                    <SelectItemText>Cherry</SelectItemText>
                </SelectItem>
                <SelectItem TValue="string" Value="@("orange")" Label="Orange">
                    <SelectItemText>Orange</SelectItemText>
                </SelectItem>
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>

<p>Selected: @(selectedFruit ?? "None")</p>
```

### With Enum Values

```razor
@code {
    public enum Priority { Low, Medium, High, Critical }
    private Priority selectedPriority = Priority.Medium;
}

<SelectRoot TValue="Priority" @bind-Value="selectedPriority">
    <SelectTrigger TValue="Priority" class="select-trigger">
        <SelectValue TValue="Priority" Placeholder="Select priority..." />
        <span class="select-icon">▼</span>
    </SelectTrigger>
    <SelectPortal TValue="Priority">
        <SelectContent TValue="Priority" class="select-content">
            <SelectViewport TValue="Priority">
                <SelectItem TValue="Priority" Value="Priority.Low" Label="Low">
                    <SelectItemText>Low Priority</SelectItemText>
                </SelectItem>
                <SelectItem TValue="Priority" Value="Priority.Medium" Label="Medium">
                    <SelectItemText>Medium Priority</SelectItemText>
                </SelectItem>
                <SelectItem TValue="Priority" Value="Priority.High" Label="High">
                    <SelectItemText>High Priority</SelectItemText>
                </SelectItem>
                <SelectItem TValue="Priority" Value="Priority.Critical" Label="Critical">
                    <SelectItemText>Critical Priority</SelectItemText>
                </SelectItem>
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>
```

### With Complex Objects

```razor
@code {
    public record Country(string Code, string Name);
    
    private List<Country> countries = new()
    {
        new("US", "United States"),
        new("UK", "United Kingdom"),
        new("CA", "Canada"),
        new("AU", "Australia")
    };
    
    private Country? selectedCountry;
}

<SelectRoot TValue="Country" @bind-Value="selectedCountry">
    <SelectTrigger TValue="Country" class="select-trigger">
        <SelectValue TValue="Country" Placeholder="Select a country..." />
        <span class="select-icon">▼</span>
    </SelectTrigger>
    <SelectPortal TValue="Country">
        <SelectContent TValue="Country" class="select-content">
            <SelectViewport TValue="Country">
                @foreach (var country in countries)
                {
                    <SelectItem TValue="Country" 
                                Value="country" 
                                Key="@country.Code" 
                                Label="@country.Name">
                        <SelectItemText>@country.Name (@country.Code)</SelectItemText>
                    </SelectItem>
                }
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>
```

### Grouped Items

```razor
<SelectRoot TValue="string" @bind-Value="selectedFood">
    <SelectTrigger TValue="string" class="select-trigger">
        <SelectValue TValue="string" Placeholder="Select food..." />
    </SelectTrigger>
    <SelectPortal TValue="string">
        <SelectContent TValue="string" class="select-content">
            <SelectViewport TValue="string">
                <SelectGroup TValue="string">
                    <SelectGroupLabel class="select-group-label">Fruits</SelectGroupLabel>
                    <SelectItem TValue="string" Value="@("apple")" Label="Apple">
                        <SelectItemText>Apple</SelectItemText>
                    </SelectItem>
                    <SelectItem TValue="string" Value="@("banana")" Label="Banana">
                        <SelectItemText>Banana</SelectItemText>
                    </SelectItem>
                </SelectGroup>
                <SelectGroup TValue="string">
                    <SelectGroupLabel class="select-group-label">Vegetables</SelectGroupLabel>
                    <SelectItem TValue="string" Value="@("carrot")" Label="Carrot">
                        <SelectItemText>Carrot</SelectItemText>
                    </SelectItem>
                    <SelectItem TValue="string" Value="@("broccoli")" Label="Broccoli">
                        <SelectItemText>Broccoli</SelectItemText>
                    </SelectItem>
                </SelectGroup>
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>
```

### EditForm Integration

```razor
@code {
    public class FormModel
    {
        [Required(ErrorMessage = "Category is required")]
        public string? Category { get; set; }
    }
    
    private FormModel formModel = new();
    private EditContext? editContext;
    
    protected override void OnInitialized()
    {
        editContext = new EditContext(formModel);
    }
    
    private void HandleSubmit()
    {
        if (editContext!.Validate())
        {
            // Process form
        }
    }
}

<EditForm EditContext="editContext" OnSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <div class="form-field">
        <label for="category">Category</label>
        <SelectRoot TValue="string" 
                    @bind-Value="formModel.Category" 
                    Name="category" 
                    Required="true">
            <SelectTrigger TValue="string" class="select-trigger" id="category">
                <SelectValue TValue="string" Placeholder="Select category..." />
            </SelectTrigger>
            <SelectPortal TValue="string">
                <SelectContent TValue="string" class="select-content">
                    <SelectViewport TValue="string">
                        <SelectItem TValue="string" Value="@("electronics")" Label="Electronics">
                            <SelectItemText>Electronics</SelectItemText>
                        </SelectItem>
                        <SelectItem TValue="string" Value="@("clothing")" Label="Clothing">
                            <SelectItemText>Clothing</SelectItemText>
                        </SelectItem>
                        <SelectItem TValue="string" Value="@("books")" Label="Books">
                            <SelectItemText>Books</SelectItemText>
                        </SelectItem>
                    </SelectViewport>
                </SelectContent>
            </SelectPortal>
        </SelectRoot>
        <ValidationMessage For="@(() => formModel.Category)" />
    </div>
    
    <button type="submit">Submit</button>
</EditForm>
```

### Disabled State

```razor
@* Disabled entire select *@
<SelectRoot TValue="string" Disabled="true">
    <SelectTrigger TValue="string" class="select-trigger">
        <SelectValue TValue="string" Placeholder="Disabled..." />
    </SelectTrigger>
    ...
</SelectRoot>

@* Disabled individual items *@
<SelectRoot TValue="string" @bind-Value="selectedValue">
    <SelectTrigger TValue="string" class="select-trigger">
        <SelectValue TValue="string" Placeholder="Select..." />
    </SelectTrigger>
    <SelectPortal TValue="string">
        <SelectContent TValue="string" class="select-content">
            <SelectViewport TValue="string">
                <SelectItem TValue="string" Value="@("option1")" Label="Option 1">
                    <SelectItemText>Option 1</SelectItemText>
                </SelectItem>
                <SelectItem TValue="string" Value="@("option2")" Label="Option 2" Disabled="true">
                    <SelectItemText>Option 2 (Disabled)</SelectItemText>
                </SelectItem>
                <SelectItem TValue="string" Value="@("option3")" Label="Option 3">
                    <SelectItemText>Option 3</SelectItemText>
                </SelectItem>
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>
```

### With External Label

```razor
<label id="country-label">Country</label>
<SelectRoot TValue="string" @bind-Value="selectedCountry">
    <SelectTrigger TValue="string" class="select-trigger" AriaLabelledBy="country-label">
        <SelectValue TValue="string" Placeholder="Select country..." />
    </SelectTrigger>
    ...
</SelectRoot>
```

### With Styling

```razor
<SelectRoot TValue="string" @bind-Value="selected" class="select">
    <SelectTrigger TValue="string" class="select-trigger">
        <SelectValue TValue="string" Placeholder="Choose..." />
        <span class="select-chevron">▼</span>
    </SelectTrigger>
    <SelectPortal TValue="string">
        <SelectContent TValue="string" class="select-content" SideOffset="4">
            <SelectViewport TValue="string" class="select-viewport">
                <SelectItem TValue="string" Value="@("opt1")" Label="Option 1" class="select-item">
                    <span class="select-item-check">✓</span>
                    <SelectItemText>Option 1</SelectItemText>
                </SelectItem>
                <SelectItem TValue="string" Value="@("opt2")" Label="Option 2" class="select-item">
                    <span class="select-item-check">✓</span>
                    <SelectItemText>Option 2</SelectItemText>
                </SelectItem>
            </SelectViewport>
        </SelectContent>
    </SelectPortal>
</SelectRoot>
```

```css
.select-trigger {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 200px;
    padding: 8px 12px;
    background: white;
    border: 1px solid #ccc;
    border-radius: 6px;
    cursor: pointer;
}

.select-trigger[data-state="open"] {
    border-color: #0066cc;
    box-shadow: 0 0 0 2px rgba(0, 102, 204, 0.2);
}

.select-trigger[data-disabled] {
    opacity: 0.5;
    cursor: not-allowed;
}

.select-trigger[data-placeholder] {
    color: #999;
}

.select-chevron {
    font-size: 12px;
    color: #666;
}

.select-content {
    background: white;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    overflow: hidden;
}

.select-viewport {
    padding: 4px;
    max-height: 300px;
    overflow-y: auto;
}

.select-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 12px;
    border-radius: 4px;
    cursor: pointer;
}

.select-item[data-highlighted] {
    background: #f0f0f0;
}

.select-item[data-state="checked"] {
    background: #e6f0ff;
}

.select-item[data-disabled] {
    opacity: 0.5;
    cursor: not-allowed;
}

.select-item-check {
    visibility: hidden;
    color: #0066cc;
}

.select-item[data-state="checked"] .select-item-check {
    visibility: visible;
}

.select-group-label {
    padding: 8px 12px;
    font-size: 12px;
    font-weight: 600;
    color: #666;
    text-transform: uppercase;
}
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Enter` / `Space` | Open dropdown or select highlighted item |
| `ArrowDown` | Open dropdown or move to next item |
| `ArrowUp` | Move to previous item |
| `Home` | Move to first item |
| `End` | Move to last item |
| `Escape` | Close dropdown |
| `A-Z` / `a-z` | Typeahead - jump to matching item |

### Typeahead

Typing characters while the dropdown is open will jump to items that match the typed text. The search buffer resets after a short delay.

### ARIA Attributes

- Trigger has `role="combobox"`, `aria-haspopup="listbox"`, and `aria-expanded`
- Content has `role="listbox"`
- Items have `role="option"` with `aria-selected`
- Groups have `role="group"` with `aria-labelledby`
- Disabled items have `aria-disabled`

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"open"` / `"closed"` | Dropdown open state |
| `data-state` | `"checked"` / `"unchecked"` | Item selection state |
| `data-highlighted` | Present when focused | Item is keyboard-focused |
| `data-disabled` | Present when disabled | Item or trigger is disabled |
| `data-placeholder` | Present when no value | Showing placeholder text |
| `data-invalid` | Present when invalid | For form validation errors |

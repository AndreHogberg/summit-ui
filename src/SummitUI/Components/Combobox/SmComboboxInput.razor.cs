using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Text input for filtering combobox options.
/// Implements the combobox role when present (editable combobox pattern).
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public partial class SmComboboxInput<TValue> : ComponentBase, IAsyncDisposable where TValue : notnull
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Placeholder text when no value is entered.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// The id attribute for the input element.
    /// When provided, this allows association with a <c>&lt;label for="..."&gt;</c> element.
    /// If not provided, an auto-generated id from the combobox context is used.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;label for="country-combobox"&gt;Select a country&lt;/label&gt;
    /// &lt;SmComboboxInput Id="country-combobox" /&gt;
    /// </code>
    /// </example>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Direct aria-label for the input.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// ID of an external label element to associate with the input.
    /// </summary>
    [Parameter]
    public string? AriaLabelledBy { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the input.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private bool _isDisposed;
    private bool _isSubscribed;
    private string _inputValue = "";

    protected override void OnInitialized()
    {
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;

        // Register focus callback
        Context.FocusInputAsync = FocusAsync;
    }

    private async void HandleStateChanged()
    {
        // Sync input value with filter text
        if (_inputValue != Context.FilterText)
        {
            _inputValue = Context.FilterText;
        }
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterInput(_elementRef);
        }
    }

    private async Task HandleInputAsync(ChangeEventArgs args)
    {
        if (Context.Disabled) return;

        _inputValue = args.Value?.ToString() ?? "";
        await Context.SetFilterTextAsync(_inputValue);

        // Open popup when typing
        if (!Context.IsOpen && !string.IsNullOrEmpty(_inputValue))
        {
            await Context.OpenAsync();
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Context.Disabled) return;

        switch (args.Key)
        {
            case "ArrowDown":
                if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                else
                {
                    await HighlightNextItemAsync();
                }
                break;

            case "ArrowUp":
                if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                else
                {
                    await HighlightPreviousItemAsync();
                }
                break;

            case "Enter":
                if (Context.IsOpen && !string.IsNullOrEmpty(Context.HighlightedKey))
                {
                    await Context.ToggleItemByKeyAsync(Context.HighlightedKey);
                }
                else if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                break;

            case " ":
                // Space should type in input, not toggle - but if popup is open and we have a highlighted item,
                // we could toggle on Ctrl+Space (optional, not implemented here)
                break;

            case "Escape":
                if (Context.IsOpen)
                {
                    await Context.CloseAsync();
                }
                else if (!string.IsNullOrEmpty(_inputValue))
                {
                    // Clear input if popup is closed
                    _inputValue = "";
                    await Context.SetFilterTextAsync("");
                }
                break;

            case "Tab":
                // Close popup on Tab, let default behavior move focus
                if (Context.IsOpen)
                {
                    await Context.CloseAsync();
                }
                break;

            case "Home":
                if (Context.IsOpen && args.CtrlKey)
                {
                    await HighlightFirstItemAsync();
                }
                // Otherwise, let input handle cursor movement
                break;

            case "End":
                if (Context.IsOpen && args.CtrlKey)
                {
                    await HighlightLastItemAsync();
                }
                // Otherwise, let input handle cursor movement
                break;

            case "Backspace":
                // If input is empty and there are selected values, remove the last one
                if (string.IsNullOrEmpty(_inputValue) && Context.SelectedValues.Count > 0)
                {
                    var lastValue = Context.SelectedValues.LastOrDefault();
                    if (lastValue is not null)
                    {
                        await Context.DeselectValueAsync(lastValue);
                    }
                }
                break;
        }
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Context.Disabled) return;

        // Open popup when clicking in the input
        if (!Context.IsOpen)
        {
            await Context.OpenAsync();
        }
    }

    private async Task HighlightNextItemAsync()
    {
        var keys = GetVisibleItemKeys();
        if (keys.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedKey)
            ? -1
            : keys.IndexOf(Context.HighlightedKey);

        var nextIndex = currentIndex;
        var startIndex = currentIndex;
        do
        {
            nextIndex++;
            if (nextIndex >= keys.Count)
            {
                nextIndex = 0;
            }
            if (nextIndex == startIndex && startIndex != -1) return;
        } while (IsItemDisabled(keys[nextIndex]) && nextIndex != currentIndex);

        if (IsItemDisabled(keys[nextIndex])) return;

        await Context.SetHighlightedKeyAsync(keys[nextIndex]);
    }

    private async Task HighlightPreviousItemAsync()
    {
        var keys = GetVisibleItemKeys();
        if (keys.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedKey)
            ? keys.Count
            : keys.IndexOf(Context.HighlightedKey);

        var prevIndex = currentIndex;
        var startIndex = currentIndex;
        do
        {
            prevIndex--;
            if (prevIndex < 0)
            {
                prevIndex = keys.Count - 1;
            }
            if (prevIndex == startIndex && startIndex != keys.Count) return;
        } while (IsItemDisabled(keys[prevIndex]) && prevIndex != currentIndex);

        if (IsItemDisabled(keys[prevIndex])) return;

        await Context.SetHighlightedKeyAsync(keys[prevIndex]);
    }

    private async Task HighlightFirstItemAsync()
    {
        var keys = GetVisibleItemKeys();
        for (var i = 0; i < keys.Count; i++)
        {
            if (!IsItemDisabled(keys[i]))
            {
                await Context.SetHighlightedKeyAsync(keys[i]);
                return;
            }
        }
    }

    private async Task HighlightLastItemAsync()
    {
        var keys = GetVisibleItemKeys();
        for (var i = keys.Count - 1; i >= 0; i--)
        {
            if (!IsItemDisabled(keys[i]))
            {
                await Context.SetHighlightedKeyAsync(keys[i]);
                return;
            }
        }
    }

    private List<string> GetVisibleItemKeys()
    {
        return Context.ItemRegistry.Keys
            .Where(key => Context.MatchesFilter(key))
            .ToList();
    }

    private bool IsItemDisabled(string key)
    {
        return Context.DisabledRegistry.TryGetValue(key, out var disabled) && disabled;
    }

    private async Task FocusAsync()
    {
        try
        {
            await FloatingInterop.FocusElementAsync(_elementRef);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";

    private string? HighlightedItemId =>
        !string.IsNullOrEmpty(Context.HighlightedKey)
            ? Context.GetItemId(Context.HighlightedKey)
            : null;

    public ValueTask DisposeAsync()
    {
        if (_isDisposed) return ValueTask.CompletedTask;
        _isDisposed = true;

        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        return ValueTask.CompletedTask;
    }
}

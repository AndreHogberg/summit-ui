using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Cascading context shared between DatePicker sub-components.
/// Coordinates state between DateField, Calendar, and Popover.
/// </summary>
public sealed class DatePickerContext
{
    /// <summary>
    /// Unique identifier for this DatePicker instance, used for ARIA relationships.
    /// </summary>
    public string Id { get; } = Identifier.NewId();

    /// <summary>
    /// The PopoverContext for managing open/close state.
    /// </summary>
    public PopoverContext PopoverContext { get; } = new();

    /// <summary>
    /// Reference to the Field element, used as anchor for popover positioning.
    /// </summary>
    public ElementReference FieldElement { get; internal set; }

    /// <summary>
    /// Current open state of the date picker popover.
    /// </summary>
    public bool IsOpen => PopoverContext.IsOpen;

    /// <summary>
    /// Whether to close the popover when a date is selected from the calendar.
    /// </summary>
    public bool CloseOnSelect { get; internal set; } = true;

    /// <summary>
    /// Whether the date picker is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Whether the date picker is read-only.
    /// </summary>
    public bool ReadOnly { get; internal set; }

    /// <summary>
    /// Callback to toggle the popover state.
    /// </summary>
    public Func<Task> ToggleAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly open the popover.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the popover.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Action to register the field element reference (used for popover anchoring).
    /// </summary>
    public Action<ElementReference> RegisterField { get; internal set; } = _ => { };

    /// <summary>
    /// Callback invoked when a date is selected from the calendar.
    /// This triggers value sync and optional auto-close.
    /// </summary>
    public Func<DateOnly, Task> OnCalendarDateSelectedAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to notify state changes for re-rendering.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Event raised when the context state changes.
    /// Child components can subscribe to this to trigger re-renders.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Raises the OnStateChanged event to notify all subscribers.
    /// </summary>
    internal void RaiseStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}

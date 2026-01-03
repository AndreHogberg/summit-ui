namespace SummitUI;

/// <summary>
/// Cascading context shared between CheckboxGroup and its child CheckboxRoot components.
/// Manages the collective state of checkboxes within a group.
/// </summary>
public sealed class CheckboxGroupContext
{
    /// <summary>
    /// Unique identifier for this checkbox group, used for ARIA relationships.
    /// </summary>
    public string GroupId { get; }

    /// <summary>
    /// The form name for hidden inputs. When set, hidden inputs are rendered for form submission.
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    /// The set of currently checked values in the group.
    /// </summary>
    public HashSet<string> Values { get; internal set; } = [];

    /// <summary>
    /// Whether the entire group is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Callback to toggle a checkbox value within the group.
    /// </summary>
    public Func<string, Task> ToggleValueAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to notify the group of state changes.
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

    /// <summary>
    /// Creates a new checkbox group context.
    /// </summary>
    public CheckboxGroupContext()
    {
        GroupId = $"ark-checkbox-group-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Checks if a value is currently checked in the group.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is checked, false otherwise.</returns>
    public bool IsChecked(string value) => Values.Contains(value);
}

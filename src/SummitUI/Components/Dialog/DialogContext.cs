using Microsoft.AspNetCore.Components;

using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Context that provides shared state and communication between Dialog sub-components.
/// Supports nested dialogs with depth tracking and CSS custom property support.
/// </summary>
public sealed class DialogContext : OpenCloseContextBase
{
    /// <summary>
    /// Creates a new dialog context with a unique ID.
    /// </summary>
    public DialogContext() : base("dialog")
    {
    }

    /// <summary>
    /// Unique identifier for the dialog, used for ARIA relationships.
    /// </summary>
    public string DialogId => ComponentId;

    /// <summary>
    /// Whether the dialog is currently animating closed.
    /// Used to keep content in DOM during close animations.
    /// True if either content or overlay is still animating.
    /// </summary>
    public new bool IsAnimatingClosed => IsContentAnimatingClosed || IsOverlayAnimatingClosed;

    /// <summary>
    /// Whether the dialog content is currently animating closed.
    /// </summary>
    public bool IsContentAnimatingClosed { get; set; }

    /// <summary>
    /// Whether the dialog overlay is currently animating closed.
    /// </summary>
    public bool IsOverlayAnimatingClosed { get; set; }

    /// <summary>
    /// Reference to the parent dialog context if this dialog is nested.
    /// </summary>
    public DialogContext? ParentDialog { get; internal set; }

    /// <summary>
    /// The nesting depth of this dialog (0 for root, 1 for first nested, etc.).
    /// Exposed as CSS custom property --summit-dialog-depth.
    /// </summary>
    public int Depth { get; internal set; }

    /// <summary>
    /// The number of currently open nested dialogs within this dialog.
    /// Updates reactively as nested dialogs open and close.
    /// Exposed as CSS custom property --summit-dialog-nested-count.
    /// </summary>
    public int NestedOpenCount { get; internal set; }

    /// <summary>
    /// Whether this dialog is a nested dialog (has a parent).
    /// </summary>
    public bool IsNested => ParentDialog is not null;

    /// <summary>
    /// Whether this dialog has any open nested dialogs.
    /// </summary>
    public bool HasNestedOpen => NestedOpenCount > 0;

    /// <summary>
    /// Reference to the trigger element that opened this dialog.
    /// Used for returning focus on close.
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the dialog content element.
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Registers the trigger element reference.
    /// </summary>
    public Action<ElementReference> RegisterTrigger { get; internal set; } = _ => { };

    /// <summary>
    /// Registers the content element reference.
    /// </summary>
    public Action<ElementReference> RegisterContent { get; internal set; } = _ => { };

    /// <summary>
    /// Called by child dialogs when they open to increment the nested count.
    /// </summary>
    internal Action IncrementNestedCount { get; set; } = () => { };

    /// <summary>
    /// Called by child dialogs when they close to decrement the nested count.
    /// </summary>
    internal Action DecrementNestedCount { get; set; } = () => { };

    /// <summary>
    /// Gets the ID for the dialog title element (used for aria-labelledby).
    /// </summary>
    public string TitleId => GetElementId("title");

    /// <summary>
    /// ID for the description element (used for aria-describedby).
    /// </summary>
    public string DescriptionId => GetElementId("description");
}

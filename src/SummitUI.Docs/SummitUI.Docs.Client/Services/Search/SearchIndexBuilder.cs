namespace SummitUI.Docs.Client.Services.Search;

/// <summary>
/// Builds and populates the search index with documentation content.
/// </summary>
public static class SearchIndexBuilder
{
    /// <summary>
    /// Populates the search service with all documentation pages.
    /// </summary>
    public static void BuildIndex(SearchService searchService)
    {
        searchService.Clear();
        searchService.AddDocuments(GetAllDocuments());
    }

    private static IEnumerable<SearchDocument> GetAllDocuments()
    {
        // Getting Started
        yield return new SearchDocument
        {
            Url = "/docs",
            Title = "Introduction",
            Description = "Get started with SummitUI, a Blazor component library focused on WCAG-compliant, fully customizable headless components.",
            Category = "Getting Started",
            Keywords = ["getting started", "overview", "blazor", "components", "headless", "wcag", "accessibility"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/installation",
            Title = "Installation",
            Description = "Install SummitUI via NuGet and configure your Blazor project.",
            Category = "Getting Started",
            Keywords = ["nuget", "install", "setup", "dotnet", "package", "configure"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/dependencies",
            Title = "Dependencies",
            Description = "Learn about SummitUI's dependencies and optional packages.",
            Category = "Getting Started",
            Keywords = ["dependencies", "packages", "requirements", "nuget"]
        };

        // AI & Agents
        yield return new SearchDocument
        {
            Url = "/docs/llms",
            Title = "llms.txt",
            Description = "Machine-readable documentation for AI assistants and LLMs.",
            Category = "AI & Agents",
            Keywords = ["llm", "ai", "agents", "machine readable", "claude", "gpt", "copilot"]
        };

        // Guides
        yield return new SearchDocument
        {
            Url = "/docs/guides/datepicker-composition",
            Title = "Datepicker Composition",
            Description = "Build a complete datepicker by composing Calendar, DateField, and Popover components.",
            Category = "Guides",
            Keywords = ["datepicker", "date picker", "calendar", "popover", "composition", "date input", "date selection"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/guides/drawer-composition",
            Title = "Drawer Composition",
            Description = "Create a drawer/sheet component using Dialog primitives.",
            Category = "Guides",
            Keywords = ["drawer", "sheet", "slide", "panel", "sidebar", "dialog", "composition"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/guides/toast-composition",
            Title = "Toast Composition",
            Description = "Build a toast notification system with the Toast component.",
            Category = "Guides",
            Keywords = ["toast", "notification", "alert", "message", "snackbar", "composition"]
        };

        // Components
        yield return new SearchDocument
        {
            Url = "/docs/accordion",
            Title = "Accordion",
            Description = "A vertically stacked set of interactive headings that each reveal an associated section of content.",
            Category = "Components",
            Keywords = ["accordion", "collapse", "expand", "collapsible", "disclosure", "faq", "expandable"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/checkbox",
            Title = "Checkbox",
            Description = "A control that allows the user to toggle between checked and not checked.",
            Category = "Components",
            Keywords = ["checkbox", "check", "toggle", "boolean", "form", "input", "indeterminate"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/calendar",
            Title = "Calendar",
            Description = "A date selection calendar supporting single, multiple, and range selection modes.",
            Category = "Components",
            Keywords = ["calendar", "date", "datepicker", "date picker", "month", "year", "selection", "range"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/datefield",
            Title = "DateField",
            Description = "A segmented date input field with keyboard navigation and locale support.",
            Category = "Components",
            Keywords = ["datefield", "date field", "date input", "date", "input", "segment", "locale", "format"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/dialog",
            Title = "Dialog",
            Description = "A modal dialog window that overlays the main content and requires user interaction.",
            Category = "Components",
            Keywords = ["dialog", "modal", "popup", "overlay", "window", "lightbox"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/alert-dialog",
            Title = "Alert Dialog",
            Description = "A modal dialog for important confirmations that interrupts the user's workflow.",
            Category = "Components",
            Keywords = ["alert", "dialog", "confirm", "confirmation", "modal", "warning", "destructive"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/dropdown-menu",
            Title = "Dropdown Menu",
            Description = "A menu of actions or options triggered by a button, with keyboard navigation and typeahead.",
            Category = "Components",
            Keywords = ["dropdown", "menu", "context menu", "actions", "options", "popover", "typeahead"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/otp",
            Title = "OTP Input",
            Description = "A one-time password input component with automatic focus management and paste support.",
            Category = "Components",
            Keywords = ["otp", "one time password", "pin", "verification", "code", "2fa", "mfa", "authentication"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/popover",
            Title = "Popover",
            Description = "A floating panel anchored to a trigger element with positioning and focus management.",
            Category = "Components",
            Keywords = ["popover", "popup", "tooltip", "floating", "overlay", "anchor", "dropdown"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/radiogroup",
            Title = "RadioGroup",
            Description = "A set of radio buttons where only one can be selected at a time.",
            Category = "Components",
            Keywords = ["radio", "radiogroup", "radio group", "option", "single select", "form", "input"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/select",
            Title = "Select",
            Description = "A dropdown component for selecting a value from a list of options.",
            Category = "Components",
            Keywords = ["select", "dropdown", "combobox", "listbox", "picker", "option", "choice"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/separator",
            Title = "Separator",
            Description = "A visual divider between content sections, horizontal or vertical.",
            Category = "Components",
            Keywords = ["separator", "divider", "hr", "line", "horizontal rule", "vertical"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/switch",
            Title = "Switch",
            Description = "A toggle control that switches between on and off states.",
            Category = "Components",
            Keywords = ["switch", "toggle", "boolean", "on off", "form", "input", "slider"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/tabs",
            Title = "Tabs",
            Description = "A set of content panels, only one of which is visible at a time.",
            Category = "Components",
            Keywords = ["tabs", "tab", "panel", "navigation", "content switch", "tablist"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/toast",
            Title = "Toast",
            Description = "A brief, auto-dismissing message that appears at the edge of the screen.",
            Category = "Components",
            Keywords = ["toast", "notification", "alert", "message", "snackbar", "flash"]
        };

        // Utilities
        yield return new SearchDocument
        {
            Url = "/docs/focustrap",
            Title = "Focus Trap",
            Description = "Traps keyboard focus within a container, essential for modal dialogs and popovers.",
            Category = "Utilities",
            Keywords = ["focus", "trap", "focus trap", "keyboard", "accessibility", "modal", "a11y"]
        };

        yield return new SearchDocument
        {
            Url = "/docs/mediaquery",
            Title = "Media Query",
            Description = "A utility for responding to CSS media query changes in Blazor components.",
            Category = "Utilities",
            Keywords = ["media query", "responsive", "breakpoint", "screen size", "mobile", "desktop", "css"]
        };
    }
}

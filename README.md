# SummitUI

A headless Blazor component library focused on accessibility and customization. SummitUI provides unstyled, WCAG-compliant components that give you full control over your application's design while handling complex behaviors and accessibility requirements.

> **Note:** This is an alpha release. APIs may change before version 1.0.

Inspired by [bits-ui](https://github.com/huntabyte/bits-ui).

## Features

- **Headless:** Logic and behavior without opinionated styling.
- **Accessible:** Built with WCAG compliance in mind, including ARIA attributes and keyboard navigation.
- **Render Modes:** Supports WebAssembly (WASM), Server-Side Rendering (SSR), and Interactive Server.
- **Customizable:** Fully customizable via standard Blazor attributes.

## Installation

Install the package via NuGet:

```bash
dotnet add package SummitUI --version 0.0.3-alpha
```

## Setup

1. Add the SummitUI services to your dependency injection container in `Program.cs`:

```csharp
using SummitUI.Extensions;

builder.Services.AddSummitUI();
```

2. Add the global using directive in your `_Imports.razor` file:

```razor
@using SummitUI
```

3. (Optional) Add the script reference in your `App.razor` (or `index.html` for WASM) if you are using components that require JavaScript interop (like `FocusTrap` or floating UI components). Note that many components load their scripts lazily, but including the main script can ensure early availability.

```html
<script type="module" src="@Assets["_content/SummitUI/summitui.js"]" ></script>
```

## Usage Example

Here is a simple example of how to use the `Accordion` component:

```razor
<AccordionRoot DefaultValue="item-1">
    <AccordionItem Value="item-1">
        <AccordionHeader>
            <AccordionTrigger class="accordion-trigger">
                Is it accessible?
            </AccordionTrigger>
        </AccordionHeader>
        <AccordionContent class="accordion-content">
            Yes. It adheres to the WAI-ARIA design pattern.
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

## Testing tools
NVDA and VoiceOver

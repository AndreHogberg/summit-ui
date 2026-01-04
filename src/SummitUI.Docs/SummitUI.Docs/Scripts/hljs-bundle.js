// Highlight.js bundle with required languages
import hljs from 'highlight.js/lib/core';
import csharp from 'highlight.js/lib/languages/csharp';
import xml from 'highlight.js/lib/languages/xml';
import css from 'highlight.js/lib/languages/css';
import cshtmlRazor from 'highlightjs-cshtml-razor';

// Register languages
hljs.registerLanguage('csharp', csharp);
hljs.registerLanguage('cs', csharp);
hljs.registerLanguage('xml', xml);
hljs.registerLanguage('html', xml);
hljs.registerLanguage('css', css);
hljs.registerLanguage('cshtml', cshtmlRazor);
hljs.registerLanguage('razor', cshtmlRazor);

// Expose globally for the inline script in App.razor
window.hljs = hljs;

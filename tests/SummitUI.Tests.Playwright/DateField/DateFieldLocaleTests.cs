namespace SummitUI.Tests.Playwright.DateField;

/// <summary>
/// Tests for locale-aware placeholder text in DateField.
/// Verifies that placeholders are correctly localized for different locales.
/// </summary>
public class DateFieldLocaleTests : SummitTestBase
{
    protected override string TestPagePath => "tests/date-field/locale";

    [Test]
    public async Task SwedishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("swedish-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("åååå");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task GermanLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("german-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("jjjj");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("tt");
    }

    [Test]
    public async Task FrenchLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("french-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("aaaa");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("jj");
    }

    [Test]
    public async Task NorwegianLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("norwegian-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("åååå");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task DanishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("danish-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("åååå");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task DutchLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("dutch-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("jjjj");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task SpanishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("spanish-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("aaaa");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task ItalianLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("italian-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("aaaa");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("gg");
    }

    [Test]
    public async Task PortugueseLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("portuguese-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("aaaa");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task PolishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("polish-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("rrrr");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task RussianLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("russian-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Russian uses Cyrillic characters
        await Expect(yearSegment).ToHaveTextAsync("гггг");
        await Expect(monthSegment).ToHaveTextAsync("мм");
        await Expect(daySegment).ToHaveTextAsync("дд");
    }

    [Test]
    public async Task FinnishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("finnish-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("vvvv");
        await Expect(monthSegment).ToHaveTextAsync("kk");
        await Expect(daySegment).ToHaveTextAsync("pp");
    }

    [Test]
    public async Task CzechLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("czech-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("rrrr");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task HungarianLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("hungarian-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("éééé");
        await Expect(monthSegment).ToHaveTextAsync("hh");
        await Expect(daySegment).ToHaveTextAsync("nn");
    }

    [Test]
    public async Task GreekLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("greek-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Greek uses Greek characters
        await Expect(yearSegment).ToHaveTextAsync("εεεε");
        await Expect(monthSegment).ToHaveTextAsync("μμ");
        await Expect(daySegment).ToHaveTextAsync("ηη");
    }

    [Test]
    public async Task JapaneseLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("japanese-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Japanese uses single kanji for each segment
        await Expect(yearSegment).ToHaveTextAsync("年");
        await Expect(monthSegment).ToHaveTextAsync("月");
        await Expect(daySegment).ToHaveTextAsync("日");
    }

    [Test]
    public async Task KoreanLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("korean-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Korean uses Korean words
        await Expect(yearSegment).ToHaveTextAsync("연도");
        await Expect(monthSegment).ToHaveTextAsync("월");
        await Expect(daySegment).ToHaveTextAsync("일");
    }

    [Test]
    public async Task ChineseLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("chinese-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Chinese uses single hanzi for each segment
        await Expect(yearSegment).ToHaveTextAsync("年");
        await Expect(monthSegment).ToHaveTextAsync("月");
        await Expect(daySegment).ToHaveTextAsync("日");
    }

    [Test]
    public async Task ArabicLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("arabic-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Arabic uses Arabic words
        await Expect(yearSegment).ToHaveTextAsync("سنة");
        await Expect(monthSegment).ToHaveTextAsync("شهر");
        await Expect(daySegment).ToHaveTextAsync("يوم");
    }

    [Test]
    public async Task TurkishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("turkish-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("yyyy");
        await Expect(monthSegment).ToHaveTextAsync("aa");
        await Expect(daySegment).ToHaveTextAsync("gg");
    }

    [Test]
    public async Task EnglishLocale_ShouldShow_LocalizedPlaceholders()
    {
        var section = Page.GetByTestId("english-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        await Expect(yearSegment).ToHaveTextAsync("yyyy");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task UnknownLocale_ShouldFallbackTo_EnglishPlaceholders()
    {
        var section = Page.GetByTestId("unknown-locale-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // Unknown locale should fall back to English
        await Expect(yearSegment).ToHaveTextAsync("yyyy");
        await Expect(monthSegment).ToHaveTextAsync("mm");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }
}

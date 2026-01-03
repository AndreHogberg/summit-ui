namespace SummitUI;

public static class Identifier
{
    public static string NewId() => "summit-" + Guid.NewGuid().ToString("N")[..8];
}

public static class StringExtensions
{
    public static int? ToSeed(this string seedString)
    {
        if (string.IsNullOrEmpty(seedString))
        {
            return null;
        }
        return int.TryParse(seedString, out var seed) ? seed : seedString.GetHashCode();
    }
}

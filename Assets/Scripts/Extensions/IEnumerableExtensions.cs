using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions
{
    public static void SortInHierarchy(this IEnumerable<Counter> counters)
    {
        foreach (var counter in counters.OrderByDescending(x => x.Count))
        {
            counter.transform.SetAsLastSibling();
        }
    }
}

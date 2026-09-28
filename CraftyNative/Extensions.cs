using System.Runtime.InteropServices;

namespace CraftyNative;

public static class Extensions
{
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }
}

// See https://aka.ms/new-console-template for more information

using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

var c = new IsComplex() { Seed = 1 };

var summary = BenchmarkRunner.Run<IsComplex>();

public class IsComplex
{
    [Params(1, 2, 3)]
    public int Seed;

    private readonly byte[] data;
    private readonly string final;

    public IsComplex(string? str = null)
    {
        if (str == null)
        {
            var rng = new Random(Seed);

            var segments = new List<string>();
            var count = rng.Next(5, 25);
            for (var j = 0; j < count; j++)
            {
                if (rng.NextDouble() < 0.05)
                {
                    if (rng.NextDouble() < 0.5)
                    {
                        segments.Add("..");
                    }
                    else
                    {
                        segments.Add(".");
                    }
                }
                else
                {
                    segments.Add(Path.GetRandomFileName());
                }
            }
            str = string.Join("/", segments);
        }

        final = str;
        data = Encoding.UTF8.GetBytes(final);
    }

    [Benchmark(Baseline = true)]
    public bool Tom()
    {
        return IsComplexTom();
    }

    [Benchmark]
    public bool Modified()
    {
        return IsComplexModified();
    }

    public bool IsComplexTom()
    {
        var remaining = (ReadOnlySpan<byte>)data.AsSpan();

        do
        {
            remaining.Split((byte)'/', out var segment, out remaining);

            if (segment.Length != 1 && segment.Length != 2)
                continue;

            var count = 0;
            for (int i = 0; i < segment.Length; i++)
            {
                if (segment[i] == (byte)'.')
                    count++;
                else
                    break;
            }

            if (count == segment.Length)
                return true;
        } while (remaining.Length > 0);

        return false;
    }

    public bool IsComplexModified()
    {
        var remaining = (ReadOnlySpan<byte>)data.AsSpan();

        do
        {
            remaining.Split((byte)'/', out var segment, out remaining);

            switch (segment.Length)
            {
                case 1 when segment[0] == '.':
                    return true;

                case 2 when segment == ".."u8:
                    return true;
            }
        } while (remaining.Length > 0);

        return false;
    }

    [Benchmark]
    public bool LoopSpanSearch()
    {
        var str = data.AsSpan();

        if (str.EndsWith("/.."u8))
            return true;
        if (str.EndsWith("/."u8))
            return true;

        if (str.StartsWith("../"u8))
            return true;
        if (str.StartsWith("./"u8))
            return true;

        var remaining = (ReadOnlySpan<byte>)data.AsSpan();
        do
        {
            var idx = remaining.IndexOf("/."u8);

            if (idx == -1)
                return false;

            remaining = remaining[(idx + 2)..];

            if (remaining.StartsWith("/"u8) || remaining.StartsWith("./"u8))
                return true;

        } while (remaining.Length > 0);

        return false;
    }

    [Benchmark]
    public bool SpanSearch()
    {
        var str = data.AsSpan();

        if (str.EndsWith("/.."u8))
            return true;
        if (str.EndsWith("/."u8))
            return true;

        if (str.StartsWith("../"u8))
            return true;
        if (str.StartsWith("./"u8))
            return true;

        if (str.IndexOf("/../"u8) != -1)
            return true;
        if (str.IndexOf("/./"u8) != -1)
            return true;

        return false;
    }
}

internal static class SpanExtensions
{
    /// <summary>
    /// Split a span around the first item which is equal to `split`
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span">Span to split</param>
    /// <param name="split">Item to split around</param>
    /// <param name="left">All items before `split`</param>
    /// <param name="right">All items after `split`</param>
    public static void Split<T>(this ReadOnlySpan<T> span, T split, out ReadOnlySpan<T> left, out ReadOnlySpan<T> right)
        where T : IEquatable<T>
    {
        var idx = span.IndexOf(split);

        if (idx < 0)
        {
            left = span;
            right = left[..0];
        }
        else
        {
            left = span[..idx];
            right = span[(idx + 1)..];
        }
    }

    /// <summary>
    /// Split a span around the last item which is equal to `split`
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span">Span to split</param>
    /// <param name="split">Item to split around</param>
    /// <param name="left">All items before `split`</param>
    /// <param name="right">All items after `split`</param>
    public static void SplitLast<T>(this ReadOnlySpan<T> span, T split, out ReadOnlySpan<T> left, out ReadOnlySpan<T> right)
        where T : IEquatable<T>
    {
        var idx = span.LastIndexOf(split);

        if (idx < 0)
        {
            left = span;
            right = left[..0];
        }
        else
        {
            left = span[..idx];
            right = span[(idx + 1)..];
        }
    }
}
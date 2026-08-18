using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDQB;

public static class GridExtensions
{
    public static IReadOnlyGrid<T> Crop<T>(this IReadOnlyGrid<T> grid, Rect bounds)
    {
        return new CroppedReadOnlyGrid<T>()
        {
            Grid = grid,
            Bounds = bounds.Intersection(grid.Bounds),
            Translation = XZ.Zero
        };
    }

    public static IGrid<T> Crop<T>(this IGrid<T> grid, Rect bounds)
    {
        return new CroppedGrid<T>()
        {
            Grid = grid,
            Bounds = bounds.Intersection(grid.Bounds),
            Translation = XZ.Zero
        };
    }

    public static IReadOnlyGrid<T> TranslateTo<T>(this IReadOnlyGrid<T> grid, XZ newTopLeft)
    {
        var translation = grid.Bounds.Start.Subtract(newTopLeft);

        return new CroppedReadOnlyGrid<T>
        {
            Grid = grid,
            Bounds = new Rect(grid.Bounds.Start.Subtract(translation), grid.Bounds.End.Subtract(translation)),
            Translation = translation,
        };
    }

    public static void CopyFrom<T>(this IGrid<T> dest, IReadOnlyGrid<T> source)
    {
        if (!dest.Bounds.Covers(source.Bounds))
        {
            throw new ArgumentException("TODO");
        }
        foreach (var xz in source.Bounds.Enumerate())
        {
            dest.Set(xz, source.Get(xz));
        }
    }

    sealed class CroppedReadOnlyGrid<T> : IReadOnlyGrid<T>
    {
        public required Rect Bounds { get; init; }
        public required IReadOnlyGrid<T> Grid { get; init; }
        public required XZ Translation { get; init; }

        public T Get(XZ xz) => Grid.Get(xz.Add(Translation));
    }

    sealed class CroppedGrid<T> : IGrid<T>
    {
        public required Rect Bounds { get; init; }
        public required IGrid<T> Grid { get; init; }
        public required XZ Translation { get; init; }

        public T Get(XZ xz) => Grid.Get(xz.Add(Translation));
        public void Set(XZ xz, T value) => Grid.Set(xz.Add(Translation), value);
    }
}

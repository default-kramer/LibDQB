using System.Diagnostics.CodeAnalysis;

namespace LibDQB.DQB2Minimap;

/// <summary>
/// Stores a grid of MinimapTiles in a format that works with the Windows Clipboard.
/// See <see cref="ToClipboardObject"/> and <see cref="FromClipboardObject"/>.
/// </summary>
public sealed class MinimapClipboardData : IReadOnlyGrid<MinimapTile?>
{
    public const string Format = "DQB2MinimapTiles";

    const int TileValueMask = 0xFFFF;
    const int HasValueBit = TileValueMask + 1;
    const int NullTileValue = 0;

    // Tack the Width and Height on to the end of the array so that the whole thing
    // is captured in a single array of integers.
    // (It seems that `int[]` is the best type we can use that "just works" with the clipboard.)
    static int WidthIndex(int[] data) => data.Length - 2;
    static int HeightIndex(int[] data) => data.Length - 1;
    const int ExtraArraySize = 2;

    private readonly int[] data;
    public int Width { get; }
    public int Height { get; }

    private MinimapClipboardData(int[] data, out bool isValid)
    {
        this.data = data;
        if (data.Length >= ExtraArraySize)
        {
            Width = data[WidthIndex(data)];
            Height = data[HeightIndex(data)];
            isValid = Width >= 0
                && Height >= 0
                && (data.Length - ExtraArraySize) == Width * Height;
        }
        else
        {
            isValid = false;
        }
    }

    public object ToClipboardObject() => data;
    public static bool FromClipboardObject(object? o, [NotNullWhen(true)] out MinimapClipboardData? data)
    {
        if (o is int[] arr)
        {
            data = new MinimapClipboardData(arr, out bool isValid);
            if (!isValid)
            {
                data = null;
            }
            return isValid;
        }
        data = null;
        return false;
    }

    public Rect Bounds => new Rect(XZ.Zero, new XZ(Width, Height));

    public MinimapTile? Get(XZ xz)
    {
        var index = Bounds.GetIndex(xz);
        if (index.HasValue)
        {
            int val = data[index.Value];
            if (val != NullTileValue)
            {
                return MinimapTile.FromRawValue(val & TileValueMask);
            }
        }
        return null;
    }

    public static MinimapClipboardData Create(IReadOnlyGrid<MinimapTile> grid)
        => Create(grid, tile => tile);

    public static MinimapClipboardData Create(IReadOnlyGrid<MinimapTile?> grid)
        => Create(grid, tile => tile);

    private static MinimapClipboardData Create<T>(IReadOnlyGrid<T> grid, Func<T, MinimapTile?> projector)
    {
        grid = grid.TranslateTo(XZ.Zero);
        int width = grid.Bounds.Size.X;
        int height = grid.Bounds.Size.Z;
        var data = new int[width * height + ExtraArraySize];

        foreach (var xz in grid.Bounds.Enumerate())
        {
            int val;
            var tile = projector(grid.Get(xz));
            if (tile.HasValue)
            {
                val = HasValueBit | (TileValueMask & tile.Value.TileValue);
            }
            else
            {
                val = NullTileValue;
            }
            data[grid.Bounds.GetIndex(xz)!.Value] = val;
        }

        data[WidthIndex(data)] = width;
        data[HeightIndex(data)] = height;

        var retval = new MinimapClipboardData(data, out bool isValid);
        if (!isValid)
        {
            throw new Exception("Assert fail");
        }
        return retval;
    }
}

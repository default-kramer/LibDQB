using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDQB;

/// <summary>
/// Start is inclusive, End is exclusive.
/// (Similar to an array whose first element is at 0 and final element is at Size-1)
/// </summary>
public sealed record Rect(XZ Start, XZ End)
{
    public static readonly Rect Zero = new(new XZ(0, 0), new XZ(0, 0));
    public bool IsZero => Size.X < 1 || Size.Z < 1;

    public XZ Size => new XZ(End.X - Start.X, End.Z - Start.Z);

    public bool Contains(XZ xz) => GetIndex(xz).HasValue;

    public Rect Intersection(Rect other)
    {
        int xStart = Math.Max(this.Start.X, other.Start.X);
        int zStart = Math.Max(this.Start.Z, other.Start.Z);
        int xEnd = Math.Min(this.End.X, other.End.X);
        int zEnd = Math.Min(this.End.Z, other.End.Z);
        return new Rect(new XZ(xStart, zStart), new XZ(xEnd, zEnd));
    }

    /// <summary>
    /// If the given <paramref name="xz"/> is within this box, returns a unique index
    /// for that xz in the inclusive range 0 .. (Width*Height - 1).
    /// </summary>
    public int? GetIndex(XZ xz)
    {
        if (xz.X >= End.X || xz.Z >= End.Z)
        {
            return null;
        }

        int zIndex = xz.Z - Start.Z;
        if (zIndex < 0)
        {
            return null;
        }

        int xIndex = xz.X - Start.X;
        if (xIndex < 0)
        {
            return null;
        }

        int width = End.X - Start.X;
        return zIndex * width + xIndex;
    }

    public bool Covers(Rect other)
    {
        return this.Start.X <= other.Start.X
            && this.Start.Z <= other.Start.Z
            && this.End.X >= other.End.X
            && this.End.Z >= other.End.Z;
    }

    public IEnumerable<XZ> Enumerate()
    {
        for (int z = Start.Z; z < End.Z; z++)
        {
            for (int x = Start.X; x < End.X; x++)
            {
                yield return new XZ(x, z);
            }
        }
    }

    public static Rect GetBounds(IEnumerable<XZ> xzs)
    {
        var rect = GetBoundsOrNull(xzs);
        if (rect == null)
        {
            throw new ArgumentException("Sequence contains no elements");
        }
        return rect;
    }

    public static Rect? GetBoundsOrNull(IEnumerable<XZ> xzs)
    {
        using var enumerator = xzs.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return null;
        }

        var finder = new BoundsFinder();
        finder.Include(enumerator.Current);
        while (enumerator.MoveNext())
        {
            finder.Include(enumerator.Current);
        }

        return finder.CurrentBounds() ?? throw new Exception("Assert fail");
    }

    public sealed class BoundsFinder
    {
        private int xMin = int.MaxValue;
        private int zMin = int.MaxValue;
        private int xMax = int.MinValue;
        private int zMax = int.MinValue;

        public BoundsFinder Include(XZ xz)
        {
            xMin = Math.Min(xMin, xz.X);
            zMin = Math.Min(zMin, xz.Z);
            xMax = Math.Max(xMax, xz.X);
            zMax = Math.Max(zMax, xz.Z);
            return this;
        }

        public BoundsFinder IncludeAll(IEnumerable<XZ> xzs)
        {
            foreach (var xz in xzs) { Include(xz); }
            return this;
        }

        public Rect? CurrentBounds()
        {
            if (xMax >= xMin && zMax >= zMin)
            {
                return new Rect(new XZ(xMin, zMin), new XZ(xMax + 1, zMax + 1));
            }
            return null;
        }
    }

    public XZ ApproximateCenter()
    {
        if (IsZero)
        {
            throw new InvalidOperationException("not defined for Zero rect");
        }
        int x = Start.X + (End.X - Start.X) / 2;
        int z = Start.Z + (End.Z - Start.Z) / 2;
        return new XZ(x, z);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDQB;

public sealed class ConstantGrid<T> : IReadOnlyGrid<T>
{
    public required Rect Bounds { get; init; }
    public required T Value { get; init; }

    public T Get(XZ xz) => Value;
}

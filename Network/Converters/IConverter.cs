
using System;

namespace GOILauncher.Multiplayer.Network.Converters
{
    public interface IConverter<TSource, TDestination>
    {
        Type Type { get; }
        TDestination Convert(TSource source);
    }
}

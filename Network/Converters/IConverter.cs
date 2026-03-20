
using System;

namespace GOILauncher.Multiplayer.Network.Converters
{

    public interface IConverter<TSource, TDestination>
    {
        TDestination Convert(TSource source);
    }
}

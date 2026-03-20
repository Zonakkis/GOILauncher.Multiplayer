namespace GOILauncher.Multiplayer.Network.Converters
{
    public interface INetworkConverter
    {
        TDestination Convert<TSource, TDestination>(TSource source);
    }
}
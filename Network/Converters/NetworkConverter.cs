using Autofac;

namespace GOILauncher.Multiplayer.Network.Converters
{
    public class NetworkConverter : INetworkConverter
    {
        private readonly IComponentContext _context;

        public NetworkConverter(IComponentContext context) 
        {
            _context = context;
        }

        public TDestination Convert<TSource, TDestination>(TSource source)
        {
            var converter = _context.Resolve<IConverter<TSource,TDestination>>();
            return converter.Convert(source);
        }
    }
}

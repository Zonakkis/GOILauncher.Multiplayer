using System.Threading;

namespace GOILauncher.Multiplayer.Shared.Utilities
{
    public class IdGenerator
    {
        private int _currentId;
        public IdGenerator(int start)
        {
            _currentId = start;
        }

        public int Generate()
        {
            return Interlocked.Increment(ref _currentId) - 1;
        }
    }
}

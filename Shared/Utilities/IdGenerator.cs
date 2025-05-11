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
            return _currentId++;
        }
    }
}

using System;
using System.Text;

namespace GOILauncher.Multiplayer.Core.Data
{
    public class ArraySegmentReader
    {
        private ArraySegment<byte> _data;
        private int _index;

        public void Begin(ArraySegment<byte> data)
        {
            _data = data;
            _index = 0;
        }

        public void End()
        {
            _data = default;
            _index = 0;
        }

        public byte ReadByte()
        {
            if (_index >= _data.Count)
                throw new IndexOutOfRangeException("No more data to read.");

            return _data.Array[_data.Offset + _index++];
        }

        public int ReadInt()
        {
            if (_index + 4 > _data.Count)
                throw new IndexOutOfRangeException("No more data to read.");

            int value = BitConverter.ToInt32(_data.Array, _data.Offset + _index);
            _index += 4;
            return value;
        }

        public string ReadString()
        {
            int length = ReadInt();
            if (_index + length > _data.Count)
                throw new IndexOutOfRangeException("No more data to read.");

            string value = Encoding.UTF8.GetString(_data.Array, _data.Offset + _index, length);
            _index += length;
            return value;
        }
    }
}
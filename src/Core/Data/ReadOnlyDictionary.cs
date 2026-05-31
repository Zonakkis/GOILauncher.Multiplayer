using System;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Core.Data
{
    public class ReadOnlyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public ReadOnlyDictionary(Dictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        public new TValue this[TKey key]
        {
            get => base[key];
            set => throw new NotSupportedException("ReadOnlyDictionary is read-only.");
        }

        public new void Add(TKey key, TValue value)
        {
            throw new NotSupportedException("ReadOnlyDictionary is read-only.");
        }

        public new bool Remove(TKey key)
        {
            throw new NotSupportedException("ReadOnlyDictionary is read-only.");
        }

        public new void Clear()
        {
            throw new NotSupportedException("ReadOnlyDictionary is read-only.");
        }
    }
}

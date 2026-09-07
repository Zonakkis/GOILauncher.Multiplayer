using System;
using System.Security.Cryptography;

namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>In-memory verifier only. Never serialize or log either field.</summary>
    internal sealed class RoomPassword
    {
        private readonly byte[] _salt = new byte[16];
        private readonly byte[] _hash;
        public RoomPassword(string password)
        {
            var random = new RNGCryptoServiceProvider();
            // These crypto types only implement IDisposable on newer runtimes.
            using ((object)random as IDisposable) random.GetBytes(_salt);
            _hash = Derive(password);
        }
        public bool Matches(string password)
        {
            if (password == null) return false;
            var candidate = Derive(password);
            int difference = 0;
            for (int i = 0; i < _hash.Length; i++) difference |= candidate[i] ^ _hash[i];
            return difference == 0;
        }
        private byte[] Derive(string password)
        {
            // PBKDF2 available on all supported runtimes, including .NET 3.5 / Unity Mono.
            var derive = new Rfc2898DeriveBytes(password, _salt, 10000);
            using ((object)derive as IDisposable) return derive.GetBytes(32);
        }
    }
}

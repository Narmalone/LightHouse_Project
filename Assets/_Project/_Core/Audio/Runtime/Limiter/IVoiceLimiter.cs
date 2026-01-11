using System.Collections.Generic;

namespace LightHouse.Core.Audio
{
    public interface IVoiceLimiter
    {
        bool TryEnter(string key); // key = "Bus:Ambience" ou "Cue:Env/Rain/Heavy"
        void Exit(string key);
    }

    public class TokenBucketLimiter : IVoiceLimiter
    {
        private readonly Dictionary<string, int> _inUse = new();
        private readonly Dictionary<string, int> _limits;
        public TokenBucketLimiter(Dictionary<string, int> limits) => _limits = limits;

        public bool TryEnter(string key)
        {
            _inUse.TryGetValue(key, out var n);
            _limits.TryGetValue(key, out var cap);
            if (cap > 0 && n >= cap) return false;
            _inUse[key] = n + 1;
            return true;
        }
        public void Exit(string key)
        {
            if (_inUse.TryGetValue(key, out var n)) _inUse[key] = System.Math.Max(0, n - 1);
        }
    }

}

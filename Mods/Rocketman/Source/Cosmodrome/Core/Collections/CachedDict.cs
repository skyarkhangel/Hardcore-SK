using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Verse;

namespace RocketMan
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CachedUnit<T>
    {
        public readonly int tick;

        public readonly T value;

        public CachedUnit(T value)
        {
            tick = GenTicks.TicksGame;
            this.value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(int expiry = 0)
        {
            if (GenTicks.TicksGame - tick <= expiry)
                return true;
            return false;
        }
    }

    public class CachedDict<A, B>
    {
        private const int MAX_CACHE_SIZE = 10000;

        private readonly Dictionary<A, CachedUnit<B>> cache = new Dictionary<A, CachedUnit<B>>();

        public B this[A key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => cache[key].value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AddPair(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(A key, out B value, int expiry = 0)
        {
            CleanUp();
            if (cache.TryGetValue(key, out var store))
            {
                if (store.IsValid(expiry))
                {
                    value = store.value;
                    return true;
                }
                Remove(key);
            }
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(A key, out B value, out bool failed, int expiry = 0)
        {
            CleanUp();
            if (cache.TryGetValue(key, out var store))
            {
                if (store.IsValid(expiry))
                {
                    failed = false;
                    value = store.value;
                    return true;
                }
                Remove(key);
            }
            failed = true;
            value = default;
            return false;
        }

        public void AddPair(A key, B value)
        {
            cache[key] = new CachedUnit<B>(value);
        }

        public void Remove(A key)
        {
            cache.Remove(key);
        }

        private void CleanUp()
        {
            if (MAX_CACHE_SIZE < cache.Count) cache.Clear();
        }
    }
}
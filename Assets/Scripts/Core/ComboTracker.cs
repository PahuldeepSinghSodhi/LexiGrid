namespace LexiGrid.Core
{
    /// <summary>Non-overlapping pairs of accepted words less than five seconds apart.</summary>
    public sealed class ComboTracker
    {
        private float? _pending;
        public void Reset() => _pending = null;
        // Call only after dictionary and duplicate validation; reject handlers call Reset.
        public float Accept(float now, float remaining)
        {
            if (remaining <= 0) { Reset(); return 0; }
            if (_pending.HasValue && now >= _pending.Value && now - _pending.Value < 5f)
            { Reset(); return 2f; }
            _pending = now;
            return 0;
        }
    }
}

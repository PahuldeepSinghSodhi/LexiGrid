namespace LexiGrid.Core
{
    /// <summary>
    /// A static scoring calculator for game words.
    /// </summary>
    public static class ScoringEngine
    {
        public const int MinWordLength = 3;
        public const float FastBonusThreshold = 5.0f;
        public const float MediumBonusThreshold = 10.0f;
        public const float FastMultiplier = 1.5f;
        public const float MediumMultiplier = 1.25f;
        public const float NoMultiplier = 1.0f;

        /// <summary>
        /// Calculates the base score for a word based on its length.
        /// </summary>
        public static int CalculateBaseScore(int wordLength)
        {
            if (wordLength < MinWordLength) return 0;
            
            switch (wordLength)
            {
                case 3: return 100;
                case 4: return 400;
                case 5: return 800;
                case 6: return 1200;
                default: return 2000;
            }
        }

        /// <summary>
        /// Determines the score multiplier based on time taken.
        /// </summary>
        public static float GetTimeMultiplier(float secondsSinceLastWord)
        {
            if (secondsSinceLastWord < FastBonusThreshold)
                return FastMultiplier;
            if (secondsSinceLastWord < MediumBonusThreshold)
                return MediumMultiplier;
            
            return NoMultiplier;
        }

        /// <summary>
        /// Calculates the final score including time multiplier.
        /// </summary>
        public static int CalculateFinalScore(int wordLength, float secondsSinceLastWord)
        {
            return (int)(CalculateBaseScore(wordLength) * GetTimeMultiplier(secondsSinceLastWord));
        }
    }
}

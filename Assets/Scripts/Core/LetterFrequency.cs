using System;

namespace LexiGrid.Core
{
    /// <summary>
    /// A static utility class for weighted random letter generation.
    /// </summary>
    public static class LetterFrequency
    {
        private static readonly Random _random = new Random();

        // E=12.7%, T=9.1%, A=8.2%, O=7.5%, I=7.0%, N=6.7%, S=6.3%, H=6.1%, R=6.0%, D=4.3%, L=4.0%, C=2.8%, U=2.8%, M=2.4%, W=2.4%, F=2.2%, G=2.0%, Y=2.0%, P=1.9%, B=1.5%, V=1.0%, K=0.8%, J=0.15%, X=0.15%, Q=0.10%, Z=0.07%
        private static readonly (char letter, double weight)[] _frequencies = new (char, double)[]
        {
            ('E', 12.7), ('T', 9.1), ('A', 8.2), ('O', 7.5), ('I', 7.0), ('N', 6.7), ('S', 6.3), ('H', 6.1), ('R', 6.0),
            ('D', 4.3), ('L', 4.0), ('C', 2.8), ('U', 2.8), ('M', 2.4), ('W', 2.4), ('F', 2.2), ('G', 2.0), ('Y', 2.0),
            ('P', 1.9), ('B', 1.5), ('V', 1.0), ('K', 0.8), ('J', 0.15), ('X', 0.15), ('Q', 0.10), ('Z', 0.07)
        };

        private static readonly double _totalWeight;

        static LetterFrequency()
        {
            _totalWeight = 0;
            foreach (var item in _frequencies)
            {
                _totalWeight += item.weight;
            }
        }

        /// <summary>
        /// Gets a random letter based on English language frequencies.
        /// </summary>
        public static char GetWeightedRandomLetter()
        {
            double value = _random.NextDouble() * _totalWeight;
            foreach (var item in _frequencies)
            {
                value -= item.weight;
                if (value <= 0)
                    return item.letter;
            }
            return _frequencies[_frequencies.Length - 1].letter; // Fallback
        }

        /// <summary>
        /// Generates an array of weighted random letters.
        /// </summary>
        public static char[] GenerateLetters(int count)
        {
            char[] letters = new char[count];
            for (int i = 0; i < count; i++)
            {
                letters[i] = GetWeightedRandomLetter();
            }
            return letters;
        }
    }
}

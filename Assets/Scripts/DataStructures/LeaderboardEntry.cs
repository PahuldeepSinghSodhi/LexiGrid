using System;

namespace LexiGrid.DataStructures
{
    /// <summary>
    /// Represents a single entry in the leaderboard.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry : IComparable<LeaderboardEntry>
    {
        /// <summary>
        /// Gets the name of the player.
        /// </summary>
        public string PlayerName { get; }

        /// <summary>
        /// Gets the score achieved by the player.
        /// </summary>
        public int Score { get; }

        /// <summary>
        /// Gets the date the score was achieved (ISO string format).
        /// </summary>
        public string Date { get; }

        /// <summary>
        /// Parameterless constructor for JSON serialization compatibility.
        /// </summary>
        private LeaderboardEntry() 
        { 
            PlayerName = string.Empty;
            Date = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardEntry"/> class.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <param name="score">The score achieved.</param>
        /// <param name="date">The date the score was achieved.</param>
        public LeaderboardEntry(string playerName, int score, string date)
        {
            PlayerName = playerName;
            Score = score;
            Date = date;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardEntry"/> class, auto-filling the date.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <param name="score">The score achieved.</param>
        public LeaderboardEntry(string playerName, int score) 
            : this(playerName, score, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        {
        }

        /// <summary>
        /// Compares this entry to another based on the score ascending.
        /// </summary>
        /// <param name="other">The other entry to compare to.</param>
        /// <returns>A value indicating the relative order of the objects being compared.</returns>
        public int CompareTo(LeaderboardEntry other)
        {
            if (other == null) return 1;
            return Score.CompareTo(other.Score);
        }

        /// <summary>
        /// Returns a string representation of the entry.
        /// </summary>
        /// <returns>A formatted string with player name, score, and date.</returns>
        public override string ToString()
        {
            return $"{PlayerName}: {Score} ({Date})";
        }
    }
}

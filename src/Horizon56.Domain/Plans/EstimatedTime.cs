using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Horizon56.Domain.Common;

namespace Horizon56.Domain.Plans
{
    // Represents how long a Step is expected to take.
    // Accepts human-friendly strings like "1h 30m" and stores them as total minutes internally.
    // This makes comparison easy: "90m" and "1h 30m" both store as 90 minutes, so they are equal.
    public class EstimatedTime : ValueObject
    {
        // Supported input formats: "45m", "1h", "1h 30m", "90m"
        // Compiled once at startup for performance — reused on every call
        private static readonly Regex DurationPattern = new Regex(
            @"^(?:(\d+)h)?\s*(?:(\d+)m)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // The duration stored as a simple number of minutes.
        // Example: "1h 30m" is stored as 90
        public int TotalMinutes { get; }

        private EstimatedTime(int totalMinutes)
        {
            TotalMinutes = totalMinutes;
        }

        // Parse a human-readable duration string and create an EstimatedTime.
        // Valid inputs: "45m", "1h", "1h 30m", "90m"
        // Rules: must be more than 0 minutes and less than 480 minutes (8 hours)
        public static EstimatedTime Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("EstimatedTime input cannot be empty.");

            int totalMinutes = ParseToMinutes(input.Trim());

            if (totalMinutes <= 0)
                throw new ArgumentException("EstimatedTime must be greater than 0 minutes.");

            if (totalMinutes >= 480)
                throw new ArgumentException("EstimatedTime must be less than 480 minutes (8 hours).");

            return new EstimatedTime(totalMinutes);
        }

        // Used by EF Core when reading back from the database.
        // The database stores total minutes, so we reconstruct from that directly.
        public static EstimatedTime FromMinutes(int totalMinutes)
        {
            if (totalMinutes <= 0)
                throw new ArgumentException("EstimatedTime must be greater than 0 minutes.");

            if (totalMinutes >= 480)
                throw new ArgumentException("EstimatedTime must be less than 480 minutes (8 hours).");

            return new EstimatedTime(totalMinutes);
        }

        // Parse the input string into total minutes.
        // Handles hours-only ("1h"), minutes-only ("45m"), and combined ("1h 30m" or "1h30m").
        private static int ParseToMinutes(string input)
        {
            var match = DurationPattern.Match(input);

            // The regex matched but both hour and minute groups are empty (e.g. input was just spaces)
            bool hasHours = match.Success && match.Groups[1].Value != "";
            bool hasMinutes = match.Success && match.Groups[2].Value != "";

            if (!hasHours && !hasMinutes)
                throw new ArgumentException(
                    $"Invalid EstimatedTime format: '{input}'. Accepted: '45m', '1h', '1h 30m', '90m'.");

            int hours = hasHours ? int.Parse(match.Groups[1].Value) : 0;
            int minutes = hasMinutes ? int.Parse(match.Groups[2].Value) : 0;

            return (hours * 60) + minutes;
        }

        // Convert back to a readable string for display.
        // Examples: 45 → "45m", 60 → "1h", 90 → "1h 30m"
        public string ToDisplayString()
        {
            int hours = TotalMinutes / 60;
            int minutes = TotalMinutes % 60;

            if (hours > 0 && minutes > 0) return $"{hours}h {minutes}m";
            if (hours > 0) return $"{hours}h";
            return $"{minutes}m";
        }

        // Two EstimatedTime values are equal if they represent the same number of minutes.
        // So "90m" equals "1h 30m" because both are 90 minutes.
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return TotalMinutes;
        }

        public override string ToString() => ToDisplayString();
    }
}



namespace BinaryBeat.Core;

/// <summary>
/// A generic engine to create chords
/// </summary>
public static class ChordFactory
{
    /// <summary>
    /// Noteoffsets
    /// </summary>
    private static readonly Dictionary<string, int> NoteOffsets = new(StringComparer.OrdinalIgnoreCase)
     {
         { "C", 60 }, { "C#", 61 }, { "Db", 61 }, { "D", 62 }, { "D#", 63 },
         { "Eb", 63 }, { "E", 64 }, { "F", 65 }, { "F#", 66 }, { "Gb", 66 },
         { "G", 67 }, { "G#", 68 }, { "Ab", 68 }, { "A", 69 }, { "A#", 70 },
         { "Bb", 70 }, { "B", 71 }
     };

    /// <summary>
    /// Chord creation engine
    /// </summary>
    /// <param name="root">Root note</param>
    /// <param name="quality">Major, Minor ...</param>
    /// <param name="confidence">Value indicating how good the translation is.</param>
    /// <returns></returns>
    public static int[] Create(string root, string quality, float confidence)
    {
        if (!NoteOffsets.TryGetValue(root, out int baseNote))
            return Array.Empty<int>(); // Eller default C Major

        // Intervalls (halftone from basenote)
        int[] intervals = quality.ToLower() switch
        {
            "major" or "maj" => [0, 4, 7],
            "minor" or "min" => [0, 3, 7],
            "major 7" or "maj7" => [0, 4, 7, 11],
            "minor 7" or "min7" => [0, 3, 7, 10],
            "dominant 7" or "7" => [0, 4, 7, 10],
            "sus4" => [0, 5, 7],
            "diminished" or "dim" => [0, 3, 6],
            _ => [0, 4, 7] // Default to Major if we dont understand(AI)
        };

        var t = intervals.Select(i => baseNote + i).ToArray();
        return intervals.Select(i => baseNote + i).ToArray();
    }
}
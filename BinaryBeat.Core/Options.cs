namespace BinaryBeat.Core;

/// <summary>
/// Application configuration.
/// </summary>
public class Options
{
    /// <summary>
    /// AI Model used in app
    /// </summary>
    [Option('m', "modelFile", Required = false, HelpText = "Model to use (filename", Default = "ggml-tiny.en.bin")]
    public string ModelName { get; set; } = "ggml-tiny.en.bin"; // If default is set in Option, this parameter is not overriden???

    [Option('w', "message", Required = false, HelpText = "")]
    public string WelcomeMessage { get; set; } = "BinaryBeat Audio Started";

    /// <summary>
    /// iD14-brusvärde
    /// </summary>
    [Option('t', "threshold", Required = false, HelpText = "Microphone Threshold")]
    public double Threshold { get; set; } = 400;

    /// <summary>
    /// AI information / instructions
    /// </summary>
    [Option('a', "aipromt", Required = false, HelpText = "Ai instructions")]
    public string AIPromt { get; } =  "C C# Db D D# Eb E F F# Gb G G# Ab A A# Bb B Major Minor Maj7 Min7 Dom7 Sus4 Diminished Augmented Chord";
}
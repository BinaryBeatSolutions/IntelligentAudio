using Melanchall.DryWetMidi.MusicTheory;
using NAudio.Utils;
using Whisper.net;
using Whisper.net.Ggml;
using static System.Net.Mime.MediaTypeNames;

namespace BinaryBeat.Core;

/// <summary>
/// IntelligentAudio
/// </summary>
public class IntelligentAudio : IDisposable
{
    private readonly ChannelReader<byte[]> _audioReader;
    private readonly MidiOutputService _midiService;
    private readonly OscService _oscService;
    private WhisperFactory _factory;

    //Dev log
    Action<string> P = input =>
    {
        if (input.Length == 0) { return; }
        #if DEBUG
        Console.WriteLine($"[BinaryBeat.Audio] {input}");
        #endif
    };

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="audioReader">ChannelReader<byte[]> audioReader</param>
    /// <param name="midiService">MidiOutputService midiService</param>
    /// <param name="oscService">OscService oscService</param>
    public IntelligentAudio(ChannelReader<byte[]> audioReader, MidiOutputService midiService, OscService oscService)
    {
        _audioReader = audioReader;
        _midiService = midiService;
       _oscService = oscService;
    }

    /// <summary>
    /// Start listen to the microphone
    /// </summary>
    /// <param name="opt">Options</param>
    /// <param name="ct">CansellationToken</param>
    /// <returns></returns>
    public async Task StartListenAsync(Options opt, CancellationToken ct)
    {
        //Ensure we have the Model donwloaded
        var path = await PathResolver.GetModelPath(opt.ModelName);
        _factory = WhisperFactory.FromPath(path);

        // Vi samlar ljudet i en lista tills vi har tillräckligt för Whisper
        var audioBuffer = new List<byte>();

        // Whisper förväntar sig 16000Hz. Vi räknar ut hur många bytes 1 sekund är:
        // (16000 samples * 2 bytes per sample * 1 kanal) = 32 000 bytes
        const int bytesPerSecond = 16000 * 2;
        int threshold = bytesPerSecond * 2; // Vi kör Whisper varannan sekund

        try
        {
            // 1. Loopen väntar här tills MicrophoneSource skriver data
            await foreach (var data in _audioReader.ReadAllAsync(ct))
            {
                audioBuffer.AddRange(data);

                // 2. Har vi samlat tillräckligt med ljud (t.ex. 2 sekunder)?
                if (audioBuffer.Count >= threshold)
                {
                    // Kopiera ut datan till en lokal variabel för analys
                    var chunkToProcess = audioBuffer.ToArray();
                    audioBuffer.Clear(); // Töm huvudbufferten direkt så vi kan samla nästa 2 sekunder

                    // 3. FIRE AND FORGET (Task.Run)
                    // Vi kör Whisper i en egen tråd så att foreach-loopen kan gå tillbaka 
                    // och hämta nästa ljudpaket från iD14 omedelbart!
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var text = await ProcessWithWhisperAsync(chunkToProcess);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                P($"[Whisper] {text.Trim()}");
                            }
                        }
                        catch (Exception ex)
                        {
                            P($"[ERROR]: {ex}, {ex.StackTrace}");
                        }
                    }, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            P("Upptagning avbruten normalt.");
        }
    }

    private async Task<string> ProcessWithWhisperAsync(byte[] raw441Bytes)
    {
        // 1. Konvertera och Resampla (steg 1 & 2 är perfekta!)
        var samples441 = new float[raw441Bytes.Length / 2];
        for (int i = 0; i < samples441.Length; i++)
        {
            short s = BitConverter.ToInt16(raw441Bytes, i * 2);
            samples441[i] = s / 32768f;
        }
        float[] samples16k = AudioAnalysis.Resample(samples441, 44100, 16000);

        // 3. Whisper Processering
        using var processor = _factory.CreateBuilder()
            .WithLanguage("en")
            .WithPrompt("Musical chords: C, C#, Db, D, Eb, E, F, F#, G, Ab, A, Bb, B. Major, Minor, Maj7, m7, Dominant, Sus4, Diminished.")
            .Build();

        var fullTextBuilder = new StringBuilder();

        // Samla ihop ALL text först
        await foreach (var segment in processor.ProcessAsync(samples16k))
        {
            fullTextBuilder.Append(segment.Text);
        }

        var finalResult = fullTextBuilder.ToString().Trim();
        if (string.IsNullOrEmpty(finalResult)) return "";

        // --- NU TOLKAR VI RESULTATET ---

        // Dela upp strängen (t.ex. "A, Minor.")
        var parts = finalResult.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);

        string root = parts.FirstOrDefault() ?? "C";
        // Ta bort punkter och gör rent quality-strängen
        string quality = parts.Length > 1 ? parts[1].Replace(".", "").Trim() : "major";

        // Skapa MIDI-noter
        int[] midiNotes = ChordFactory.Create(root, quality, 1.0f);

        if (midiNotes.Length > 0)
        {
            P($"[BinaryBeat] MIDI skapad för {root} {quality}: {string.Join(", ", midiNotes)}");

            // Skicka till MIDI
            _midiService.PlayChord(midiNotes);

            // Skicka till Max for Live (OSC)
            _oscService.SendChord($"{root} {quality}");
        }

        return finalResult;
    }


    /// <summary>
    /// Dispose components to free up resources. WhisperFactory och WhisperProcessor kan använda mycket GPU/CPU-resurser, så det är viktigt att städa upp ordentligt.
    /// </summary>
    public void Dispose()
    {
        if (_factory != null)
            _factory.Dispose();

        GC.SuppressFinalize(this);
    }
}



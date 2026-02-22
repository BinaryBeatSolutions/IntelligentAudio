using Whisper.net;
using Whisper.net.Ggml;

namespace BinaryBeat.Core;

public class IntelligentAudio : IDisposable
{
    private readonly ChannelReader<byte[]> _audioReader;
    private WhisperFactory _factory;

    Action<string> P = input =>
    {
        if (input.Length == 0) { return; }
        #if DEBUG
        Console.WriteLine($"[BinaryBeat.Audio] {input}");
        #endif
    };

    public IntelligentAudio(ChannelReader<byte[]> audioReader)
    {
        _audioReader = audioReader;
    }

    public async Task StartListenAsync(Options opt, CancellationToken ct)
    {
        var path = await PathResolver.GetModelPath(opt.ModelName);

        _factory = WhisperFactory.FromPath(path);

        Console.WriteLine($"{opt.ModelName}\n {_factory}\n {path}");

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
                    P($"{audioBuffer.Count}");

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
        // 1. Konvertera 16-bit bytes till float-samples (-1.0 till 1.0)
        // Whisper kräver 32-bit float för sin interna matematik
        var samples441 = new float[raw441Bytes.Length / 2];
        for (int i = 0; i < samples441.Length; i++)
        {
            short s = BitConverter.ToInt16(raw441Bytes, i * 2);
            samples441[i] = s / 32768f;
        }

        // 2. RESAMPLING (44100 -> 16000)
        // Utan detta hör Whisper "Musse Pigg", med detta hör den din iD14 i hi-fi!
        float[] samples16k = AudioAnalysis.Resample(samples441, 44100, 16000);

        // 3. Whisper.net Processering
        using var processor = _factory.CreateBuilder()
            .WithLanguage("en")
            .WithPrompt("Musical chords: C, C#, Db, D, Eb, E, F, F#, G, Ab, A, Bb, B. Major, Minor, Maj7, m7, Dominant, Sus4, Diminished.")
            .Build();

        var result = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(samples16k))
        {

            var rawText = segment.Text.ToString().Trim();
            if (string.IsNullOrEmpty(rawText)) return "";

            // 1. Dela upp strängen (Whisper kan ge "C Major" eller "C, Major")
            var parts = rawText.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);

            string root = parts.FirstOrDefault() ?? "C";
            // Försök hitta kvaliteten (t.ex. "minor"), annars defaulta till "major"

            string quality = parts.Length > 1 ? parts[1].Replace(".", "").Trim() : "major";

            // 2. Skapa MIDI-noterna!
            int[] midiNotes = ChordFactory.Create(root, quality, 1.0f);

            if (midiNotes.Length > 0)
            {
                P($"[BinaryBeat] MIDI skapad för {root} {quality}: {string.Join(", ", midiNotes)}");
                // HÄR skickar vi noterna till DryWetMidi (nästa steg)
            }
            result.Append(segment.Text);
        }

        return result.ToString().Trim();
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



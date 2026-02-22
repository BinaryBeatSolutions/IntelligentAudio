using Whisper.net;
using Whisper.net.Ggml;

namespace BinaryBeat.Core;

public class IntelligentAudio : IDisposable
{
    private readonly ChannelReader<byte[]> _audioReader;
    private WhisperFactory? _factory;

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

        Console.WriteLine($"{opt.ModelName} {path} {_factory}");

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
                            var text = await ProcessWithWhisperAsync(chunkToProcess, opt);
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


    private async Task<string> ProcessWithWhisperAsync(byte[] raw441Bytes, Options opt)
    {
        // 1. Konvertera 16-bit Integer till 32-bit Float
        var samples = new float[raw441Bytes.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short s = BitConverter.ToInt16(raw441Bytes, i * 2);
            samples[i] = s / 32768f;
        }

        // 2. Downsampling: 44100 -> 16000
        float[] samples16k = AudioAnalysis.Resample(samples, 44100, 16000);

        // 3. Whisper.net processering
        using var processor = _factory.CreateBuilder()
            .WithLanguage("en")
            .WithPrompt("C C# Db D D# Eb E F F# Gb G G# Ab A A# Bb B Major Minor Maj7 Min7 Dom7 Sus4 Diminished Augmented Chord")
            .Build();
       
        var fullText = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(samples16k))
        {
            fullText.Append(segment.Text);
        }
        return fullText.ToString();
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



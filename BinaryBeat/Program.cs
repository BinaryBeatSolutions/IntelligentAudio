
await Parser.Default
    .ParseArguments<Options>(args)
    .WithParsedAsync(AudioEngineAsync);

async Task AudioEngineAsync(Options opt)
{
    var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BinaryBeat", "startup_log.txt");
    File.AppendAllText(logPath, $"[{DateTime.Now}] Start attempt from: {AppDomain.CurrentDomain.BaseDirectory}\n");

    var audioChannel = Channel.CreateUnbounded<byte[]>();
    string procName = Process.GetCurrentProcess().ProcessName;

    //Only run one(1) instance of the service
    if (Process.GetProcessesByName(procName).Length > 1) return;

    // Services
    var services = new ServiceCollection()
        .AddSingleton(audioChannel.Reader) 
        .AddSingleton(audioChannel.Writer)
        .AddSingleton<MicrophoneSource>()
        .AddSingleton<MidiOutputService>()
        .AddSingleton<OscService>()
        .AddSingleton(opt)
        .AddSingleton<IntelligentAudio>();

    try
    {
        using var cts = new CancellationTokenSource();

        var serviceProvider = services.BuildServiceProvider();
        var mic = serviceProvider.GetRequiredService<MicrophoneSource>();
        var midi = serviceProvider.GetRequiredService<MidiOutputService>();
        var engine = serviceProvider.GetRequiredService<IntelligentAudio>();
        var osc = serviceProvider.GetRequiredService<OscService>();

        // Start microphone
        // Wash audio with Gate and a highpass filter
        mic?.Start(deviceNumber: 0);

        //Listen for audio
        var analysisTask = engine?.StartListenAsync(opt, cts.Token);

        P("System started");
#if DEBUG
        File.AppendAllText(logPath, $"[{DateTime.Now}] Start attempt from: {AppDomain.CurrentDomain.BaseDirectory}\n");
#endif
        await analysisTask;

        mic?.Stop();
    }
    catch (Exception ex)
    {
        // AppData log!
        var errPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BinaryBeat", "error.txt");
        File.WriteAllText(errPath, ex.ToString());
    }
}
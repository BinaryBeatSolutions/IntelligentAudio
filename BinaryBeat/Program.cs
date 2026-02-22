
var audioChannel = Channel.CreateUnbounded<byte[]>();

var serviceProvider = new ServiceCollection()
    .AddSingleton(audioChannel) 
    .AddSingleton(audioChannel.Writer)
    .AddSingleton(audioChannel.Reader)
    .AddSingleton<MicrophoneSource>()
    .AddSingleton<IntelligentAudio>()
    .AddSingleton<MidiOutputService>()
    .BuildServiceProvider();

await Parser.Default
    .ParseArguments<Options>(args)
    .WithParsedAsync(AudioEngineAsync);

async Task AudioEngineAsync(Options opt)
{
    using var cts = new CancellationTokenSource();

    var mic = serviceProvider.GetRequiredService<MicrophoneSource>();
    var midi = serviceProvider.GetRequiredService<MidiOutputService>();
    var engine = serviceProvider.GetRequiredService<IntelligentAudio>(); 

    // 1. Starta mikrofonen (Producenten)
    // Den börjar lyssna på iD14 och skicka "tvättad" data till kanalen
    mic?.Start(deviceNumber: 0);

    // 2. Starta AI-motorn (Konsumenten)
    // Denna loop körs så länge det finns data i kanalen
    var analysisTask = engine?.StartListenAsync(opt, cts.Token);

    P("Systemet lyssnar. Säg ett ackord...");

    // Vänta på att analysen blir klar (eller att användaren avbryter)
    await analysisTask;

    mic?.Stop();
}

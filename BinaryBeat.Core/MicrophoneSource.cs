

using NAudio.Wave;

namespace BinaryBeat.Core;


public class MicrophoneSource : IDisposable
{
    private WaveInEvent _waveIn;
    private readonly ChannelWriter<byte[]> _writer;
    private readonly NoiseGateProcessor _gate = new();

    public MicrophoneSource(ChannelWriter<byte[]> writer)
    {
        _writer = writer;
    }

    public void Start(int deviceNumber = 0)
    {
        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(44100, 16, 1), // 44.1kHz, 16-bit, Mono
            BufferMilliseconds = 100 // Skickar data var 100:e ms
        };

        _waveIn.DataAvailable += async (s, e) =>
        {
            // Viktigt: Skicka bara de bytes som faktiskt spelats in
            await _gate.ProcessBufferAsync(e.Buffer, e.BytesRecorded, _writer);
        };

        _waveIn.StartRecording();
    }

    public void Stop() => _waveIn?.StopRecording();
    public void Dispose() => _waveIn?.Dispose();
}
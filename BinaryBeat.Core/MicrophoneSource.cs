
using NAudio.Wave;

namespace BinaryBeat.Core;

/// <summary>
/// Microphone 
/// </summary>
public class MicrophoneSource : IDisposable
{
    private WaveInEvent _waveIn;
    private readonly ChannelWriter<byte[]> _writer;
    private readonly NoiseGateProcessor _gate = new();

    /// <summary>
    /// Ctor (DI)
    /// </summary>
    /// <param name="writer"></param>
    public MicrophoneSource(ChannelWriter<byte[]> writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Start microphone, and apply gate and filter(eq)
    /// </summary>
    /// <param name="deviceNumber">Channel to use</param>
    public void Start(int deviceNumber = 0)
    {
        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(44100, 16, 1), // 44.1kHz, 16-bit, Mono
            BufferMilliseconds = 100 // Send data each 100ms
        };

        _waveIn.DataAvailable += async (s, e) =>
        {
            //Important: Only send bytes that acually contains audio.
            await _gate.ProcessBufferAsync(e.Buffer, e.BytesRecorded, _writer);
        };

        _waveIn.StartRecording();
    }

    public void Stop() => _waveIn?.StopRecording();
    public void Dispose() => _waveIn?.Dispose();
}
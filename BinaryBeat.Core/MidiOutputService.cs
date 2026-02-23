
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace BinaryBeat.Core;

/// <summary>
/// Midi 
/// </summary>
public class MidiOutputService : IDisposable
{
    private OutputDevice? _outputDevice;

    public void Initialize(string deviceName = "loopMIDI Port")
    {
        // Öppna en virtuell MIDI-port (kräver loopMIDI installerat)
        _outputDevice = OutputDevice.GetByName(deviceName);
#if DEBUG
        Console.WriteLine($"[MIDI] Ansluten till: {deviceName}");
#endif
    }

    public void PlayChord(int[] notes, int velocity = 90)
    {
        if (_outputDevice == null) return;

        foreach (var noteNumber in notes)
        {
            // Skicka Note On för varje ton i ackordet
            _outputDevice.SendEvent(new NoteOnEvent((SevenBitNumber)noteNumber, (SevenBitNumber)velocity));
#if DEBUG
            Console.Write($"{noteNumber} ");
#endif
        }
        
        // Stoppa noterna efter 2 sekunder (eller låt dem ringa)
        _ = Task.Delay(2000).ContinueWith(_ => StopChord(notes));
    }

    private void StopChord(int[] notes)
    {
        foreach (var noteNumber in notes)
        {
            _outputDevice?.SendEvent(new NoteOffEvent((SevenBitNumber)noteNumber, (SevenBitNumber)0));
        }
    }

    public void Dispose() => _outputDevice?.Dispose();
}
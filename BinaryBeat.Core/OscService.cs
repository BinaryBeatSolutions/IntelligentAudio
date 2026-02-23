
using OscCore;
using OscCore.LowLevel;
using System.Net;
using System.Net.Sockets;

namespace BinaryBeat.Core;

/// <summary>
/// Service worker
/// </summary>
public class OscService : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _maxEndPoint;

    /// <summary>
    /// Ctor
    /// </summary>
    public OscService()
    {
        // Vi använder standard UDP-klient från .NET
        _udpClient = new UdpClient();
        _maxEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
    }

    /// <summary>
    /// Returns the created chord to stream
    /// </summary>
    /// <param name="chord"></param>
    public void SendChord(string chord)
    {
        var message = new OscMessage("/binarybeat/chord", chord);
        byte[] data = message.ToByteArray();
        _udpClient.Send(data, data.Length, _maxEndPoint);

        // Lägg till denna rad för att verifiera:
        Console.WriteLine($"[OSC Out] Skickade {data.Length} bytes till {_maxEndPoint.Address}:{_maxEndPoint.Port}");
    }

    /// <summary>
    /// Extra, for eg. to control funtions in a DAW
    /// </summary>
    /// <param name="command"></param>
    /// <param name="value"></param>
    public void SendCommand(string address, string value)
    {
        // Validering: Skriv ut exakt vad vi skickar till loggen
        ValidateOscPacket(address, value);

        var message = new OscMessage(address, value);
        byte[] data = message.ToByteArray();
        _udpClient.Send(data, data.Length, _maxEndPoint);
    }
    private void ValidateOscPacket(string address, string value)
    {

        // Detta hjälper dig att se exakt vad Max tar emot
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[OSC-VALIDATOR] [{timestamp}] Address: \"{address}\" | Value: \"{value}\" | Bytes: {Encoding.UTF8.GetByteCount(address) + Encoding.UTF8.GetByteCount(value) + 12}");

        // Tips för Max-patchen baserat på adressen
        if (!address.StartsWith("/"))
            Console.WriteLine("[VARNING] OSC-adresser måste börja med '/'. Max kan ignorera detta!");
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
        _udpClient?.Dispose();

        GC.SuppressFinalize(this);
    }
}


using OscCore;
using OscCore.LowLevel;
using System.Net;
using System.Net.Sockets;

namespace BinaryBeat.Core;

public class OscService : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _maxEndPoint;

    public OscService()
    {
        // Vi använder standard UDP-klient från .NET
        _udpClient = new UdpClient();
        _maxEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
    }

    public void SendChord(string chord)
    {
        var message = new OscMessage("/binarybeat/chord", chord);
        byte[] data = message.ToByteArray();
        _udpClient.Send(data, data.Length, _maxEndPoint);

        // Lägg till denna rad för att verifiera:
        Console.WriteLine($"[OSC Out] Skickade {data.Length} bytes till {_maxEndPoint.Address}:{_maxEndPoint.Port}");
    }

    public void SendCommand(string command)
    {
        var message = new OscMessage("/binarybeat/command", command);
        byte[] data = message.ToByteArray();
        _udpClient.Send(data, data.Length, _maxEndPoint);
    }

    public void Dispose()
    {
        _udpClient?.Dispose();

        GC.SuppressFinalize(this);
    }
}

namespace BinaryBeat.Core;


public class NoiseGateProcessor
{
    private readonly SimpleHighPass _highPass = new(); // Vårt nya filter
    private DateTime _lastSoundTime = DateTime.MinValue;
    private readonly TimeSpan _holdTime = TimeSpan.FromMilliseconds(400);
    private readonly double _threshold = 400;

    public async Task ProcessBufferAsync(byte[] buffer, int bytesRecorded, ChannelWriter<byte[]> writer)
    {
        // 1. Skapa kopian (viktigt för trådsäkerhet)
        var actualData = new byte[bytesRecorded];
        Buffer.BlockCopy(buffer, 0, actualData, 0, bytesRecorded);

        // 2. FILTRERA: Kör varje sample genom filtret INNAN analys
        for (int i = 0; i < actualData.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(actualData, i);
            short filteredSample = _highPass.Apply(sample); // Här sker magin

            // Skriv tillbaka det filtrerade värdet till bufferten
            byte[] filteredBytes = BitConverter.GetBytes(filteredSample);
            actualData[i] = filteredBytes[0];
            actualData[i + 1] = filteredBytes[1];
        }

        // 3. ANALYSERA: Nu mäter vi RMS på det filtrerade ljudet
        double rms = AudioAnalysis.CalculateRMS(actualData);

        // 4. BESLUT: Ska vi skicka vidare?
        if (rms > _threshold || (DateTime.UtcNow - _lastSoundTime) < _holdTime)
        {
            if (rms > _threshold) _lastSoundTime = DateTime.UtcNow;
            await writer.WriteAsync(actualData);
        }
    }
}
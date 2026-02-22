using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace BinaryBeat.Core;

public static class PathResolver
{

    /// <summary>
    /// AI Model path
    /// </summary>
    /// <param name="opt"></param>
    /// <returns></returns>
    public static async Task<string> GetModelPath(string modelName)
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
        Directory.CreateDirectory(folder); // Skapar mappen om den saknas

        var path = Path.Combine(folder, modelName);

        if (!File.Exists(path))
        {
            Console.WriteLine($"[BinaryBeat] Laddar ner {modelName} via GgmlDownloader...");

            // Vi mappar GgmlType.TinyEn (kan automatiseras senare baserat på modelName)
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.TinyEn);
            using var fileWriter = File.Create(path); // File.Create rensar ev. korrupta rester
            await modelStream.CopyToAsync(fileWriter);

            Console.WriteLine("[BinaryBeat] Nedladdning slutförd.");
        }

        return path;
    }
}
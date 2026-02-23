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
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "BinaryBeat", "Models");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var path = Path.Combine(folder, modelName);
#if DEBUG
        Console.WriteLine($"{path}");
#endif
        if (!File.Exists(path))
        {
            #if DEBUG
            Console.WriteLine($"[BinaryBeat] Laddar ner {modelName} via GgmlDownloader...");
#endif
            // Vi mappar GgmlType.TinyEn (kan automatiseras senare baserat på modelName)
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.TinyEn);
            using var fileWriter = File.Create(path); // File.Create rensar ev. korrupta rester
            await modelStream.CopyToAsync(fileWriter);
#if DEBUG
            Console.WriteLine("[BinaryBeat] Nedladdning slutförd.");
#endif
        }

        return path;
    }
}
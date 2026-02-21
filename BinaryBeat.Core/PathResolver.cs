using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whisper.net.Ggml;

namespace BinaryBeat.Core;

public static class PathResolver
{
    /// <summary>
    /// AI Model path
    /// </summary>
    /// <param name="opt"></param>
    /// <returns></returns>
    public static async Task<string> GetModelPath(string model)
    {
      
        // AppContext.BaseDirectory is more secure for Native AOT and VST-plugins
        string baseDir = AppContext.BaseDirectory;

        // Debug (Visual Studio), back up to projectroot to keep everything simple and close.
#if DEBUG
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\")); //Microsoft!!! Still after all these years.
        string path = Path.Combine(projectRoot, "Models", model);
#else
        string path = Path.Combine(baseDir, "Models", model);
#endif

        //Download model if not exists.
        if (!File.Exists(path))
        {
            Console.WriteLine($"[BinaryBeat] Downloading Model to '{path}'.");
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            using var fileStream = File.OpenWrite(path);
            await modelStream.CopyToAsync(fileStream);
#if DEBUG
            Console.WriteLine($"[BinaryBeat] Download Model to '{path}' completed.");
#endif
        }
        else
        {
            Console.WriteLine($"[BinaryBeat] Using Model {model}");
        }

        return path;
    }
}
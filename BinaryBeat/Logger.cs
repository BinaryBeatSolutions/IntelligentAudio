using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat;

/// <summary>
/// Simple console-logger util
/// </summary>
public static class Logger
{
    /// <summary>
    /// Log to console
    /// </summary>
    /// <param name="input"></param>
    public static void P(string input)
    {
        if (string.IsNullOrEmpty(input)) return;
#if DEBUG
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[BinaryBeat.Audio]");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($" {input}\n");
        Console.ForegroundColor = ConsoleColor.White;
#endif
    }


    /// <summary>
    /// Log to console
    /// </summary>
    /// <param name="input"></param>
    public static void P(string section, string input)
    {
        if (string.IsNullOrEmpty(input)) return;
#if DEBUG
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"[{section}]");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($" {input}\n");
        Console.ForegroundColor = ConsoleColor.White;
#endif
    }

    /// <summary>
    /// Log to console
    /// </summary>
    /// <param name="input"></param>
    public static void E(string input)
    {
        if (string.IsNullOrEmpty(input)) return;
#if DEBUG
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[BinaryBeat.Audio]");
        Console.Write($" {input}\n");
        Console.ForegroundColor = ConsoleColor.White;
#endif
    }

    /// <summary>
    /// Log to console
    /// </summary>
    /// <param name="input"></param>
    public static void E(string section, string input)
    {
        if (string.IsNullOrEmpty(input)) return;
#if DEBUG
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"[{section}]");
    
        Console.Write($" {input}\n");
        Console.ForegroundColor = ConsoleColor.White;
#endif
    }

    /// <summary>
    /// Translate (Globalization)
    /// </summary>
    /// <param name="input"></param>
    public static void T(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        if (input == "true") //Reverse :-)
            input = "false";
#if DEBUG
        Console.WriteLine($"[BinaryBeat.Audio] {input}");
#endif
    }
}


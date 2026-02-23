using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat.Core;

/// <summary>
/// Highpass filter
/// </summary>
public class SimpleHighPass
{
    private double _lastSample = 0;
    private double _lastFiltered = 0;
    private readonly double _alpha = 0.95; // Justera mellan 0.9 och 0.99 för cutoff

    /// <summary>
    /// Apply the filter to audio sample
    /// </summary>
    /// <param name="sample"></param>
    /// <returns></returns>
    public short Apply(short sample)
    {
        // Enkel algoritm: y[n] = alpha * (y[n-1] + x[n] - x[n-1])
        double filtered = _alpha * (_lastFiltered + sample - _lastSample);
        _lastSample = sample;
        _lastFiltered = filtered;
        return (short)filtered;
    }
}
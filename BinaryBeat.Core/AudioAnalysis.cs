using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat.Core
{
    internal class AudioAnalysis
    {
        /// <summary>
        /// RMS calculation.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>Normalized value RMS</returns>
        public static double CalculateRMS(byte[] buffer)
        {
            // 16-bitars ljud består av 2 bytes per sample.
            // Vi konverterar dessa till "shorts" för att få det faktiska vågvärdet.
            long sumOfSquares = 0;
            int sampleCount = buffer.Length / 2;

            for (int i = 0; i < buffer.Length; i += 2)
            {
                // Bit-shifting för att kombinera två bytes till en 16-bitars integer
                short sample = BitConverter.ToInt16(buffer, i);

                // Summera kvadraten av varje sample
                sumOfSquares += (long)sample * sample;
            }

            // Medelvärdet av kvadraterna
            double averageSquare = sumOfSquares / (double)sampleCount;

            // Roten ur medelvärdet = RMS
            return Math.Sqrt(averageSquare);
        }

        /// <summary>
        /// Resampler-method (44.1 to 16)
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="inputSampleRate"></param>
        /// <param name="outputSampleRate"></param>
        /// <returns>Resampled audio</returns>
        public static float[] Resample(float[] samples, int inputSampleRate, int outputSampleRate)
        {
            double ratio = (double)inputSampleRate / outputSampleRate;
            var result = new float[(int)(samples.Length / ratio)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = samples[(int)(i * ratio)];
            }
            return result;
        }
    }
}

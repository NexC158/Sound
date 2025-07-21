using System.Runtime.InteropServices;
using Concentus;
using Concentus.Structs;
using NAudio.Wave;

namespace BlazorAppExactlyWebAssembly.ServerWorkingWithAudio;

public class AudioOpusDecodingAndPlay
{
    private static readonly int _sampleRate = 8000;
    private static readonly int _channels = 1;

    private static IOpusDecoder _decoder = OpusCodecFactory.CreateDecoder(_sampleRate, _channels);
    private static BufferedWaveProvider _bufferedWaveProvider = null;
    private static WaveOutEvent _waveOut = null;
    private static bool _initialized = false;

    public static void DecodingAndPlayOpusBuffer(byte[] opusData)
    {
        try
        {
            if (!_initialized)
            {
                //var genericDecoder = OpusCodecFactory.CreateDecoder(_sampleRate, _channels);
                //var specificDecoder = (OpusDecoder)genericDecoder;

                var waveFormat = new WaveFormat(_sampleRate, 16, _channels);
                _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
                {
                    DiscardOnBufferOverflow = true,
                    BufferLength = 1024 * 128
                };
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_bufferedWaveProvider);
                _waveOut.Play();

                _initialized = true;
            }

            if (opusData == null || opusData.Length == 0) 
            { 
                return; 
            }
            var numberBytes = 0;
            Console.WriteLine("\n\n\nСейчас буду смотреть какой же все-таки пакет приходит в DecodingAndPlayOpusBuffer\n");
            foreach (var item in opusData)
            {
                Console.Write($"{item} ");
                numberBytes++;
            }

            Console.WriteLine($"\nИтого пришедший пакет: {numberBytes}\n\n\n");

            Span<byte> opusSpan = opusData.AsSpan();
            Span<float> pcmFloatSpan = new float[4096];

            int samplesDecoded = -1;

            try
            {
                 samplesDecoded = _decoder.Decode(opusSpan, pcmFloatSpan, pcmFloatSpan.Length, false); // посмотреть как включить исправление потерь PLS
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Опять ошибка там же: {e.Message}");
            }

           
            
            if (samplesDecoded <= 0)
            {
                Console.WriteLine("Нулевые сэмплы");
                return;
            }

            var pcmByteCount = samplesDecoded * sizeof(short);
            byte[] pcmBytes = new byte[pcmByteCount];

            for (int i = 0; i < samplesDecoded; i++)
            {
                float sample = MathF.Max(-1.0f, MathF.Min(1.0f, pcmFloatSpan[i]));
                short pcmSample = (short)(sample * short.MaxValue);

                pcmBytes[i * 2] = (byte)(pcmSample & 0xFF);        // младший байт
                pcmBytes[i * 2 + 1] = (byte)((pcmSample >> 8) & 0xFF); // старший байт
            }

            

            //MemoryMarshal.Cast<float, byte>(pcmFloatSpan.Slice(0, samplesDecoded)).CopyTo(pcmBytes);

            _bufferedWaveProvider.AddSamples(pcmBytes, 0, pcmBytes.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AudioOpusDecodingAndPlay DecodingAndPlayOpusBuffer Ошибка воспроизведения: {ex.Message}");
        }
    }
}

using System.Runtime.InteropServices;
using Concentus;
using Concentus.Structs;
using NAudio.Wave;

namespace BlazorAppExactlyWebAssembly.ServerWorkingWithAudio;

public class AudioOpusDecodingAndPlay
{
    private static readonly int _sampleRate = 8000;
    private static readonly int _channels = 1;
    private static readonly int _opusFrameInMiliseconds = 20;
    private static readonly int _numberOfSamples = _sampleRate * _opusFrameInMiliseconds / 1000;

    private static IOpusDecoder _decoder = OpusCodecFactory.CreateDecoder(_sampleRate, _channels);
    private static BufferedWaveProvider _bufferedWaveProvider = null;
    private static WaveOutEvent _waveOut = null;
    private static bool _initialized = false;

    public static void DecodingFrames(byte[] opusFrameData)
    {
        try
        {            
            if (opusFrameData == null || opusFrameData.Length == 0) 
            {
                Console.WriteLine("AudioOpusDecodingAndPlay DecodingFrames Пришел нулевой пакет");
                return; 
            }

            Span<byte> opusSpan = opusFrameData.AsSpan();
            Span<float> pcmFloatSpan = new float[_numberOfSamples];

            int samplesDecoded = _decoder.Decode(opusSpan, pcmFloatSpan, pcmFloatSpan.Length, false); 

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

                pcmBytes[i * 2] = (byte)(pcmSample & 0xFF);
                pcmBytes[i * 2 + 1] = (byte)((pcmSample >> 8) & 0xFF);
            }

            PlaySound(pcmBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AudioOpusDecodingAndPlay DecodingFrames Ошибка: {ex.Message}");
        }
    }

    public static void PlaySound(byte[] pcmBytes)
    {
        if (!_initialized)
        {
            var waveFormat = new WaveFormat(_sampleRate, 16, _channels);

            _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = 4096, // минимально допустимый буфер
                BufferDuration = TimeSpan.FromMilliseconds(100), // если поддерживается
                DiscardOnBufferOverflow = true
            };

            _waveOut = new WaveOutEvent()
            {
                DesiredLatency = 50,
                NumberOfBuffers = 3 // меньше буферов = ниже задержка, но больше шанс заикания
            };

            _waveOut.Init(_bufferedWaveProvider);
            _waveOut.Play();

            _initialized = true;
        }

        _bufferedWaveProvider.AddSamples(pcmBytes, 0, pcmBytes.Length);

    }
}

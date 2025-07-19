using NAudio.Wave;

namespace BlazorAppExactlyWebAssembly.AudioService;

public class AudioPlayerService
{
    private readonly BufferedWaveProvider _provider;
    private readonly IWavePlayer _waveOut;
    private bool _started = false;

    public AudioPlayerService()
    {
        var format = new WaveFormat(8000, 16, 1); // 8000 Hz, 16bit, mono
        _provider = new BufferedWaveProvider(format)
        {
            DiscardOnBufferOverflow = true
        };

        _waveOut = new WaveOutEvent();
        _waveOut.Init(_provider);
    }

    public void PlayRawPcm(byte[] buffer)
    {
        _provider.AddSamples(buffer, 0, buffer.Length);

        if (!_started)
        {
            _waveOut.Play();
            _started = true;
        }
    }
}

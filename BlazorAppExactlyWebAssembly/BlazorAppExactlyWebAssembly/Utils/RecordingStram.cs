using System.Threading.Channels;

namespace SoundBlazorApp.Utils;

public class RhqVoiceRecordStream
{

    private int _position;
    private Channel<byte> _sampleQueue;
    private int _skippedBytesCount;
    private int _stoppedRaw;

    private readonly ILogger logger;

    private bool _stopped => _stoppedRaw > 0;

    public RhqVoiceRecordStream(
        ILogger logger
        )
    {
        _position = 0;
        _skippedBytesCount = 0;
        _stoppedRaw = 0;
        _sampleQueue = Channel.CreateUnbounded<byte>();
        this.logger = logger;
    }

    public void Stop()
    {
        int oldStoppedRaw = Interlocked.Exchange(ref _stoppedRaw, 1);
        if (oldStoppedRaw == 0)
        {
            // has been stopped:

            // by this we release all waiting readers:
            // _sampleQueue.Writer.Complete(); 
            _sampleQueue.Writer.TryComplete(); // safer paranoid code
        }
    }


    public void Add(ReadOnlySpan<byte> data)
    {
        if (_stopped)
        {
            return;
        }

        // TODO: use stopwatch too detect 'too-long' pause between two call of the add() and to long processing inside the Add() method - write logWarning/ logCritical in suach case

        // logger.LogCritical("VRS: adding voice data portion: {data.Length} bytes", data.Length);
        logger.LogTrace("VRS: adding voice data portion: {data.Length} bytes", data.Length);

        var writer = _sampleQueue.Writer;
        for (int i = 0; i < data.Length; i++)
        {
            if (writer.TryWrite(data[i]))
            {
                Interlocked.Increment(ref _position);
            }
            else if (_stopped)
            {
                // it is normal case - we cannot write after stop because channel has been completed
            }
            else
            {
                Interlocked.Increment(ref _skippedBytesCount);
            }
        }

        logger.LogTrace("VRS: added voice data portion: {data.Length} bytes, {_skippedBytesCount} skipped bytes", data.Length, _skippedBytesCount);
    }

    public async Task<bool> WaitWritingThreshold(int bytesCount)
    {
        var reader = _sampleQueue.Reader;

        while (!_stopped)
        {
            await reader.WaitToReadAsync();

            // TEMP solution - we could stop our waiting later then it happens:
            int currentPosition = _position;
            int bytesInQueue = reader.Count; // at this moment position could be increased but it is ok for our decision
            if (bytesInQueue + currentPosition >= bytesCount)
            {
                return true;
            }
        }

        return false;
    }


    public async Task<(bool active, int bufferedBytes)> WaitBuffered(int bytesCount)
    {

        var reader = _sampleQueue.Reader;

        //while (!_stopped)
        //{
        //    var completed = !await reader.WaitToReadAsync();

        //    // even after writer has been completed we could have enough data:
        //    int bytesInQueue = reader.Count;
        //    if (bytesInQueue >= bytesCount)
        //    {
        //        return true;
        //    }

        //    if (completed)
        //    {
        //        break;
        //    }
        //}

        //logger.LogCritical("VRS: waiting buffered");

        while (await reader.WaitToReadAsync())
        {
            if (_stopped)
            {
                break;
            }

            // FUTURE: use spin-semaphore to coordinate with Add() method:
            await Task.Delay(2); // to give chance for writing process to finish  copping recorded data

            // even after writer has been completed we could have enough data:
            int bytesInQueue = reader.Count;
            if (bytesInQueue >= bytesCount)
            {
                //logger.LogCritical("VRS: wait buffered finished");
                return (true, bytesInQueue);
            }
        }

        //logger.LogCritical("VRS: wait buffered aborted");
        return (false, reader.Count);
    }


    public bool ReadPortionOrZeroAsync(Span<byte> buffer)
    {
        var reader = _sampleQueue.Reader;
        bool noZeros = true;

        for (int i = 0; i < buffer.Length; i++)
        {
            if (reader.TryRead(out byte b))
            {
                buffer[i] = b;
            }
            else
            {
                buffer[i] = 0;
                noZeros = false;
            }
        }

        return noZeros;
    }
}

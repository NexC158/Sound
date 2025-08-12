using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using BlazorAppExactlyWebAssembly.ServerWorkingWithAudio;

namespace BlazorAppExactlyWebAssembly.ServerAudioWorking;

public class PipeLineMethods
{
    private const int _minBufferSize = 256;
    public async Task FillPipeAsync(Stream body, PipeWriter pipeWriter)
    {
        while (true)
        {
            try
            {
                Memory<byte> memory = pipeWriter.GetMemory(_minBufferSize);

                int bytesRead = await body.ReadAsync(memory);

                if (bytesRead == 0)
                {
                    Console.WriteLine("PipeLineMethods FillPipeAsync прочитано 0 байт");
                    break;
                }

                pipeWriter.Advance(bytesRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PipeLineMethods FillPipeAsync ошибка: {ex.ToString()}");
                break;
            }

            FlushResult flushResult = await pipeWriter.FlushAsync();

            if(flushResult.IsCompleted)
            {
                break;
            }
        }
        await pipeWriter.CompleteAsync();
    }

    public async Task ReadPipeAsync(PipeReader pipeReader)
    {
        while (true)
        {
            ReadResult readResult = await pipeReader.ReadAsync();
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            var consumed = ProcessBuffer(buffer);            

            pipeReader.AdvanceTo(consumed, buffer.End);

            if (readResult.IsCompleted)
            {
                break;
            }
        }
        await pipeReader.CompleteAsync();
    }

    public static SequencePosition ProcessBuffer(in ReadOnlySequence<byte> buffer) 
    {
        var reader = new SequenceReader<byte>(buffer);
        var consumed = buffer.Start;

        while (TryReadFrame(ref reader, out ReadOnlySpan<byte> frame))
        {
            AudioOpusDecodingAndPlay.DecodingFrames(frame);
            consumed = reader.Position;
        }
        return consumed;
    }

    public static bool TryReadFrame(ref SequenceReader<byte> sequenceReader, out ReadOnlySpan<byte> frame)
    {
        frame = default;

        short frameLength;

        if (!sequenceReader.TryReadLittleEndian(out frameLength))
        {
            return false;
        }

        if (sequenceReader.Remaining < frameLength)
        {
            sequenceReader.Rewind(2);
            return false;
        }

        frame = sequenceReader.UnreadSpan.Slice(0, frameLength);
        sequenceReader.Advance(frameLength);

        return true;
    }
}

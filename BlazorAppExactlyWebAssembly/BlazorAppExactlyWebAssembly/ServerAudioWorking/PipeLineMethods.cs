using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using BlazorAppExactlyWebAssembly.ServerWorkingWithAudio;

namespace BlazorAppExactlyWebAssembly.ServerAudioWorking
{
    public class PipeLineMethods
    {
        private const int _minBufferSize = 256; // протестировать с минимально возможным буфером (нужен макс опус фрейм)
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

                ProcessBuffer(buffer);

                pipeReader.AdvanceTo(buffer.End); // переделать buffer.End 
                                                  // ProcessBuffer должен мне отдать позицию, на которую я буду AdvanceTo
                if (readResult.IsCompleted)
                {
                    break;
                }
            }
            await pipeReader.CompleteAsync();
        }

        public static void ProcessBuffer(in ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);

            while (TryReadFrame(ref reader, out ReadOnlySpan<byte> frame))
            {
                AudioOpusDecodingAndPlay.DecodingFrames(frame);
            }
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

            frame = sequenceReader.UnreadSpan.Slice(0, frameLength); // посмотреть UnreadSpan и Slice. Тут возможно свинство 
            sequenceReader.Advance(frameLength);
            //Console.WriteLine($"AudioOpusDecodingAndPlay TryReadFrame принял фрейм опуса {frameLength}");
            return true;
        }
    }
}

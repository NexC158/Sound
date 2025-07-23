[HttpPost]
[Route("/VoiceLibrary/Upload/")]
public async Task<IActionResult> UploadVoiceFile(IFormFile formFile)
{
    string formFileName = formFile.FileName;
    using var rs = formFile.OpenReadStream();

    // rs.ReadAtLeastAsync()

    RhqResult res = await this.service.UploadFile(rs, formFileName);

    if (res.IsFailed)
    {
        return this.Problem(res.Message.Localized, formFileName, 400, "сбой при загрузке файла");
    }

    // TEMP CODE for test
    if (formFileName.Contains("error"))
    {
        return this.Problem("имя содержит 'error'", formFileName, 400, "сбой при загрузке файла");
    }

    return Ok(new { recordKey = formFileName });
}



https://learn.microsoft.com/ru-ru/aspnet/core/signalr/messagepackhubprotocol?view=aspnetcore-9.0 :::
{

        на клиенте установил npm install @microsoft/signalr-protocol-msgpack
        из-за этого добавилась папка node_modules и из-за этого пришлось лезть в tsconfig.json
}

Влез в tsconfig.json  добавил 
{
"target": "es2015", // или выше: 'es2020', 'esnext' и т.д.
"lib": [ "es2015", "dom" ],
"moduleResolution": "nodenext",
"module": "nodenext"
}

подрубил https://github.com/tj10200/raw-opus-stream-recorder


## work with pipi

sync Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken stopReceivingTocken)
{
    while (true)
    {
        if (stopReceivingTocken.IsCancellationRequested)
        {
            break;
        }
        // Allocate at least 512 bytes from the PipeWriter
        Memory<byte> memory = writer.GetMemory(_options.minimumBufferSize);
        try
        {
            int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, stopReceivingTocken);
            if (bytesRead == 0) 
            {
                break;
            }
            // Tell the PipeWriter how much was read from the Socket
            writer.Advance(bytesRead);
        }
        catch (OperationCanceledException err)
        {
            break;
        }
        catch (SocketException err)
        {
            break;
        }
        catch (Exception err)
        {
            break;
        }
        // Make the data available to the PipeReader
        FlushResult readingResult = await writer.FlushAsync();
        if (readingResult.IsCompleted)
        {
            break;
        }
    }
    // Tell the PipeReader that there's no more data coming
    writer.Complete();
    return;
}
async Task ReadPipeAsync(PipeReader reader, IDCSubscriptionPublisher<TFrameHeader> subscriptions)
{
    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;
        var unprocessed = ProcessReceivedBuffer(in buffer, subscriptions);
        if (unprocessed.Start.Equals(unprocessed.End))
        {
            if (DebugMode.Spam)
            { log.LogTrace("read-pipe: processed all bytes."); }
        }
        else
        {
            log.LogTrace("read-pipe: processed with unprocessed bytes.");
        }
        // Tell the PipeReader how much of the buffer we have consumed
        reader.AdvanceTo(unprocessed.Start, unprocessed.End);
        if (unprocessed.protocolError is not null)
        {
            protocolError = unprocessed.protocolError;
            log.LogError("read-pipe: processing stopped because protocol error: {reason}.", protocolError.Value.ToLog());
            break;
        }
        // Stop reading if there's no more data coming
        if (result.IsCompleted)
        {
            log.LogDebug("read-pipe: processing stopped because result is-completed");
            break;
        }
    }
    // Mark the PipeReader as complete
    reader.Complete();
}
	  
	  
private (SequencePosition Start, SequencePosition End, RhqText? protocolError) ProcessReceivedBuffer(
    in ReadOnlySequence<byte> receivedBuffer,
    IDCSubscriptionPublisher<TFrameHeader> subscriptions)
{
ReadOnlySequence<byte> buffer = receivedBuffer;
ReadOnlySequence<byte> remainingBuffer = receivedBuffer;
SequenceReader<byte> reader = new SequenceReader<byte>(receivedBuffer);
int indexInPacket = 0;
int _staticHeaderSize = 2;
do
{
if (reader.Remaining == 0)
{
    // no more bytes in packet
        break;
    }

    if (reader.Remaining < _staticHeaderSize)
    {
        // lets wait rest of  bytes for header
        break;
    }

    int size;
    var headerResult = ParseHeader(ref reader, out size, out var header, out RhqText headerProtocolError);

    if (headerResult is ParseHeaderResult.ProtocolError)
    {
        // DONE: abort connection
        protocolError = headerProtocolError;
        break;
    }
    else if (headerResult is ParseHeaderResult.ToFrame)
    {
        // nothing to do , lets go to frame processing
        // size is frame size
    }
    else
    {
        // lets abort connection:
        protocolError = new RhqText("неизвестный тип заголовка.".ю(), $"unsupported header result {headerResult}.");
        break;
    }

    if (reader.Remaining < size)
    {
        // lets wait rest of bytes for the frame
        break;
    }

    if (size <= 0 || size > _options.maxBytesInFrame)
    {
        // FUTURE: log warning ???
    }



    
    var loggingReader = new SequenceReader<byte>(reader.UnreadSequence);
    LogFrame(loggingReader, size, header, idxInStream);

    var frameReader = new SequenceReader<byte>(reader.UnreadSequence);
    PublishFrame(in frameReader, in header, subscriptions, size, idxInStream);

    reader.Advance(size);
    
    // not work! : remainingBuffer = reader.UnreadSecuence ; - size could be read already
    remainingBuffer = receivedBuffer.Slice(reader.Consumed); // reader.Cosumed include processing of all frames in the packet
    
}
while (true);

return (remainingBuffer.Start, remainingBuffer.End, protocolError);
}



## RecordingStream
using System.Threading.Channels;

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


Это было в package-lock.json

{
  "name": "BlazorAppExactlyWebAssembly.Client",
  "lockfileVersion": 3,
  "requires": true,
  "packages": {
    "": {
      "dependencies": {
        "@wasm-audio-decoders/common": "^9.0.7",
        "opus-recorder": "^8.0.5"
      },
      "devDependencies": {
        "@types/node": "^24.0.14"
      }
    },
    "node_modules/@eshaz/web-worker": {
      "version": "1.2.2",
      "resolved": "https://registry.npmjs.org/@eshaz/web-worker/-/web-worker-1.2.2.tgz",
      "integrity": "sha512-WxXiHFmD9u/owrzempiDlBB1ZYqiLnm9s6aPc8AlFQalq2tKmqdmMr9GXOupDgzXtqnBipj8Un0gkIm7Sjf8mw==",
      "license": "Apache-2.0"
    },
    "node_modules/@types/node": {
      "version": "24.0.14",
      "resolved": "https://registry.npmjs.org/@types/node/-/node-24.0.14.tgz",
      "integrity": "sha512-4zXMWD91vBLGRtHK3YbIoFMia+1nqEz72coM42C5ETjnNCa/heoj7NT1G67iAfOqMmcfhuCZ4uNpyz8EjlAejw==",
      "dev": true,
      "license": "MIT",
      "dependencies": {
        "undici-types": "~7.8.0"
      }
    },
    "node_modules/@wasm-audio-decoders/common": {
      "version": "9.0.7",
      "resolved": "https://registry.npmjs.org/@wasm-audio-decoders/common/-/common-9.0.7.tgz",
      "integrity": "sha512-WRaUuWSKV7pkttBygml/a6dIEpatq2nnZGFIoPTc5yPLkxL6Wk4YaslPM98OPQvWacvNZ+Py9xROGDtrFBDzag==",
      "license": "MIT",
      "dependencies": {
        "@eshaz/web-worker": "1.2.2",
        "simple-yenc": "^1.0.4"
      }
    },
    "node_modules/opus-recorder": {
      "version": "8.0.5",
      "resolved": "https://registry.npmjs.org/opus-recorder/-/opus-recorder-8.0.5.tgz",
      "integrity": "sha512-tBRXc9Btds7i3bVfA7d5rekAlyOcfsivt5vSIXHxRV1Oa+s6iXFW8omZ0Lm3ABWotVcEyKt96iIIUcgbV07YOw==",
      "license": "MIT"
    },
    "node_modules/simple-yenc": {
      "version": "1.0.4",
      "resolved": "https://registry.npmjs.org/simple-yenc/-/simple-yenc-1.0.4.tgz",
      "integrity": "sha512-5gvxpSd79e9a3V4QDYUqnqxeD4HGlhCakVpb6gMnDD7lexJggSBJRBO5h52y/iJrdXRilX9UCuDaIJhSWm5OWw==",
      "license": "MIT",
      "funding": {
        "type": "individual",
        "url": "https://github.com/sponsors/eshaz"
      }
    },
    "node_modules/undici-types": {
      "version": "7.8.0",
      "resolved": "https://registry.npmjs.org/undici-types/-/undici-types-7.8.0.tgz",
      "integrity": "sha512-9UJ2xGDvQ43tYyVMpuHlsgApydB8ZKfVYTsLDhXkFL/6gfkp+U8xTGdh8pMJv1SpZna0zxG1DwsKZsreLbXBxw==",
      "dev": true,
      "license": "MIT"
    }
  }
}



а это в package.json
{
  "dependencies": {
    "@wasm-audio-decoders/common": "^9.0.7",
    "opus-recorder": "^8.0.5"
  },
  "devDependencies": {
    "@types/node": "^24.0.14"
  }
}

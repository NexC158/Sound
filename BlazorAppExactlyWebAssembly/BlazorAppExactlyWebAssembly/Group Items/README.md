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

     async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken stopReceivingTocken)
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


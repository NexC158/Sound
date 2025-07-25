﻿using System.IO.Pipelines;
using BlazorAppExactlyWebAssembly.ServerWorkingWithAudio;
using Microsoft.AspNetCore.Mvc;

using BlazorAppExactlyWebAssembly.ServerAudioWorking;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{

    [HttpPost("stream")]
    public async Task<IActionResult> UploadStreamWithPipe()
    {
        //Console.WriteLine("AudioController UploadStream стартует...\n");

        var pipe = new Pipe();
        var workPipe = new PipeLineMethods();
        var writing = workPipe.FillPipeAsync(Request.Body, pipe.Writer);
        _ = workPipe.ReadPipeAsync(pipe.Reader);

        await Task.WhenAll(writing);

        //Console.WriteLine("AudioController UploadStreamWithPipe завершен\n");

        return Ok();
    }


    [HttpPost("streamBadBuffer")]
    public async Task<IActionResult> UploadStreamWithBadBuffer()
    {
        var buffer = new byte[512]; //  128, 256, 512, 1024, 2048, 4096

        List<byte> inputBuffer = new List<byte>();

        int bytesRead;
        long total = 0;

        Console.WriteLine("AudioController UploadStream Старт приёма аудиопотока...\n");

        while ((bytesRead = await Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0) // логику ниже можно засунуть в одельный метод
        {
            total += bytesRead;

            Console.WriteLine($"AudioController UploadStream прочитал: {bytesRead}\n");

            inputBuffer.AddRange(buffer.Take(bytesRead));
            

            while (inputBuffer.Count >= 2)
            {
                var header = inputBuffer.GetRange(0, 2).ToArray();
                var realOpusFrameLength = BitConverter.ToUInt16(header, 0);

                Console.WriteLine($"UploadStream заголовок: {realOpusFrameLength}");

                if (inputBuffer.Count < realOpusFrameLength + 2)
                {
                    Console.WriteLine("AudioController UploadStream [break]");
                    break;
                }

                byte[] opusFrame = inputBuffer.Skip(2).Take(realOpusFrameLength).ToArray();

                Console.WriteLine($"UploadStream размер кадра: {opusFrame.Length}");

                inputBuffer.RemoveRange(0, realOpusFrameLength + header.Length);

                AudioOpusDecodingAndPlay.DecodingFrames(opusFrame);                
            }
        }

        Console.WriteLine("Конец потока");

        return Ok(new { bytesReceived = total });
    }
}

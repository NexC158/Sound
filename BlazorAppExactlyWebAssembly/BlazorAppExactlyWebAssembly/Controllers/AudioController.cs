using BlazorAppExactlyWebAssembly.ServerWorkingWithAudio;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{

    [HttpPost("stream")]
    public async Task<IActionResult> UploadStream()
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

                //Console.WriteLine($"AudioController UploadStream передал в декодинг {opusFrame.Length}\n");
                AudioOpusDecodingAndPlay.DecodingFrames(opusFrame);                
            }
        }

        Console.WriteLine("Конец потока");

        return Ok(new { bytesReceived = total });
    }
}



//[HttpPost("stream")]
//public async Task<IActionResult> UploadStream()
//{
//    Console.WriteLine("AudioController UploadStream");
//    string uploadsPath = "Uploads";
//    Directory.CreateDirectory(uploadsPath);

//    string fileName = $"audio_{DateTime.Now:yyyyMMdd_HHmmss}.raw";
//    string fullPath = Path.Combine(uploadsPath, fileName);

//    try
//    {
//        using var output = System.IO.File.Create(fullPath);
//        await Request.Body.CopyToAsync(output); // вот тут получаю поток
//        Console.WriteLine($"AudioController UploadStream; закончился await Request.Body.CopyToAsync(output) {output}");
//        return Ok(new { message = "поток байт получен", file = fileName });
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, new { error = "Ошибка AudioController UploadStream", ex.Message });
//    }
//}


//[HttpPost("stream")]
//public async Task<IActionResult> UploadStream2()
//{
//    var filePath = Path.Combine("Uploads", $"stream_{DateTime.Now:yyyyMMdd_HHmmss}.raw");
//    Directory.CreateDirectory("Uploads");

//    using var fileStream = System.IO.File.Create(filePath);
//    var buffer = new byte[4096];
//    int bytesRead;

//    while ((bytesRead = await Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
//    {
//        // можно анализировать buffer[0..bytesRead] прямо здесь
//        await fileStream.WriteAsync(buffer, 0, bytesRead);
//    }

//    return Ok();
//}
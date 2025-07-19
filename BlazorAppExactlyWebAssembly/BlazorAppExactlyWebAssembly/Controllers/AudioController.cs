using BlazorAppExactlyWebAssembly.AudioService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{

    [HttpPost("stream")]
    public async Task<IActionResult> UploadStream()
    {
        var buffer = new byte[256]; //  128, 256, 512, 1024, 2048, 4096
        int bytesRead;
        long total = 0;

        Console.WriteLine("AudioController UploadStream Старт приёма аудиопотока...\n");

        while ((bytesRead = await Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            total += bytesRead;

            
            for (int i = 0; i < bytesRead; i++) // вывожу каждый принятый байт
            {
                // Console.Write($"{buffer[i]:X2} "); // хекс
                Console.Write($"{buffer[i]} "); // десятичная
            }

            Console.WriteLine($"\nПринято: {bytesRead} байт | Всего: {total} байт\n");
        }

        Console.WriteLine("Конец потока");

        return Ok(new { bytesReceived = total });
    }


    [HttpPost("play")]
    public async Task<IActionResult> PlayAudioStream()
    {
        var buffer = new byte[256];
        using var memoryStream = new MemoryStream();
        int bytesRead;

        while ((bytesRead = await Request.Body.ReadAsync(buffer)) > 0)
        {
            memoryStream.Write(buffer, 0, bytesRead);
        }

        byte[] audioBytes = memoryStream.ToArray();

        // Воспроизведение:
        var player = new AudioPlayerService(); // или через DI
        player.PlayRawPcm(audioBytes);

        return Ok();
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
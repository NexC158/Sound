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



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
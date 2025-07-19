let mediaStream = null;
let audioContext = null;;
let audioWorkletNode = null;;
let source; /////// вот это проверить

let isTransmitting = false; // Флаг активности передачи
let portHandler = null; 
let subject = null;

const connectionForAudioHub = new signalR
    .HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .withAutomaticReconnect() // хз
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol()) // new signalR.protocols.msgpack.MessagePackHubProtocol()  new MessagePackHubProtocol()
    .build();



connectionForAudioHub.on("OnCustomCommandStart", async () => {

    await startTranslate();
    
});

connectionForAudioHub.on("OnCustomCommandStop", () => {

    stopTranslate();
});

connectionForAudioHub.start();

async function requestMicrophoneAccess() {

    if (mediaStream === null) {

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
            isTransmitting = true;
            //alert('Доступ к микрофону получен!');
        } catch (err) {
            alert('Нет доступа к микрофону: ' + err);
        }
    }
}

async function startTranslate() {

    if (isTransmitting) {

        console.log('Передача уже запущена');
        return;
    }
    await requestMicrophoneAccess();
    if (!mediaStream) {
        return;
    }

    if (audioContext) {
        await audioContext.close(); // закрываю на всякий случай, если он уже есть (на случай повторного старта)
        audioContext = null;
    }

    subject = new signalR.Subject();

    //send - для отправки на сервер\\ //stream - для отправки с сервера\\ //invoke - не знаю зачем\\
    await connectionForAudioHub.send("ReceiveAudioStream", subject); // ReceiveAudioStream ReceiveAudioChunk

    audioContext = new window.AudioContext({ sampleRate: 8000 });

    await audioContext.audioWorklet.addModule('js/audioProcessorClient.js');
    audioWorkletNode = new AudioWorkletNode(audioContext, 'audioProcessorClient');

    /*source = */audioContext.createMediaStreamSource(mediaStream).connect(audioWorkletNode);
    /*source.connect(audioWorkletNode);*/

    audioWorkletNode.connect(audioContext.destination);// если без destination, то обработка будет идти не в средство вывода а на сервер

    portHandler = async (event) => { // ВАЖНО: нужно что-то сделать, чтобы обработчик адекватно отвечал на команду стоп
        try {

            // \\
            // вот этим способом я не вижу в консоли mvs уровень громкости, походу нужно будет вернуться к старому методу
            if (!isTransmitting) { 

                return;
            }
            const float32Array = event.data;
            const uint8 = new Uint8Array(float32Array.buffer);

            for (i = 0; i < uint8.length; i++) {
                await subject.next(uint8[i]); // короче  вроде как можно передавать float32Array, но на сервере надо будет обрабатывать
                                              // это как сэмплы а не байты
            }
            console.log('чанк в audioWorkletNode.port.onmessage:::', uint8);
            // \\


            //const int16Array = new Int16Array(float32Array.length); // вот тут нужно пошаманить

            //for (let i = 0; i < float32Array.length; i++) {

            //    // Способ 0
            //    //int16Array[i] = Math.min(1, Math.max(-1, float32Array[i])) * 0x7FFF; // преобразование каждого сэмпла в диапазон [-32767, 32767]
            //    ////\\

            //    //// Способ 1 быстрее чем способ 0
            //    //let s = float32Array[i];
            //    //s = s < -1 ? -1 : (s > 1 ? 1 : s);
            //    //int16Array[i] = (s * 0x7FFF) | 0;
            //    //\\

            //    // Способ 2 быстрее чем 1
            //    int16Array[i] = float32Array[i] * 0x7FFF | 0; // хз че за |0

            //}

            //const uint8 = new Uint8Array(int16Array.buffer);

            //for (i = 0; i < uint8.length; i++)
            //{
            //    await subject.next(uint8[i]);
            //}
            //console.log('чанк в audioWorkletNode.port.onmessage:::', uint8);


        }
        catch (err) {
            console.error("Ошибка получения данных из аудиопроцессора:", err);
        }
    };


    audioWorkletNode.port.onmessage = portHandler;
    isTransmitting = true;
}

function stopTranslate() {

    console.log('Команда остановки передачи')
    if (!isTransmitting) {

        console.log('Передача уже остановлена');
        return;
    }
    isTransmitting = false;

    // Отключаем обработчик, чтобы не отправлять чанки
    if (audioWorkletNode && portHandler) {

        audioWorkletNode.port.onmessage = null;
        subject.complete();
        subject = null;
    }

    // Если нужно полностью освободить микрофон (а не только треки (до конца не понял))
    //if (mediaStream) mediaStream.getTracks().forEach(track => track.stop());
    //mediaStream = null;

    // Можно закрыть аудиоконтекст, если не нужен до следующего старта:
    if (audioContext) {

        audioContext.close();
        audioContext = null;
    }
    audioWorkletNode = null;
    portHandler = null;

    console.log('Передача звука остановлена, но соединение осталось открытым');
}






//send stream invoke
//connectionForAudioHub.invoke("ReceiveAudioChunk", subject); //  GetBytesFromAudioStream

//yield connectionForAudioHub.send("GetBytesFromAudioStream", subject);
//var iteration = 0;
//const intervalHandle = setInterval(() => {
//    iteration++;
//    subject.next(iteration.toString());
//    if (iteration === 10) {
//        clearInterval(intervalHandle);
//        subject.complete();
//    }
//}, 500);




///audioWorkletNode.port.onmessage = e => subject.next(e.data); // пробую вместо верхнего

//const CHUNK_SIZE = 512; // 1024 2048 4096

//portHandler = e => { // вот это кое-как работает

//    if (!isTransmitting) {

//        return;
//    }

//    const data = new Uint8Array(1024);

//    for (let i = 0; i < data.length; i += CHUNK_SIZE) {

//        const chunk = data.slice(i, i + CHUNK_SIZE);
//        console.log('чанк в audioWorkletNode.port.onmessage:::', chunk);
//        connectionForAudioHub.on("ReceiveAudioChunk", chunk);
//    }
//};



//audioWorkletNode.port.onmessage = e => {
//    connectionForAudioHub.send("ReceiveAudioChunk", e.data);
//};
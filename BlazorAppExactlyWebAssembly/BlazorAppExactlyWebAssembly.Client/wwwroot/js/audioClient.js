let mediaStream = null;
let audioContext = null;;
let audioWorkletNode = null;;
let source; /////// вот это проверить

let isTransmitting = false; // Флаг активности передачи
let portHandler = null; 
let subject = null;



alert('запустился audioClient.js');

const connectionForAudioHub = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .build();

connectionForAudioHub.on("OnCustomCommandStart", async () => {

    //alert('была нажата кнопка на начало трансляции звука: должен сработать startTranslate()');
    await startTranslate();
    
});

connectionForAudioHub.on("OnCustomCommandStop", () => {

    //alert('сработал connection.on("stopTranslateAudio"');
    stopTranslate();
});

connectionForAudioHub.start(); // вызываю после обработчиков чтобы они не получили никаких сообщений до регистрации

async function requestMicrophoneAccess() {

    if (mediaStream === null) {

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
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

    await connectionForAudioHub.send("ReceiveAudioChunk", subject); // ReceiveAudioStream ReceiveAudioChunk

    audioContext = new window.AudioContext({ sampleRate: 8000 });

    await audioContext.audioWorklet.addModule('js/audioProcessorClient.js');
    audioWorkletNode = new AudioWorkletNode(audioContext, 'audioProcessorClient');

    /*source = */audioContext.createMediaStreamSource(mediaStream).connect(audioWorkletNode);
    /*source.connect(audioWorkletNode);*/

    audioWorkletNode.connect(audioContext.destination);// если без destination, то обработка будет идти не в средство вывода а на сервер



    portHandler = async (event) => { // обработчик на получение сообщений из аудиопроцессора audioProcessor
        try {

            if (!isTransmitting) {

                return;
            }
            const float32Array = event.data;
            const int16Array = new Int16Array(float32Array.length); // вот тут нужно пошаманить

            for (let i = 0; i < float32Array.length; i++) {

                int16Array[i] = Math.min(1, Math.max(-1, float32Array[i])) * 0x7FFF; // преобразование каждого сэмпла в диапазон [-32767, 32767]
            }

            const whatIsToSend = new Uint8Array(int16Array.buffer);
            console.log('чанк в audioWorkletNode.port.onmessage:::', whatIsToSend);
            
            subject.next(whatIsToSend); // отправка в hub командой subject.next
            
        }
        catch (err) {
            console.error("Ошибка получения данных из аудиопроцессора:", err);
        }
    };


    audioWorkletNode.port.onmessage = portHandler;
    isTransmitting = true;

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
}

function stopTranslate() {

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



let isTransmitting = false;
let controllerRef = null;
let chunkSize = 4; // количество Opus-пакетов в одном массиве 
let chunkBuffer = [];
let mediaStream = null;
let recorder = null;

const connectionForAudioHub = new signalR
    .HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    //.withAutomaticReconnect() // хз
    //.withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol()) // new signalR.protocols.msgpack.MessagePackHubProtocol()  new MessagePackHubProtocol()
    .build();

connectionForAudioHub.on("OnCustomCommandStart", async () => {

    await startTranslate();

});

connectionForAudioHub.on("OnCustomCommandStop", () => {

    stopTranslate();
});

connectionForAudioHub.start();

async function startTranslate() {

    if (mediaStream === null) {

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
            isTransmitting = true;
            // alert('Доступ к микрофону получен!');
        }
        catch (err) {
            alert('Нет доступа к микрофону: ' + err);
            return;
        }
    }

    try {
        const stream = new ReadableStream({ // стартую ReadableStream для POST

            start(controller) {

                controllerRef = controller;
                console.log('%c Stream started', 'color:green');
            },
            pull() { },
            cancel() {

                console.log('%c Stream cancelled', 'color:red');
                controllerRef = null;
            }
        });

        fetch('/api/audio/stream', { // отправка потока fetch-ом

            method: 'POST',
            body: stream,
            headers: {

                'Content-Type': 'application/octet-stream'
            },
            duplex: "half"
        }).then(res => console.log('поток успешно отправился:', res))
            .catch(err => console.error('Ошибка при отправке:', err));

        recorder = new Recorder({

            encoderSampleRate: 8000,
            encoderPath: 'js/forOpusMinJs/encoderWorker.min.js',
            encoderFrameSize: 20,
            bitrate: 16000, // 16000
            // cbr: true, 
            streamPages: true
        });

        recorder.ondataavailable = (typedArray) => {

            if (isTransmitting && typedArray.length > 0 && controllerRef) {

                console.log('массив в опусе', typedArray);

                const len = typedArray.length;
                const header = new Uint8Array([len & 0xff, (len >> 8) & 0xff]);
                const payload = new Uint8Array(len + 2);
                payload.set(header, 0);
                payload.set(typedArray, 2);

                // chunkBuffer.push(typedArray);
                //if (chunkBuffer.length >= chunkSize) {

                    // склейка всех opus страниц в один Uint8Array
                    //let totalLength = chunkBuffer.reduce((acc, arr) => acc + arr.length, 0);
                    //let merged = new Uint8Array(totalLength);
                    //let offset = 0;
                    //chunkBuffer.forEach(arr => {
                    //    merged.set(arr, offset);
                    //    offset += arr.length;
                    //});
                    //controllerRef.enqueue(merged);
                    //chunkBuffer = [];
                //}

                console.log('Отсылаю массив в опусе с добавленной длинной', payload);

                controllerRef.enqueue(payload);
            }
        };
        recorder.start(mediaStream);
    }
    catch (err) {
        console.error("Ошибка получения данных из аудиопроцессора:", err);
    }
}


function stopTranslate() {

    console.log('Команда остановки передачи')
    if (!isTransmitting) {

        console.log('Передача уже остановлена');
        return;
    }
    isTransmitting = false;

    recorder.stop();

    if (controllerRef) { // Закрытие стрима

        controllerRef.close();
        controllerRef = null;
    }

    if (mediaStream) {

        mediaStream.getTracks().forEach(track => track.stop());
        mediaStream = null;
    }

    console.log('Передача остановлена');
}

//function stopTranslate() {
//    isTransmitting = false;
//    recorder.stop();
//    if (controllerRef) {
//        controllerRef.close();
//        controllerRef = null;
//    }
//}
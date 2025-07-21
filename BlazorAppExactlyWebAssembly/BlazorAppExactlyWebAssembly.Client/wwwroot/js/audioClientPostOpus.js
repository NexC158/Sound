let isTransmitting = false;
let controllerRef = null;
let mediaStream = null;
let recorder = null;

let isStopping = false;
let stopResolve = null;
let stopPromise = null;

const connectionForAudioHub = new signalR
    .HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .withAutomaticReconnect() // хз
    //.withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();

connectionForAudioHub.on("OnCustomCommandStart", async () => {

    await startTranslate();

});

connectionForAudioHub.on("OnCustomCommandStop", async () => {

    await stopTranslate();
});

connectionForAudioHub.start();

async function startTranslate() {

    if (mediaStream === null) {

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });

            // alert('Доступ к микрофону получен!');
        }
        catch (err) {
            alert('Нет доступа к микрофону: ' + err);
            return;
        }
    }


    if (isTransmitting) {
        console.warn('Передача уже идёт');
        return;
    }

    if (recorder) {
        recorder.stop();
        recorder = null;
    }

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
        encoderFrameSize: 20,
        // bitrate: 16000, // 16000
        // cbr: true,
        streamPages: true,
        maxFramesPerPage: 1,
        rawOpus: true,
        encoderPath: 'lib/raw-opus-stream-recorder/dist/encoderWorker.min.js'
    });


    recorder.ondataavailable = (typedArray) => { // TODO: реализовать остановку только после того, как передастся то, что уже записывается
        // на 20 мс это будет незаметно, но вот такой минус тут есть

        if (isTransmitting || isStopping && typedArray.length > 0 && controllerRef) {

            const now = new Date().toISOString();

            //console.log('BYTES:', [...typedArray].slice(0, 8).map(b => b.toString(16)).join('-'), typedArray);
            console.log(`[ondataavailable] ${now} -> ${typedArray.length} байт`, typedArray);


            /*console.log('массив в опусе', typedArray);*/

            const len = typedArray.length;
            const header = new Uint8Array(2);
            header[0] = len & 0xff;
            header[1] = (len >> 8) & 0xff;

            const payload = new Uint8Array(len + header.length); // len + 2
            payload.set(header, 0);
            payload.set(typedArray, 2);



            //console.log('Отсылаю массив в опусе с добавленной длинной', payload);

            controllerRef.enqueue(payload);

            if (isStopping && stopResolve) {

                stopResolve();
                stopResolve = null;
            }
        }

    };
    await recorder.start(mediaStream);
    isTransmitting = true;

}


async function stopTranslate() {

    console.log('Команда остановки передачи');

    if (!isTransmitting) {

        console.log('Передача уже остановлена');
        return;
    }

    isTransmitting = false;
    isStopping = true;


    if (recorder) {

        recorder.stop();
        recorder = null;
    }


    if (controllerRef) {

        stopPromise = new Promise((resolve) => stopResolve = resolve);

        console.log("Ожидаю отправку последнего кадра...");
        await stopPromise;

        controllerRef.close();
        controllerRef = null;
        console.log('Последний кадр отправлен, поток закрыт');
    }

    if (mediaStream) {
        mediaStream.getTracks().forEach(track => track.stop());
        mediaStream = null;
    }

    isStopping = false;

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



// это было в recorder.ondataavailable:

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
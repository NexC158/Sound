let isTransmitting = false;
let controllerRef = null;
let mediaStream = null;
let recorder = null;

let isStopping = false;
let stopPromise = null;
let stopResolve = null;

const connectionForAudioHub = new signalR
    .HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .withAutomaticReconnect()
    .build();

connectionForAudioHub.on("OnCustomCommandStart", async () => {

    await startTranslate();

});

connectionForAudioHub.on("OnCustomCommandStop", async () => {

    await stopTranslate();
});

connectionForAudioHub.start();

async function startTranslate() {

    if (isTransmitting) {
        console.log('Передача уже идёт');
        return;
    }


    if (!mediaStream) {

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });

            // alert('Доступ к микрофону получен!');
        }
        catch (err) {
            alert('Нет доступа к микрофону: ' + err);
            return;
        }
    }

    isTransmitting = true;
    isStopping = false;

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

    if (!recorder) {

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


        recorder.ondataavailable = (typedArray) => {

            if (!controllerRef) {

                console.log('Поток закрыт');
                return;
            }

            if ((isTransmitting || isStopping) && typedArray.length > 0 && controllerRef) {

                const now = new Date().toISOString();

                //console.log('BYTES:', [...typedArray].slice(0, 8).map(b => b.toString(16)).join('-'), typedArray);
                console.log(`[ondataavailable] ${now} -> ${typedArray.length} байт`, typedArray);

                const len = typedArray.length;
                const header = new Uint8Array(2);
                header[0] = len & 0xff;
                header[1] = (len >> 8) & 0xff;

                const payload = new Uint8Array(len + header.length); // len + 2
                payload.set(header, 0);
                payload.set(typedArray, 2);

                //console.log('Отсылаю массив в опусе с добавленной длинной', payload);

                try {

                    controllerRef.enqueue(payload);
                } catch (enqueueEx) {

                    console.warn('Ошибка при enqueue: ', enqueueEx);
                }

                if (isStopping && stopResolve) {

                    stopResolve();
                    stopResolve = null;
                }
            }
        };
    }
    await recorder.start(mediaStream);
}


async function stopTranslate() {

    console.log('Команда остановки передачи');

    if (!isTransmitting) {

        console.log('Передача уже остановлена');
        return;
    }

    isTransmitting = false;
    isStopping = true;

    stopPromise = new Promise(resolve => stopResolve = resolve); // ожидаю отправку кадр после команды стоп

    if (recorder) {

        recorder.stop();
    }

    if (controllerRef) {

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
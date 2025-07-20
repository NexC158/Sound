let mediaStream = null;
let audioContext = null;
let audioWorkletNode = null;
let controllerRef = null;
let isTransmitting = false;

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

async function startTranslate() {

    if (isTransmitting) return;

    try {

        mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true }); // запрос микрофона

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

        audioContext = new AudioContext({ sampleRate: 8000 });
        await audioContext.audioWorklet.addModule('js/audioProcessorClient.js');

        audioWorkletNode = new AudioWorkletNode(audioContext, 'audioProcessorClient');

        const sourceNode = audioContext.createMediaStreamSource(mediaStream);
        sourceNode.connect(audioWorkletNode);

        // audioWorkletNode.connect(audioContext.destination); // только анализ:

        audioWorkletNode.port.onmessage = (event) => {

            if (!isTransmitting || !controllerRef) return;
            const float32Array = event.data;
            const int16Array = new Int16Array(float32Array.length);

            for (let i = 0; i < float32Array.length; i++) {

                let s = float32Array[i];
                s = s < -1 ? -1 : s > 1 ? 1 : s;
                int16Array[i] = (s * 32767) | 0;
            }
            
            const uint8Array = new Uint8Array(int16Array.buffer);
            console.log('audioClientForRestApi event :', uint8Array);
            controllerRef.enqueue(uint8Array); // Потоковая отправка
        };

        isTransmitting = true;
    } catch (err) {

        console.error('Ошибка доступа к микрофону:', err);
    }
}

function stopTranslate() {

    console.log('Команда остановки передачи')
    if (!isTransmitting) {

        console.log('Передача уже остановлена');
        return;
    }
    isTransmitting = false;

    if (controllerRef) { // Закрытие стрима

        controllerRef.close();
        controllerRef = null;
    }

    if (audioWorkletNode) {

        audioWorkletNode.port.onmessage = null;
        audioWorkletNode = null;
    }

    if (audioContext) {

        audioContext.close();
        audioContext = null;
    }

    if (mediaStream) {

        mediaStream.getTracks().forEach(track => track.stop());
        mediaStream = null;
    }

    console.log('Передача остановлена');
}
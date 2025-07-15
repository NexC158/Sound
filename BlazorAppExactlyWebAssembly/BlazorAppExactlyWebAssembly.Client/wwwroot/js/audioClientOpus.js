let isTransmitting = false;
let subject = null;

const connectionForAudioHub = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .build();

connectionForAudioHub.on("SignalRHubStartStreamingCommand", async () => {

    await startTranslate();
});

connectionForAudioHub.on("SignalRHubStopStreamingCommand", () => {

    stopTranslate();
});

connectionForAudioHub.start();

let recorder = null;

async function startTranslate() {

    if (isTransmitting) {

        return;
    }

    subject = new signalR.Subject();

    connectionForAudioHub.send("ReceiveAudioChunk", subject);

    recorder = new Recorder({
        encoderSampleRate: 16000,
        encoderPath: 'js/forOpusMinJs/encoderWorker.min.js',
        streamPages: true
    });

    recorder.ondataavailable = (typedArray) => {
        if (isTransmitting && typedArray.length > 0) {
            subject.next(typedArray);
        }
    };

    recorder.start();
    isTransmitting = true;
}

function stopTranslate() {
    if (!isTransmitting) return;
    isTransmitting = false;
    if (recorder) {
        recorder.stop();
        recorder = null;
    }
    if (subject) {
        subject.complete();
        subject = null;
    }
    console.log('Передача звука остановлена, соединение осталось открытым');
}

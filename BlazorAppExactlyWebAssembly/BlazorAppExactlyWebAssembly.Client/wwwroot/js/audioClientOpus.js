let isTransmitting = false;
let subject = null;

const connectionForAudioHub = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .build();

connectionForAudioHub.on("OnCustomCommandStart", async () => {

    await startTranslate();
});

connectionForAudioHub.on("OnCustomCommandStop", () => {

    stopTranslate();
});

connectionForAudioHub.start();

let recorder = null;

async function startTranslate() {

    if (isTransmitting) {

        return;
    }

    subject = new signalR.Subject();

    connectionForAudioHub.send("ReceiveAudioStream", subject);

    recorder = new Recorder({
        encoderSampleRate: 8000,
        encoderPath: 'js/forOpusMinJs/encoderWorker.min.js',
        streamPages: true
    });

    recorder.ondataavailable = (typedArray) => {
        try {
            if (isTransmitting && typedArray.length > 0) {
                console.log('Опус передает', typedArray)
                for (i = 0; i < typedArray.length; i++) {
                    subject.next(typedArray[i]);
                }
            }
        }
        catch (err) {
            console.error("Ошибка получения данных из аудиопроцессора:", err);
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

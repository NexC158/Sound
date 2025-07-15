const connectionForAudioHub = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7069/hubs/audiohub")
    .build();

connectionForAudioHub.on("SignalRHubStartStreamingCommand", async () => {

    //alert('была нажата кнопка на начало трансляции звука: должен сработать startTranslate()');
    await startTranslate();

});

connectionForAudioHub.on("SignalRHubStopStreamingCommand", () => {

    //alert('сработал connection.on("stopTranslateAudio"');
    stopTranslate();
});

connectionForAudioHub.start();

async function startTranslate() {

    navigator.mediaDevices.getUserMedia({ audio: true })
        .then(async stream => {
            const recorder = new MediaRecorder(stream);
            const subject = new signalR.Subject();

            recorder.ondataavailable = e => e.data.arrayBuffer()
                .then(buf => subject.next(new Uint8Array(buf)));

            await audioConn.start();
            await audioConn.send("ReceiveAudioStream", subject);
            recorder.start(200); // каждые 200 мс
        });;

}

function stopTranslate() {

    connectionForAudioHub.stop();
}
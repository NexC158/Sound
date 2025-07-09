let subject;
let mediaStream = null;
let audioContext;
let audioWorkletNode;
let source; /////// вот это проверить

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/audiohub")
    .build();

connection.start();

connection.on("startTranslateAudio", async () => {

    await startTranslate();
});

connection.on("stopTranslateAudio", () => {

    stopTranslate();
});


async function requestMicrophoneAccess() {

    if (mediaStream === null) {

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
            alert('Доступ к микрофону получен!');
        } catch (err) {
            alert('Нет доступа к микрофону: ' + err);
        }
    }
}

async function startTranslate() {

    if (!mediaStream) {
        await requestMicrophoneAccess();
        if (!mediaStream) return;
    }


    alert("before subject = new signalR.Subject();");
    subject = new signalR.Subject();

    let dbg2 = await connection.invoke("GetHelloWorld");
    alert(dbg2);
    alert(connection.state);

    connection.send("GetBytesFromAudioStream", subject);

    audioContext = new window.AudioContext({ sampleRate: 8000 });

    await audioContext.audioWorklet.addModule('js/audioProcessorClient.js');
    audioWorkletNode = new AudioWorkletNode(audioContext, 'audioProcessorClient');

    /*source = */audioContext.createMediaStreamSource(mediaStream).connect(audioWorkletNode);
    /*source.connect(audioWorkletNode);*/

    

    //audioWorkletNode.port.onmessage = (event) => { // обработчик на получение сообщений из аудиопроцессора audioProcessor
    //    try {
    //        const float32Array = event.data; // тут можно кодировать в opus помощью libopus.js
    //        // const opusFrame = opusEncoder.encode(pcm);
    //        const int16Array = new Int16Array(float32Array.length); // вот тут нужно пошаманить

    //        for (let i = 0; i < float32Array.length; i++) {

    //            int16Array[i] = Math.min(1, Math.max(-1, float32Array[i])) * 0x7FFF; // преобразование каждого сэмпла в диапазон [-32767, 32767]
    //        }
    //        subject.next(new Uint8Array(int16Array.buffer)); // отправка в hub командой subject.next
    //    }
    //    catch (err) {
    //        console.err("Ошибка получения данных из аудиопроцессора:", err);
    //    }
    //};

    audioWorkletNode.port.onmessage = e => subject.next(e.data); // вместо верхнего пробую

    audioWorkletNode.connect(audioContext.destination);// если без destination, то обработка будет идти не в средство вывода а на сервер

}

function stopTranslate() {
    if (mediaStream) mediaStream.getTracks().forEach(track => track.stop());
    if (audioContext) audioContext.close();
    if (subject) subject.complete();
    if (connection) connection.stop();
    
}

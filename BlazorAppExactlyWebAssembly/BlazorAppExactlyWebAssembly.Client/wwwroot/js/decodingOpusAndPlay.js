let decoder = null;
let audioCtx = null;
let playbackQueue = [];

async function setupOpusDecoder() {
    decoder = new window['opusDecoder'].OpusDecoder({ channels: 1, sampleRate: 48000 });
    await decoder.ready;
}

function playDecodedPCM(channelData, sampleRate) {
    if (!audioCtx) {
        audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate });
    }
    const buffer = audioCtx.createBuffer(
        channelData.length, // channels
        channelData[0].length,
        sampleRate
    );
    for (let ch = 0; ch < channelData.length; ch++) {
        buffer.getChannelData(ch).set(channelData[ch]);
    }
    const source = audioCtx.createBufferSource();
    source.buffer = buffer;
    source.connect(audioCtx.destination);
    source.start();
}

// Обработка каждого Opus-чанка
async function handleOpusChunk(opusChunk) {
    // Декодируем Opus в PCM
    const { channelData, samplesDecoded, sampleRate } = decoder.decodeFrame(opusChunk);
    playDecodedPCM(channelData, sampleRate);
    // Здесь же можно отправить opusChunk на сервер через SignalR, если нужно
}

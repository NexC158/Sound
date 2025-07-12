//class AudioProcessorClient extends AudioWorkletProcessor {
//    process(inputs) {

//        const input = inputs[0]; // первый канал из первого входа
//        if (input && input.length > 0) {

//            this.port.postMessage(input[0].slice()); // отправляю сырые PCM данные
//        }
//        return true;
//    }
//}
//registerProcessor('audioProcessorClient', AudioProcessorClient);

class AudioProcessorClient extends AudioWorkletProcessor { // пробую вместо верхнего
    process(inputs) {
        const channelData = inputs[0][0];
        if (channelData) {
            const pcm = new Int16Array(channelData.length); // Float32 → Int16 PCM
            for (let i = 0; i < channelData.length; i++) // channelData.length
                pcm[i] = Math.max(-1, Math.min(1, channelData[i])) * 0x7FFF;
            this.port.postMessage(new Uint8Array(pcm.buffer));
        }
        return true; // keep alive
    }
}
registerProcessor('audioProcessorClient', AudioProcessorClient);

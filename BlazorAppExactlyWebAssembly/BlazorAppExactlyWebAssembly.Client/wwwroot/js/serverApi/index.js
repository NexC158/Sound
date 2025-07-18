"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.getReceiverRegister = exports.getHubProxyFactory = void 0;
class ReceiverMethodSubscription {
    constructor(connection, receiverMethod) {
        this.connection = connection;
        this.receiverMethod = receiverMethod;
        this.dispose = () => {
            for (const it of this.receiverMethod) {
                this.connection.off(it.methodName, it.method);
            }
        };
    }
}
exports.getHubProxyFactory = ((hubType) => {
    if (hubType === "ISignalRHub") {
        return ISignalRHub_HubProxyFactory.Instance;
    }
});
exports.getReceiverRegister = ((receiverType) => {
});
// HubProxy
class ISignalRHub_HubProxyFactory {
    constructor() {
        this.createHubProxy = (connection) => {
            return new ISignalRHub_HubProxy(connection);
        };
    }
}
ISignalRHub_HubProxyFactory.Instance = new ISignalRHub_HubProxyFactory();
class ISignalRHub_HubProxy {
    constructor(connection) {
        this.connection = connection;
        this.startStreamingCommand = (connectionId) => __awaiter(this, void 0, void 0, function* () {
            return yield this.connection.invoke("StartStreamingCommand", connectionId);
        });
        this.stopStreamingCommand = (connectionId) => __awaiter(this, void 0, void 0, function* () {
            return yield this.connection.invoke("StopStreamingCommand", connectionId);
        });
        this.getMyConnectionId = () => __awaiter(this, void 0, void 0, function* () {
            return yield this.connection.invoke("GetMyConnectionId");
        });
        this.getBytesFromAudioStream = (stream) => __awaiter(this, void 0, void 0, function* () {
            return yield this.connection.send("GetBytesFromAudioStream", stream);
        });
        this.createAndSendAudioChunk = () => __awaiter(this, void 0, void 0, function* () {
            return yield this.connection.invoke("CreateAndSendAudioChunk");
        });
        this.getHelloWorld = () => __awaiter(this, void 0, void 0, function* () {
            return yield this.connection.invoke("GetHelloWorld");
        });
    }
}
// Receiver

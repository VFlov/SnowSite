<template>
  <div class="video-call-wrapper custom-text-color">
    <div class="video-call-container">
      <h1>Voice Chat</h1>
      
      <!-- Navigation -->
      <button @click="goToHome" class="btn btn-default">Return to Home</button>
      <button @click="showCreateRoomModal = true" class="btn btn-create bubbly-button">Создать комнату</button>

      <!-- Create Room Modal -->
      <div v-if="showCreateRoomModal" class="modal-overlay">
        <div class="modal-content">
          <h2>Создать комнату</h2>
          <input 
            type="text" 
            v-model="roomName" 
            placeholder="Введите имя комнаты"
            class="modal-input"
          >
          <div class="modal-buttons">
            <button @click="joinRoom" class="btn btn-primary">Создать</button>
            <button @click="showCreateRoomModal = false" class="btn btn-secondary">Отмена</button>
          </div>
        </div>
      </div>

      <!-- Join Room Form -->
      <div v-if="!inRoom" class="room-form">
        <input 
          type="text" 
          v-model="roomName" 
          placeholder="Enter Room Name"
          class="form-input"
        >
        <button @click="joinRoom" class="btn btn-primary">Join Room</button>
      </div>

      <!-- Room Content -->
      <div v-else class="room-content">
        <p>В комнате: {{ roomName }}</p>
        <button @click="leaveRoom" class="btn btn-secondary">Leave Room</button>
        <audio ref="localAudio" muted autoplay></audio>
        <audio 
          v-for="(peer, index) in remotePeers" 
          :key="index" 
          :ref="'remoteAudio' + peer.connectionId"
          autoplay
        ></audio>
      </div>
    </div>

    <!-- Room Cards -->
    <div class="card-grid">
      <div v-for="item in items" :key="item.id" class="card-container">
        <template-card :item="item" />
      </div>
    </div>
  </div>
</template>

<script>
import TemplateCard from './components/Card.vue';
import * as signalR from "@microsoft/signalr";
import * as adapter from 'webrtc-adapter';

export default {
  name: 'VoiceChat',
  components: { TemplateCard },
  data() {
    return {
      items: [],
      showCreateRoomModal: false,
      hubConnection: null,
      roomName: '',
      userId: '',
      inRoom: false,
      localStream: null,
      peerConnections: {},
      remotePeers: [],
      originalBodyBackgroundColor: '',
      originalAppBackgroundColor: ''
    };
  },
  mounted() {
    this.setupBackgroundColors();
    this.initializeSignalR();
    this.userId = Math.floor(Math.random() * 1000);
  },
  beforeUnmount() {
    this.restoreBackgroundColors();
    this.cleanupConnections();
  },
  methods: {
    setupBackgroundColors() {
      const appElement = document.getElementById('app');
      this.originalAppBackgroundColor = appElement.style.backgroundColor;
      this.originalBodyBackgroundColor = document.body.style.backgroundColor;
      appElement.style.backgroundColor = '#1C1C1C';
      document.body.style.backgroundColor = '#1d031f';
    },
    restoreBackgroundColors() {
      document.body.style.backgroundColor = this.originalBodyBackgroundColor;
      document.getElementById('app').style.backgroundColor = this.originalAppBackgroundColor;
    },
    initializeSignalR() {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://45.130.214.139:5020/chatHub")
        .build();

      this.hubConnection.on("UserJoined", this.handleUserJoined);
      this.hubConnection.on("UserLeft", this.handleUserLeft);
      this.hubConnection.on("ReceiveSignal", this.handleReceiveSignal);
      this.hubConnection.on("ReceiveRoomList", (rooms) => this.items = rooms);

      this.hubConnection.start()
        .then(() => console.log("SignalR Connected!"))
        .then(() => this.hubConnection.invoke("GetRoomList"))
        .catch(err => console.error("SignalR Error:", err));
    },
    goToHome() {
      this.$router.push('/');
    },
    async joinRoom() {
      try {
        this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
        this.$refs.localAudio.srcObject = this.localStream;
        await this.hubConnection.invoke("JoinRoom", this.roomName, this.userId);
        this.inRoom = true;
        this.showCreateRoomModal = false;
      } catch (error) {
        console.error("Error joining room:", error);
      }
    },
    async leaveRoom() {
      await this.hubConnection.invoke("LeaveRoom");
      this.inRoom = false;
      if (this.localStream) {
        this.localStream.getTracks().forEach(track => track.stop());
        this.localStream = null;
      }
      Object.keys(this.peerConnections).forEach(this.closePeerConnection);
    },
    handleUserJoined(connectionId, userId) {
      console.log(`User ${userId} joined`);
      this.createPeerConnection(connectionId);
      this.createOffer(connectionId);
    },
    handleUserLeft(connectionId) {
      console.log(`User ${connectionId} left`);
      this.closePeerConnection(connectionId);
    },
    handleReceiveSignal(sendingClientId, signal) {
      if (this.peerConnections[sendingClientId]) {
        this.processAnswer(sendingClientId, signal);
      } else {
        this.processOffer(sendingClientId, signal);
      }
    },
    async createPeerConnection(connectionId) {
      const pc = new RTCPeerConnection({
        iceServers: [
          { urls: "stun:stun.l.google.com:19302" },
          { urls: "stun:stun1.l.google.com:19302" }
        ]
      });
      
      pc.ontrack = event => this.handleTrackEvent(event, connectionId);
      pc.onicecandidate = event => this.handleIceCandidate(event, connectionId);
      pc.oniceconnectionstatechange = () => this.handleIceStateChange(connectionId);
      
      this.localStream?.getTracks().forEach(track => pc.addTrack(track, this.localStream));
      this.peerConnections[connectionId] = pc;
    },
    async createOffer(connectionId) {
      try {
        const offer = await this.peerConnections[connectionId].createOffer();
        await this.peerConnections[connectionId].setLocalDescription(offer);
        this.hubConnection.invoke("SendSignal", JSON.stringify({ "sdp": offer }), connectionId);
      } catch (error) {
        console.error("Error creating offer:", error);
      }
    },
    async processOffer(connectionId, signal) {
      await this.createPeerConnection(connectionId);
      const parsedSignal = JSON.parse(signal);
      
      if (parsedSignal.sdp) {
        await this.peerConnections[connectionId].setRemoteDescription(new RTCSessionDescription(parsedSignal.sdp));
        const answer = await this.peerConnections[connectionId].createAnswer();
        await this.peerConnections[connectionId].setLocalDescription(answer);
        this.hubConnection.invoke("SendSignal", JSON.stringify({ "sdp": answer }), connectionId);
      }
      if (parsedSignal.ice) {
        await this.peerConnections[connectionId].addIceCandidate(parsedSignal.ice);
      }
    },
    async processAnswer(connectionId, signal) {
      const parsedSignal = JSON.parse(signal);
      if (parsedSignal.sdp) {
        await this.peerConnections[connectionId].setRemoteDescription(new RTCSessionDescription(parsedSignal.sdp));
      }
      if (parsedSignal.ice) {
        await this.peerConnections[connectionId].addIceCandidate(parsedSignal.ice);
      }
    },
    handleTrackEvent(event, connectionId) {
      if (!this.remotePeers.some(p => p.connectionId === connectionId)) {
        this.remotePeers.push({ connectionId });
        this.$nextTick(() => {
          this.$refs[`remoteAudio${connectionId}`][0].srcObject = event.streams[0];
        });
      }
    },
    handleIceCandidate(event, connectionId) {
      if (event.candidate) {
        this.hubConnection.invoke("SendSignal", JSON.stringify({ "ice": event.candidate }), connectionId);
      }
    },
    handleIceStateChange(connectionId) {
      if (this.peerConnections[connectionId]?.iceConnectionState === 'disconnected') {
        this.closePeerConnection(connectionId);
      }
    },
    closePeerConnection(connectionId) {
      if (this.peerConnections[connectionId]) {
        this.peerConnections[connectionId].close();
        delete this.peerConnections[connectionId];
        this.remotePeers = this.remotePeers.filter(p => p.connectionId !== connectionId);
      }
    },
    cleanupConnections() {
      this.leaveRoom();
      this.hubConnection?.stop();
    }
  }
};
</script>

<style scoped>
.video-call-wrapper {
  color: #e5dcdc;
}

.video-call-container {
  max-width: 600px;
  margin: 50px auto;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

h1 {
  text-align: center;
  font-size: 2.5rem;
  margin-bottom: 20px;
}

/* Buttons */
.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  transition: background-color 0.3s ease, transform 0.2s ease;
}

.btn-default {
  background-color: #4a4e69;
  color: #e5dcdc;
}

.btn-default:hover {
  background-color: #5a6268;
  transform: translateY(-1px);
}

.btn-create {
  background-color: #70abaf;
  color: #e5dcdc;
  margin-left: 10px;
}

.btn-create:hover {
  background-color: #5a8d91;
  transform: translateY(-1px);
}

.btn-primary {
  background-color: #007bff;
  color: #fff;
}

.btn-primary:hover {
  background-color: #0056b3;
  transform: translateY(-1px);
}

.btn-secondary {
  background-color: #6c757d;
  color: #fff;
}

.btn-secondary:hover {
  background-color: #5a6268;
  transform: translateY(-1px);
}

/* Form and Content */
.room-form, .room-content {
  margin-top: 20px;
}

.form-input {
  width: calc(100% - 22px);
  padding: 10px;
  margin-bottom: 10px;
  border-radius: 4px;
  border: 1px solid #ccc;
  transition: border-color 0.3s ease;
}

.form-input:focus {
  border-color: #007bff;
  outline: none;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background-color: #fff;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
  width: 300px;
  text-align: center;
}

.modal-input {
  width: 90%;
  padding: 10px;
  margin-bottom: 20px;
  border: 1px solid #ccc;
  border-radius: 5px;
  font-size: 1em;
}

.modal-buttons {
  display: flex;
  justify-content: space-between;
  gap: 10px;
}

/* Card Grid */
.card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
  direction: rtl;
  padding: 20px;
}

.card-container {
  direction: ltr;
}
</style>
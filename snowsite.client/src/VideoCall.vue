<template>
  <div class="video-call-wrapper custom-text-color">
    <div class="video-call-container">
      <h1>Voice Chat</h1>
      <div class="nav-buttons">
        <button @click="goToHome" class="btn btn-default">Return to Home</button>
      </div>

      <!-- Локальная демонстрация экрана -->
      <div v-if="screenStream" class="screen-share-container">
        <video ref="screenVideo" autoplay class="screen-video"></video>
      </div>

      <!-- Демонстрация экрана от других участников -->
      <div v-for="peer in remotePeers" :key="peer.connectionId" class="screen-share-container">
        <video v-if="peer.videoStream" :ref="`remoteVideo-${peer.connectionId}`" autoplay class="screen-video"></video>
      </div>

      <div class="media-controls" v-if="inRoom">
        <button @click="toggleScreenShare" class="btn btn-primary">
          {{ screenStream ? 'Остановить демонстрацию' : 'Поделиться экраном' }}
        </button>
        <button @click="toggleMicrophone" class="btn btn-secondary">
          {{ isMicMuted ? 'Включить микрофон' : 'Выключить микрофон' }}
        </button>
      </div>

      <div v-if="showCreateRoomModal" class="modal-overlay">
        <div class="modal-content">
          <h2>Создать комнату</h2>
          <input type="text" v-model="roomName" placeholder="Введите имя комнаты" class="modal-input">
          <div class="modal-buttons">
            <button @click="createAndJoinRoom" class="btn btn-primary">Создать</button>
            <button @click="showCreateRoomModal = false" class="btn btn-secondary">Отмена</button>
          </div>
        </div>
      </div>

      <div v-if="!inRoom" class="room-form">
        <input type="text" v-model="roomName" placeholder="Enter Room Name" class="form-input">
        <button @click="joinRoom" class="btn btn-primary">Join Room</button>
      </div>

      <div v-else class="room-content">
        <p>В комнате: {{ roomName }}</p>
        <button @click="leaveRoom" class="btn btn-secondary">Leave Room</button>

        <!-- Temporary Chat -->
        <div class="chat-container">
          <div class="chat-messages">
            <div v-for="(message, index) in chatMessages" :key="index" class="chat-message">
              <span class="chat-user">{{ message.userId }}:</span> {{ message.text }}
            </div>
          </div>
          <div class="chat-input">
            <input v-model="newMessage" @keyup.enter="sendChatMessage" placeholder="Type a message..."
                   class="form-input">
            <button @click="sendChatMessage" class="btn btn-primary">Send</button>
          </div>
        </div>
      </div>

      <audio ref="localAudio" muted autoplay></audio>
      <audio v-for="peer in remotePeers" :key="peer.connectionId" :ref="`remoteAudio-${peer.connectionId}`"
             autoplay></audio>
    </div>

    <div v-if="!inRoom" class="card-grid">
      <div class="card-container">
        <template-card :item="createRoomCard" @my-event="showCreateRoomModal = true" />
      </div>
      <div v-for="item in items" :key="item.name" class="card-container">
        <template-card v-if="item && item.name" :item="item" @my-event="joinRoomFromCard" />
      </div>
    </div>
  </div>
</template>

<script>
  import { getApiUrl } from '@/config/api';
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
        screenStream: null,
        peerConnections: {},
        remotePeers: [],
        originalBodyBackgroundColor: '',
        originalAppBackgroundColor: '',
        isMicMuted: false,
        chatMessages: [], // Store chat messages
        newMessage: '', // Input for new message
        createRoomCard: { name: 'Создать комнату', participantCount: '' } // Фиктивная карточка
      };
    },
    mounted() {
      this.setupBackgroundColors();
      this.initializeSignalR();
      this.userId = Math.floor(Math.random() * 1000).toString();
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
        const token = localStorage.getItem('token');
        if (!token) {
          this.$router.push('/auth');
          return;
        }
        this.hubConnection = new signalR.HubConnectionBuilder()
          .withUrl(getApiUrl("/callHub"), { accessTokenFactory: () => token })
          .withAutomaticReconnect()
          .build();

        this.hubConnection.on("UserJoined", (connectionId, userId) => {
          console.log(`User ${userId} joined with ConnectionId ${connectionId}`);
          this.handleUserJoined(connectionId, userId);
        });
        this.hubConnection.on("UserLeft", this.handleUserLeft);
        this.hubConnection.on("ReceiveSignal", this.handleReceiveSignal);
        this.hubConnection.on("ReceiveRoomList", (rooms) => {
          console.log("Received room list:", rooms);
          this.items = rooms;
        });
        this.hubConnection.on("ReceiveChatMessage", (userId, message) => {
          console.log(`Received real-time message - UserId: ${userId}, Message: ${message}`);
          this.chatMessages.push({ userId, text: message });
        });
        this.hubConnection.on("ReceiveChatHistory", (history) => {
          console.log("Received chat history:", JSON.stringify(history));
          this.chatMessages = history.map(msg => {
            console.log(`Mapping history item - userId: ${msg.userId}, message: ${msg.message}`);
            return { userId: msg.userId, text: msg.message };
          });
        });

        this.hubConnection.start()
          .then(() => {
            console.log("SignalR Connected!");
            this.hubConnection.invoke("GetRoomList");
          })
          .catch(err => console.error("SignalR Error:", err));
      },
      async sendChatMessage() {
        if (this.newMessage.trim() && this.inRoom) {
          console.log(`Sending message: ${this.newMessage}`);
          await this.hubConnection.invoke("SendChatMessage", this.roomName, this.userId, this.newMessage);
          this.newMessage = '';
        }
      },
      goToHome() {
        this.$router.push('/');
      },
      async createAndJoinRoom() {
        try {
          this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
          if (this.$refs.localAudio) {
            this.$refs.localAudio.srcObject = this.localStream;
          }
          await this.hubConnection.invoke("JoinRoom", this.roomName, this.userId);
          this.inRoom = true;
          this.showCreateRoomModal = false;
          await this.hubConnection.invoke("GetRoomList");
        } catch (error) {
          console.error("Error creating and joining room:", error);
        }
      },
      async joinRoom() {
        try {
          this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
          if (this.$refs.localAudio) {
            this.$refs.localAudio.srcObject = this.localStream;
          }
          await this.hubConnection.invoke("JoinRoom", this.roomName, this.userId);
          this.inRoom = true;
        } catch (error) {
          console.error("Error joining room:", error);
        }
      },
      async joinRoomFromCard(roomName) {
        this.roomName = roomName;
        await this.joinRoom();
      },
      async leaveRoom() {
        await this.hubConnection.invoke("LeaveRoom");
        this.inRoom = false;
        if (this.localStream) {
          this.localStream.getTracks().forEach(track => track.stop());
          this.localStream = null;
        }
        if (this.screenStream) {
          this.screenStream.getTracks().forEach(track => track.stop());
          this.screenStream = null;
        }
        Object.keys(this.peerConnections).forEach(this.closePeerConnection);
        this.remotePeers = [];
        await this.hubConnection.invoke("GetRoomList");
      },
      handleUserJoined(connectionId, userId) {
        console.log(`User ${userId} joined with ConnectionId ${connectionId}`);
        if (connectionId !== this.hubConnection.connectionId) {
          this.createPeerConnection(connectionId);
          this.createOffer(connectionId);
        }
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

        // Добавляем локальные треки (аудио и видео, если есть)
        this.localStream?.getTracks().forEach(track => pc.addTrack(track, this.localStream));
        this.screenStream?.getTracks().forEach(track => pc.addTrack(track, this.screenStream));

        this.peerConnections[connectionId] = pc;
      },
      async toggleScreenShare() {
        if (this.screenStream) {
          // Остановить демонстрацию экрана
          this.screenStream.getTracks().forEach(track => track.stop());
          this.screenStream = null;
          Object.values(this.peerConnections).forEach(pc => {
            const videoSender = pc.getSenders().find(s => s.track && s.track.kind === 'video');
            if (videoSender) pc.removeTrack(videoSender);
            // Обновляем SDP после удаления трека
            this.createOffer(pc.connectionId || Object.keys(this.peerConnections).find(key => this.peerConnections[key] === pc));
          });
        } else {
          // Начать демонстрацию экрана
          try {
            this.screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
            this.$nextTick(() => {
              if (this.$refs.screenVideo) {
                this.$refs.screenVideo.srcObject = this.screenStream;
              }
              // Добавляем видео-трек в существующие соединения и обновляем SDP
              Object.entries(this.peerConnections).forEach(([connectionId, pc]) => {
                this.screenStream.getTracks().forEach(track => pc.addTrack(track, this.screenStream));
                this.createOffer(connectionId);
              });
            });
            this.screenStream.getVideoTracks()[0].onended = () => this.toggleScreenShare();
          } catch (error) {
            console.error("Error sharing screen:", error);
            this.screenStream = null;
          }
        }
      },
      toggleMicrophone() {
        if (this.localStream) {
          this.isMicMuted = !this.isMicMuted;
          this.localStream.getAudioTracks().forEach(track => {
            track.enabled = !this.isMicMuted;
          });
        }
      },
      async createOffer(connectionId) {
        try {
          const pc = this.peerConnections[connectionId];
          const offer = await pc.createOffer();
          await pc.setLocalDescription(offer);
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
          await this.peerConnections[connectionId].addIceCandidate(new RTCIceCandidate(parsedSignal.ice));
        }
      },
      async processAnswer(connectionId, signal) {
        const parsedSignal = JSON.parse(signal);
        if (parsedSignal.sdp) {
          await this.peerConnections[connectionId].setRemoteDescription(new RTCSessionDescription(parsedSignal.sdp));
        }
        if (parsedSignal.ice) {
          await this.peerConnections[connectionId].addIceCandidate(new RTCIceCandidate(parsedSignal.ice));
        }
      },
      handleTrackEvent(event, connectionId) {
        const peer = this.remotePeers.find(p => p.connectionId === connectionId);
        if (!peer) {
          this.remotePeers.push({ connectionId, videoStream: null });
        }
        this.$nextTick(() => {
          if (event.track.kind === 'audio') {
            const audioRef = this.$refs[`remoteAudio-${connectionId}`];
            if (audioRef && audioRef[0]) {
              audioRef[0].srcObject = event.streams[0];
            }
          } else if (event.track.kind === 'video') {
            const videoRef = this.$refs[`remoteVideo-${connectionId}`];
            const peerIndex = this.remotePeers.findIndex(p => p.connectionId === connectionId);
            if (videoRef && videoRef[0] && peerIndex !== -1) {
              this.remotePeers[peerIndex].videoStream = event.streams[0];
              videoRef[0].srcObject = event.streams[0];
            }
          }
        });
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
    min-height: 100vh;
    background: #1d031f;
    padding: 20px;
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .video-call-container {
    max-width: 1000px;
    width: 100%;
    margin: 40px 0;
    padding: 25px;
    border-radius: 12px;
    background: #252525;
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.3);
    border: 1px solid #333;
  }

  h1 {
    text-align: center;
    font-size: 2.2rem;
    margin-bottom: 20px;
    color: #70abaf;
    font-weight: 600;
  }

  .nav-buttons,
  .media-controls {
    display: flex;
    justify-content: center;
    gap: 15px;
    margin: 20px 0;
  }

  .screen-share-container {
    display: flex;
    justify-content: center;
    margin: 20px 0;
  }

  .screen-video {
    max-width: 100%;
    max-height: 400px;
    border-radius: 8px;
    border: 2px solid #70abaf;
  }

  .room-form,
  .room-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 15px;
    margin-top: 20px;
  }

  .form-input {
    width: 100%;
    padding: 10px;
    border-radius: 6px;
    border: 1px solid #444;
    background: #333;
    color: #e5dcdc;
  }

    .form-input:focus {
      border-color: #70abaf;
      outline: none;
    }

  .chat-container {
    width: 100%;
    max-width: 600px;
    margin-top: 20px;
    background: #1e1e1e;
    border-radius: 8px;
    padding: 15px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  }

  .chat-messages {
    max-height: 300px;
    overflow-y: auto;
    margin-bottom: 15px;
    padding: 10px;
    background: #2a2a2a;
    border-radius: 6px;
  }

  .chat-message {
    margin: 5px 0;
    word-wrap: break-word;
    color: #e5dcdc;
  }

  .chat-user {
    font-weight: bold;
    color: #70abaf;
  }

  .chat-input {
    display: flex;
    gap: 10px;
  }

    .chat-input input {
      flex: 1;
    }

  .btn {
    padding: 10px 16px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    min-width: 80px;
    /* Larger touch target */
    transition: background-color 0.2s ease;
  }

  .btn-default {
    background-color: #4a4e69;
    color: #e5dcdc;
  }

    .btn-default:hover {
      background-color: #5a6268;
    }

  .btn-primary {
    background-color: #007bff;
    color: #fff;
  }

    .btn-primary:hover {
      background-color: #0056b3;
    }

  .btn-secondary {
    background-color: #6c757d;
    color: #fff;
  }

    .btn-secondary:hover {
      background-color: #5a6268;
    }

  .card-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 25px;
    padding: 30px;
    width: 100%;
    max-width: 1200px;
  }

  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.7);
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .modal-content {
    background: #252525;
    padding: 20px;
    border-radius: 12px;
    width: 100%;
    max-width: 320px;
    text-align: center;
    color: #e5dcdc;
  }

  .modal-input {
    width: 95%;
    padding: 10px;
    margin-bottom: 15px;
    border: 1px solid #444;
    border-radius: 6px;
    background: #333;
    color: #e5dcdc;
  }

  .modal-buttons {
    display: flex;
    justify-content: space-between;
    gap: 10px;
  }

  @media (max-width: 768px) {
    h1 {
      font-size: 1.3rem;
    }

    .screen-video {
      max-height: 200px;
    }

    .chat-messages {
      max-height: 30vh;
    }

    .btn {
      padding: 8px 12px;
      font-size: 0.8rem;
    }
  }

  @media (max-width: 480px) {
    .video-call-wrapper {
      padding: 2px;
    }

    .video-call-container {
      margin: 5px 0;
      padding: 8px;
    }

    h1 {
      font-size: 1.1rem;
    }

    .chat-input {
      flex-direction: column;
    }

      .chat-input input {
        width: 100%;
      }

    .btn {
      width: 100%;
      padding: 10px;
      font-size: 0.9rem;
    }

    .card-grid {
      grid-template-columns: 1fr;
      /* Single column for very small screens */
    }
  }
</style>

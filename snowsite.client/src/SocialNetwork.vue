<template>
  <div class="messenger-container">
    <div class="sidebar">
      <div class="sidebar-header">
        <h3>Диалоги</h3>
        <span class="online-status">{{ dialogs.length }} контактов</span>
      </div>
      <ul class="dialogs">
        <li v-for="dialog in dialogs"
            :key="dialog.id"
            @click="selectDialog(dialog)"
            :class="{ 'active': selectedDialog?.id === dialog.id }">
          <div class="avatar" :style="{ backgroundColor: dialog.avatarColor }"></div>
          <div class="dialog-info">
            <span class="username">{{ dialog.username }}</span>
            <span class="last-message">{{ dialog.lastMessage }}</span>
            <span v-if="dialog.unreadCount" class="unread-count">{{ dialog.unreadCount }}</span>
          </div>
        </li>
      </ul>
    </div>
    <div class="chat" v-if="selectedDialog">
      <div class="chat-header">
        <div class="avatar" :style="{ backgroundColor: selectedDialog.avatarColor }"></div>
        <span>{{ selectedDialog.username }}</span>
      </div>
      <div class="chat-messages" ref="chatMessages">
        <div v-for="message in messages"
             :key="message.id"
             :class="['message', message.sender === 'sent' ? 'sent' : 'received']">
          <div class="message-content">
            <span class="message-text">{{ message.text }}</span>
            <span class="message-time">{{ formatTime(message.time) }}</span>
          </div>
        </div>
      </div>
      <div class="chat-input">
        <input v-model="newMessage"
               @keypress.enter="sendMessage"
               placeholder="Введите сообщение..."
               :disabled="!selectedDialog" />
        <button @click="sendMessage" :disabled="!newMessage.trim()">Отправить</button>
      </div>
    </div>
    <div class="chat-placeholder" v-else>
      <p>Выберите диалог для начала общения</p>
    </div>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        token: localStorage.getItem('token'),
        socket: null,
        dialogs: [
          {
            id: 1,
            username: 'Иван Иванов',
            lastMessage: 'Привет!',
            avatarColor: '#4CAF50',
            unreadCount: 2
          },
          {
            id: 2,
            username: 'Мария Петрова',
            lastMessage: 'Как дела?',
            avatarColor: '#2196F3',
            unreadCount: 0
          }
        ],
        selectedDialog: null,
        messages: [],
        newMessage: ''
      };
    },
    methods: {
      async selectDialog(dialog) {
        this.selectedDialog = dialog;

        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${dialog.id}`);
        this.messages = await response.json();
        this.$nextTick(() => this.scrollToBottom());
      },
      async getDialogs() {

      },
      getMessagesForDialog(dialogId) {
        // Симуляция загрузки сообщений
        const messages = {
          1: [
            { id: 1, sender: 'received', text: 'Привет!', time: new Date() },
            { id: 2, sender: 'sent', text: 'Привет! Как дела?', time: new Date() }
          ],
          2: [
            { id: 1, sender: 'received', text: 'Как дела?', time: new Date() },
            { id: 2, sender: 'sent', text: 'Отлично, а у тебя?', time: new Date() }
          ]
        };
        return messages[dialogId] || [];
      },
      async sendMessage() {
        if (!this.newMessage.trim() || !this.selectedDialog) return;

        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${this.selectedDialog.id}`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ text: this.newMessage })
        });

        const message = await response.json();
        this.messages.push(message);
        this.newMessage = '';
        this.$nextTick(() => this.scrollToBottom());
      },
      updateLastMessage(text) {
        const dialog = this.dialogs.find(d => d.id === this.selectedDialog.id);
        if (dialog) {
          dialog.lastMessage = text;
        }
      },
      markAsRead(dialog) {
        dialog.unreadCount = 0;
      },
      scrollToBottom() {
        const chatMessages = this.$refs.chatMessages;
        if (chatMessages) {
          chatMessages.scrollTop = chatMessages.scrollHeight;
        }
      },
      formatTime(date) {
        return new Date(date).toLocaleTimeString([], {
          hour: '2-digit',
          minute: '2-digit'
        });
      },
      async login(username, password) {
        const response = await this.fetchWithAuth('https://45.130.214.139:5020/api/auth/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ username, password })
        });
        const data = await response.json();
        this.token = data.token;
        localStorage.setItem('token', this.token);
        this.connectWebSocket();
      },

      async fetchWithAuth(url, options = {}) {
        options.headers = {
          ...options.headers,
          'Authorization': `Bearer ${this.token}`
        };
        return fetch(url, options);
      },

      connectWebSocket() {
        this.socket = new WebSocket(`ws://45.130.214.139:5020/ws?token=${this.token}`);
        this.socket.onmessage = (event) => {
          const message = JSON.parse(event.data);
          if (message.dialogId === this.selectedDialog?.id) {
            this.messages.push(message);
            this.$nextTick(() => this.scrollToBottom());
          }
        };
      }
    },
    watch: {
      selectedDialog(newVal) {
        if (newVal) {
          this.messages = this.getMessagesForDialog(newVal.id);
        }
      }
    }
  };
</script>

<style scoped>
  .messenger-container {
    display: flex;
    width: 900px;
    height: 600px;
    background: #fff;
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .sidebar {
    width: 35%;
    background: #f8f9fa;
    border-right: 1px solid #eee;
    display: flex;
    flex-direction: column;
  }

  .sidebar-header {
    padding: 15px;
    border-bottom: 1px solid #eee;
  }

    .sidebar-header h3 {
      margin: 0;
      font-size: 18px;
    }

  .online-status {
    font-size: 12px;
    color: #666;
  }

  .dialogs {
    list-style: none;
    padding: 0;
    margin: 0;
    overflow-y: auto;
    flex: 1;
  }

    .dialogs li {
      display: flex;
      align-items: center;
      padding: 10px 15px;
      cursor: pointer;
      transition: background 0.2s;
    }

      .dialogs li:hover {
        background: #e9ecef;
      }

      .dialogs li.active {
        background: #e1f0ff;
      }

  .avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    margin-right: 10px;
    flex-shrink: 0;
  }

  .dialog-info {
    flex: 1;
    min-width: 0;
  }

  .username {
    display: block;
    font-weight: 500;
  }

  .last-message {
    display: block;
    font-size: 14px;
    color: #666;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .unread-count {
    background: #007bff;
    color: white;
    border-radius: 10px;
    padding: 2px 6px;
    font-size: 12px;
    margin-left: 5px;
  }

  .chat {
    flex: 1;
    display: flex;
    flex-direction: column;
  }

  .chat-header {
    padding: 15px;
    border-bottom: 1px solid #eee;
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .chat-messages {
    flex: 1;
    padding: 20px;
    overflow-y: auto;
    background: #f0f2f5;
  }

  .message {
    display: flex;
    margin-bottom: 15px;
  }

    .message.sent {
      justify-content: flex-end;
    }

    .message.received {
      justify-content: flex-start;
    }

  .message-content {
    padding: 10px 15px;
    border-radius: 15px;
    background: #fff;
    position: relative;
    max-width: 70%;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  }

  .message.sent .message-content {
    background: #007bff;
    color: #fff;
  }

  .message-text {
    word-break: break-word;
  }

  .message-time {
    font-size: 12px;
    color: #999;
    margin-top: 5px;
    display: block;
  }

  .chat-input {
    padding: 15px;
    border-top: 1px solid #eee;
    display: flex;
    gap: 10px;
    background: #fff;
  }

    .chat-input input {
      flex: 1;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 20px;
      outline: none;
    }

    .chat-input button {
      padding: 10px 20px;
      background: #007bff;
      color: #fff;
      border: none;
      border-radius: 20px;
      cursor: pointer;
      transition: background 0.2s;
    }

      .chat-input button:hover {
        background: #0056b3;
      }

      .chat-input button:disabled {
        background: #ccc;
        cursor: not-allowed;
      }

  .chat-placeholder {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #666;
    background: #f0f2f5;
  }
</style>

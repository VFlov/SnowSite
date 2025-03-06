<template>
  <div class="messenger-container">
    <div class="sidebar">
      <div class="sidebar-header">
        <h3>Диалоги</h3>
        <button @click="logout" class="logout-button">Выйти</button>
        <input v-model="searchQuery" placeholder="Найти пользователя..." @input="searchUsers">
        <ul class="search-results" v-if="searchedUsers.length">
          <li v-for="user in searchedUsers" :key="user.id" @click="createDialog(user.id)">
            {{ user.username }}
          </li>
        </ul>
      </div>
      <ul class="dialogs">
        <li v-for="dialog in dialogs" :key="dialog.id" @click="selectDialog(dialog)"
            :class="{ 'active': selectedDialog?.id === dialog.id }">
          <div class="avatar" :style="{ backgroundColor: getAvatarColor(dialog) }"></div>
          <div class="dialog-info">
            <span class="username">{{ getDialogUsername(dialog) }}</span>
            <span class="last-message">{{ dialog.lastMessage }}</span>
            <span v-if="getUnreadCount(dialog)" class="unread-count">{{ getUnreadCount(dialog) }}</span>
          </div>
        </li>
      </ul>
    </div>
    <div class="chat" v-if="selectedDialog">
      <div class="chat-header">
        <div class="avatar" :style="{ backgroundColor: getAvatarColor(selectedDialog) }"></div>
        <span>{{ getDialogUsername(selectedDialog) }}</span>
      </div>
      <div class="chat-messages background-image" ref="chatMessages">

        <div v-for="message in messages" :key="message.id"
             :class="['message', message.senderId === this.currentUserId ? 'sent' : 'received']">
          <div class="message-content">
            <span class="message-text">{{ message.text }}</span>
            <img v-if="message.attachmentUrl" :src="'https://45.130.214.139:5020' + message.attachmentUrl"
                 class="attachment" @click="openImage(message.attachmentUrl)">
            <span class="message-time">{{ formatTime(message.time) }}</span>
          </div>
        </div>
      </div>
      <div class="chat-input">
        <input v-model="newMessage" @keypress.enter="sendMessage" placeholder="Введите сообщение..." />
        <input type="file" ref="fileInput" @change="handleFileUpload" hidden>
        <button @click="$refs.fileInput.click()">📎</button>
        <button @click="sendMessage">Отправить</button>
      </div>
    </div>
    <div v-if="selectedImage" class="image-modal" @click="closeImage">
      <img :src="'https://45.130.214.139:5020' + selectedImage" class="full-image">
    </div>
  </div>
</template>

<script>
import { jwtDecode } from 'jwt-decode';

export default {
  data() {
    return {
      dialogs: [],
      selectedDialog: null,
      messages: [],
      newMessage: '',
      searchQuery: '',
      searchedUsers: [],
      currentUserId: 0,
      socket: null,
      file: null,
      isSending: false,
      selectedImage: null
    };
  },
  methods: {
    async fetchDialogs() {
      const response = await this.fetchWithAuth('https://45.130.214.139:5020/api/chat/dialogs');
      if (response.ok) {
        this.dialogs = await response.json();
      } else {
        console.error('Ошибка загрузки диалогов:', response.status);
        this.$router.push('/auth');
      }
    },
    async selectDialog(dialog) {
      this.searchedUsers = [];
      this.selectedDialog = dialog;
      try {
        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${dialog.id}`);
        if (!response.ok) {
          if (response.status === 401) {
            console.error('Неавторизованный доступ, перенаправление на /auth');
            this.$router.push('/auth');
            return;
          }
          throw new Error(`Ошибка ${response.status}: ${response.statusText}`);
        }
        this.messages = await response.json();
        this.$nextTick(() => this.scrollToBottom());
        // Отмечаем сообщения как прочитанные
        await this.markMessagesAsRead(dialog.id);
      } catch (error) {
        console.error('Ошибка загрузки сообщений:', error.message);
        this.messages = [];
      }
    },
    async searchUsers() {
      if (!this.searchQuery.trim()) {
        this.searchedUsers = [];
        return;
      }
      try {
        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/search-users?query=${encodeURIComponent(this.searchQuery)}`);
        if (!response.ok) {
          if (response.status === 401) {
            console.error('Неавторизованный доступ, перенаправление на /auth');
            this.$router.push('/auth');
            return;
          }
          throw new Error(`Ошибка ${response.status}: ${response.statusText}`);
        }
        this.searchedUsers = await response.json();
      } catch (error) {
        console.error('Ошибка поиска пользователей:', error.message);
        this.searchedUsers = [];
      }
    },
    async createDialog(userId) {
      try {
        const response = await this.fetchWithAuth('https://45.130.214.139:5020/api/chat/dialogs', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(userId)
        });
        if (!response.ok) {
          const error = await response.json();
          throw new Error(error.message || 'Ошибка создания диалога');
        }
        const newDialog = await response.json();
        this.dialogs.push(newDialog);
        this.selectDialog(newDialog);
        this.searchQuery = '';
        this.searchedUsers = [];
      } catch (error) {
        console.error('Ошибка в createDialog:', error.message);
      }
    },
    async sendMessage() {
      if (!this.newMessage.trim() && !this.file) return;
      if (this.isSending) return; // Блокируем повторную отправку

      this.isSending = true; // Устанавливаем флаг блокировки

      const tempId = Date.now(); // Временный ID для сообщения
      const pendingMessage = {
        tempId,
        senderId: this.currentUserId,
        text: this.newMessage,
        time: new Date(),
        attachmentUrl: this.file ? URL.createObjectURL(this.file) : null, // Локальный предпросмотр вложения
        failed: false
      };
      this.messages.push(pendingMessage); // Добавляем сообщение в UI сразу
      this.$nextTick(() => this.scrollToBottom());

      const formData = new FormData();
      formData.append('text', this.newMessage);
      if (this.file) formData.append('attachment', this.file);

      try {
        // Устанавливаем таймаут на 10 секунд
        const timeoutPromise = new Promise((_, reject) =>
          setTimeout(() => reject(new Error('Таймаут отправки сообщения')), 10000)
        );

        const fetchPromise = this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${this.selectedDialog.id}`, {
          method: 'POST',
          body: formData
        });

        const response = await Promise.race([fetchPromise, timeoutPromise]);

        if (!response.ok) {
          const errorText = await response.text();
          throw new Error(`Ошибка ${response.status}: ${errorText || 'Неизвестная ошибка'}`);
        }

        const message = await response.json();
        // Заменяем временное сообщение на полученное с сервера
        const index = this.messages.findIndex(m => m.tempId === tempId);
        if (index !== -1) {
          this.messages.splice(index, 1, message);
        }
      } catch (error) {
        console.error('Ошибка отправки сообщения:', error.message);
        // Помечаем сообщение как неудачное
        const index = this.messages.findIndex(m => m.tempId === tempId);
        if (index !== -1) {
          this.messages[index].failed = true;
        }
      } finally {
        this.isSending = false; // Разблокируем отправку
        this.newMessage = '';
        this.file = null;
      }
    },
    async markMessagesAsRead(dialogId) {
      try {
        const response = await this.fetchWithAuth(`https://45.130.214.139:5020/api/chat/messages/${dialogId}/mark-read`, {
          method: 'POST'
        });
        if (!response.ok) {
          throw new Error(`Ошибка ${response.status}: ${await response.text()}`);
        }
        // Обновляем счетчик непрочитанных сообщений
        const dialogIndex = this.dialogs.findIndex(d => d.id === dialogId);
        if (dialogIndex !== -1) {
          if (this.dialogs[dialogIndex].user1Id === this.currentUserId) {
            this.dialogs[dialogIndex].user1UnreadCount = 0;
          } else {
            this.dialogs[dialogIndex].user2UnreadCount = 0;
          }
        }
      } catch (error) {
        console.error('Ошибка отметки сообщений как прочитанных:', error.message);
      }
    },
    fetchWithAuth(url, options = {}) {
      const token = localStorage.getItem('token');
      if (!token) {
        console.error('Токен отсутствует, перенаправление на /auth');
        this.$router.push('/auth');
        return Promise.reject(new Error('No token available'));
      }
      options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`
      };
      return fetch(url, options);
    },
    logout() {
      localStorage.removeItem('token');
      if (this.socket) this.socket.close();
      this.$router.push('/auth');
    },
    getDialogUsername(dialog) {
      if (!dialog || (!dialog.user1 && !dialog.user2)) return 'Unknown User';
      const targetUser = dialog.user1Id === this.currentUserId ? dialog.user2 : dialog.user1;
      return targetUser?.username || 'Unknown User';
    },
    getAvatarColor(dialog) {
      return dialog?.user1Id === this.currentUserId ? '#2196F3' : '#4CAF50';
    },
    getUnreadCount(dialog) {
      return dialog?.user1Id === this.currentUserId ? dialog.user2UnreadCount : dialog.user1UnreadCount;
    },
    scrollToBottom() {
      const chatMessages = this.$refs.chatMessages;
      if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
      }
    },
    formatTime(date) {
      return new Date(date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    },
    connectWebSocket() {
      const token = localStorage.getItem('token');
      this.socket = new WebSocket('wss://45.130.214.139:5020/ws'); // Предполагаемый маршрут WebSocket

      this.socket.onopen = () => {
        console.log('WebSocket подключен');
        // Отправляем токен для аутентификации (если требуется сервером)
        this.socket.send(JSON.stringify({ token }));
      };

      this.socket.onmessage = (event) => {
        const data = JSON.parse(event.data);
        if (data.Event === 'ReceiveMessage') {
          const message = data.Data;
          if (message.dialogId === this.selectedDialog?.id) {
            const index = this.messages.findIndex(m => m.tempId === message.tempId);
            if (index === -1) {
              this.messages.push(message);
              this.$nextTick(() => this.scrollToBottom());
            }
          }
          const dialogIndex = this.dialogs.findIndex(d => d.id === message.dialogId);
          if (dialogIndex !== -1) {
            this.dialogs[dialogIndex].lastMessage = message.text;
            if (message.senderId !== this.currentUserId) {
              if (this.dialogs[dialogIndex].user1Id === this.currentUserId) {
                this.dialogs[dialogIndex].user1UnreadCount++;
              } else {
                this.dialogs[dialogIndex].user2UnreadCount++;
              }
            }
          }
        } else if (data.Event === 'MessagesRead') {
          const { dialogId, userId } = data.Data;
          const dialogIndex = this.dialogs.findIndex(d => d.id === dialogId);
          if (dialogIndex !== -1) {
            if (this.dialogs[dialogIndex].user1Id === userId) {
              this.dialogs[dialogIndex].user1UnreadCount = 0;
            } else if (this.dialogs[dialogIndex].user2Id === userId) {
              this.dialogs[dialogIndex].user2UnreadCount = 0;
            }
          }
        }
      };

      this.socket.onerror = (error) => {
        console.error('Ошибка WebSocket:', error);
      };

      this.socket.onclose = () => {
        console.log('WebSocket закрыт');
      };
    },
    handleFileUpload(event) {
      this.file = event.target.files[0];
    },
    openImage(attachmentUrl) {
      this.selectedImage = attachmentUrl; // Открываем модальное окно с изображением
    },
    closeImage() {
      this.selectedImage = null; // Закрываем модальное окно
    }
  },
  created() {
    const token = localStorage.getItem('token');
    if (!token) {
      this.$router.push('/auth');
      return;
    }
    try {
      const decodedToken = jwtDecode(token);
      this.currentUserId = parseInt(decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      console.log('Current User ID:', this.currentUserId, 'Decoded Token:', decodedToken);
      this.fetchDialogs();
      this.connectWebSocket();
    } catch (error) {
      console.error('Ошибка декодирования токена:', error);
      localStorage.removeItem('token');
      this.$router.push('/auth');
    }
  },
};
</script>

<style scoped>
  .messenger-container {
    display: flex;
    background: #fff;
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    overflow: hidden;
    height: 100vh;
    /* Changed from min-height to height */
    width: 100%;
    margin: 0 auto;
    max-width: 1680px;
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
    position: relative;
  }

    .sidebar-header h3 {
      margin: 0;
      font-size: 18px;
    }

  .logout-button {
    padding: 5px 15px;
    background: #dc3545;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    transition: background 0.2s;
    margin-left: 10px;
  }

    .logout-button:hover {
      background: #c82333;
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
    display: flex;
    flex-direction: column;
  }

  .background-image {
    background-image: url('./assets/backgrounds/pattern.svg');
    background-size: cover;
    /* или 'contain', в зависимости от желаемого эффекта */
    background-position: center;
    background-repeat: no-repeat;
    position: relative;
    /* Это обеспечивает, что содержимое будет позиционироваться относительно этого элемента */
  }

  .message {
    display: flex;
    margin-bottom: 15px;
    width: 100%;
    /* Устанавливаем ширину для выравнивания */
  }

    .message.sent {
      justify-content: flex-end;
      /* Отправленные сообщения справа */
    }

    .message.received {
      justify-content: flex-start;
      /* Полученные сообщения слева */
    }

  .message-content {
    padding: 10px 15px;
    border-radius: 15px;
    background: #fff;
    max-width: 70%;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  }

  .message.sent .message-content {
    background: #007bff;
    color: #fff;
  }

  .message.received .message-content {
    background: #fff;
    /* Явно указываем фон для полученных сообщений */
  }


  .message.with-attachment .message-content {
    padding: 8px;
    /* Уменьшаем внутренний отступ для аккуратности */
    border: 1px solid #ddd;
    /* Добавляем границу для выделения */
  }

  .message.sent.with-attachment .message-content {
    border-color: #0056b3;
    /* Темнее для отправленных сообщений */
  }

  .message.received.with-attachment .message-content {
    border-color: #ccc;
    /* Светлее для полученных сообщений */
  }

  .message.failed .message-content {
    opacity: 0.7;
    /* Полупрозрачность для неудачных сообщений */
    border: 1px dashed #dc3545;
    /* Красная пунктирная граница */
  }

  .message-text {
    word-break: break-word;
    display: block;
    margin-bottom: 5px;
    /* Отступ перед вложением или временем */
  }

  .message-time {
    font-size: 12px;
    color: #999;
    margin-top: 5px;
    display: block;
  }

  .attachment {
    max-width: 200px;
    margin-top: 5px;
    border-radius: 8px;
    display: block;
    /* Убедимся, что изображение занимает отдельную строку */
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    /* Тень для визуального выделения */
    cursor: pointer;
  }

  .image-modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .full-image {
    max-width: 90%;
    max-height: 90%;
    border-radius: 10px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
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

  .search-results {
    list-style: none;
    padding: 0;
    margin: 10px 0;
    position: absolute;
    background: white;
    border: 1px solid #ddd;
    border-radius: 5px;
    max-height: 200px;
    overflow-y: auto;
    width: calc(100% - 30px);
    z-index: 10;
  }

    .search-results li {
      padding: 10px;
      cursor: pointer;
      transition: background 0.2s;
    }

      .search-results li:hover {
        background: #f0f0f0;
      }
</style>

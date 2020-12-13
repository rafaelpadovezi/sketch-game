<template>
  <div class="main">
    <nav class="navbar" role="navigation" aria-label="main navigation">
      <div class="navbar-brand">
        <span class="navbar-item">
          <h1 class="title is-2">Sketch</h1>
        </span>
      </div>
    </nav>
    <div class="content">
      <div class="sk-section">
        <div class="notification is-primary room-picker-outer">
          <div class="field">
            <h3 class="title is-3">Rooms</h3>
          </div>
          <div class="wrapper">
            <div class="room-picker">
              <div
                class="is-size-5"
                v-for="room in gameRooms"
                :key="room.id"
                @click="onChangeRoom(room)"
              >
                {{ room.name }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="sk-section">
        <div class="notification is-primary chat-outer">
          <div class="wrapper">
            <div class="chat">
              <p v-for="(message, index) in messages" :key="index">
                {{ message.content }}
              </p>
            </div>
          </div>
        </div>

        <div class="notification is-primary">
          <div class="field">
            <textarea
              class="textarea has-fixed-size"
              rows="3"
              v-model="textInput"
              v-on:keyup.enter="onMessageInput()"
            ></textarea>
          </div>
          <div class="field">
            <button class="button is-default" @click="onMessageInput()">
              Send
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapActions, mapGetters } from "vuex";

export default {
  name: "GeneralRoom",
  data() {
    return {
      textInput: ""
    };
  },
  mounted() {
    if (!this.isConnected) this.$router.push("/");
    this.$watch("messageCount", () => {
      const lastMessage = document.querySelector(".chat p:last-child");
      if (lastMessage) this.$nextTick(() => lastMessage.scrollIntoView());
    });
  },
  computed: {
    ...mapGetters("chat", {
      messages: "messages",
      isConnected: "isConnected",
      gameRooms: "gameRooms"
    }),
    messageCount() {
      return this.messages.length;
    }
  },
  methods: {
    ...mapActions("chat", ["sendMessage", "changeGameRoom"]),
    onMessageInput() {
      if (this.textInput.trim() === "") return;
      this.sendMessage(this.textInput);
      this.textInput = "";
    },
    onChangeRoom(room) {
      this.changeGameRoom(room);
    }
  },
  watch: {
    isConnected(isConnected) {
      if (!isConnected) this.$router.push("/");
    }
  }
};
</script>

<style>
.textarea {
  font-family: "Roboto Mono", monospace;
  color: black;
  overflow-y: scroll;
}

.room-picker-outer {
  min-height: 300px;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.room-picker {
  background: white;
  color: black;
  overflow: auto;
  font-family: "Roboto Mono", monospace;
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

.room-picker div {
  padding: 0.8em;
  cursor: pointer;
}

.room-picker div:hover {
  background: #eee;
}

.chat {
  background: white;
  min-height: 200px;
  height: 100%;
  color: black;
  overflow-y: auto;
  font-family: "Roboto Mono", monospace;
  scroll-behavior: smooth;
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

.chat-outer {
  min-height: 300px;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.chat > p {
  margin: 0.5em;
}
</style>

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
        <div class="columns">
          <div class="column is-1">
            <span class="icon is-large is-clickable" @click="onGoBack()">
              <i class="fas fa-arrow-left "></i>
            </span>
          </div>
          <div class="column">
            <h2>{{ gameRoom }}</h2>
            <div>Countdown: {{ Math.round(countdown) }}</div>
          </div>
          <div class="column">
            <div class="guess-word" v-if="isDrawing">
              <h3>{{ word }}</h3>
            </div>
          </div>
        </div>
        <div class="notification is-primary canvas-outer">
          <Canvas
            :canvas-id="gameRoom"
            :height="400"
            @draw="onDraw"
            ref="canvas"
            :isDrawing="isDrawing"
          />
        </div>
      </div>
      <div class="sk-section">
        <div class="notification is-primary chat-outer my-class">
          <div class="wrapper">
            <div class="chat">
              <p
                v-for="(message, index) in messages"
                :key="index"
                :class="{ 'has-text-primary': message.hit }"
              >
                <span style="white-space: pre;">{{ message.content }}</span>
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
              @keyup.enter="onMessageInput()"
              :disabled="isDrawing"
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
import Canvas from "@/components/Canvas.vue";

import { mapActions, mapGetters } from "vuex";

export default {
  name: "GameRoom",
  components: { Canvas },
  data() {
    return {
      textInput: ""
    };
  },
  computed: {
    ...mapGetters("chat", {
      messages: "messages",
      gameRoom: "gameRoom",
      isConnected: "isConnected",
      countdown: "countdown",
      isDrawing: "isDrawing",
      word: "word"
    }),
    messageCount() {
      return this.messages.length;
    }
  },
  mounted() {
    if (!this.isConnected) this.$router.push("/");

    const canvas = this.$refs.canvas;
    this.$store.subscribe(mutation => {
      if (mutation.type === "chat/REMOTE_DRAWING")
        canvas.updateDrawing(mutation.payload);
      if (mutation.type === "chat/RESET_DRAWING") canvas.reset();
    });

    this.$watch("messageCount", () => {
      const lastMessage = document.querySelector(".chat p:last-child");
      if (lastMessage) this.$nextTick(() => lastMessage.scrollIntoView());
    });
  },
  methods: {
    ...mapActions("chat", ["sendMessage", "goToGeneral", "sendDrawing"]),
    onMessageInput() {
      if (this.textInput.trim() === "") return;
      this.sendMessage(this.textInput);
      this.textInput = "";
    },
    onGoBack() {
      this.goToGeneral();
    },
    onDraw(pathData) {
      this.sendDrawing(pathData);
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
.canvas-outer {
  width: fit-content;
  align-self: center;
}
</style>

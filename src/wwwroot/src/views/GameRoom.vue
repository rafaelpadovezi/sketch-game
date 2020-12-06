<template>
  <div class="tile is-ancestor">
    <div class="tile is-6 is-parent is-vertical">
      <article class="tile is-child">
        <div class="container">
          <h2 class="title is-2">Sketch</h2>
        </div>
      </article>
      <article class="tile is-child">
        <div class="columns">
          <div class="column is-1">
            <span class="icon is-large is-clickable" @click="onGoBack()">
              <i class="fas fa-arrow-left "></i>
            </span>
          </div>
          <div class="column">
            <h2>{{ gameRoom }}</h2>
            <div>{{ Math.round(countdown) }}</div>
          </div>
          <div class="column">
            <h3>{{ word }}</h3>
          </div>
        </div>
      </article>

      <article class="tile is-child notification is-primary">
        <div style="">
          <Canvas
            :canvas-id="gameRoom"
            :height="400"
            @draw="onDraw"
            ref="canvas"
            :isDrawing="isDrawing"
          />
        </div>
      </article>
    </div>

    <div class="tile is-6 is-parent is-vertical">
      <article class="tile is-child">
        <div class="select is-multiple is-fullwidth"></div>
      </article>

      <article class="tile is-child notification is-primary">
        <div class="chat">
          <p
            v-for="(message, index) in messages"
            :key="index"
            :class="{ 'has-text-primary': message.hit }"
          >
            <span style="white-space: pre;">{{ message.content }}</span>
          </p>
        </div>
      </article>

      <article class="tile is-child notification is-primary">
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
      </article>
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
    })
  },
  mounted() {
    if (!this.isConnected) this.$router.push("/");
    const canvas = this.$refs.canvas;
    this.$store.subscribe(mutation => {
      if (mutation.type === "chat/REMOTE_DRAWING")
        canvas.updateDrawing(mutation.payload);
      if (mutation.type === "chat/RESET_DRAWING") canvas.reset();
    });
  },
  methods: {
    ...mapActions("chat", ["sendMessage", "goToGeneral", "sendDrawing"]),
    onMessageInput() {
      if (this.textInput === "") return;
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

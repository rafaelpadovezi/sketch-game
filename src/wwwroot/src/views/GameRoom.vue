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
            <div>{{ round.timer }}</div>
          </div>
        </div>
      </article>

      <article class="tile is-child notification is-primary">
        <div style="">
          <Canvas :canvas-id="gameRoom" height="400" />
        </div>
      </article>
    </div>

    <div class="tile is-6 is-parent is-vertical">
      <article class="tile is-child">
        <div class="select is-multiple is-fullwidth"></div>
      </article>

      <article class="tile is-child notification is-primary">
        <div class="chat">
          <p v-for="(message, index) in messages" :key="index">{{ message }}</p>
        </div>
      </article>

      <article class="tile is-child notification is-primary">
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
      round: {
        timer: 100
      },
      textInput: ""
    };
  },
  computed: {
    ...mapGetters("chat", {
      messages: "messages",
      gameRoom: "gameRoom"
    })
  },
  methods: {
    ...mapActions("chat", ["sendMessage", "goToGeneral"]),
    onMessageInput() {
      if (this.textInput === "") return;
      this.sendMessage(this.textInput);
      this.textInput = "";
    },
    onGoBack() {
      this.goToGeneral();
    }
  }
};
</script>

<style></style>

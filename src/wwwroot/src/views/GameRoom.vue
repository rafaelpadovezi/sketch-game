<template>
  <div class="tile is-ancestor">
    <div class="tile is-6 is-parent is-vertical">
      <article class="tile is-child">
        <div class="container">
          <h2 class="title is-2">Sketch</h2>
        </div>
      </article>
      <article class="tile is-child">
        relogio
      </article>

      <article class="tile is-child notification is-primary">
        <div style="height:400px"></div>
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
export default {
  name: "GameRoom",
  data() {
    return {
      messages: [],
      textInput: ""
    };
  },
  methods: {
    onMessageInput() {
      if (this.textInput === "") return;
      this.sendMessage(this.textInput);
      this.textInput = "";
    }
  }
};
</script>

<style></style>
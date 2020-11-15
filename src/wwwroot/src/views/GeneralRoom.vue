<template>
  <div class="tile is-ancestor">
    <div class="tile is-6 is-parent is-vertical">
      <article class="tile is-child">
        <div class="container">
          <h2 class="title is-2">Sketch</h2>
        </div>
      </article>
      <article class="tile is-child notification is-primary">
        <div class="field">
          <h3 class="title is-3">Rooms</h3>
        </div>
        <div class="select is-multiple is-fullwidth">
          <select multiple style="height:300px">
            <option
              v-for="room in gameRooms"
              :key="room.id"
              @click="onChangeRoom(room)"
              >{{ room.name }}</option
            >
          </select>
        </div>
      </article>

      <article class="tile is-child"></article>
    </div>

    <div class="tile is-6 is-parent is-vertical">
      <article class="tile is-child">
        <div class="select is-multiple is-fullwidth"></div>
      </article>

      <article class="tile is-child notification is-primary">
        <div class="chat">
          <p v-for="(message, index) in messages" :key="index">
            {{ message.content }}
          </p>
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
  },
  computed: {
    ...mapGetters("chat", {
      messages: "messages",
      isConnected: "isConnected",
      gameRooms: "gameRooms"
    })
  },
  methods: {
    ...mapActions("chat", ["sendMessage", "changeGameRoom"]),
    onMessageInput() {
      if (this.textInput === "") return;
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
.chat {
  background: white;
  height: 300px;
  color: black;
  overflow-y: scroll;
  font-family: "Roboto Mono", monospace;
}

.textarea.message {
  height: 50px;
  color: black;
  overflow-y: scroll;
}
</style>

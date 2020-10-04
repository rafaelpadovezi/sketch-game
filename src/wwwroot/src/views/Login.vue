<template>
  <div class="container">
    <div class="section columns is-centered is-vcentered">
      <div class="column is-two-fifths has-text-centered">
        <h1 class="title is-1">Sketch</h1>
        <div class="field has-addons">
          <div class="control is-expanded">
            <input
              class="input"
              type="text"
              placeholder="username"
              v-model="username"
            />
          </div>
          <div class="control">
            <a class="button is-primary" @click="onLogin(username)">
              Enter
            </a>
          </div>
        </div>
        <div class="error" v-if="errorMessage">
          <p>{{ errorMessage }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapActions } from "vuex";

export default {
  name: "Login",
  data: function() {
    return {
      username: "",
      errorMessage: ""
    };
  },
  methods: {
    ...mapActions("chat", ["login"]),
    onLogin(username) {
      this.errorMessage = "";
      return this.login(username).catch(err => {
        this.errorMessage = err.response.data;
      });
    }
  }
};
</script>

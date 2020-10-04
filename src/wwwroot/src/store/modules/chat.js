import api, { parseItem } from "../api.js";
import router from "../../router";

const WS_API = process.env.VUE_APP_WS;

const ADD_MESSAGE = "ADD_MESSAGE";
const CONNECT = "CONNECT";
const LOGIN = "LOGIN";
const DISCONNECT = "DISCONNECT";

export default {
  strict: process.env.NODE_ENV !== "production",
  namespaced: true,
  state: {
    socket: undefined,
    messages: [],
    player: undefined,
    isConnected: false
  },
  mutations: {
    [ADD_MESSAGE](state, message) {
      state.messages.push(message);
    },
    [CONNECT](state, socket) {
      state.socket = socket;
      state.isConnected = true;
    },
    [DISCONNECT](state) {
      state.socket = undefined;
      state.isConnected = false;
    },
    [LOGIN](state, player) {
      state.player = player;
    }
  },
  actions: {
    sendMessage({ state }, message) {
      state.socket.send(message);
    },
    onMessage({ commit }, response) {
      const serverResponse = JSON.parse(response.data);

      switch (serverResponse.Type) {
        case 0:
          commit(ADD_MESSAGE, serverResponse.Message);
      }
    },
    async connect({ commit, dispatch }, player) {
      const socket = new WebSocket(WS_API);

      socket.onopen = () => {
        commit(CONNECT, socket);
        router.push("/general");
        socket.send(player.id);
      };

      socket.onmessage = response => dispatch("onMessage", response);

      socket.onclose = () => commit(DISCONNECT);
    },
    async login({ commit, dispatch }, username) {
      const response = await api.post(
        "player/login",
        JSON.stringify(username),
        {
          headers: {
            "Content-Type": "application/json"
          }
        }
      );
      const player = parseItem(response, 200);
      commit(LOGIN, player);
      dispatch("connect", player);
    }
  },
  getters: {
    messages: state => state.messages,
    isConnected: state => state.isConnected
  }
};

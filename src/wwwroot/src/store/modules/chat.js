import api, { parseItem } from "../api.js";
import router from "../../router";

const WS_API = process.env.VUE_APP_WS;

const ADD_MESSAGE = "ADD_MESSAGE";
const CONNECT = "CONNECT";
const LOGIN = "LOGIN";
const DISCONNECT = "DISCONNECT";
const SET_GAME_ROOMS = "SET_GAME_ROOMS";
const CHANGE_ROOM = "CHANGE_ROOM";
const START_TIMER = "START_TIMER";

export default {
  strict: process.env.NODE_ENV !== "production",
  namespaced: true,
  state: {
    socket: undefined,
    gameRooms: [],
    messages: [],
    player: undefined,
    isConnected: false,
    gameRoom: undefined,
    timer: {
      total: 0,
      current: 0
    }
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
      state.messages.length = 0;
    },
    [LOGIN](state, player) {
      state.player = player;
    },
    [SET_GAME_ROOMS](state, gameRooms) {
      state.gameRooms.length = 0;
      state.gameRooms.push(...gameRooms);
    },
    [CHANGE_ROOM](state, room) {
      state.gameRoom = room;
      state.messages.length = 0;
      if (room === "general") router.push(`/general`);
      else router.push(`/gameroom`);
    },
    [START_TIMER](state, duration) {
      clearInterval(state.interval);
      state.timer.total = duration / 1000;
      state.timer.current = state.timer.total;
      state.interval = setInterval(() => {
        state.timer.current--;
        if (state.timer.current <= 0) clearInterval(state.interval);
      }, 1000);
    }
  },
  actions: {
    sendMessage({ state }, message) {
      state.socket.send(message);
    },
    onMessage({ commit }, response) {
      const serverResponse = JSON.parse(response.data);

      switch (serverResponse.type) {
        case 0:
        case 6:
          commit(ADD_MESSAGE, {
            content: serverResponse.message,
            type: serverResponse.type
          });
          break;
        case 7:
          commit(ADD_MESSAGE, {
            content: serverResponse.message,
            type: serverResponse.type
          });
          commit(START_TIMER, serverResponse.details[0]);
          break;
        case 2:
          commit(SET_GAME_ROOMS, serverResponse.details);
          break;
        case 4:
          commit(CHANGE_ROOM, serverResponse.details[0]);
          break;
        case 5:
          {
            const { results } = serverResponse.details[0];
            commit(ADD_MESSAGE, {
              content:
                "******* TURN RESULT ********\n" +
                "Name".padEnd(18, " ") +
                "Points\n" +
                "****************************\n" +
                Object.keys(results)
                  .map(key => key.padEnd(18, " ") + results[key])
                  .join("\n"),
              type: serverResponse.type
            });
          }
          break;
        case 8: {
          const { results } = serverResponse.details[0];
          commit(ADD_MESSAGE, {
            content:
              "******* FINAL RESULT *******\n" +
              "Name".padEnd(18, " ") +
              "Points\n" +
              "****************************\n" +
              Object.keys(results)
                .sort((a, b) => results[b] - results[a])
                .map(key => key.padEnd(18, " ") + results[key])
                .join("\n"),
            type: serverResponse.type
          });
        }
      }
    },
    goToGeneral({ dispatch }) {
      dispatch("sendMessage", `\\c general`);
    },
    changeGameRoom({ dispatch }, gameRoom) {
      dispatch("sendMessage", `\\c ${gameRoom.name}`);
    },
    async connect({ commit, dispatch }, player) {
      const socket = new WebSocket(WS_API);

      socket.onopen = () => {
        commit(CONNECT, socket);
        router.push("/general");
        socket.send(player.id);
        dispatch("sendMessage", "\\list");
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
    isConnected: state => state.isConnected,
    gameRooms: state => state.gameRooms,
    gameRoom: state => state.gameRoom,
    countdown: state => state.timer.current
  }
};

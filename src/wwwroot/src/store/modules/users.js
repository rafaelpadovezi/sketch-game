import api, { parseItem } from "../api.js";

const LOGIN = "LOGIN";

export default {
  strict: process.env.NODE_ENV !== "production",
  namespaced: true,
  state: {
    player: undefined
  },
  mutations: {
    [LOGIN](state, player) {
      state.player = player;
    }
  },
  actions: {
    async login({ commit }, username) {
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
    }
  },
  getters: {
    player: state => state.player
  }
};

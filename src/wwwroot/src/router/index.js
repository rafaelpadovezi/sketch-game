import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import GeneralRoom from "../views/GeneralRoom.vue";
import GameRoom from "../views/GameRoom.vue";

const routes = [
  {
    path: "/",
    name: "Login",
    component: Login
  },
  {
    path: "/general",
    name: "GeneralRoom",
    component: GeneralRoom
  },
  {
    path: "/gameroom",
    name: "GameRoom",
    component: GameRoom
  }
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
});

export default router;

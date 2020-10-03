import { shallowMount } from "@vue/test-utils";
import { createStore } from "vuex";
import Login from "@/views/Login.vue";

const shallowMountWithStore = (component, store) =>
  shallowMount(component, {
    global: {
      mocks: {
        $store: createStore(store)
      }
    }
  });

describe("Login.vue", () => {
  let store;
  let actions;

  beforeEach(() => {
    actions = {};
    store = {
      modules: {
        users: {
          actions,
          namespaced: true
        }
      }
    };
  });

  it("calls login action when button pressed", async () => {
    actions.login = jest.fn();
    const wrapper = shallowMountWithStore(Login, store);
    const button = wrapper.find("a.button");
    wrapper.find("input").setValue("user.test");

    await button.trigger("click");

    expect(actions.login).toHaveBeenCalled();
  });

  it("renders error message when login fails", async () => {
    actions.login = jest
      .fn()
      .mockRejectedValue({ response: { data: "Login is taken" } });
    const wrapper = shallowMountWithStore(Login, store);

    await wrapper.vm.onLogin("usertest");

    expect(wrapper.find(".error").exists()).toBeTruthy();
    expect(wrapper.find(".error").text()).toBe("Login is taken");
  });
});

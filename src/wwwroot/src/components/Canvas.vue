<template>
  <div class="drawing-control">
    <input type="color" :disabled="!isDrawing" v-model="lineColor" />
    <span class="icon is-clickable" @click="resetAndSend">
      <i class="fas fa-trash"></i>
    </span>
  </div>
  <div>
    <canvas
      :id="canvasId"
      class="canvas"
      :style="`height: ${height}px`"
      @mousedown="mouseDown"
    />
  </div>
</template>

<script>
// TODO: move all of this logic to master
// packages
const paper = require("paper");
export default {
  name: "Canvas",
  emits: ["draw"],
  props: {
    canvasId: String,
    height: Number,
    isDrawing: {
      type: Boolean,
      default: false
    }
  },
  data: () => ({
    path: null,
    scope: null,
    lineColor: "#000"
  }),
  methods: {
    resetAndSend() {
      this.reset();
      this.$emit("draw", {
        reset: true
      });
    },
    reset() {
      this.scope.project.activeLayer.removeChildren();
    },
    updateDrawing(drawing) {
      const { path, color, reset } = drawing;
      if (reset) {
        this.reset();
        return;
      }
      this.scope.activate();
      this.path = new paper.Path(path);
      this.path.strokeColor = color;
      this.path.strokeJoin = "round";
      this.path.strokeWidth = 1.5;
    },
    pathCreate(scope) {
      scope.activate();
      return new paper.Path({
        strokeColor: this.lineColor,
        strokeJoin: "round",
        strokeWidth: 1.5
      });
    },
    createTool(scope) {
      scope.activate();
      return new paper.Tool();
    },
    mouseDown() {
      // in order to access functions in nested tool
      let self = this;
      // create drawing tool
      this.tool = this.createTool(this.scope);
      this.tool.onMouseDown = event => {
        if (!this.isDrawing) return;
        // init path
        self.path = self.pathCreate(self.scope);
        // add point to path
        self.path.add(event.point);
      };
      this.tool.onMouseDrag = event => {
        if (!this.isDrawing) return;
        self.path.add(event);
      };
      this.tool.onMouseUp = event => {
        if (!this.isDrawing) return;
        // line completed
        self.path.add(event.point);
        self.$emit("draw", {
          path: self.path.pathData,
          color: this.lineColor,
          reset: false
        });
      };
    }
  },
  mounted() {
    this.scope = new paper.PaperScope();
    this.scope.setup(this.canvasId);
  }
};
</script>

<style scoped>
.canvas {
  cursor: crosshair;
  background: white;
  width: 530px;
  display: block;
  margin: auto;
}

.drawing-control {
  display: flex;
  width: 100%;
  background: #eee;
  border-bottom: 1px black solid;
}

.drawing-control .icon {
  color: black;
}
</style>

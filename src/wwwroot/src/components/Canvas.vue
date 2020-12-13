<template>
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
    scope: null
  }),
  methods: {
    reset() {
      this.scope.project.activeLayer.removeChildren();
    },
    updateDrawing(path) {
      this.scope.activate();
      this.path = new paper.Path(path);
      this.path.strokeColor = "#000000";
      this.path.strokeJoin = "round";
      this.path.strokeWidth = 1.5;
    },
    pathCreate(scope) {
      scope.activate();
      return new paper.Path({
        strokeColor: "#000000",
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
        self.$emit("draw", self.path.pathData);
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
</style>

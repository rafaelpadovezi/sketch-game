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
  props: ["canvasId", "height"],
  data: () => ({
    path: null,
    scope: null
  }),
  methods: {
    reset() {
      this.scope.project.activeLayer.removeChildren();
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
        // init path
        self.path = self.pathCreate(self.scope);
        // add point to path
        self.path.add(event.point);
      };
      this.tool.onMouseDrag = event => {
        self.path.add(event);
      };
      this.tool.onMouseUp = event => {
        // line completed
        self.path.add(event.point);
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
  width: 100%;
  display: block;
  margin: auto;
}
</style>

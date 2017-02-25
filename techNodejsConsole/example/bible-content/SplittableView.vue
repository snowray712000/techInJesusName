<template>
  <div class="splittable-view"
    :class="classObject">
    <div v-if="hasTabs">
      <div v-for="tab in this.content.tabs">
        Tabs: {{tab.bookIndex}}
      </div>
    </div>
    <div v-else-if="hasSubViews">
      <splittable-view
        v-for="content in this.content.subViews"
        :content="content"
        :subViewsOrientation="nextLevelSubViewsOrientation">
      </splittable-view>
    </div>
    <div v-else>
      Error! Please report this error.
    </div>
  </div>
</template>

<script>
export default {
  name: 'splittable-view',
  props: {
    content: {
      type: Object, // could be tabs or views
      required: true,
      default: function () { // TODO
        return { errorMsg: 'no object' }
      },
      validator: function (value) { // TODO
        return true
      }
    },
    subViewsOrientation: {
      type: String,
      validator: function (value) {
        return value === 'vertical' || value === 'horizontal' || value === null
      }
    }
  },
  data () {
    return {
      msg: this.content,
      hasTabs: this.content.hasOwnProperty('tabs'),
      hasSubViews: this.content.hasOwnProperty('subViews')
    }
  },
  computed: {
    nextLevelSubViewsOrientation: function () {
      if (this.content.hasOwnProperty('subViewsOrientation')) {
        return this.content.subViewsOrientation
      }
      return null
    },
    classObject: function () {
      return {
        horizontal: this.subViewsOrientation === 'horizontal',
        vertical: this.subViewsOrientation === 'vertical'
      }
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.splittable-view {
  border: solid 1px black;
}
.horizontal {
  display: inline-block;
}
.vertical {
  display: block;
}
</style>

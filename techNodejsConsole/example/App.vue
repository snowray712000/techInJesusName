<template>
  <div id="app">
    <draggable-view-container>
      <span slot="top-bar">top-bar</span>
      <div slot="left-bar">
        <div id="upper-left-bar-container">
          <span slot="upper-left-bar" v-for="view in upperLeftViews">{{view.name}}</span>
        </div>
        <div id="lower-left-bar-container">
          <span slot="lower-left-bar" v-for="view in lowerLeftViews">{{view.name}}</span>
        </div>
      </div>
      <span slot="right-bar" v-for="view in rightViews">{{view.name}}</span>
      <span slot="bottom-bar" v-for="view in bottomViews">{{view.name}}</span>
      <div slot="upper-left-view-container" v-for="view in upperLeftViews" :is="view.name"></div>
      <div slot="lower-left-view-container" v-for="view in lowerLeftViews" :is="view.name"></div>
      <div slot="right-view-container" v-for="view in rightViews" :is="view.name"></div>
      <div slot="bottom-view-container" v-for="view in bottomViews" :is="view.name"></div>
      <div slot="middleViewContainer">
        <splittable-view
          v-for="content in middleViewContent"
          :content="content"
          :subViewsOrientation="subViewsOrientation">
        </splittable-view>
      </div>
    </draggable-view-container>
  </div>
</template>

<script>
import store from 'store/index.js'
import { mapGetters } from 'vuex'
import DraggableViewContainer from 'components/DraggableViewContainer'
import SplittableView from 'components/views/bible-content/SplittableView'

import AdvancedSearch from 'components/views/AdvancedSearch'
import Archive from 'components/views/Archive'
import AudioBible from 'components/views/AudioBible'
import BibleMap from 'components/views/BibleMap'
import BookSelect from 'components/views/BookSelect'
import ChainReference from 'components/views/ChainReference'
import Footnotes from 'components/views/Footnotes'
import HistoryRecords from 'components/views/HistoryRecords'
import Parsing from 'components/views/Parsing'
import SearchResult from 'components/views/SearchResult'
import Sermons from 'components/views/Sermons'
import Settings from 'components/views/Settings'
import VersionSelect from 'components/views/VersionSelect'

// TODO: import only icons needs in production
import 'vue-awesome/icons'

export default {
  name: 'app',
  computed: {
    middleViewContent () {
      const container = store.state.middleViewContainer
      if (container.hasOwnProperty('subViews')) {
        return container.subViews
      }
      return container.tabs
    },
    subViewsOrientation () {
      if (store.state.middleViewContainer.hasOwnProperty('subViewsOrientation')) {
        return store.state.middleViewContainer.subViewsOrientation
      }
      return null
    },
    ...mapGetters({
      upperLeftViews: 'upperLeftViews',
      lowerLeftViews: 'lowerLeftViews',
      rightViews: 'rightViews',
      bottomViews: 'bottomViews'
    })
  },
  components: {
    SplittableView,
    DraggableViewContainer,
    AdvancedSearch,
    Archive,
    AudioBible,
    BibleMap,
    BookSelect,
    ChainReference,
    Footnotes,
    HistoryRecords,
    Parsing,
    SearchResult,
    Sermons,
    Settings,
    VersionSelect
  }
}
</script>

<style lang="scss" scoped>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>

let tChaRenCom = {
  props: {
    tp: { default: 1 }, //type: 0, 1
    itab: {default: 0 },//0, 1
    ibook: { default: -1 },
    ichap: { default: -1 },
    isec: { default: -1 },
    books: { default: fhl.g_book_all },
    cntchaps: { default: fhl.g_cnt_chap}
  },
  methods: {
    clicktab: function (itab0based) {
      if (itab0based === 1 && this.ibook > 0)
        this.itab = itab0based;
      if (itab0based !== 1)
        this.itab = itab0based;
    },
    clickbook: function (ibook1based) {
      this.ibook = ibook1based;
      var cntchap = this.cntchaps[this.ibook - 1];
      if (cntchap === 1) //如果只有1個chap,就直接自動選此章
        this.ichap = 1;
      else
        this.ichap = -1;
      this.isec = -1;

      this.itab = 1;

      console.log(this.ichap);
      if (this.ichap === 1)
        this.clickchap(this.ichap);
    },
    clickchap: function (ichap1based) {
      this.ichap = ichap1based;
      console.log(ichap1based);
      this.itab = 2;
      //var pthis = this;
      //getbible(this.ibook, this.ichap, function (a1) {
      //    // async when get
      //    var a2 = JSON.parse(a1);
      //    if (a2.status == "success") {
      //        pthis.record = a2.record;
      //    }
      //});
    },
    clicksec: function (isec1based) {
      this.isec = isec1based;
    }
  }
}; 
tChaRenCom.template = "    <div>        <div v-if=\"tp==0\">            <ul class=\"nav nav-tabs\">                <li v-bind:class=\"{active:itab==0}\" v-on:click=\"clicktab(0)\"><a href=\"#\">卷</a></li>                <li v-bind:class=\"{active:itab==1,disabled:ibook<1}\" v-on:click=\"clicktab(1)\"><a href=\"#\">章</a></li>                <li v-if=\"ibook>0\" v-bind:class=\"{active:itab==2}\" v-on:click=\"clicktab(2)\"><a href=\"#\">{{books[ibook-1][2]}}<span v-if=\"ichap>0\">{{ichap}}</span></a></li>            </ul>            <div v-if=\"itab==0\">                <span v-for=\"(a1,idx) in books\" v-on:click=\"clickbook(idx+1)\" class=\"btn\" v-bind:class=\"{beSelected:idx==ibook-1}\">{{a1[2]}} </span>            </div>            <div v-if=\"itab==1 && ibook>0\">                <span v-for=\"a1 in cntchaps[ibook-1]\" v-on:click=\"clickchap(a1)\" class=\"btn\" v-bind:class=\"{beSelected:a1==ichap-1}\">{{a1}} </span>            </div>        </div>        <div v-if=\"tp==1\">            <div>                <span v-for=\"(a1,idx) in books\" v-on:click=\"clickbook(idx+1)\" class=\"btn\" v-bind:class=\"{beSelected:idx==ibook-1}\">{{a1[2]}} </span>            </div>            <div v-if=\"ibook>0\">                <span v-for=\"a1 in cntchaps[ibook-1]\" v-on:click=\"clickchap(a1)\" class=\"btn\" v-bind:class=\"{beSelected:a1==ichap-1}\">{{a1}} </span>            </div>        </div>    </div>";
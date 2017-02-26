<template>
    <span>
        <span class="pr1">{{pr1}}</span>
        <template v-for="a1 in sp1">
            <span v-if="a1.tp==0" v-bind:class="{ky:a1.ky}" v-on:click="Ky1Click(a1,$event)">{{a1.tx}}</span>
            <span v-else-if="a1.tp==1" v-bind:class="{ky:a1.ky, sn:!a1.ky}" v-on:click="SnClick(a1,$event)">({{a1.tx}})</span>
            <span v-else-if="a1.tp==2" v-bind:class="{ky:a1.ky, sn:!a1.ky}" v-on:click="SnClick(a1,$event)"><{{a1.tx}}></span>
        </template>
    </span>
</template>

<script>
    let tSecRenCom = {
        props: {
            "bible_text": {
                type: String,
                required: true,
                default: '耶和華<WH03068>吩咐<WAH0413>摩西<WH04872>說<WH0559><WTH8799>：「你進去<WH0935><WTH8798>見<WAH0413>法老<WH06547>，對他<WAH0413>說<WH01696><WTH8765>：『耶和華<WH03068>─希伯來人<WH05680>的　神<WH0430>這樣<WAH03541>說<WH0559><WTH8804>：容<WAH0853>我的百姓<WH05971>去<WH07971><WTH8761>，好事奉我<WH05647><WTH8799>。'
            },
            "ky1": { type: Array, required: false, default: ["和", "摩西"] },
            "ky2": { type: Array, required: false, default: ["03068", "0413", "8799"] },
            "engs": { default: "Ex", type: String, required: false },
            "chinese": { default: "出", type: String, requred: false },
            "chap": { default: "9", type: String, requred: false },//配合sec
            "sec": { default: "1", type: String, requred: false },//sec將來可能是 "9-12",所以用String,非Int
            "version": { default: "unv", type: String, requred: false },
            "isv": { default: 0, type: Boolean, requred: false },//is version show
            "isb": { default: 0, type: Boolean, requred: false },//is book show
            "isc": { default: 0, type: Boolean, requred: false },//is chap show
            "iss": { default: 1, type: Boolean, requred: false },//is section show
        },
        data: function () { return {}; },
        methods: {
            SnClick: function (a1, event) {
                let jo = {};
                jo["tx"] = a1.tx;
                jo["x"] = event.clientX;
                jo["y"] = event.clientY;
                this.$emit('snclick', jo);
            },
            Ky1Click: function (a1, event) {
                let jo = {};
                jo["tx"] = a1.tx;
                jo["x"] = event.clientX;
                jo["y"] = event.clientY;
                if (a1.ky == 1)
                    this.$emit('ky1click', jo);
            }
        },
        computed: {
            sp1: function () {
                // split bible_text
                var od = {};
                let bible_text = this.bible_text;
                let ky1 = this.ky1;
                let ky2 = this.ky2;
                {
                    // 關鍵字
                    var r1 = "(?:" + ky1.join(")|(?:") + ")"; // reg 中的 (?:耶和華)|(?:摩西), '?:'是提升效率用的，不補捉
                    let reg1 = new RegExp(r1, 'g');
                    while ((match = reg1.exec(bible_text)) !== null) {
                        let jo = {};
                        jo["tp"] = 0;
                        jo["le"] = match[0].length;
                        jo["tx"] = match[0];
                        jo["ky"] = 1;
                        od[match.index.toString()] = jo;
                    }
                }
                {
                    // (08799a)
                    let reg1 = new RegExp('<(WTG|WTH)([0]*)([0-9]+a{0,1})>', 'g');
                    while ((match = reg1.exec(bible_text)) !== null) {

                        // "<WTH8799>", "WTH", "", "8799"
                        // "<WTH8799>", "WTH", "", "8799a"
                        let jo = {};
                        jo["tp"] = 1;
                        jo["le"] = match[0].length;
                        let sn = match[2] + match[3];
                        jo["tx"] = sn;
                        jo["ky"] = ky2.indexOf(sn) == -1 ? 0 : 1;
                        od[match.index.toString()] = jo;
                    }
                }
                {
                    // <04872a>
                    let reg1 = new RegExp('<(WH|WAH|WG|WAG|WH)([0]*)([0-9]+a{0,1})>', 'g');
                    while ((match = reg1.exec(bible_text)) !== null) {
                        // "<WH04872>", "WH", "0", "4872"
                        let jo = {};
                        jo["tp"] = 2;
                        jo["le"] = match[0].length;
                        let sn = match[2] + match[3];
                        jo["tx"] = sn;
                        jo["ky"] = ky2.indexOf(sn) == -1 ? 0 : 1;
                        od[match.index.toString()] = jo;
                    }
                }
                {
                    let r1 = [];
                    for (let a1 in od) {
                        let a2 = parseInt(a1);
                        r1.push(a2);
                        r1.push(a2 + od[a1].le);
                    }

                    let cur = 0;
                    for (let i = 0; i < r1.length; i += 2) {
                        if (cur == r1[i]) {
                            cur = r1[i + 1];
                        }
                        else {
                            let jo = {};
                            jo["tx"] = bible_text.substr(cur, r1[i] - cur);
                            jo["tp"] = 0;
                            jo["le"] = r1[i] - cur;
                            jo["ky"] = 0;
                            od[cur.toString()] = jo;

                            cur = r1[i + 1];
                        }
                    }
                    if (cur != bible_text.length) {
                        let jo = {}; //若這節經文不是SN結束，就要再多抓一個。
                        let le = bible_text.length - cur;
                        jo["tx"] = bible_text.substr(cur, le)
                        jo["tp"] = 0;
                        jo["le"] = le;
                        jo["ky"] = 0;
                        od[cur.toString()] = jo;
                    }
                }
                return od;
            },
            pr1: function () {
                // prefix  太 10:31(UNV), 太 10:31, 10:31, 31,

                if (this.isb) {
                    let verch = ["unv"]; //用中文的版本放這.
                    let na1 = verch.indexOf(this.version) == -1 ? this.engs : this.chinese;

                    let ver = "(" + this.version.toUpperCase() + ")";//(UNV)

                    if (this.isv && this.isc && this.iss)
                        return na1 + " " + this.chap + ":" + this.sec + ver; //太 10:31(UNV)
                    if (!this.isv && this.isc && this.iss)
                        return na1 + " " + this.chap + ":" + this.sec; //太 10:31
                    //if (this.isv && this.isc && !this.iss)
                    //  return na1 + " " + this.chap + ver; //太 10(UNV)
                    //if (!this.isv && this.isc && !this.iss)
                    //  return na1 + " " + this.chap; //太 10
                }
                else { //book false
                    let ver = "(" + this.version.toUpperCase() + ")";//(UNV)

                    if (this.isv && this.isc && this.iss)
                        return this.chap + ":" + this.sec + ver; //10:31(UNV)
                    if (!this.isv && this.isc && this.iss)
                        return this.chap + ":" + this.sec; //10:31
                    if (this.isv && !this.isc && this.iss)
                        return this.sec; //31(UNV)
                    if (!this.isv && !this.isc && this.iss)
                        return this.sec; //31
                }
                return "";
            }
        }
    };
</script>

<style scoped>
        .sn {
            color: blue;
            cursor:pointer;
        }

        .ky {
            color: red;
            cursor:pointer;
        }
        .pr1 {
            color:gray;
            font-size:small;
        }
</style>
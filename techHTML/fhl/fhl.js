const fhl = {
  g_book_all: JSON.parse('[["Gen","Genesis","創","創世記","Ge"],["Ex","Exodus","出","出埃及記","Ex"],["Lev","Leviticus","利","利未記","Le"],["Num","Numbers","民","民數記","Nu"],["Deut","Deuteronomy","申","申命記","De"],["Josh","Joshua","書","約書亞記","Jos"],["Judg","Judges","士","士師記","Jud"],["Ruth","Ruth","得","路得記","Ru"],["1 Sam","First Samuel","撒上","撒母耳記上","1Sa"],["2 Sam","Second Samuel","撒下","撒母耳記下","2Sa"],["1 Kin","First Kings","王上","列王紀上","1Ki"],["2 Kin","Second Kings","王下","列王紀下","2Ki"],["1 Chr","First Chronicles","代上","歷代志上","1Ch"],["2 Chr","Second Chronicles","代下","歷代志下","2Ch"],["Ezra","Ezra","拉","以斯拉記","Ezr"],["Neh","Nehemiah","尼","尼希米記","Ne"],["Esth","Esther","斯","以斯帖記","Es"],["Job","Job","伯","約伯記","Job"],["Ps","Psalms","詩","詩篇","Ps"],["Prov","Proverbs","箴","箴言","Pr"],["Eccl","Ecclesiastes","傳","傳道書","Ec"],["Song","Song of Solomon","歌","雅歌","So"],["Is","Isaiah","賽","以賽亞書","Isa"],["Jer","Jeremiah","耶","耶利米書","Jer"],["Lam","Lamentations","哀","耶利米哀歌","La"],["Ezek","Ezekiel","結","以西結書","Eze"],["Dan","Daniel","但","但以理書","Da"],["Hos","Hosea","何","何西阿書","Ho"],["Joel","Joel","珥","約珥書","Joe"],["Amos","Amos","摩","阿摩司書","Am"],["Obad","Obadiah","俄","俄巴底亞書","Ob"],["Jon","Jonah","拿","約拿書","Jon"],["Mic","Micah","彌","彌迦書","Mic"],["Nah","Nahum","鴻","那鴻書","Na"],["Hab","Habakkuk","哈","哈巴谷書","Hab"],["Zeph","Zephaniah","番","西番雅書","Zep"],["Hag","Haggai","該","哈該書","Hag"],["Zech","Zechariah","亞","撒迦利亞書","Zec"],["Mal","Malachi","瑪","瑪拉基書","Mal"],["Matt","Matthew","太","馬太福音","Mt"],["Mark","Mark","可","馬可福音","Mr"],["Luke","Luke","路","路加福音","Lu"],["John","John","約","約翰福音","Joh"],["Acts","Acts","徒","使徒行傳","Ac"],["Rom","Romans","羅","羅馬書","Ro"],["1 Cor","First Corinthians","林前","哥林多前書","1Co"],["2 Cor","Second Corinthians","林後","哥林多後書","2Co"],["Gal","Galatians","加","加拉太書","Ga"],["Eph","Ephesians","弗","以弗所書","Eph"],["Phil","Philippians","腓","腓立比書","Php"],["Col","Colossians","西","歌羅西書","Col"],["1 Thess","First Thessalonians","帖前","帖撒羅尼迦前書","1Th"],["2 Thess","Second Thessalonians","帖後","帖撒羅尼迦後書","2Th"],["1 Tim","First Timothy","提前","提摩太前書","1Ti"],["2 Tim","Second Timothy","提後","提摩太後書","2Ti"],["Titus","Titus","多","提多書","Tit"],["Philem","Philemon","門","腓利門書","Phm"],["Heb","Hebrews","來","希伯來書","Heb"],["James","James","雅","雅各書","Jas"],["1 Pet","First Peter","彼前","彼得前書","1Pe"],["2 Pet","Second Peter","彼後","彼得後書","2Pe"],["1 John","First John","約一","約翰一書","1Jo"],["2 John","second John","約二","約翰二書","2Jo"],["3 John","Third John","約三","約翰三書","3Jo"],["Jude","Jude","猶","猶大書","Jude"],["Rev","Revelation","啟","啟示錄","Re"]]'),
  g_cnt_chap: [50, 40, 27, 36, 34, 24, 21, 4, 31, 24, 22, 25, 29, 36, 10, 13, 10, 42, 150, 31, 12, 8, 66, 52, 5, 48, 12, 14, 3, 9, 1, 4, 7, 3, 3, 3, 2, 14, 4, 28, 16, 24, 21, 28, 16, 16, 13, 6, 6, 4, 4, 5, 3, 6, 4, 3, 1, 13, 5, 5, 3, 5, 1, 1, 1, 22],
};
function getbible(ibook1based, ichap1based, when_get) {
  var url = "qb.php?chineses=" + fhl.g_book_all[ibook1based - 1][2] + "&chap=" + ichap1based + "&version=unv&strong=1&gb=0";
  fhl.json_api_text(url, function (a1, a2) {
    when_get(a1);
  }, function (a1) {
    when_get(a1);
  }, this, true);
}
//var fhl = fhl || {};

//// listall.html 
//// [0][0]: Gen [0][1]=Genesis [0][2]=創 [0][3]=創世記 [0][4]=Ge ;
//fhl.g_book_all = JSON.parse('[["Gen","Genesis","創","創世記","Ge"],["Ex","Exodus","出","出埃及記","Ex"],["Lev","Leviticus","利","利未記","Le"],["Num","Numbers","民","民數記","Nu"],["Deut","Deuteronomy","申","申命記","De"],["Josh","Joshua","書","約書亞記","Jos"],["Judg","Judges","士","士師記","Jud"],["Ruth","Ruth","得","路得記","Ru"],["1 Sam","First Samuel","撒上","撒母耳記上","1Sa"],["2 Sam","Second Samuel","撒下","撒母耳記下","2Sa"],["1 Kin","First Kings","王上","列王紀上","1Ki"],["2 Kin","Second Kings","王下","列王紀下","2Ki"],["1 Chr","First Chronicles","代上","歷代志上","1Ch"],["2 Chr","Second Chronicles","代下","歷代志下","2Ch"],["Ezra","Ezra","拉","以斯拉記","Ezr"],["Neh","Nehemiah","尼","尼希米記","Ne"],["Esth","Esther","斯","以斯帖記","Es"],["Job","Job","伯","約伯記","Job"],["Ps","Psalms","詩","詩篇","Ps"],["Prov","Proverbs","箴","箴言","Pr"],["Eccl","Ecclesiastes","傳","傳道書","Ec"],["Song","Song of Solomon","歌","雅歌","So"],["Is","Isaiah","賽","以賽亞書","Isa"],["Jer","Jeremiah","耶","耶利米書","Jer"],["Lam","Lamentations","哀","耶利米哀歌","La"],["Ezek","Ezekiel","結","以西結書","Eze"],["Dan","Daniel","但","但以理書","Da"],["Hos","Hosea","何","何西阿書","Ho"],["Joel","Joel","珥","約珥書","Joe"],["Amos","Amos","摩","阿摩司書","Am"],["Obad","Obadiah","俄","俄巴底亞書","Ob"],["Jon","Jonah","拿","約拿書","Jon"],["Mic","Micah","彌","彌迦書","Mic"],["Nah","Nahum","鴻","那鴻書","Na"],["Hab","Habakkuk","哈","哈巴谷書","Hab"],["Zeph","Zephaniah","番","西番雅書","Zep"],["Hag","Haggai","該","哈該書","Hag"],["Zech","Zechariah","亞","撒迦利亞書","Zec"],["Mal","Malachi","瑪","瑪拉基書","Mal"],["Matt","Matthew","太","馬太福音","Mt"],["Mark","Mark","可","馬可福音","Mr"],["Luke","Luke","路","路加福音","Lu"],["John","John","約","約翰福音","Joh"],["Acts","Acts","徒","使徒行傳","Ac"],["Rom","Romans","羅","羅馬書","Ro"],["1 Cor","First Corinthians","林前","哥林多前書","1Co"],["2 Cor","Second Corinthians","林後","哥林多後書","2Co"],["Gal","Galatians","加","加拉太書","Ga"],["Eph","Ephesians","弗","以弗所書","Eph"],["Phil","Philippians","腓","腓立比書","Php"],["Col","Colossians","西","歌羅西書","Col"],["1 Thess","First Thessalonians","帖前","帖撒羅尼迦前書","1Th"],["2 Thess","Second Thessalonians","帖後","帖撒羅尼迦後書","2Th"],["1 Tim","First Timothy","提前","提摩太前書","1Ti"],["2 Tim","Second Timothy","提後","提摩太後書","2Ti"],["Titus","Titus","多","提多書","Tit"],["Philem","Philemon","門","腓利門書","Phm"],["Heb","Hebrews","來","希伯來書","Heb"],["James","James","雅","雅各書","Jas"],["1 Pet","First Peter","彼前","彼得前書","1Pe"],["2 Pet","Second Peter","彼後","彼得後書","2Pe"],["1 John","First John","約一","約翰一書","1Jo"],["2 John","second John","約二","約翰二書","2Jo"],["3 John","Third John","約三","約翰三書","3Jo"],["Jude","Jude","猶","猶大書","Jude"],["Rev","Revelation","啟","啟示錄","Re"]]');

//fhl.g_cnt_chap = [50, 40, 27, 36, 34, 24, 21, 4, 31, 24, 22, 25, 29, 36, 10, 13, 10, 42, 150, 31, 12, 8, 66, 52, 5, 48, 12, 14, 3, 9, 1, 4, 7, 3, 3, 3, 2, 14, 4, 28, 16, 24, 21, 28, 16, 16, 13, 6, 6, 4, 4, 5, 3, 6, 4, 3, 1, 13, 5, 5, 3, 5, 1, 1, 1, 22];

fhl.json_api_text = function json_api_text(url, fncb_success, fncb_error, obj_param, isAsync) {
  /// <summary> 取fhl的json資料, 但是確是取得最原始資料, 原因是 json 有時候不正確, 還是回傳純文字好了 </summary>
  /// <param type="string" name="url">例如 se.php?q=.... 不用包含全部網址 </param>
  /// <param type="Action&lt;string,T>" name="fncb_success">當API成功，要作什麼事，arg1是回傳的文字，arg2是obj_param傳入的。可傳null表示不作事</param>
  /// <param type="Action&lt;string,T>" name="fncb_error">當API失敗時，要作什麼事，arg1是回傳的文字，arg2是obj_param傳入的。可傳null表示不作事</param>
  /// <param type="Action&lt;T>" name="obj_param">傳入給fnch_success第2個參數。通常是作為存回傳值用的</param>
  /// <param type="bool" name="isAsync" optional="true">true表示主執行緒會繼續執行，false表示主執行緒會等這個api回傳後再繼續。</param>

  if (isAsync == undefined)//default value
    isAsync = true;
  var root_url = "https://bible.fhl.net/json/";
  var ab_url = root_url + url;
  ab_url = encodeURI(ab_url).replace("#", "%23"); // encodeURI 不會轉換#符號, 手動轉換
  return $.ajax({
    url: ab_url,
    type: "GET",
    dataType: "text",
    async: isAsync,
    error: function (jstr) {
      console.debug("xml api error ...");
      if (fncb_error != null) fncb_error(jstr, obj_param);
    },
    success: function (jstr) {
      if (fncb_success != null) fncb_success(jstr, obj_param);
    }
  });// $.ajax(...);
};//fhl.json_api_text function
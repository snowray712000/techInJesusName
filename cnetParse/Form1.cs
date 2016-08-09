using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novacode;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Data.SQLite;
namespace cnetParse
{
  public partial class Form1 : Form
  {
    private string srd;
    private string[][] book_name_reference;

    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      srd = AppDomain.CurrentDomain.BaseDirectory;

      {
        var aaa = @"1,Gen,Genesis,創,創世記,Ge
2,Ex,Exodus,出,出埃及記,Ex
3,Lev,Leviticus,利,利未記,Le
4,Num,Numbers,民,民數記,Nu
5,Deut,Deuteronomy,申,申命記,De
6,Josh,Joshua,書,約書亞記,Jos
7,Judg,Judges,士,士師記,Jud
8,Ruth,Ruth,得,路得記,Ru
9,1 Sam,First Samuel,撒上,撒母耳記上,1Sa
10,2 Sam,Second Samuel,撒下,撒母耳記下,2Sa
11,1 Kin,First Kings,王上,列王紀上,1Ki
12,2 Kin,Second Kings,王下,列王紀下,2Ki
13,1 Chr,First Chronicles,代上,歷代志上,1Ch
14,2 Chr,Second Chronicles,代下,歷代志下,2Ch
15,Ezra,Ezra,拉,以斯拉記,Ezr
16,Neh,Nehemiah,尼,尼希米記,Ne
17,Esth,Esther,斯,以斯帖記,Es
18,Job,Job,伯,約伯記,Job
19,Ps,Psalms,詩,詩篇,Ps
20,Prov,Proverbs,箴,箴言,Pr
21,Eccl,Ecclesiastes,傳,傳道書,Ec
22,Song,Song of Solomon,歌,雅歌,So
23,Is,Isaiah,賽,以賽亞書,Isa
24,Jer,Jeremiah,耶,耶利米書,Jer
25,Lam,Lamentations,哀,耶利米哀歌,La
26,Ezek,Ezekiel,結,以西結書,Eze
27,Dan,Daniel,但,但以理書,Da
28,Hos,Hosea,何,何西阿書,Ho
29,Joel,Joel,珥,約珥書,Joe
30,Amos,Amos,摩,阿摩司書,Am
31,Obad,Obadiah,俄,俄巴底亞書,Ob
32,Jon,Jonah,拿,約拿書,Jon
33,Mic,Micah,彌,彌迦書,Mic
34,Nah,Nahum,鴻,那鴻書,Na
35,Hab,Habakkuk,哈,哈巴谷書,Hab
36,Zeph,Zephaniah,番,西番雅書,Zep
37,Hag,Haggai,該,哈該書,Hag
38,Zech,Zechariah,亞,撒迦利亞書,Zec
39,Mal,Malachi,瑪,瑪拉基書,Mal
40,Matt,Matthew,太,馬太福音,Mt
41,Mark,Mark,可,馬可福音,Mr
42,Luke,Luke,路,路加福音,Lu
43,John,John,約,約翰福音,Joh
44,Acts,Acts,徒,使徒行傳,Ac
45,Rom,Romans,羅,羅馬書,Ro
46,1 Cor,First Corinthians,林前,哥林多前書,1Co
47,2 Cor,Second Corinthians,林後,哥林多後書,2Co
48,Gal,Galatians,加,加拉太書,Ga
49,Eph,Ephesians,弗,以弗所書,Eph
50,Phil,Philippians,腓,腓立比書,Php
51,Col,Colossians,西,歌羅西書,Col
52,1 Thess,First Thessalonians,帖前,帖撒羅尼迦前書,1Th
53,2 Thess,Second Thessalonians,帖後,帖撒羅尼迦後書,2Th
54,1 Tim,First Timothy,提前,提摩太前書,1Ti
55,2 Tim,Second Timothy,提後,提摩太後書,2Ti
56,Titus,Titus,多,提多書,Tit
57,Philem,Philemon,門,腓利門書,Phm
58,Heb,Hebrews,來,希伯來書,Heb
59,James,James,雅,雅各書,Jas
60,1 Pet,First Peter,彼前,彼得前書,1Pe
61,2 Pet,Second Peter,彼後,彼得後書,2Pe
62,1 John,First John,約一,約翰一書,1Jo
63,2 John,second John,約二,約翰二書,2Jo
64,3 John,Third John,約三,約翰三書,3Jo
65,Jude,Jude,猶,猶大書,Jude
66,Rev,Revelation,啟,啟示錄,Re";
        var b = aaa.Split('\n');
        book_name_reference = b.Select(a1 => a1.Trim().Split(',')).ToArray();
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      //var aaa =File.ReadAllText("01.xml");
      //var reg = new Regex(@"<(\w+)([^>]*)>[\S\s]*<\/\1>", RegexOptions.Multiline);
      //var aa= reg.Match(aaa);
      
      //// 一本書
      var files = Directory.EnumerateFiles(srd, "*.docx", SearchOption.TopDirectoryOnly);
      var file0 = files.First();

      System.Xml.Linq.XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
      using (var doc = DocX.Load(file0))
      {
        List<oneP> elemPs = new List<oneP>();
        
        foreach(var par in doc.Paragraphs)
        {
          var o1 = new oneP();
          o1.style = par.StyleName;
          
          var elemsR  = par.Xml.Elements(w + "r").ToList();
          elemsR.ForEach(a1 => {
            var rPr = a1.Element(w + "rPr");
            Func<string> rstyle = () => {
              //var rr1 = a1.Element(w + "rPr");//因為很多人用,就在外面先取
              var rr2 = rPr == null ? null : rPr.Element(w + "rStyle");
              return rr2==null?null:rr2.Attribute(w+ "val").Value;
            };
            Func<string> t = () => {
              var rr1 = a1.Element(w + "t");
              return rr1 == null ? null : rr1.Value;
            };
            Func<int> ft = () => {
              var rr1 = a1.Element(w + "footnoteReference");
              return rr1 == null ? -1 : int.Parse( rr1.Attribute(w+"id").Value);
            };
            Func<bool> underline = () => {
              //var rr1 = a1.Element(w + "rPr");//因為很多人用,就在外面先取
              var rr2 = rPr == null ? null : rPr.Element(w + "u");
              return rr2 == null ? false : true;
            };
            Func<string> lang = () => {
              var r2 = rPr == null ? null : rPr.Element(w + "lang");
              return r2 == null ? null : r2.Attribute(w + "eastAsia").Value;
            };

            var o2 = new oneR();
            o2.style = rstyle();
            o2.t = t();
            o2.ft = ft();
            o2.underline = underline();
            o2.lang = lang();

            o1.elemRs.Add(o2);
          });
          elemPs.Add(o1);
        }

        {
          int sum = 0;
          elemPs.ForEach(a1 => sum += a1.elemRs.Count);
          foreach (var p in elemPs)
          {
            for (int i = 0; i < p.elemRs.Count-1; i++)
            {
              var r1 = p.elemRs[i];
              var r2 = p.elemRs[i + 1];
              if ( r1.style =="Verse" && r1.t.Trim().Length==0)// @verbatim 後來發現Verse 中可能有「空字串」 @endverbatim
              {
                p.elemRs.RemoveAt(i);i--;
                continue;//next verse
              }
              if ( r1.style == "Verse" && r2.style=="Verse")//大概會有幾千筆(出埃及記 806/8458)機率
              {
                r2.t = r1.t + r2.t;
                p.elemRs.RemoveAt(i);
                i--;
                continue;//next verse
              }
              
              if ( r1.underline && r2.underline && r1.ft==-1 && r2.ft==-1 ) // 2/8458機率
              {
                // <u>以</u><u>色列</u> 要還原為 <u>以色列</u>
                if ( r1.style == r2.style)
                {
                  r2.t = r1.t + r2.t;
                  p.elemRs.RemoveAt(i);
                  i--;
                }
              }
            }
          }

          {// Verse 卻是特別的 (27:31), 同時把空的Verse去掉 
            var reg1 = new Regex(@"([0-9]+):([0-9]+)", RegexOptions.Compiled);
            var reg2 = new Regex(@"\(([0-9]+):([0-9下]+)\)", RegexOptions.Compiled);//多了一個下,是為了以賽亞書
            elemPs.ForEach(a1 => {
              var r1 = a1.elemRs.Where(aa1 => aa1.style == "Verse");
              var r2 = r1.Where(aa1 => reg1.IsMatch(aa1.t)==false).ToList();
              if ( r2.Count !=0)//以賽亞書有3處,
                r2.ForEach(aa1 => aa1.style = null);
             
              var r3 = r1.Where(aa1 => reg2.IsMatch(aa1.t)).ToList();
              if ( r3!=null && r3.Count != 0)
              {
                if ( r3.First().t == "5:1(4:17)") //傳道書
                {
                  r3.First().t = "5:1";
                  var addR = new oneR();
                  addR.t = "(4:17)";
                  var idx = a1.elemRs.FindIndex(aa1 => aa1.t == "5:1");
                  var tmp2 = a1.elemRs.Take(idx+1).ToList();
                  tmp2.Add(addR);
                  tmp2.AddRange(a1.elemRs.Skip(idx+1));
                  a1.elemRs = tmp2;
                }
                else if ( r3.First().t=="(63:19下)")//以賽亞書
                {
                  r3.First().style = null;
                }
                else
                  r3.ForEach(aa1 => aa1.style = null); //使它成為一般文字
              }
            });
          }

          {// 可能錯字, 簡打成繁
            Console.WriteLine("可能有簡體字");
            elemPs.ForEach(a1 => {
              var r1=a1.elemRs.Any(aa1 => aa1.lang != null && aa1.ft==-1 && aa1.lang == "zh-CN");
              var r2 = a1.elemRs.Where(aa1 => aa1.lang != null && aa1.ft ==-1 && aa1.lang=="zh-CN");
              if ( r1 == true )
              {
                var msg = "「" + string.Join(",", r2.Select(aa1=>aa1.t).ToArray()) + "」";
                Console.WriteLine(a1.elemRs.First().t + ":" + msg);
              }
            });
          }

          {// check 章節是否有不合理
            Console.WriteLine("可能有錯的經節");

            int ichap = 1;
            int isec = 0;
            var reg1 = new Regex(@"1:1");
            var reg2 = new Regex(@"2:1");
            for (int j = 0; j < elemPs.Count; j++)
            {
              var r1 = elemPs[j];
              for (int i = 0; i < r1.elemRs.Count; i++)
              {
                var r2 = r1.elemRs[i];
                if (r2.style != "Verse")
                  continue;
                if (reg1.IsMatch(r2.t))
                {
                  isec++;
                  reg1 = new Regex(ichap + ":" + (isec + 1));
                }
                else if (reg2.IsMatch(r2.t))
                {
                  isec = 1;
                  ichap++;
                  reg1 = new Regex(ichap + ":2" );
                  reg2 = new Regex((ichap + 1) + ":1");
                }
                else
                {
                  Console.WriteLine(r2.t);
                }
              }
            }
          }

          sum = 0;
          elemPs.ForEach(a1 => sum += a1.elemRs.Count);
        }
        

        File.WriteAllText("23-step1.txt", string.Join("\r\n", elemPs));
      }
    }
    public class oneP
    {
      public string style=null;
      public List<oneR> elemRs=new List<oneR>();
      public override string ToString()
      {
        return style + " " + string.Join(",", elemRs);
      }
    }
    public class oneR
    {
      public string style=null;
      public string t=null;
      public int ft=-1;
      internal bool underline=false;
      internal string lang=null;
      //public override bool Equals(object obj)
      //{
      //  var o=obj as oneR;
      //  if (o.ft != ft || o.underline != underline || o.lang != lang || o.style != style)
      //    return false;
      //  return true;
      //  return base.Equals(obj);
      //}
      public override string ToString()
      {
        if (ft != -1)
          return "【" + ft + "】";
        if (t != null)
          return underline? "<u>"+t+"</u>": t ;
        
        return style == null ? "" : "(s:" + style + ")";
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      var files = Directory.EnumerateFiles(srd, "*.docx", SearchOption.AllDirectories);
      var reg1_na = new Regex(@"^([0-9]+)[ -]", RegexOptions.Compiled);
      foreach (var file in files)
      {
        var dir = Path.GetDirectoryName(file);
        var na = Path.GetFileNameWithoutExtension(file);
        var mat1 = reg1_na.Match(na);
        if (mat1 == null)
          continue; //next file


        var ibook = int.Parse(mat1.Groups[1].Value);

          System.Xml.Linq.XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
          List<oneP> elemPs = new List<oneP>();

        List<string> footnotestext = new List<string>();

        oneP pP = null; //debug看看剛剛算到哪,造成throw
        try
        {
          using (var doc = DocX.Load(file))
          {
            foreach (var par in doc.Paragraphs)
            {
              var o1 = new oneP();
              pP = o1;//debug
              o1.style = par.StyleName;

              var elemsR = par.Xml.Elements(w + "r").ToList();
              elemsR.ForEach(a1 =>
              {
                var rPr = a1.Element(w + "rPr");
                Func<string> rstyle = () =>
                {
                  //var rr1 = a1.Element(w + "rPr");//因為很多人用,就在外面先取
                  var rr2 = rPr == null ? null : rPr.Element(w + "rStyle");
                  return rr2 == null ? null : rr2.Attribute(w + "val").Value;
                };
                Func<string> t = () =>
                {
                  var rr1 = a1.Element(w + "t");
                  return rr1 == null ? null : rr1.Value;
                };
                Func<int> ft = () =>
                {
                  var rr1 = a1.Element(w + "footnoteReference");
                  return rr1 == null ? -1 : int.Parse(rr1.Attribute(w + "id").Value);
                };
                Func<bool> underline = () =>
                {
                  //var rr1 = a1.Element(w + "rPr");//因為很多人用,就在外面先取
                  var rr2 = rPr == null ? null : rPr.Element(w + "u");
                  return rr2 == null ? false : true;
                };
                Func<string> lang = () =>
                {
                  var r2 = rPr == null ? null : rPr.Element(w + "lang");
                  var r3 = r2 == null ? null : r2.Attribute(w + "eastAsia");
                  return r3 == null ? null : r3.Value;
                };

                var o2 = new oneR();
                o2.style = rstyle();
                o2.t = t();
                o2.ft = ft();
                o2.underline = underline();
                o2.lang = lang();

                o1.elemRs.Add(o2);
              });
              elemPs.Add(o1);
            }

            // 下面這個是獨立的. 註腳
            footnotestext.AddRange(doc.FootnotesText);
          }
        }
        catch(Exception ex)
        {
          continue;//next doc
        }

        Action fix1 = () => {
          /// @verbatim 
          /// 1. 把所有的 Verse 卻是空字串的移除
          /// 2. 把所有的 Verse 相鄰的連在一起 (主要)
          /// 3. 把所有的 underline 相鄰卻被斷開的合在一起
          /// @endverbatim
          foreach (var p in elemPs)
          {
            for (int i = 0; i < p.elemRs.Count - 1; i++)
            {
              var r1 = p.elemRs[i];
              var r2 = p.elemRs[i + 1];
              if (r1.style == "Verse" && r1.t != null && r1.t.Trim().Length == 0)// @verbatim 後來發現Verse 中可能有「空字串」 @endverbatim
              {
                p.elemRs.RemoveAt(i); i--;
                continue;//next verse
              }
              if (r1.style == "Verse" && r2.style == "Verse")//大概會有幾千筆(出埃及記 806/8458)機率
              {
                r2.t = r1.t + r2.t;
                p.elemRs.RemoveAt(i);
                i--;
                continue;//next verse
              }

              if (r1.underline && r2.underline && r1.ft == -1 && r2.ft == -1) // 2/8458機率
              {
                // <u>以</u><u>色列</u> 要還原為 <u>以色列</u>
                if (r1.style == r2.style && r1.lang == r2.lang )
                {
                  r2.t = r1.t + r2.t;
                  p.elemRs.RemoveAt(i);
                  i--;
                }
                else if (r1.style == r2.style )
                {
                  r2.t = r1.t + r2.t;
                  p.elemRs.RemoveAt(i);
                  i--;
                }
              }
              
            }
          }
        };

        fix1();

        Action fix2 = () => {
          /// @verbatim
          /// fix2中必須把fix1完整作完才能作, 例如若去判斷 1, :2 就會錯誤, fix1 會先把它們合在一起
          /// 1. 有些 (21:42) 其實是被算在一般文字中, 就把style從verse轉為null (主要)
          /// 2. 承上 (63:19下) 以賽亞書
          /// 3. 傳道書 5:1(4:17) 連在一起分開
          /// 4. 是Verse卻不是 xx:xx 格式, 而是文字, 變為一般文字
          /// @endverbatim
          // Verse 卻是特別的 (27:31), 同時把空的Verse去掉 
          var reg1 = new Regex(@"([0-9]+):([0-9]+)", RegexOptions.Compiled);
          var reg2 = new Regex(@"\(([0-9]+):([0-9下]+)\)", RegexOptions.Compiled);//多了一個下,是為了以賽亞書
          elemPs.ForEach(a1 => {
            var r1 = a1.elemRs.Where(aa1 => aa1.style == "Verse");
            var r2 = r1.Where(aa1 => reg1.IsMatch(aa1.t) == false).ToList();
            if (r2.Count != 0)//以賽亞書有3處,
              r2.ForEach(aa1 => aa1.style = null);

            var r3 = r1.Where(aa1 => reg2.IsMatch(aa1.t)).ToList();
            if (r3 != null && r3.Count != 0)
            {
              if (r3.First().t == "5:1(4:17)") //傳道書
              {
                r3.First().t = "5:1";
                var addR = new oneR();
                addR.t = "(4:17)";
                var idx = a1.elemRs.FindIndex(aa1 => aa1.t == "5:1");
                var tmp2 = a1.elemRs.Take(idx + 1).ToList();
                tmp2.Add(addR);
                tmp2.AddRange(a1.elemRs.Skip(idx + 1));
                a1.elemRs = tmp2;
              }
              else if (r3.First().t == "(63:19下)")//以賽亞書
              {
                r3.First().style = null;
              }
              else
                r3.ForEach(aa1 => aa1.style = null); //使它成為一般文字
            }
          });
        };

        fix2();

        Action fix3 = () => {
          while (footnotestext[0].Length == 0)
            footnotestext.RemoveAt(0);
        };
        fix3();

        Action maybe1 = () => {
          // 可能錯字, 簡打成繁
          StringBuilder str = new StringBuilder();
          str.Append("可能有簡體字\r\n");
            //Console.WriteLine("可能有簡體字");

            elemPs.ForEach(a1 => {
              var r1 = a1.elemRs.Any(aa1 => aa1.lang != null && aa1.ft == -1 && aa1.lang == "zh-CN");
              var r2 = a1.elemRs.Where(aa1 => aa1.lang != null && aa1.ft == -1 && aa1.lang == "zh-CN");
              if (r1 == true)
              {
                var msg = "「" + string.Join(",", r2.Select(aa1 => aa1.t).ToArray()) + "」";
                //Console.WriteLine(a1.elemRs.First().t + ":" + msg);
                str.Append(a1.elemRs.First().t + ":" + msg +"\r\n" );
              }
            });
          if ( str.Length > 8)
          {
          File.WriteAllText(ibook + "-maybe簡體.txt", str.ToString());
          }
        };

        maybe1();

        Action maybe2 = () => {
            // check 章節是否有不合理
            StringBuilder str = new StringBuilder();
            str.Append("可能有錯的經節\r\n");

            int ichap = 1;
            int isec = 0;
            var reg1 = new Regex(@"1:1");
            var reg2 = new Regex(@"2:1");
            for (int j = 0; j < elemPs.Count; j++)
            {
              var r1 = elemPs[j];
              for (int i = 0; i < r1.elemRs.Count; i++)
              {
                var r2 = r1.elemRs[i];
                if (r2.style != "Verse")
                  continue;
                if (reg1.IsMatch(r2.t))
                {
                  isec++;
                  reg1 = new Regex(ichap + ":" + (isec + 1));
                }
                else if (reg2.IsMatch(r2.t))
                {
                  isec = 1;
                  ichap++;
                  reg1 = new Regex(ichap + ":2");
                  reg2 = new Regex((ichap + 1) + ":1");
                }
                else
                {
                  Console.WriteLine(r2.t);
                  str.Append(r2.t + "\r\n");
                }
              }
            }

          if ( str.Length > 9)
          {
            File.WriteAllText(ibook + "-maybe章節分析.txt", str.ToString());
          }

        };

        maybe2();

        //var srd2 = AppDomain.CurrentDomain.BaseDirectory + "step1/";
        //Directory.CreateDirectory(srd2);
        //File.WriteAllText( srd2 +  ibook + "-step1.txt", string.Join("\r\n", elemPs) );

        //// 註腳. 
        //var srd3 = AppDomain.CurrentDomain.BaseDirectory + "footnotes/";
        //Directory.CreateDirectory(srd3);
        //File.WriteAllText(srd3 + ibook + "-footnotes.txt", string.Join("\r\n", footnotestext));

        {
          // 下面已經確定過, 沒有一個 PTitle 會有2個以上的 Verse ... 歷下30:6修正後.
          //var aa = elemPs.Where(a1 => a1.style == "CParagraphTitle" && a1.elemRs.Where(aa1 => aa1.style == "Verse").Count() > 1);

          JObject jtxt = new JObject();
          string verse = null;
          string title = "";
          var body = new StringBuilder();
          for (int ip = 0; ip < elemPs.Count; ip++)
          {
            var p = elemPs[ip];
            if (p.style != "CTitle" && p.style != "CParagraphTitle")
            {
              for (int ir = 0; ir < p.elemRs.Count; ir++)
              {
                var r = p.elemRs[ir];
                if ( r.style != "Verse")
                {
                  body.Append(r.ToString());
                }
                else
                {
                  if (verse != null )
                  {
                    jtxt[verse] = title.Length==0? body.ToString() : title + body.ToString();
                    verse = null;
                    title = "" ; body.Clear(); //因為要+=, 所以不設為 null
                  }
                  verse = r.t;
                }
              }
            }
            else if ( p.style != "CTitle")
            {
              if ( verse != null )
              {
                jtxt[verse] = title.Length == 0? body.ToString() : title + body.ToString();
                verse = null;
                title = ""; body.Clear();//因為要+=用, 所以不用 null
              }

              var title1 = new StringBuilder();
              for (int ir = 0; ir < p.elemRs.Count; ir++)
              {
                var r = p.elemRs[ir];
                if (r.style != "Verse")
                  title1.Append(r.ToString());
                else
                  verse = r.t;
              }
              title += "<h3>"+ title1.ToString() + "</h3>";
            }
            else { }//"CTitle"

          }
          if ( verse != null )
          {
            jtxt[verse] = title.Length == 0 ? body.ToString() : title + body.ToString();
            verse = null;
            title = ""; body.Clear();//因為要+=用, 所以不用 null
          }

          // 註腳. 
          var srd3 = AppDomain.CurrentDomain.BaseDirectory + "jtxt/";
          Directory.CreateDirectory(srd3);
          File.WriteAllText(srd3 + ibook + "-jtxt.txt", jtxt.ToString());
        }

        // sql 
        {

        }
      }
    }
//    void sql()
//    {
//      var path = "cnet.sqlite";
//      using (var sqlite_connect = new SQLiteConnection("Data source=" + path))
//      {
//        sqlite_connect.Open();

//        {
//          var cmd = sqlite_connect.CreateCommand(); //create command
//          cmd.CommandText =
//            @"CREATE TABLE IF NOT EXISTS cnet (
//id INTEGER, 
//engs TEXT,
//chap INTEGER,
//sec INTEGER,
//txt TEXT,
//username TEXT,
//modtime TEXT,
//origuser TEXT
//)";
//          cmd.ExecuteNonQuery();

//          cmd.CommandText = "delete from cnet";
//          cmd.ExecuteNonQuery();
//        }

//        {
//          var cmd = sqlite_connect.CreateCommand(); //create command
//          cmd.CommandText =
//            @"CREATE TABLE IF NOT EXISTS recnet  (
//engs TEXT,
//id INTEGER, 
//txt TEXT
//)";
//          cmd.ExecuteNonQuery();

//          cmd.CommandText = "delete from recnet";
//          cmd.ExecuteNonQuery();
//        }

//        {// jtxt
//          int cnt = 0;
//          var tran = sqlite_connect.BeginTransaction();
//          try
//          {
//            var txtid = 0;
//            foreach (var jb in jnet.Properties())
//            {
//              int bookid = int.Parse(jb.Name); // 1-based

//              //var engs = bookid.ToString();
//              var engs = (string)jbookid2nas[bookid.ToString()][0];

//              var jtxt = jb.Value["txt"] as JObject;
//              foreach (var jv in jtxt.Properties())
//              {
//                var str1s = jv.Name.Split(':');
//                var ichap = int.Parse(str1s[0]);
//                var isec = int.Parse(str1s[1]);

//                var cmd = sqlite_connect.CreateCommand(); //create command
//                                                          // id, engs, chap, sec, txt, username, xxx, xxx

//                cmd.CommandText =
//  @"INSERT INTO cnet VALUES (" + txtid++ +
//  @",'" + engs.Replace("'", "''") +
//  @"'," + ichap +
//  @"," + isec +
//  @",'" + ((string)jv.Value).Replace("'", "''") +
//  "',null,null,null);";
//                cmd.ExecuteNonQuery();



//                //if ( cnt++ > 3646)
//                //{
//                //  tran.Commit();
//                //  tran = sqlite_connect.BeginTransaction();
//                //  cnt = 0;
//                //}
//              }
//            }
//            tran.Commit();
//          }
//          catch (Exception ex)
//          {
//            Console.WriteLine(ex.Message);
//            tran.Rollback();
//          }
//        }

//        {
//          int cnt = 0;
//          var tran = sqlite_connect.BeginTransaction();
//          try
//          {
//            var txtid = 0;
//            foreach (var jb in jnet.Properties())
//            {
//              int bookid = int.Parse(jb.Name); // 1-based

//              //var engs = bookid.ToString();
//              var engs = (string)jbookid2nas[bookid.ToString()][0];

//              var jtxt = jb.Value["fts"] as JObject;
//              foreach (var jv in jtxt.Properties())
//              {
//                //var str1s = jv.Name.Split(':');
//                //var ichap = int.Parse(str1s[0]);
//                //var isec = int.Parse(str1s[1]);

//                var cmd = sqlite_connect.CreateCommand(); //create command
//                                                          // id, engs, chap, sec, txt, username, xxx, xxx
//                cmd.CommandText =
//                                  @"INSERT INTO recnet VALUES ('" + engs.Replace("'", "''") +
//                                  @"'," + int.Parse(jv.Name) +
//                                  @",'" + ((string)jv.Value).Replace("'", "''") +
//                                  "');";
//                cmd.ExecuteNonQuery();

//                //if ( cnt++ > 3646)
//                //{
//                //  tran.Commit();
//                //  tran = sqlite_connect.BeginTransaction();
//                //  cnt = 0;
//                //}
//              }
//            }
//            tran.Commit();
//          }
//          catch (Exception ex)
//          {
//            Console.WriteLine(ex.Message);
//            tran.Rollback();
//          }
//        }


//      }// sqlite
//    }
    void asdfs()
    {
      
    }
  }

}

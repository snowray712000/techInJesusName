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
namespace cnetParse
{
  public partial class Form1 : Form
  {
    private string srd;

    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      srd = AppDomain.CurrentDomain.BaseDirectory;
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

        File.WriteAllText(ibook + "-step1.txt", string.Join("\r\n", elemPs) );
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
namespace techWinForm
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void button01_serverStart_Click(object sender, EventArgs e)
    {
      var server1 = new httpserver(9599);
      server1.startServer();
      button01_serverStart.Enabled = false;
    }
    /// <summary>
    /// 先開發這個, 因為 ajax 是發送 http 1.1 的協定
    /// </summary>
    class httpserver : tcpserver
    {
      public httpserver(int port = 8000,string ip = "127.0.0.1")
        : base(ip, port) { }

      protected override void whenServerAcceptOneClient(IAsyncResult ar)
      {
        Func<Exception, string> deepex = ex => {
          List<string> ers = new List<string>();
          Exception ex2 = ex;
          while (ex2 != null)
          {
            ers.Add(ex2.Message);
            ex2 = ex2.InnerException;
          }
          return string.Join("\r\n", ers.ToArray());
        };
        try
        {
          var client = _listener.EndAcceptTcpClient(ar);

          Action<httpserverreader> when_done = null;
          when_done = a1 => {

            var url = (string)a1.m_jheader["url"];
            if (url.EndsWith(".mp4"))
            {
              Console.WriteLine(a1.m_jheader.ToString());

              long bys_start = 0;
              var reg_range = new Regex(@"bytes=(?<i1>[0-9]+)-");
              if (a1.m_jheader["range"] != null)
              {
                var rangeask = (string)a1.m_jheader["range"];
                var re1 = reg_range.Match(rangeask);
                var idx1 = int.Parse(re1.Groups["i1"].Value);
                bys_start = idx1;
              }

              var path = AppDomain.CurrentDomain.BaseDirectory + "test1.mp4";
              var info = new FileInfo(path);
              long byslength = info.Length;
              //var bys = File.ReadAllBytes(path);
              long translen = 1024 * 400;
              if (bys_start + translen - 1 >= byslength - 1)
                translen = byslength - bys_start;
              long bys_end = bys_start + translen - 1;
              
              //var cmd = @"HTTP/1.1 200 OK
              var cmd = @"HTTP/1.1 206 Partial Content
Accept-Ranges: bytes
Content-Range: bytes " + bys_start + "-" + (bys_start + translen - 1) + @"/" + byslength + @"
Content-Length: " + translen + @"
connection: keep-alive
access-control-allow-origin: *
Content-Type: video/mp4

";
              var bys = new byte[translen];
              using (var stm = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
              {
                stm.Seek(bys_start, SeekOrigin.Begin);
                stm.Read(bys, 0, (int)translen);
              }
              

              var byssend = Encoding.UTF8.GetBytes(cmd).Concat(bys).ToArray();
              a1.m_networkStream.Write(byssend, 0, byssend.Length);

              new httpserverreader(a1.m_client, when_done, aa1 => {
                try { using (a1.m_networkStream) { } } catch { }
                try { using (a1.m_client) { } } catch { }
                throw aa1;
              });
              return;
            }
            else
            {
              Console.WriteLine(a1.m_jheader.ToString());
              /*     http/1.1 200 ok
               *     content-length: 53
               *     content-type: text/plain
               *     
               *     hello world
               */
              var contentstr = a1.m_jheader.ToString() + Encoding.UTF8.GetString(a1.m_byscontent.ToArray());

              var bys2 = Encoding.UTF8.GetBytes(contentstr);
              var cmd = @"HTTP/1.1 200 OK
  content-type:text/plain; charset=UTF-8
  connection: close
  content-length:" + bys2.Length + @"
  access-control-allow-origin: *

  ";
              var byssend = Encoding.UTF8.GetBytes(cmd).Concat(bys2).ToArray();

              a1.m_networkStream.Write(byssend, 0, byssend.Length);
              try { using (a1.m_networkStream) { } } catch { }
              try { using (a1.m_client) { } } catch { }
              return;
              // 結束
            }
          };
          var reader = new httpserverreader(client,when_done,ex => MessageBox.Show(deepex(ex)));

        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
        // listen next 
        try
        {
          _listener.BeginAcceptTcpClient(whenServerAcceptOneClient, null);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }

      /// <summary>
      /// 開發給 httpserver 用的 
      /// </summary>
      class httpserverreader
      {
        /// <summary> http1.1 的標頭協定(全部已經是小寫了), 數字也是文字方式存著 </summary>
        public JObject m_jheader { get { return jheader; } }
        /// <summary> post 的 data 或是 get 之後 ? 之後的字串轉為byte[]. (也就是post與get通用) </summary>
        public List<byte> m_byscontent { get { return byscontent; } }
        /// <summary> client.getStream() 出來的, 回傳給結果的時候可以用 </summary>
        public NetworkStream m_networkStream { get { return net; } }
        /// <summary> dispose 關閉時可以用 </summary>
        public TcpClient m_client { get { return client; } }
        private NetworkStream net;
        /// <summary> 不要呼叫這個, 呼叫 when_ex(), 會用 try-catch 包起來</summary>
        private Action<Exception> when_exception;
        private byte[] buffer = new byte[520];
        private int retrycnt = 0;
        private List<byte> bysread;//若是post,當\r\n\r\n到的時候,會把後面的資料移到byscontent
        List<byte> byscontent;//post才會有
        private TcpClient client;
        private JObject jheader=null;
        /// <summary> 不要呼叫這個, 呼叫 when_do(), 會用 try-catch 包起來</summary>
        private Action<httpserverreader> when_done;
        /// <summary> get xxxxxxx http/1.1 用的 , 建構子初始1次</summary>
        static private Regex reg;

        void when_do()
        {
          try
          {
            this.when_done(this);
          }
          catch (Exception e)
          {
            when_ex(e);
          }
        }
        void when_ex(Exception e)
        {
          // 作為一個 server 就是不要跳出 exception, 不然也太慘, 有時候使用者的 ex 處理反而造成 ex
          try
          {
            when_exception(e);
          }
          catch { }
        }
        void safe_do(Action act, string msg = "")
        {
          try
          {
            act();
          }
          catch (Exception ex)
          {
            this.when_ex(msg == null || msg.Length == 0 ? ex : new Exception(msg, ex));
          }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="when_done"></param>
        /// <param name="when_ex">除了流程有誤會丟出exception到這, 使用者when done中產生exception也會丟到這</param>
        public httpserverreader(TcpClient client,Action<httpserverreader> when_done,Action<Exception> when_ex)
        {
          this.when_exception = when_ex;
          this.client = client;
          this.when_done = when_done;
          if ( reg== null )
            reg = new System.Text.RegularExpressions.Regex(@"(?<method>(get)|(post)) (?<url>\S+) http/(?<ver>[0-9.]+)", RegexOptions.Compiled);

          safe_do(() => {
            bysread = new List<byte>();
            net = client.GetStream();
            net.BeginRead(buffer, 0, buffer.Length, when_read_4096, net);
          }, "constructor error");

        }

        private void when_read_4096(IAsyncResult ar)
        {
          Action readasync = () => net.BeginRead(buffer, 0, buffer.Length, when_read_4096, net);

          safe_do(() => {
            var cnt = net.EndRead(ar);

            if (cnt != 0)
              retrycnt = 0;
            else
              retrycnt++;
            if (retrycnt == 7)
              throw new Exception("retry read 7 cnts");

            if (cnt != 0)
            {

              if ( jheader != null )
              {
                /// @verbatim 已經讀到 \r\n\r\n 但是 content-length 長度還不夠 byscontent, 所以繼續嘗試讀 @endverbatim
                /// 
                safe_do(() => {
                  byscontent.AddRange(cnt == buffer.Length ? buffer : buffer.Take(cnt));

                  if (jheader["content-length"] == null)
                    throw new Exception("assert content-length exist");
                  var contentlength = (int)jheader["content-length"];
                  if (contentlength == 0 || byscontent.Count >= contentlength)
                    when_do();// 完成了讀取
                  else
                    readasync();//再嘗試取得
                },"try get post content");
              }
              else
              {
                /// @verbatim 還沒有讀到 \r\n\r\n @endverbatim
                /// 

                safe_do(() => {

                  bysread.AddRange(cnt == buffer.Length ? buffer : buffer.Take(cnt));

                  var str1 = Encoding.UTF8.GetString(bysread.ToArray());
                  var idx1 = str1.IndexOf("\r\n\r\n");
                  if (idx1 != -1)
                  {
                    var str2 = str1.Substring(0, idx1 + 4); //header
                    str2 = str2.ToLower();
                    var str3s = str2.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (str3s.Length < 1)
                      throw new Exception("request header error");

                    jheader = new JObject();
                    str3s.Skip(1).ToList().ForEach(a1 => {
                      if (a1.Trim().Length == 0)
                        return;

                      var tmp1 = a1.Split(new char[] { ':', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                      if (tmp1.Length == 2)
                        jheader[tmp1[0].Trim()] = tmp1[1].Trim();
                      else if (tmp1.Length == 0)
                        jheader[tmp1[0].Trim()] = "";
                      else
                        jheader[tmp1[0].Trim()] = string.Join(":", tmp1.Skip(1));
                    });//header 1

                    
                    //var reg = new System.Text.RegularExpressions.Regex(@"(?<method>(get)|(post)) (?<url>\S+) http/(?<ver>[0-9.]+)");
                    if ( reg.IsMatch(str3s[0])==false)
                      throw new NotImplementedException("method not implement" + str3s[0]);

                    var reg1 = reg.Match(str3s[0]);
                    jheader["method"] = reg1.Groups["method"].Value;
                    jheader["url"] = reg1.Groups["url"].Value;
                    jheader["ver"] = reg1.Groups["ver"].Value;

                    if ((string)jheader["method"] == "post")
                    {
                      if (jheader["content-length"] != null)
                      {
                        byscontent = new List<byte>();
                        var bysheader = Encoding.UTF8.GetBytes(str2); //包含\r\n\rn之前的bys
                        byscontent.AddRange(bysread.Skip(bysheader.Count()));
                        bysread.RemoveRange(bysheader.Length, bysread.Count - bysheader.Length);

                        var contentlength = (int)jheader["content-length"];
                        if (contentlength == 0 || byscontent.Count >= contentlength)
                          when_do();// 完成了讀取
                        else
                          readasync();//再嘗試取得
                      }
                      else
                      {
                        /// @verbatim 雖然是post 卻沒有傳資料過來 @endverbatim
                        when_do(); //完成了
                      }
                    }
                    else if ((string)jheader["method"] == "get")
                    {
                      var url = (string)jheader["url"];
                      var idx2 = url.IndexOf('?');
                      if ( idx2 != -1 )
                      {
                        var par = url.Substring(idx2 + 1);
                        var bys = Encoding.UTF8.GetBytes(par);
                        byscontent = new List<byte>();
                        byscontent.AddRange(bys);
                        jheader["url"] = (string)url.Substring(0, idx2);
                      }
                      when_do();
                    }
                  }
                  else
                    readasync();
                }, "get header");
              }
            }
            else
              readasync();//重試, 連7次read==0, 放棄,
          }, "occurs async read");
        }
      }


    }

    /// <summary>
    /// 還無法實用
    /// </summary>
    class tcpserver
    {
      protected TcpListener _listener;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ip"> 127.0.0.1 </param>
      /// <param name="port"> 8001</param>
      public tcpserver(string ip = "127.0.0.1", int port = 8000)
      {
        var ip2 = IPAddress.Parse(ip);
        _listener = new TcpListener(ip2, port);
      }
      /// <summary>
      /// callback 設好, 就呼叫此 startServer 
      /// </summary>
      public void startServer()
      {
        _listener.Start();

        _listener.BeginAcceptTcpClient(whenServerAcceptOneClient, null);
      }

      virtual protected void whenServerAcceptOneClient(IAsyncResult ar)
      {
        try
        {
          var client = _listener.EndAcceptTcpClient(ar);

        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
        // listen next 
        try
        {
          _listener.BeginAcceptTcpClient(whenServerAcceptOneClient, null);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }

    private void button02_connect_Click(object sender, EventArgs e)
    {
      JObject jarg = new JObject();
      jarg["hd"] = new JObject();
      jarg["da"] = new JObject();
      jarg["da"]["fana"] = "CCM";
      jarg["da"]["t"] = DateTime.UtcNow;

      var argstr = JsonConvert.SerializeObject(jarg);
      var argbys = Encoding.UTF8.GetBytes(argstr);
      Task.Factory.StartNew(() => {

        var req = (HttpWebRequest)HttpWebRequest.CreateDefault(new Uri("http://127.0.0.1:8000/nc/data"));
        req.Method = "POST";
        req.ContentLength = argbys.Length;
        var stm1 = req.GetRequestStream();
        stm1.Write(argbys, 0, argbys.Length);
        var resp = req.GetResponse();

        string ret2 = "";
        if ( resp.ContentLength!= 0)
        {
          var bys2 = new byte[resp.ContentLength];
          var stm2 = resp.GetResponseStream();
          stm2.Read(bys2, 0, bys2.Length);
          ret2 =Encoding.UTF8.GetString(bys2);
        }



      });
    }
  }
}

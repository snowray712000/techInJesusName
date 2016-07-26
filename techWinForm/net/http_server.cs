using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
/// <summary> 使用 http_server, 
/// 1. 讓單機可以寫 js 程式, 一個簡易的 http server  2016.07
/// 2. 可以用 c# 寫一些 127.0.0.1 的 功能, 輔助在信望愛網站程式用 (例如-文字語音唸出, 例如buffer) 2016.07
/// </summary>
namespace IJN.net
{
  /// <summary>
  /// 先開發這個, 因為 ajax 是發送 http 1.1 的協定.
  /// 繼承這個的 class, 過載 whenServerAcceptOneClientHttp
  /// </summary>
  public class http_server : tcpserver
  {
    private Regex reg_range=null;
    #region http response state
    static void response_200_OK(NetworkStream stream, string content, string content_type = "text/plain")
    {
      var bys2 = Encoding.UTF8.GetBytes(content);
      response_200_OK(stream, bys2, content_type);
    }
    static void response_200_OK(NetworkStream stream, byte[] bys, string content_type = "text/plain")
    {
      var header = @"HTTP/1.1 200 OK
Content-Type:" + content_type + @"; charset=UTF-8
Content-Length:" + bys.Length + @"
Connection: Close
Access-Control-Allow-Origin: *

";
      var bys3 = Encoding.UTF8.GetBytes(header).Concat(bys).ToArray();
      stream.Write(bys3, 0, bys3.Length);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="h_range">(string)m_jheader["range"] </param>
    /// <param name="path_inserver">server的絕對路徑</param>
    /// <param name="content_type">video/mp4</param>
    static void response_206_Partial_Content(NetworkStream stream,string h_range,string path_inserver, string content_type = "application/octet-stream")
    {
      long bys_start = 0;
      var reg_range = new Regex(@"bytes=(?<i1>[0-9]+)-");
      if (h_range != null)
      {
        var re1 = reg_range.Match(h_range);
        var idx1 = int.Parse(re1.Groups["i1"].Value);
        bys_start = idx1;
      }

      var info = new FileInfo(path_inserver);
      long byslength = info.Length;
      //var bys = File.ReadAllBytes(path);
      long translen = 1024 * 400;//html5 vedio 一次最小的size.
      if (bys_start + translen - 1 >= byslength - 1)
        translen = byslength - bys_start;
      long bys_end = bys_start + translen - 1;

      //var cmd = @"HTTP/1.1 200 OK
      var cmd = @"HTTP/1.1 206 Partial Content
Accept-Ranges: bytes
Content-Range: bytes " + bys_start + "-" + (bys_start + translen - 1) + @"/" + byslength + @"
Content-Length: " + translen + @"
Content-Type: "+ content_type +@"
connection: close
access-control-allow-origin: *

";
      var bys = new byte[translen];
      using (var stm = File.Open(path_inserver, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        stm.Seek(bys_start, SeekOrigin.Begin);
        stm.Read(bys, 0, (int)translen);
      }


      var byssend = Encoding.UTF8.GetBytes(cmd).Concat(bys).ToArray();
      stream.Write(byssend, 0, byssend.Length);      
    }
    #endregion

    public http_server(int port = 8000, string ip = "127.0.0.1")
      : base(port,ip) { }

    protected override void whenServerAcceptOneClient(TcpClient client)
    {
      // 呼叫 whenServerAcceptOneClient 的有用 try-catch 包起來

      Action<http_server_read_tool> when_done = null;
      when_done = a1 => {
        Action exit_net = () => {
          try { using (a1.m_networkStream) { } } catch { }
          try { using (a1.m_client) { } } catch { }
        };

        var url = (string)a1.m_jheader["url"];
        if ( url.StartsWith("/web1/"))
        {
          var ext =Path.GetExtension(url);
          var contenttype = "text/plain"; //default
          var reghtml = new Regex(@"\.htm");
          if (reghtml.IsMatch(ext))
            contenttype = "text/html";
          else if (ext == ".js")
            contenttype = "application/javascript";
          else if (ext == ".css")
            contenttype = "text/css";

          var srd = AppDomain.CurrentDomain.BaseDirectory;
          var bys = File.ReadAllBytes(srd + url.Substring(1));
          response_200_OK(a1.m_networkStream, bys,contenttype);
          exit_net(); return;
        }
        else if (url.EndsWith(".mp4"))
        {
          //Console.WriteLine(a1.m_jheader.ToString());

          long bys_start = 0;
          if ( reg_range == null )
            reg_range = new Regex(@"bytes=(?<i1>[0-9]+)-", RegexOptions.Compiled);

          if (a1.m_jheader["range"] != null)
          {
            var rangeask = (string)a1.m_jheader["range"];
            var re1 = reg_range.Match(rangeask);
            var idx1 = int.Parse(re1.Groups["i1"].Value);
            bys_start = idx1;
          }

          var path = AppDomain.CurrentDomain.BaseDirectory + "test1.mp4";
          response_206_Partial_Content(a1.m_networkStream, (string)a1.m_jheader["range"], path, "vedio/mp4");

          // 還不開放 keep-alive
          //new httpserverreader(a1.m_client, when_done, aa1 => {
          //  exit_net();
          //  throw aa1;
          //});
          exit_net(); return;
        }
        else
        {
          response_200_OK(a1.m_networkStream, "Hello World Server Response");
          exit_net(); return;
          // 結束
        }

      };
      new http_server_read_tool(client, when_done, ex => this.trigger_exceptionoccured(ex));
      return;
    }





    /// <summary> 建構子就開始 async 讀取. 開發給 http_server 用的, 會得到標頭. 因為從 client 端讀取資料, 有點複雜(要對http協定有基本了解)  </summary>
    protected class http_server_read_tool
    {
      #region 供 http_server class callback 用的 (public)
      /// <summary> http1.1 的標頭協定(全部已經是小寫了), 數字也是文字方式存著 </summary>
      public JObject m_jheader { get { return jheader; } }
      /// <summary> post 的 data 或是 get 之後 ? 之後的字串轉為byte[]. (也就是post與get通用) </summary>
      public List<byte> m_byscontent { get { return byscontent; } }
      /// <summary> client.getStream() 出來的, 回傳給結果的時候可以用 </summary>
      public NetworkStream m_networkStream { get { return net; } }
      /// <summary> dispose 關閉時可以用 </summary>
      public TcpClient m_client { get { return client; } }
      #endregion

      private NetworkStream net;
      /// <summary> 不要呼叫這個, 呼叫 when_ex(), 會用 try-catch 包起來</summary>
      private Action<Exception> when_exception;
      private byte[] buffer = new byte[520];
      private int retrycnt = 0;
      private List<byte> bysread;//若是post,當\r\n\r\n到的時候,會把後面的資料移到byscontent
      List<byte> byscontent;//post才會有
      private TcpClient client;
      private JObject jheader = null;
      /// <summary> 不要呼叫這個, 呼叫 when_do(), 會用 try-catch 包起來</summary>
      private Action<http_server_read_tool> when_done;
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
      /// <summary> 此處不會呼叫 try { using (a1.m_client) { } } catch { }，因為可能不是連續錯，可能是when_done錯誤 </summary>
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
      /// <summary>建構子就會開始 async 讀取. 讀取完不會釋放 client.( 那是callback自己要作的事) </summary>
      /// <param name="client"></param>
      /// <param name="when_done">要使用 m_client 等資料 </param>
      /// <param name="when_ex">除了流程有誤會丟出exception到這, 使用者when done中產生exception也會丟到這, 不會自動釋放 client唷. </param>
      public http_server_read_tool(TcpClient client, Action<http_server_read_tool> when_done, Action<Exception> when_ex)
      {
        this.when_exception = when_ex;
        this.client = client;
        this.when_done = when_done;
        if (reg == null)
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

            if (jheader != null)
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
              }, "try get post content");
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
                  if (reg.IsMatch(str3s[0]) == false)
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
                    if (idx2 != -1)
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
  public class TcpSeverExceptionArgs : ErrorEventArgs
  {
    public string getDeepString()
    {
      StringBuilder str = new StringBuilder();
      Exception ex = base.GetException();
      while ( ex!=null)
      {
        str.Append(ex.Message + "<-");
        ex = ex.InnerException;
      }

      return str.ToString();
    }
    public TcpSeverExceptionArgs(Exception exception) : base(exception)
    {
    }
  }
  /// <summary>
  /// 2016.07
  /// 還無法實用, http_server 父類別.
  /// 抽離出來, 以後要作 ftp server 或其它 server 可以繼承次
  /// </summary>
  abstract public class tcpserver
  {
    protected TcpListener _listener;

    public tcpserver(int port, string ip = "127.0.0.1")
    {
      var ip2 = IPAddress.Parse(ip);
      _listener = new TcpListener(ip2, port);
    }
    /// <summary> 設定好(例如callback或事件) 才 startServer() </summary>
    public void startServer()
    {
      _listener.Start();

      _listener.BeginAcceptTcpClient(whenServerAcceptOneClient2, null);
    }
    /// <summary> 為了繼承的class,不用再作一樣的事(try-catch或是Next Listen) </summary>
    void whenServerAcceptOneClient2(IAsyncResult ar) {
      try
      {
        var client = _listener.EndAcceptTcpClient(ar);
        whenServerAcceptOneClient(client);
      }
      catch (Exception ex)
      {
        trigger_exceptionoccured(ex);
      }
      // listen next 
      try
      {
        _listener.BeginAcceptTcpClient(whenServerAcceptOneClient2, null);
      }
      catch (Exception ex)
      {
        trigger_exceptionoccured(ex);
      }
    }

    /// <summary> 父類別從 async.end() 得到 client, 呼叫此函式 (而且有包try-catch), 也會自動再次進入 listen, 子類別只要專心處理 client 的事件. (請善用 trigger_exceptionoccured)</summary>
    abstract protected void whenServerAcceptOneClient(System.Net.Sockets.TcpClient client);    
    

    #region event 相關 
    public EventHandler<TcpSeverExceptionArgs> OnExceptionOccured;
    protected void trigger_exceptionoccured(Exception ex)
    {
      try
      {
        var ex2  = new TcpSeverExceptionArgs(ex);
        if (OnExceptionOccured != null)
          OnExceptionOccured(this, ex2);
        else
          Console.WriteLine(ex2.getDeepString());
      }
      catch { }
    }
    #endregion
  }

}

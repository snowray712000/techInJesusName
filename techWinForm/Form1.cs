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
      var server1 = new httpserver();
      server1.startServer();
    }
    /// <summary>
    /// 先開發這個, 因為 ajax 是發送 http 1.1 的協定
    /// </summary>
    class httpserver : tcpserver
    {
      public httpserver(string ip = "127.0.0.1", int port = 8000)
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
          var reader = new httpserverreader(client, ex => MessageBox.Show(deepex(ex)));

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
        private NetworkStream net;
        private Action<Exception> when_exception;
        private byte[] buffer = new byte[4096];
        private int retrycnt = 0;
        private List<byte> bysread;
        private TcpClient client;

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

        public httpserverreader(TcpClient client, Action<Exception> when_ex)
        {
          this.when_exception = when_ex;
          this.client = client;

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
              safe_do(() => {

                bysread.AddRange(cnt == buffer.Length ? buffer : buffer.Take(cnt));

                var str1 = Encoding.UTF8.GetString(bysread.ToArray());
                var idx1 = str1.IndexOf("\r\n\r\n");
                if (idx1 != -1)
                {
                  var str2 = str1.Substring(0, idx1 + 4); //header
                  Console.WriteLine(str2);
                }
                else
                  readasync();
              }, "get header");
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
      }
    }

  }
}

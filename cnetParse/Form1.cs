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

      var txt = "<div>wfwefew</div>";
//  var reg = new Regex(@"<([\S]+)>([\S\s]*)</\1>)");
      //reg.Match(txt);

    }

    private void button1_Click(object sender, EventArgs e)
    {
      var files = Directory.EnumerateFiles(srd, "*.docx", SearchOption.AllDirectories);

      // 一本書
      var file0 = files.First();
      using (var doc = DocX.Load(file0))
      {
        File.WriteAllText("xmldemo.txt", doc.Xml.ToString());
      }
    }
  }
}

namespace techWinForm
{
  partial class Form1
  {
    /// <summary>
    /// 設計工具所需的變數。
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 清除任何使用中的資源。
    /// </summary>
    /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form 設計工具產生的程式碼

    /// <summary>
    /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
    /// 這個方法的內容。
    /// </summary>
    private void InitializeComponent()
    {
      this.button01_serverStart = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button01_serverStart
      // 
      this.button01_serverStart.Location = new System.Drawing.Point(26, 25);
      this.button01_serverStart.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.button01_serverStart.Name = "button01_serverStart";
      this.button01_serverStart.Size = new System.Drawing.Size(210, 48);
      this.button01_serverStart.TabIndex = 0;
      this.button01_serverStart.Text = "01_serverStart";
      this.button01_serverStart.UseVisualStyleBackColor = true;
      this.button01_serverStart.Click += new System.EventHandler(this.button01_serverStart_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(537, 395);
      this.Controls.Add(this.button01_serverStart);
      this.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.Name = "Form1";
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button01_serverStart;
  }
}


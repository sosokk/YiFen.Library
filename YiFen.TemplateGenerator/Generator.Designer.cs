namespace YiFen.TemplateGenerator
{
    partial class Generator
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.EntityButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ConnectionText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.EntityNameSpaceText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.PathText = new System.Windows.Forms.TextBox();
            this.ServiceButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.DataTypeComboBox = new System.Windows.Forms.ComboBox();
            this.ServiceNameSpaceText = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // EntityButton
            // 
            this.EntityButton.Location = new System.Drawing.Point(369, 109);
            this.EntityButton.Name = "EntityButton";
            this.EntityButton.Size = new System.Drawing.Size(87, 23);
            this.EntityButton.TabIndex = 0;
            this.EntityButton.Text = "生成实体层";
            this.EntityButton.UseVisualStyleBackColor = true;
            this.EntityButton.Click += new System.EventHandler(this.EntityButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "数据库连接字符串";
            // 
            // ConnectionText
            // 
            this.ConnectionText.Location = new System.Drawing.Point(119, 20);
            this.ConnectionText.Name = "ConnectionText";
            this.ConnectionText.Size = new System.Drawing.Size(514, 21);
            this.ConnectionText.TabIndex = 2;
            this.ConnectionText.Text = "Server=122.11.53.27;UID=sa;PWD=bD567%^&;DataBase=mengjie;Connect Timeout=90";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "实体层命名空间";
            // 
            // EntityNameSpaceText
            // 
            this.EntityNameSpaceText.Location = new System.Drawing.Point(119, 111);
            this.EntityNameSpaceText.Name = "EntityNameSpaceText";
            this.EntityNameSpaceText.Size = new System.Drawing.Size(232, 21);
            this.EntityNameSpaceText.TabIndex = 4;
            this.EntityNameSpaceText.Text = "BlueEyes.Weibo.Entity";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(298, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "生成路径";
            // 
            // PathText
            // 
            this.PathText.Location = new System.Drawing.Point(357, 54);
            this.PathText.Name = "PathText";
            this.PathText.Size = new System.Drawing.Size(276, 21);
            this.PathText.TabIndex = 4;
            // 
            // ServiceButton
            // 
            this.ServiceButton.Location = new System.Drawing.Point(369, 175);
            this.ServiceButton.Name = "ServiceButton";
            this.ServiceButton.Size = new System.Drawing.Size(87, 23);
            this.ServiceButton.TabIndex = 0;
            this.ServiceButton.Text = "生成业务层";
            this.ServiceButton.UseVisualStyleBackColor = true;
            this.ServiceButton.Click += new System.EventHandler(this.ServiceButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(48, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "数据库类型";
            // 
            // DataTypeComboBox
            // 
            this.DataTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataTypeComboBox.FormattingEnabled = true;
            this.DataTypeComboBox.Items.AddRange(new object[] {
            "SQLServer",
            "MySQL"});
            this.DataTypeComboBox.Location = new System.Drawing.Point(119, 55);
            this.DataTypeComboBox.Name = "DataTypeComboBox";
            this.DataTypeComboBox.Size = new System.Drawing.Size(164, 20);
            this.DataTypeComboBox.TabIndex = 6;
            this.DataTypeComboBox.Tag = "";
            // 
            // ServiceNameSpaceText
            // 
            this.ServiceNameSpaceText.Location = new System.Drawing.Point(119, 177);
            this.ServiceNameSpaceText.Name = "ServiceNameSpaceText";
            this.ServiceNameSpaceText.Size = new System.Drawing.Size(232, 21);
            this.ServiceNameSpaceText.TabIndex = 8;
            this.ServiceNameSpaceText.Text = "BlueEyes.Weibo.Service";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 180);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "业务层命名空间";
            // 
            // Generator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 302);
            this.Controls.Add(this.ServiceNameSpaceText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.DataTypeComboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.PathText);
            this.Controls.Add(this.EntityNameSpaceText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ConnectionText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ServiceButton);
            this.Controls.Add(this.EntityButton);
            this.Name = "Generator";
            this.Text = "Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button EntityButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ConnectionText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox EntityNameSpaceText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox PathText;
        private System.Windows.Forms.Button ServiceButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox DataTypeComboBox;
        private System.Windows.Forms.TextBox ServiceNameSpaceText;
        private System.Windows.Forms.Label label5;
    }
}


namespace JmdLoader
{
    partial class AboutMe
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            button1 = new Button();
            label2 = new Label();
            version = new Label();
            checkUpdateStatus = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Candara", 27.75F, FontStyle.Bold);
            label1.Location = new Point(14, 10);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(204, 45);
            label1.TabIndex = 0;
            label1.Text = "Jmd Loader";
            // 
            // button1
            // 
            button1.BackColor = SystemColors.Control;
            button1.Font = new Font("Bahnschrift SemiCondensed", 12F);
            button1.Location = new Point(588, 22);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(131, 40);
            button1.TabIndex = 2;
            button1.Text = "Feedback";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Bahnschrift", 12F);
            label2.Location = new Point(13, 93);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(611, 57);
            label2.TabIndex = 3;
            label2.Text = "Jmd Loader is a tool which helps you to explore the files included in Jmd Type File. \r\nAuthor: eP Studio\r\nLicense: GPL v3";
            // 
            // version
            // 
            version.AutoSize = true;
            version.Font = new Font("Bahnschrift", 12F);
            version.Location = new Point(255, 27);
            version.Margin = new Padding(4, 0, 4, 0);
            version.Name = "version";
            version.Size = new Size(93, 19);
            version.TabIndex = 4;
            version.Text = "Version : 1.0";
            // 
            // checkUpdateStatus
            // 
            checkUpdateStatus.AutoSize = true;
            checkUpdateStatus.Cursor = Cursors.Hand;
            checkUpdateStatus.Font = new Font("Segoe UI", 8.25F);
            checkUpdateStatus.ForeColor = Color.OrangeRed;
            checkUpdateStatus.Location = new Point(257, 54);
            checkUpdateStatus.Margin = new Padding(4, 0, 4, 0);
            checkUpdateStatus.Name = "checkUpdateStatus";
            checkUpdateStatus.Size = new Size(239, 13);
            checkUpdateStatus.TabIndex = 5;
            checkUpdateStatus.Text = "New Version Released, Clicking for updating.";
            checkUpdateStatus.Click += checkUpdateStatus_Click;
            // 
            // AboutMe
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(741, 200);
            Controls.Add(checkUpdateStatus);
            Controls.Add(version);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 3, 4, 3);
            Name = "AboutMe";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Label checkUpdateStatus;
    }
}
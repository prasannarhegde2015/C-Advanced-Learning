namespace RunRTUEmulator
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtControllerType = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnasync = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtControllerType
            // 
            this.txtControllerType.Location = new System.Drawing.Point(93, 27);
            this.txtControllerType.Name = "txtControllerType";
            this.txtControllerType.Size = new System.Drawing.Size(100, 20);
            this.txtControllerType.TabIndex = 0;
            this.txtControllerType.TextChanged += new System.EventHandler(this.txtControllerType_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "ControllerType";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(75, 118);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(397, 111);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Image = global::RunRTUEmulator.Properties.Resources.Custom_Icon_Design_Flatastic_9_Stop_red;
            this.btnStop.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnStop.Location = new System.Drawing.Point(384, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(99, 66);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "Stop RTUEmu";
            this.btnStop.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnasync
            // 
            this.btnasync.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnasync.Image = global::RunRTUEmulator.Properties.Resources.Custom_Icon_Design_Flatastic_9_Start;
            this.btnasync.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnasync.Location = new System.Drawing.Point(257, 12);
            this.btnasync.Name = "btnasync";
            this.btnasync.Size = new System.Drawing.Size(98, 66);
            this.btnasync.TabIndex = 4;
            this.btnasync.Text = "Run RTUEmu";
            this.btnasync.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnasync.UseVisualStyleBackColor = true;
            this.btnasync.Click += new System.EventHandler(this.btnasync_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(544, 241);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnasync);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtControllerType);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Run RTU EMU Controller ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtControllerType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnasync;
        private System.Windows.Forms.Button btnStop;
    }
}


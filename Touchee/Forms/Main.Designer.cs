namespace Touchee.Forms {
    partial class Main {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.niSystemTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.mnuSystemTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuiShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSystemTray.SuspendLayout();
            this.SuspendLayout();
            // 
            // niSystemTray
            // 
            this.niSystemTray.ContextMenuStrip = this.mnuSystemTray;
            this.niSystemTray.Icon = ((System.Drawing.Icon)(resources.GetObject("niSystemTray.Icon")));
            this.niSystemTray.Text = "Touchee";
            this.niSystemTray.Visible = true;
            this.niSystemTray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niSystemTray_MouseDoubleClick);
            // 
            // mnuSystemTray
            // 
            this.mnuSystemTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuiShow,
            this.toolStripMenuItem2,
            this.mnuiExit});
            this.mnuSystemTray.Name = "mnuSystemTray";
            this.mnuSystemTray.Size = new System.Drawing.Size(101, 54);
            // 
            // mnuiShow
            // 
            this.mnuiShow.Name = "mnuiShow";
            this.mnuiShow.Size = new System.Drawing.Size(100, 22);
            this.mnuiShow.Text = "Show";
            this.mnuiShow.Click += new System.EventHandler(this.mnuiShow_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(97, 6);
            // 
            // mnuiExit
            // 
            this.mnuiExit.Name = "mnuiExit";
            this.mnuiExit.Size = new System.Drawing.Size(100, 22);
            this.mnuiExit.Text = "Exit";
            this.mnuiExit.Click += new System.EventHandler(this.mnuiExit_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.ShowInTaskbar = false;
            this.Text = "Touchee";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.mnuSystemTray.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon niSystemTray;
        private System.Windows.Forms.ContextMenuStrip mnuSystemTray;
        private System.Windows.Forms.ToolStripMenuItem mnuiShow;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuiExit;
    }
}
using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Touchee.Forms {

    /// <remarks>
    /// The main form of the application.
    /// </remarks>
    public partial class Main : Form {

        bool _setVisibleCoreCalled = false;

        /// <summary>
        /// Inits the form.
        /// </summary>
        public Main() {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(main_FormClosing);
        }

        /// <summary>
        /// Start the shutdown of the program.
        /// </summary>
        private void mnuiExit_Click(object sender, EventArgs e) {
            Program.Shutdown();
        }

        /// <summary>
        /// Shows the main form and makes sure it is shown in the taskbar.
        /// </summary>
        private void mnuiShow_Click(object sender, EventArgs e) {
            Show();
            WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        /// <summary>
        /// When the form is closed, hides it instead of shutting down the application.
        /// </summary>
        private void main_FormClosing(object sender, FormClosingEventArgs e) {
            Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// Double click the sys tray icon: call is redirected to the Show item of the menu.
        /// </summary>
        private void niSystemTray_MouseDoubleClick(object sender, MouseEventArgs e) {
            mnuiShow_Click(sender, e);
        }

        /// <summary>
        /// Hide the main window
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(_setVisibleCoreCalled ? value : false);
            _setVisibleCoreCalled = true;
        } 

    }
}

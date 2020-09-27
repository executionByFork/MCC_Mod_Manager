using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MCC_Mod_Manager.Api;


namespace MCC_Mod_Manager {
    public partial class ModpackRenameForm : Form {
        string modpackName;
        
        public ModpackRenameForm(string packName) {
            InitializeComponent();

            this.modpackName = packName;
            txtRename.Text = packName;

            PictureBox closebtn = new PictureBox();
            closebtn.Image = closebtn.ErrorImage;    // bit of a hack to get the error image to appear
            closebtn.Width = 14;
            closebtn.Height = 16;
            closebtn.Location = new Point(248, 3);
            closebtn.Click += btnRenameClose_Click;
            this.pnlRename.Controls.Add(closebtn);

            LoadEventHandlers();
        }
 
        public void LoadEventHandlers() {
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Drag_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Drag_MouseMove);

            this.pnlRename.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Drag_MouseDown);
            this.pnlRename.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Drag_MouseMove);

            this.lblRename.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Drag_MouseDown);
            this.lblRename.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Drag_MouseMove);

        }

        private void btnRenameClose_Click(object sender, EventArgs e) {
            Close();
        }

        private void txtRename_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {  // if enter key pressed
                Path.Combine(Config.Modpack_dir, this.modpackName);
                File.Move(
                    Path.Combine(Config.Modpack_dir, this.modpackName + ".zip"),
                    Path.Combine(Config.Modpack_dir, txtRename.Text + ".zip")
                );
                MyMods.LoadModpacks();
                Close();
            }
        }


        #region DRAGGABLE

        Point lastPoint;
        private void Drag_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void Drag_MouseDown(object sender, MouseEventArgs e) {
            lastPoint = new Point(e.X, e.Y);
        }

        #endregion
    }
}

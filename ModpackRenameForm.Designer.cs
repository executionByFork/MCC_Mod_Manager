namespace MCC_Mod_Manager {
    partial class ModpackRenameForm {
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
            this.lblRename = new System.Windows.Forms.Label();
            this.txtRename = new System.Windows.Forms.TextBox();
            this.pnlRename = new System.Windows.Forms.Panel();
            this.pnlRename.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRename
            // 
            this.lblRename.AutoSize = true;
            this.lblRename.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblRename.Font = new System.Drawing.Font("Gadugi", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRename.Location = new System.Drawing.Point(9, 18);
            this.lblRename.Name = "lblRename";
            this.lblRename.Size = new System.Drawing.Size(71, 19);
            this.lblRename.TabIndex = 0;
            this.lblRename.Text = "Rename";
            // 
            // txtRename
            // 
            this.txtRename.Location = new System.Drawing.Point(9, 40);
            this.txtRename.Name = "txtRename";
            this.txtRename.Size = new System.Drawing.Size(244, 20);
            this.txtRename.TabIndex = 1;
            this.txtRename.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtRename_KeyDown);
            // 
            // pnlRename
            // 
            this.pnlRename.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.pnlRename.Controls.Add(this.txtRename);
            this.pnlRename.Controls.Add(this.lblRename);
            this.pnlRename.Location = new System.Drawing.Point(3, 3);
            this.pnlRename.Name = "pnlRename";
            this.pnlRename.Size = new System.Drawing.Size(265, 72);
            this.pnlRename.TabIndex = 3;
            // 
            // ModpackRenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.MenuText;
            this.ClientSize = new System.Drawing.Size(271, 78);
            this.Controls.Add(this.pnlRename);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ModpackRenameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MCC Mod Manager";
            this.pnlRename.ResumeLayout(false);
            this.pnlRename.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRename;
        private System.Windows.Forms.TextBox txtRename;
        private System.Windows.Forms.Panel pnlRename;
    }
}
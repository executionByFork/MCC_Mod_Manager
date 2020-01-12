namespace MCC_Mod_Manager
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
            this.topBar = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.PictureBox();
            this.minButton = new System.Windows.Forms.PictureBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.PictureBox();
            this.homeTab = new System.Windows.Forms.Button();
            this.createTab = new System.Windows.Forms.Button();
            this.homePanel = new System.Windows.Forms.Panel();
            this.delModpack = new System.Windows.Forms.Button();
            this.patchButton = new System.Windows.Forms.Button();
            this.homeNameLabel = new System.Windows.Forms.Label();
            this.homeEnableLabel = new System.Windows.Forms.Label();
            this.modListPanel = new System.Windows.Forms.Panel();
            this.configTab = new System.Windows.Forms.Button();
            this.createPanel = new System.Windows.Forms.Panel();
            this.modpackName_label = new System.Windows.Forms.Label();
            this.modpackName_txt = new System.Windows.Forms.TextBox();
            this.clearBtn = new System.Windows.Forms.Button();
            this.addRowButton = new System.Windows.Forms.PictureBox();
            this.createModpackBtn = new System.Windows.Forms.Button();
            this.createLabel2 = new System.Windows.Forms.Label();
            this.createLabel1 = new System.Windows.Forms.Label();
            this.createFilesPanel = new System.Windows.Forms.Panel();
            this.configPanel = new System.Windows.Forms.Panel();
            this.delOldBaks_chb = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cfgTextBox3 = new System.Windows.Forms.TextBox();
            this.cfgBrowseBtn3 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cfgTextBox2 = new System.Windows.Forms.TextBox();
            this.cfgBrowseBtn2 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cfgTextBox1 = new System.Windows.Forms.TextBox();
            this.cfgBrowseBtn1 = new System.Windows.Forms.Button();
            this.configLabel3 = new System.Windows.Forms.Label();
            this.configLabel2 = new System.Windows.Forms.Label();
            this.cfgUpdateBtn = new System.Windows.Forms.Button();
            this.configLabel1 = new System.Windows.Forms.Label();
            this.backupTab = new System.Windows.Forms.Button();
            this.backupPanel = new System.Windows.Forms.Panel();
            this.restoreSelectedBtn = new System.Windows.Forms.Button();
            this.delAllBaksBtn = new System.Windows.Forms.Button();
            this.makeBakBtn = new System.Windows.Forms.Button();
            this.restoreAllBaksBtn = new System.Windows.Forms.Button();
            this.delSelectedBak = new System.Windows.Forms.Button();
            this.bakLabel2 = new System.Windows.Forms.Label();
            this.bakLabel1 = new System.Windows.Forms.Label();
            this.bakListPanel = new System.Windows.Forms.Panel();
            this.betterPBar = new System.Windows.Forms.Panel();
            this.fullBakPath_chb = new System.Windows.Forms.CheckBox();
            this.topBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.refreshButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.exitButton)).BeginInit();
            this.homePanel.SuspendLayout();
            this.createPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.addRowButton)).BeginInit();
            this.configPanel.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.backupPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // topBar
            // 
            this.topBar.BackColor = System.Drawing.Color.DarkGray;
            this.topBar.Controls.Add(this.label1);
            this.topBar.Controls.Add(this.refreshButton);
            this.topBar.Controls.Add(this.minButton);
            this.topBar.Controls.Add(this.titleLabel);
            this.topBar.Controls.Add(this.exitButton);
            this.topBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.topBar.Location = new System.Drawing.Point(0, 0);
            this.topBar.Name = "topBar";
            this.topBar.Size = new System.Drawing.Size(582, 37);
            this.topBar.TabIndex = 1;
            this.topBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.topBar_MouseDown);
            this.topBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.topBar_MouseMove);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Font = new System.Drawing.Font("Gadugi", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(429, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 19);
            this.label1.TabIndex = 6;
            this.label1.Text = "v0.5";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.topBar_MouseDown);
            this.label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.topBar_MouseMove);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshButton.Image = ((System.Drawing.Image)(resources.GetObject("refreshButton.Image")));
            this.refreshButton.Location = new System.Drawing.Point(478, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(30, 30);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.TabStop = false;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            this.refreshButton.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.refreshButton.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // minButton
            // 
            this.minButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minButton.Image = ((System.Drawing.Image)(resources.GetObject("minButton.Image")));
            this.minButton.Location = new System.Drawing.Point(514, 3);
            this.minButton.Name = "minButton";
            this.minButton.Size = new System.Drawing.Size(30, 30);
            this.minButton.TabIndex = 4;
            this.minButton.TabStop = false;
            this.minButton.Click += new System.EventHandler(this.minButton_Click);
            this.minButton.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.minButton.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.titleLabel.Font = new System.Drawing.Font("Gadugi", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(16, 9);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(240, 19);
            this.titleLabel.TabIndex = 3;
            this.titleLabel.Text = "MCC Mod Manager by MrFRZ0";
            this.titleLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.topBar_MouseDown);
            this.titleLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.topBar_MouseMove);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
            this.exitButton.Location = new System.Drawing.Point(548, 3);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(30, 30);
            this.exitButton.TabIndex = 2;
            this.exitButton.TabStop = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            this.exitButton.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.exitButton.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // homeTab
            // 
            this.homeTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.homeTab.FlatAppearance.BorderSize = 0;
            this.homeTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.homeTab.Font = new System.Drawing.Font("Reem Kufi", 9.749999F, System.Drawing.FontStyle.Bold);
            this.homeTab.Location = new System.Drawing.Point(10, 43);
            this.homeTab.Name = "homeTab";
            this.homeTab.Size = new System.Drawing.Size(135, 37);
            this.homeTab.TabIndex = 2;
            this.homeTab.Text = "My Mods";
            this.homeTab.UseVisualStyleBackColor = false;
            this.homeTab.Click += new System.EventHandler(this.homeTab_Click);
            this.homeTab.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.homeTab.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // createTab
            // 
            this.createTab.BackColor = System.Drawing.Color.DarkGray;
            this.createTab.FlatAppearance.BorderSize = 0;
            this.createTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.createTab.Font = new System.Drawing.Font("Reem Kufi", 9.749999F, System.Drawing.FontStyle.Bold);
            this.createTab.Location = new System.Drawing.Point(151, 43);
            this.createTab.Name = "createTab";
            this.createTab.Size = new System.Drawing.Size(135, 37);
            this.createTab.TabIndex = 3;
            this.createTab.Text = "Create Modpack";
            this.createTab.UseVisualStyleBackColor = false;
            this.createTab.Click += new System.EventHandler(this.CreateTab_Click);
            this.createTab.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.createTab.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // homePanel
            // 
            this.homePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.homePanel.Controls.Add(this.delModpack);
            this.homePanel.Controls.Add(this.patchButton);
            this.homePanel.Controls.Add(this.homeNameLabel);
            this.homePanel.Controls.Add(this.homeEnableLabel);
            this.homePanel.Controls.Add(this.modListPanel);
            this.homePanel.Location = new System.Drawing.Point(5, 89);
            this.homePanel.Name = "homePanel";
            this.homePanel.Size = new System.Drawing.Size(563, 356);
            this.homePanel.TabIndex = 4;
            // 
            // delModpack
            // 
            this.delModpack.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.delModpack.ForeColor = System.Drawing.Color.Red;
            this.delModpack.Location = new System.Drawing.Point(434, 311);
            this.delModpack.Name = "delModpack";
            this.delModpack.Size = new System.Drawing.Size(124, 35);
            this.delModpack.TabIndex = 5;
            this.delModpack.Text = "Delete Selected";
            this.delModpack.UseVisualStyleBackColor = true;
            this.delModpack.Click += new System.EventHandler(this.delModpack_Click);
            // 
            // patchButton
            // 
            this.patchButton.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.patchButton.Location = new System.Drawing.Point(434, 270);
            this.patchButton.Name = "patchButton";
            this.patchButton.Size = new System.Drawing.Size(124, 35);
            this.patchButton.TabIndex = 3;
            this.patchButton.Text = "Patch Game";
            this.patchButton.UseVisualStyleBackColor = true;
            this.patchButton.Click += new System.EventHandler(this.patchButton_Click);
            this.patchButton.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.patchButton.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // homeNameLabel
            // 
            this.homeNameLabel.AutoSize = true;
            this.homeNameLabel.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.homeNameLabel.Location = new System.Drawing.Point(100, 7);
            this.homeNameLabel.Name = "homeNameLabel";
            this.homeNameLabel.Size = new System.Drawing.Size(96, 23);
            this.homeNameLabel.TabIndex = 2;
            this.homeNameLabel.Text = "Modpack Name";
            // 
            // homeEnableLabel
            // 
            this.homeEnableLabel.AutoSize = true;
            this.homeEnableLabel.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.homeEnableLabel.Location = new System.Drawing.Point(32, 7);
            this.homeEnableLabel.Name = "homeEnableLabel";
            this.homeEnableLabel.Size = new System.Drawing.Size(42, 23);
            this.homeEnableLabel.TabIndex = 1;
            this.homeEnableLabel.Text = "Select";
            // 
            // modListPanel
            // 
            this.modListPanel.AutoScroll = true;
            this.modListPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.modListPanel.Location = new System.Drawing.Point(14, 33);
            this.modListPanel.Name = "modListPanel";
            this.modListPanel.Size = new System.Drawing.Size(407, 322);
            this.modListPanel.TabIndex = 0;
            // 
            // configTab
            // 
            this.configTab.BackColor = System.Drawing.Color.DarkGray;
            this.configTab.FlatAppearance.BorderSize = 0;
            this.configTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.configTab.Font = new System.Drawing.Font("Reem Kufi", 9.749999F, System.Drawing.FontStyle.Bold);
            this.configTab.Location = new System.Drawing.Point(292, 43);
            this.configTab.Name = "configTab";
            this.configTab.Size = new System.Drawing.Size(135, 37);
            this.configTab.TabIndex = 5;
            this.configTab.Text = "Configuration";
            this.configTab.UseVisualStyleBackColor = false;
            this.configTab.Click += new System.EventHandler(this.configTab_Click);
            this.configTab.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.configTab.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // createPanel
            // 
            this.createPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.createPanel.Controls.Add(this.modpackName_label);
            this.createPanel.Controls.Add(this.modpackName_txt);
            this.createPanel.Controls.Add(this.clearBtn);
            this.createPanel.Controls.Add(this.addRowButton);
            this.createPanel.Controls.Add(this.createModpackBtn);
            this.createPanel.Controls.Add(this.createLabel2);
            this.createPanel.Controls.Add(this.createLabel1);
            this.createPanel.Controls.Add(this.createFilesPanel);
            this.createPanel.Location = new System.Drawing.Point(5, 89);
            this.createPanel.Name = "createPanel";
            this.createPanel.Size = new System.Drawing.Size(563, 356);
            this.createPanel.TabIndex = 5;
            this.createPanel.Visible = false;
            // 
            // modpackName_label
            // 
            this.modpackName_label.AutoSize = true;
            this.modpackName_label.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.modpackName_label.Location = new System.Drawing.Point(15, 295);
            this.modpackName_label.Name = "modpackName_label";
            this.modpackName_label.Size = new System.Drawing.Size(96, 23);
            this.modpackName_label.TabIndex = 9;
            this.modpackName_label.Text = "Modpack Name";
            // 
            // modpackName_txt
            // 
            this.modpackName_txt.Location = new System.Drawing.Point(18, 320);
            this.modpackName_txt.Name = "modpackName_txt";
            this.modpackName_txt.Size = new System.Drawing.Size(288, 20);
            this.modpackName_txt.TabIndex = 8;
            // 
            // clearBtn
            // 
            this.clearBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.clearBtn.Location = new System.Drawing.Point(438, 311);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(120, 35);
            this.clearBtn.TabIndex = 7;
            this.clearBtn.Text = "Clear All";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // addRowButton
            // 
            this.addRowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addRowButton.Image = ((System.Drawing.Image)(resources.GetObject("addRowButton.Image")));
            this.addRowButton.Location = new System.Drawing.Point(19, 7);
            this.addRowButton.Name = "addRowButton";
            this.addRowButton.Size = new System.Drawing.Size(25, 25);
            this.addRowButton.TabIndex = 6;
            this.addRowButton.TabStop = false;
            this.addRowButton.Click += new System.EventHandler(this.addRowButton_Click);
            this.addRowButton.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.addRowButton.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // createModpackBtn
            // 
            this.createModpackBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.createModpackBtn.Location = new System.Drawing.Point(312, 311);
            this.createModpackBtn.Name = "createModpackBtn";
            this.createModpackBtn.Size = new System.Drawing.Size(120, 35);
            this.createModpackBtn.TabIndex = 3;
            this.createModpackBtn.Text = "Create Modpack";
            this.createModpackBtn.UseVisualStyleBackColor = true;
            this.createModpackBtn.Click += new System.EventHandler(this.createModpackBtn_Click);
            // 
            // createLabel2
            // 
            this.createLabel2.AutoSize = true;
            this.createLabel2.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.createLabel2.Location = new System.Drawing.Point(360, 7);
            this.createLabel2.Name = "createLabel2";
            this.createLabel2.Size = new System.Drawing.Size(72, 23);
            this.createLabel2.TabIndex = 2;
            this.createLabel2.Text = "Destination";
            // 
            // createLabel1
            // 
            this.createLabel1.AutoSize = true;
            this.createLabel1.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.createLabel1.Location = new System.Drawing.Point(98, 7);
            this.createLabel1.Name = "createLabel1";
            this.createLabel1.Size = new System.Drawing.Size(76, 23);
            this.createLabel1.TabIndex = 1;
            this.createLabel1.Text = "Modded File";
            // 
            // createFilesPanel
            // 
            this.createFilesPanel.AutoScroll = true;
            this.createFilesPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.createFilesPanel.Location = new System.Drawing.Point(14, 33);
            this.createFilesPanel.Name = "createFilesPanel";
            this.createFilesPanel.Size = new System.Drawing.Size(536, 259);
            this.createFilesPanel.TabIndex = 0;
            // 
            // configPanel
            // 
            this.configPanel.Controls.Add(this.delOldBaks_chb);
            this.configPanel.Controls.Add(this.panel3);
            this.configPanel.Controls.Add(this.panel2);
            this.configPanel.Controls.Add(this.panel1);
            this.configPanel.Controls.Add(this.configLabel3);
            this.configPanel.Controls.Add(this.configLabel2);
            this.configPanel.Controls.Add(this.cfgUpdateBtn);
            this.configPanel.Controls.Add(this.configLabel1);
            this.configPanel.Location = new System.Drawing.Point(5, 89);
            this.configPanel.Name = "configPanel";
            this.configPanel.Size = new System.Drawing.Size(569, 356);
            this.configPanel.TabIndex = 6;
            this.configPanel.Visible = false;
            // 
            // delOldBaks_chb
            // 
            this.delOldBaks_chb.AutoSize = true;
            this.delOldBaks_chb.Location = new System.Drawing.Point(217, 82);
            this.delOldBaks_chb.Name = "delOldBaks_chb";
            this.delOldBaks_chb.Size = new System.Drawing.Size(243, 17);
            this.delOldBaks_chb.TabIndex = 15;
            this.delOldBaks_chb.Text = "Delete backups after restoring? (saves space)";
            this.delOldBaks_chb.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.cfgTextBox3);
            this.panel3.Controls.Add(this.cfgBrowseBtn3);
            this.panel3.Location = new System.Drawing.Point(33, 156);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(475, 31);
            this.panel3.TabIndex = 14;
            // 
            // cfgTextBox3
            // 
            this.cfgTextBox3.Location = new System.Drawing.Point(5, 3);
            this.cfgTextBox3.Name = "cfgTextBox3";
            this.cfgTextBox3.Size = new System.Drawing.Size(413, 20);
            this.cfgTextBox3.TabIndex = 11;
            // 
            // cfgBrowseBtn3
            // 
            this.cfgBrowseBtn3.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cfgBrowseBtn3.Location = new System.Drawing.Point(423, 3);
            this.cfgBrowseBtn3.Name = "cfgBrowseBtn3";
            this.cfgBrowseBtn3.Size = new System.Drawing.Size(39, 20);
            this.cfgBrowseBtn3.TabIndex = 12;
            this.cfgBrowseBtn3.Text = "...";
            this.cfgBrowseBtn3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cfgBrowseBtn3.UseVisualStyleBackColor = true;
            this.cfgBrowseBtn3.Click += new System.EventHandler(this.cfgFolderBrowseBtn_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cfgTextBox2);
            this.panel2.Controls.Add(this.cfgBrowseBtn2);
            this.panel2.Location = new System.Drawing.Point(33, 98);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(475, 31);
            this.panel2.TabIndex = 14;
            // 
            // cfgTextBox2
            // 
            this.cfgTextBox2.Location = new System.Drawing.Point(5, 3);
            this.cfgTextBox2.Name = "cfgTextBox2";
            this.cfgTextBox2.Size = new System.Drawing.Size(413, 20);
            this.cfgTextBox2.TabIndex = 8;
            // 
            // cfgBrowseBtn2
            // 
            this.cfgBrowseBtn2.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cfgBrowseBtn2.Location = new System.Drawing.Point(423, 3);
            this.cfgBrowseBtn2.Name = "cfgBrowseBtn2";
            this.cfgBrowseBtn2.Size = new System.Drawing.Size(39, 20);
            this.cfgBrowseBtn2.TabIndex = 9;
            this.cfgBrowseBtn2.Text = "...";
            this.cfgBrowseBtn2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cfgBrowseBtn2.UseVisualStyleBackColor = true;
            this.cfgBrowseBtn2.Click += new System.EventHandler(this.cfgFolderBrowseBtn_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cfgTextBox1);
            this.panel1.Controls.Add(this.cfgBrowseBtn1);
            this.panel1.Location = new System.Drawing.Point(33, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(475, 31);
            this.panel1.TabIndex = 13;
            // 
            // cfgTextBox1
            // 
            this.cfgTextBox1.Location = new System.Drawing.Point(5, 3);
            this.cfgTextBox1.Name = "cfgTextBox1";
            this.cfgTextBox1.Size = new System.Drawing.Size(412, 20);
            this.cfgTextBox1.TabIndex = 1;
            // 
            // cfgBrowseBtn1
            // 
            this.cfgBrowseBtn1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cfgBrowseBtn1.Location = new System.Drawing.Point(423, 3);
            this.cfgBrowseBtn1.Name = "cfgBrowseBtn1";
            this.cfgBrowseBtn1.Size = new System.Drawing.Size(39, 20);
            this.cfgBrowseBtn1.TabIndex = 4;
            this.cfgBrowseBtn1.Text = "...";
            this.cfgBrowseBtn1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cfgBrowseBtn1.UseVisualStyleBackColor = true;
            this.cfgBrowseBtn1.Click += new System.EventHandler(this.cfgFolderBrowseBtn_Click);
            // 
            // configLabel3
            // 
            this.configLabel3.AutoSize = true;
            this.configLabel3.Font = new System.Drawing.Font("Reem Kufi", 8.25F);
            this.configLabel3.Location = new System.Drawing.Point(33, 132);
            this.configLabel3.Name = "configLabel3";
            this.configLabel3.Size = new System.Drawing.Size(146, 21);
            this.configLabel3.TabIndex = 10;
            this.configLabel3.Text = "Modpack Storage Directory";
            // 
            // configLabel2
            // 
            this.configLabel2.AutoSize = true;
            this.configLabel2.Font = new System.Drawing.Font("Reem Kufi", 8.25F);
            this.configLabel2.Location = new System.Drawing.Point(33, 74);
            this.configLabel2.Name = "configLabel2";
            this.configLabel2.Size = new System.Drawing.Size(95, 21);
            this.configLabel2.TabIndex = 7;
            this.configLabel2.Text = "Backup Directory";
            // 
            // cfgUpdateBtn
            // 
            this.cfgUpdateBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.cfgUpdateBtn.Location = new System.Drawing.Point(402, 221);
            this.cfgUpdateBtn.Name = "cfgUpdateBtn";
            this.cfgUpdateBtn.Size = new System.Drawing.Size(96, 35);
            this.cfgUpdateBtn.TabIndex = 5;
            this.cfgUpdateBtn.Text = "Update";
            this.cfgUpdateBtn.UseVisualStyleBackColor = true;
            this.cfgUpdateBtn.Click += new System.EventHandler(this.cfgUpdateBtn_Click);
            // 
            // configLabel1
            // 
            this.configLabel1.AutoSize = true;
            this.configLabel1.Font = new System.Drawing.Font("Reem Kufi", 8.25F);
            this.configLabel1.Location = new System.Drawing.Point(33, 16);
            this.configLabel1.Name = "configLabel1";
            this.configLabel1.Size = new System.Drawing.Size(115, 21);
            this.configLabel1.TabIndex = 0;
            this.configLabel1.Text = "MCC Install Directory";
            // 
            // backupTab
            // 
            this.backupTab.BackColor = System.Drawing.Color.DarkGray;
            this.backupTab.FlatAppearance.BorderSize = 0;
            this.backupTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backupTab.Font = new System.Drawing.Font("Reem Kufi", 9.749999F, System.Drawing.FontStyle.Bold);
            this.backupTab.Location = new System.Drawing.Point(433, 43);
            this.backupTab.Name = "backupTab";
            this.backupTab.Size = new System.Drawing.Size(135, 37);
            this.backupTab.TabIndex = 7;
            this.backupTab.Text = "Backups";
            this.backupTab.UseVisualStyleBackColor = false;
            this.backupTab.Click += new System.EventHandler(this.backupTab_Click);
            this.backupTab.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.backupTab.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // backupPanel
            // 
            this.backupPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.backupPanel.Controls.Add(this.fullBakPath_chb);
            this.backupPanel.Controls.Add(this.restoreSelectedBtn);
            this.backupPanel.Controls.Add(this.delAllBaksBtn);
            this.backupPanel.Controls.Add(this.makeBakBtn);
            this.backupPanel.Controls.Add(this.restoreAllBaksBtn);
            this.backupPanel.Controls.Add(this.delSelectedBak);
            this.backupPanel.Controls.Add(this.bakLabel2);
            this.backupPanel.Controls.Add(this.bakLabel1);
            this.backupPanel.Controls.Add(this.bakListPanel);
            this.backupPanel.Location = new System.Drawing.Point(5, 89);
            this.backupPanel.Name = "backupPanel";
            this.backupPanel.Size = new System.Drawing.Size(563, 356);
            this.backupPanel.TabIndex = 8;
            this.backupPanel.Visible = false;
            // 
            // restoreSelectedBtn
            // 
            this.restoreSelectedBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.restoreSelectedBtn.Location = new System.Drawing.Point(427, 184);
            this.restoreSelectedBtn.Name = "restoreSelectedBtn";
            this.restoreSelectedBtn.Size = new System.Drawing.Size(135, 35);
            this.restoreSelectedBtn.TabIndex = 7;
            this.restoreSelectedBtn.Text = "Restore Selected";
            this.restoreSelectedBtn.UseVisualStyleBackColor = true;
            this.restoreSelectedBtn.Click += new System.EventHandler(this.restoreSelectedBtn_Click);
            this.restoreSelectedBtn.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.restoreSelectedBtn.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // delAllBaksBtn
            // 
            this.delAllBaksBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.delAllBaksBtn.ForeColor = System.Drawing.Color.Red;
            this.delAllBaksBtn.Location = new System.Drawing.Point(427, 308);
            this.delAllBaksBtn.Name = "delAllBaksBtn";
            this.delAllBaksBtn.Size = new System.Drawing.Size(135, 35);
            this.delAllBaksBtn.TabIndex = 6;
            this.delAllBaksBtn.Text = "Delete All Backups";
            this.delAllBaksBtn.UseVisualStyleBackColor = true;
            this.delAllBaksBtn.Click += new System.EventHandler(this.delAllBaksBtn_Click);
            this.delAllBaksBtn.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.delAllBaksBtn.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // makeBakBtn
            // 
            this.makeBakBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.makeBakBtn.Location = new System.Drawing.Point(427, 143);
            this.makeBakBtn.Name = "makeBakBtn";
            this.makeBakBtn.Size = new System.Drawing.Size(135, 35);
            this.makeBakBtn.TabIndex = 5;
            this.makeBakBtn.Text = "New Backup";
            this.makeBakBtn.UseVisualStyleBackColor = true;
            this.makeBakBtn.Click += new System.EventHandler(this.makeBakBtn_Click);
            this.makeBakBtn.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.makeBakBtn.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // restoreAllBaksBtn
            // 
            this.restoreAllBaksBtn.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.restoreAllBaksBtn.Location = new System.Drawing.Point(427, 225);
            this.restoreAllBaksBtn.Name = "restoreAllBaksBtn";
            this.restoreAllBaksBtn.Size = new System.Drawing.Size(135, 36);
            this.restoreAllBaksBtn.TabIndex = 4;
            this.restoreAllBaksBtn.Text = "Restore All Files";
            this.restoreAllBaksBtn.UseVisualStyleBackColor = true;
            this.restoreAllBaksBtn.Click += new System.EventHandler(this.restoreAllBaksBtn_Click);
            this.restoreAllBaksBtn.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.restoreAllBaksBtn.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // delSelectedBak
            // 
            this.delSelectedBak.Font = new System.Drawing.Font("Reem Kufi", 9.749999F);
            this.delSelectedBak.ForeColor = System.Drawing.Color.Red;
            this.delSelectedBak.Location = new System.Drawing.Point(427, 267);
            this.delSelectedBak.Name = "delSelectedBak";
            this.delSelectedBak.Size = new System.Drawing.Size(135, 35);
            this.delSelectedBak.TabIndex = 3;
            this.delSelectedBak.Text = "Delete Selected";
            this.delSelectedBak.UseVisualStyleBackColor = true;
            this.delSelectedBak.Click += new System.EventHandler(this.delSelectedBak_Click);
            this.delSelectedBak.MouseEnter += new System.EventHandler(this.btnHoverOn);
            this.delSelectedBak.MouseLeave += new System.EventHandler(this.btnHoverOff);
            // 
            // bakLabel2
            // 
            this.bakLabel2.AutoSize = true;
            this.bakLabel2.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.bakLabel2.Location = new System.Drawing.Point(100, 7);
            this.bakLabel2.Name = "bakLabel2";
            this.bakLabel2.Size = new System.Drawing.Size(68, 23);
            this.bakLabel2.TabIndex = 2;
            this.bakLabel2.Text = "Backup file";
            // 
            // bakLabel1
            // 
            this.bakLabel1.AutoSize = true;
            this.bakLabel1.Font = new System.Drawing.Font("Reem Kufi", 8.999999F);
            this.bakLabel1.Location = new System.Drawing.Point(32, 7);
            this.bakLabel1.Name = "bakLabel1";
            this.bakLabel1.Size = new System.Drawing.Size(42, 23);
            this.bakLabel1.TabIndex = 1;
            this.bakLabel1.Text = "Select";
            // 
            // bakListPanel
            // 
            this.bakListPanel.AutoScroll = true;
            this.bakListPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.bakListPanel.Location = new System.Drawing.Point(14, 33);
            this.bakListPanel.Name = "bakListPanel";
            this.bakListPanel.Size = new System.Drawing.Size(402, 322);
            this.bakListPanel.TabIndex = 0;
            // 
            // betterPBar
            // 
            this.betterPBar.Location = new System.Drawing.Point(4, 448);
            this.betterPBar.Name = "betterPBar";
            this.betterPBar.Size = new System.Drawing.Size(570, 44);
            this.betterPBar.TabIndex = 10;
            // 
            // fullBakPath_chb
            // 
            this.fullBakPath_chb.AutoSize = true;
            this.fullBakPath_chb.Location = new System.Drawing.Point(427, 120);
            this.fullBakPath_chb.Name = "fullBakPath_chb";
            this.fullBakPath_chb.Size = new System.Drawing.Size(93, 17);
            this.fullBakPath_chb.TabIndex = 8;
            this.fullBakPath_chb.Text = "Show full path";
            this.fullBakPath_chb.UseVisualStyleBackColor = true;
            this.fullBakPath_chb.CheckedChanged += new System.EventHandler(this.fullBakPath_chb_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.ClientSize = new System.Drawing.Size(582, 497);
            this.Controls.Add(this.betterPBar);
            this.Controls.Add(this.backupTab);
            this.Controls.Add(this.configTab);
            this.Controls.Add(this.createTab);
            this.Controls.Add(this.homeTab);
            this.Controls.Add(this.topBar);
            this.Controls.Add(this.backupPanel);
            this.Controls.Add(this.createPanel);
            this.Controls.Add(this.configPanel);
            this.Controls.Add(this.homePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MCC Mod Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.topBar.ResumeLayout(false);
            this.topBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.refreshButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.exitButton)).EndInit();
            this.homePanel.ResumeLayout(false);
            this.homePanel.PerformLayout();
            this.createPanel.ResumeLayout(false);
            this.createPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.addRowButton)).EndInit();
            this.configPanel.ResumeLayout(false);
            this.configPanel.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.backupPanel.ResumeLayout(false);
            this.backupPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel topBar;
        private System.Windows.Forms.PictureBox exitButton;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.PictureBox minButton;
        private System.Windows.Forms.Button homeTab;
        private System.Windows.Forms.Button createTab;
        private System.Windows.Forms.Panel homePanel;
        private System.Windows.Forms.Button configTab;
        private System.Windows.Forms.Panel modListPanel;
        private System.Windows.Forms.Label homeEnableLabel;
        private System.Windows.Forms.Label homeNameLabel;
        private System.Windows.Forms.Button patchButton;
        private System.Windows.Forms.PictureBox refreshButton;
        private System.Windows.Forms.Panel createPanel;
        private System.Windows.Forms.Button createModpackBtn;
        private System.Windows.Forms.Panel createFilesPanel;
        private System.Windows.Forms.Label createLabel2;
        private System.Windows.Forms.Label createLabel1;
        private System.Windows.Forms.PictureBox addRowButton;
        private System.Windows.Forms.Panel configPanel;
        private System.Windows.Forms.Label configLabel1;
        private System.Windows.Forms.TextBox cfgTextBox1;
        private System.Windows.Forms.Button cfgBrowseBtn1;
        private System.Windows.Forms.Button cfgUpdateBtn;
        private System.Windows.Forms.Button cfgBrowseBtn3;
        private System.Windows.Forms.TextBox cfgTextBox3;
        private System.Windows.Forms.Label configLabel3;
        private System.Windows.Forms.Button cfgBrowseBtn2;
        private System.Windows.Forms.TextBox cfgTextBox2;
        private System.Windows.Forms.Label configLabel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button clearBtn;
        private System.Windows.Forms.TextBox modpackName_txt;
        private System.Windows.Forms.Label modpackName_label;
        private System.Windows.Forms.Button backupTab;
        private System.Windows.Forms.Panel backupPanel;
        private System.Windows.Forms.Button restoreAllBaksBtn;
        private System.Windows.Forms.Button delSelectedBak;
        private System.Windows.Forms.Label bakLabel2;
        private System.Windows.Forms.Label bakLabel1;
        private System.Windows.Forms.Panel bakListPanel;
        private System.Windows.Forms.Button makeBakBtn;
        private System.Windows.Forms.Button delAllBaksBtn;
        private System.Windows.Forms.Button restoreSelectedBtn;
        private System.Windows.Forms.Button delModpack;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox delOldBaks_chb;
        private System.Windows.Forms.Panel betterPBar;
        private System.Windows.Forms.CheckBox fullBakPath_chb;
    }
}


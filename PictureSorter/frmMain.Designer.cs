namespace PictureSorter
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fichierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ouvrirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sauvegarderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ouvrirDossierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exporterLesImaesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.généralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.réinitialisertoutSélectionnerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toutCocherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toutDécocherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.voirLaideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.langueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.frenchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblImageCounter = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.voirLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.voirDossierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.TabStop = false;
            // 
            // treeView1
            // 
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fichierToolStripMenuItem,
            this.généralToolStripMenuItem,
            this.aideToolStripMenuItem,
            this.langueToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fichierToolStripMenuItem
            // 
            this.fichierToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ouvrirToolStripMenuItem,
            this.sauvegarderToolStripMenuItem,
            this.ouvrirDossierToolStripMenuItem,
            this.toolStripSeparator2,
            this.exporterLesImaesToolStripMenuItem,
            this.renameExportToolStripMenuItem,
            this.toolStripSeparator4,
            this.renameToolStripMenuItem,
            this.toolStripSeparator1,
            this.quitterToolStripMenuItem});
            this.fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            resources.ApplyResources(this.fichierToolStripMenuItem, "fichierToolStripMenuItem");
            // 
            // ouvrirToolStripMenuItem
            // 
            this.ouvrirToolStripMenuItem.Name = "ouvrirToolStripMenuItem";
            resources.ApplyResources(this.ouvrirToolStripMenuItem, "ouvrirToolStripMenuItem");
            this.ouvrirToolStripMenuItem.Click += new System.EventHandler(this.ouvrirToolStripMenuItem_Click);
            // 
            // sauvegarderToolStripMenuItem
            // 
            this.sauvegarderToolStripMenuItem.Name = "sauvegarderToolStripMenuItem";
            resources.ApplyResources(this.sauvegarderToolStripMenuItem, "sauvegarderToolStripMenuItem");
            this.sauvegarderToolStripMenuItem.Click += new System.EventHandler(this.sauvegarderToolStripMenuItem_Click);
            // 
            // ouvrirDossierToolStripMenuItem
            // 
            this.ouvrirDossierToolStripMenuItem.Name = "ouvrirDossierToolStripMenuItem";
            resources.ApplyResources(this.ouvrirDossierToolStripMenuItem, "ouvrirDossierToolStripMenuItem");
            this.ouvrirDossierToolStripMenuItem.Click += new System.EventHandler(this.ouvrirDossierToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // exporterLesImaesToolStripMenuItem
            // 
            this.exporterLesImaesToolStripMenuItem.Name = "exporterLesImaesToolStripMenuItem";
            resources.ApplyResources(this.exporterLesImaesToolStripMenuItem, "exporterLesImaesToolStripMenuItem");
            this.exporterLesImaesToolStripMenuItem.Click += new System.EventHandler(this.exporterLesImaesToolStripMenuItem_Click);
            // 
            // renameExportToolStripMenuItem
            // 
            this.renameExportToolStripMenuItem.Name = "renameExportToolStripMenuItem";
            resources.ApplyResources(this.renameExportToolStripMenuItem, "renameExportToolStripMenuItem");
            this.renameExportToolStripMenuItem.Click += new System.EventHandler(this.renameExportToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            resources.ApplyResources(this.renameToolStripMenuItem, "renameToolStripMenuItem");
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // quitterToolStripMenuItem
            // 
            this.quitterToolStripMenuItem.Name = "quitterToolStripMenuItem";
            resources.ApplyResources(this.quitterToolStripMenuItem, "quitterToolStripMenuItem");
            this.quitterToolStripMenuItem.Click += new System.EventHandler(this.quitterToolStripMenuItem_Click);
            // 
            // généralToolStripMenuItem
            // 
            this.généralToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.réinitialisertoutSélectionnerToolStripMenuItem});
            this.généralToolStripMenuItem.Name = "généralToolStripMenuItem";
            resources.ApplyResources(this.généralToolStripMenuItem, "généralToolStripMenuItem");
            // 
            // réinitialisertoutSélectionnerToolStripMenuItem
            // 
            this.réinitialisertoutSélectionnerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toutCocherToolStripMenuItem,
            this.toutDécocherToolStripMenuItem});
            this.réinitialisertoutSélectionnerToolStripMenuItem.Name = "réinitialisertoutSélectionnerToolStripMenuItem";
            resources.ApplyResources(this.réinitialisertoutSélectionnerToolStripMenuItem, "réinitialisertoutSélectionnerToolStripMenuItem");
            // 
            // toutCocherToolStripMenuItem
            // 
            this.toutCocherToolStripMenuItem.Name = "toutCocherToolStripMenuItem";
            resources.ApplyResources(this.toutCocherToolStripMenuItem, "toutCocherToolStripMenuItem");
            this.toutCocherToolStripMenuItem.Click += new System.EventHandler(this.toutCocherToolStripMenuItem_Click);
            // 
            // toutDécocherToolStripMenuItem
            // 
            this.toutDécocherToolStripMenuItem.Name = "toutDécocherToolStripMenuItem";
            resources.ApplyResources(this.toutDécocherToolStripMenuItem, "toutDécocherToolStripMenuItem");
            this.toutDécocherToolStripMenuItem.Click += new System.EventHandler(this.toutDécocherToolStripMenuItem_Click);
            // 
            // aideToolStripMenuItem
            // 
            this.aideToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.voirLaideToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.toolStripSeparator3,
            this.voirLogsToolStripMenuItem,
            this.voirDossierToolStripMenuItem});
            this.aideToolStripMenuItem.Name = "aideToolStripMenuItem";
            resources.ApplyResources(this.aideToolStripMenuItem, "aideToolStripMenuItem");
            // 
            // voirLaideToolStripMenuItem
            // 
            this.voirLaideToolStripMenuItem.Name = "voirLaideToolStripMenuItem";
            resources.ApplyResources(this.voirLaideToolStripMenuItem, "voirLaideToolStripMenuItem");
            this.voirLaideToolStripMenuItem.Click += new System.EventHandler(this.voirLaideToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // langueToolStripMenuItem
            // 
            this.langueToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.frenchToolStripMenuItem});
            this.langueToolStripMenuItem.Name = "langueToolStripMenuItem";
            resources.ApplyResources(this.langueToolStripMenuItem, "langueToolStripMenuItem");
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            resources.ApplyResources(this.englishToolStripMenuItem, "englishToolStripMenuItem");
            this.englishToolStripMenuItem.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
            // 
            // frenchToolStripMenuItem
            // 
            this.frenchToolStripMenuItem.Name = "frenchToolStripMenuItem";
            resources.ApplyResources(this.frenchToolStripMenuItem, "frenchToolStripMenuItem");
            this.frenchToolStripMenuItem.Click += new System.EventHandler(this.frenchToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblImageCounter);
            this.panel1.Controls.Add(this.button1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lblImageCounter
            // 
            resources.ApplyResources(this.lblImageCounter, "lblImageCounter");
            this.lblImageCounter.Name = "lblImageCounter";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.TabStop = false;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // voirLogsToolStripMenuItem
            // 
            this.voirLogsToolStripMenuItem.Name = "voirLogsToolStripMenuItem";
            resources.ApplyResources(this.voirLogsToolStripMenuItem, "voirLogsToolStripMenuItem");
            this.voirLogsToolStripMenuItem.Click += new System.EventHandler(this.voirLogsToolStripMenuItem_Click);
            // 
            // voirDossierToolStripMenuItem
            // 
            this.voirDossierToolStripMenuItem.Name = "voirDossierToolStripMenuItem";
            resources.ApplyResources(this.voirDossierToolStripMenuItem, "voirDossierToolStripMenuItem");
            this.voirDossierToolStripMenuItem.Click += new System.EventHandler(this.voirDossierToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fichierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem voirLaideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem ouvrirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sauvegarderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exporterLesImaesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem quitterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem généralToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem réinitialisertoutSélectionnerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toutCocherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toutDécocherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ouvrirDossierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem langueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem frenchToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.Label lblImageCounter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem renameExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem voirLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem voirDossierToolStripMenuItem;
    }
}


namespace MEMeshMorphExporter
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPccToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportMorphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllPccMeshesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllPccMorphsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aSFbxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aSJsonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.LeftTree = new System.Windows.Forms.TreeView();
            this.LeftTreeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DetailsTreeView = new System.Windows.Forms.TreeView();
            this.clearTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.LeftTreeContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(555, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openPccToolStripMenuItem,
            this.clearTreeToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openPccToolStripMenuItem
            // 
            this.openPccToolStripMenuItem.Name = "openPccToolStripMenuItem";
            this.openPccToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.openPccToolStripMenuItem.Text = "Open Mass Effect Package...";
            this.openPccToolStripMenuItem.Click += new System.EventHandler(this.openPccToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.exitToolStripMenuItem.Text = "Exit...";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportMeshToolStripMenuItem,
            this.exportMorphToolStripMenuItem,
            this.exportAllPccMeshesToolStripMenuItem,
            this.exportAllPccMorphsToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // exportMeshToolStripMenuItem
            // 
            this.exportMeshToolStripMenuItem.Name = "exportMeshToolStripMenuItem";
            this.exportMeshToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.exportMeshToolStripMenuItem.Text = "Export mesh as fbx...";
            this.exportMeshToolStripMenuItem.Click += new System.EventHandler(this.exportMeshToolStripMenuItem_Click);
            // 
            // exportMorphToolStripMenuItem
            // 
            this.exportMorphToolStripMenuItem.Name = "exportMorphToolStripMenuItem";
            this.exportMorphToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.exportMorphToolStripMenuItem.Text = "Export morph...";
            this.exportMorphToolStripMenuItem.Click += new System.EventHandler(this.exportMorphToolStripMenuItem_Click);
            // 
            // exportAllPccMeshesToolStripMenuItem
            // 
            this.exportAllPccMeshesToolStripMenuItem.Name = "exportAllPccMeshesToolStripMenuItem";
            this.exportAllPccMeshesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.exportAllPccMeshesToolStripMenuItem.Text = "Export all pcc meshes...";
            this.exportAllPccMeshesToolStripMenuItem.Click += new System.EventHandler(this.exportAllPccMeshesToolStripMenuItem_Click);
            // 
            // exportAllPccMorphsToolStripMenuItem
            // 
            this.exportAllPccMorphsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aSFbxToolStripMenuItem,
            this.aSJsonToolStripMenuItem});
            this.exportAllPccMorphsToolStripMenuItem.Name = "exportAllPccMorphsToolStripMenuItem";
            this.exportAllPccMorphsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.exportAllPccMorphsToolStripMenuItem.Text = "Export all pcc morphs...";
            // 
            // aSFbxToolStripMenuItem
            // 
            this.aSFbxToolStripMenuItem.Name = "aSFbxToolStripMenuItem";
            this.aSFbxToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.aSFbxToolStripMenuItem.Text = "As fbx...";
            this.aSFbxToolStripMenuItem.Click += new System.EventHandler(this.aSFbxToolStripMenuItem_Click);
            // 
            // aSJsonToolStripMenuItem
            // 
            this.aSJsonToolStripMenuItem.Name = "aSJsonToolStripMenuItem";
            this.aSJsonToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.aSJsonToolStripMenuItem.Text = "As json...";
            this.aSJsonToolStripMenuItem.Click += new System.EventHandler(this.aSJsonToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.LeftTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.DetailsTreeView);
            this.splitContainer1.Size = new System.Drawing.Size(555, 366);
            this.splitContainer1.SplitterDistance = 213;
            this.splitContainer1.TabIndex = 1;
            // 
            // LeftTree
            // 
            this.LeftTree.ContextMenuStrip = this.LeftTreeContextMenuStrip;
            this.LeftTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftTree.Location = new System.Drawing.Point(0, 0);
            this.LeftTree.Name = "LeftTree";
            this.LeftTree.Size = new System.Drawing.Size(213, 366);
            this.LeftTree.TabIndex = 0;
            this.LeftTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.LeftTree_AfterSelect);
            // 
            // LeftTreeContextMenuStrip
            // 
            this.LeftTreeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportContextMenuItem});
            this.LeftTreeContextMenuStrip.Name = "LeftTreeContextMenuStrip";
            this.LeftTreeContextMenuStrip.Size = new System.Drawing.Size(117, 26);
            // 
            // exportContextMenuItem
            // 
            this.exportContextMenuItem.Enabled = false;
            this.exportContextMenuItem.Name = "exportContextMenuItem";
            this.exportContextMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exportContextMenuItem.Text = "Export...";
            this.exportContextMenuItem.Click += new System.EventHandler(this.exportContextMenuItem_Click);
            // 
            // DetailsTreeView
            // 
            this.DetailsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsTreeView.Location = new System.Drawing.Point(0, 0);
            this.DetailsTreeView.Name = "DetailsTreeView";
            this.DetailsTreeView.Size = new System.Drawing.Size(338, 366);
            this.DetailsTreeView.TabIndex = 0;
            // 
            // clearTreeToolStripMenuItem
            // 
            this.clearTreeToolStripMenuItem.Name = "clearTreeToolStripMenuItem";
            this.clearTreeToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.clearTreeToolStripMenuItem.Text = "Clear Tree...";
            this.clearTreeToolStripMenuItem.Click += new System.EventHandler(this.clearTreeToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 390);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "ME3 HeadMorphs exporter";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.LeftTreeContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openPccToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView LeftTree;
        private System.Windows.Forms.TreeView DetailsTreeView;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportMeshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllPccMeshesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllPccMorphsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportMorphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aSFbxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aSJsonToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip LeftTreeContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearTreeToolStripMenuItem;
    }
}


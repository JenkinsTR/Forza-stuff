namespace ForzaStudio
{
    partial class ForzaStudioForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForzaStudioForm));
            this.tvCarStructure = new System.Windows.Forms.TreeView();
            this.ctxTreeViewMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxMenuExport = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpLayout = new System.Windows.Forms.GroupBox();
            this.tabCtrl = new System.Windows.Forms.TabControl();
            this.tabViewport = new System.Windows.Forms.TabPage();
            this.pnlVisual = new System.Windows.Forms.Panel();
            this.ctxViewportMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmbFillMode = new System.Windows.Forms.ToolStripComboBox();
            this.cmbCullMode = new System.Windows.Forms.ToolStripComboBox();
            this.chkNormals = new System.Windows.Forms.ToolStripMenuItem();
            this.chkWireframe = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.cmbModelColor = new System.Windows.Forms.ToolStripComboBox();
            this.tabInformation = new System.Windows.Forms.TabPage();
            this.txtInformation = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusRenderInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.mnuStrip = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuSelectiveExport = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTreeViewMenu.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpLayout.SuspendLayout();
            this.tabCtrl.SuspendLayout();
            this.tabViewport.SuspendLayout();
            this.ctxViewportMenu.SuspendLayout();
            this.tabInformation.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.mnuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvCarStructure
            // 
            this.tvCarStructure.CheckBoxes = true;
            this.tvCarStructure.ContextMenuStrip = this.ctxTreeViewMenu;
            this.tvCarStructure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvCarStructure.Location = new System.Drawing.Point(3, 16);
            this.tvCarStructure.Name = "tvCarStructure";
            this.tvCarStructure.ShowNodeToolTips = true;
            this.tvCarStructure.Size = new System.Drawing.Size(194, 499);
            this.tvCarStructure.TabIndex = 1;
            this.tvCarStructure.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvCarStructure_AfterCheck);
            this.tvCarStructure.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvCarStructure_AfterSelect);
            this.tvCarStructure.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvCarStructure_NodeMouseClick);
            this.tvCarStructure.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvCarStructure_BeforeCheck);
            // 
            // ctxTreeViewMenu
            // 
            this.ctxTreeViewMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxMenuExport,
            this.ctxMenuSelectiveExport});
            this.ctxTreeViewMenu.Name = "ctxTreeViewMenu";
            this.ctxTreeViewMenu.Size = new System.Drawing.Size(157, 70);
            // 
            // ctxMenuExport
            // 
            this.ctxMenuExport.Name = "ctxMenuExport";
            this.ctxMenuExport.Size = new System.Drawing.Size(156, 22);
            this.ctxMenuExport.Text = "Export";
            this.ctxMenuExport.Click += new System.EventHandler(this.ctxMenuExport_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpLayout);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabCtrl);
            this.splitContainer1.Size = new System.Drawing.Size(875, 518);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 2;
            // 
            // grpLayout
            // 
            this.grpLayout.Controls.Add(this.tvCarStructure);
            this.grpLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLayout.Location = new System.Drawing.Point(0, 0);
            this.grpLayout.MinimumSize = new System.Drawing.Size(175, 0);
            this.grpLayout.Name = "grpLayout";
            this.grpLayout.Size = new System.Drawing.Size(200, 518);
            this.grpLayout.TabIndex = 2;
            this.grpLayout.TabStop = false;
            this.grpLayout.Text = "Layout";
            // 
            // tabCtrl
            // 
            this.tabCtrl.Controls.Add(this.tabViewport);
            this.tabCtrl.Controls.Add(this.tabInformation);
            this.tabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrl.Location = new System.Drawing.Point(0, 0);
            this.tabCtrl.Name = "tabCtrl";
            this.tabCtrl.SelectedIndex = 0;
            this.tabCtrl.Size = new System.Drawing.Size(671, 518);
            this.tabCtrl.TabIndex = 1;
            // 
            // tabViewport
            // 
            this.tabViewport.Controls.Add(this.pnlVisual);
            this.tabViewport.Location = new System.Drawing.Point(4, 22);
            this.tabViewport.Name = "tabViewport";
            this.tabViewport.Padding = new System.Windows.Forms.Padding(3);
            this.tabViewport.Size = new System.Drawing.Size(663, 492);
            this.tabViewport.TabIndex = 0;
            this.tabViewport.Text = "Viewport";
            this.tabViewport.UseVisualStyleBackColor = true;
            // 
            // pnlVisual
            // 
            this.pnlVisual.AutoScroll = true;
            this.pnlVisual.ContextMenuStrip = this.ctxViewportMenu;
            this.pnlVisual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlVisual.Location = new System.Drawing.Point(3, 3);
            this.pnlVisual.Name = "pnlVisual";
            this.pnlVisual.Size = new System.Drawing.Size(657, 486);
            this.pnlVisual.TabIndex = 2;
            this.pnlVisual.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlVisual_Paint);
            this.pnlVisual.Click += new System.EventHandler(this.pnlVisual_Click);
            this.pnlVisual.Resize += new System.EventHandler(this.pnlVisual_Resize);
            // 
            // ctxViewportMenu
            // 
            this.ctxViewportMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmbFillMode,
            this.cmbCullMode,
            this.chkNormals,
            this.chkWireframe,
            this.chkGrid,
            this.cmbModelColor});
            this.ctxViewportMenu.Name = "ctxViewportMenu";
            this.ctxViewportMenu.Size = new System.Drawing.Size(182, 151);
            // 
            // cmbFillMode
            // 
            this.cmbFillMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFillMode.Items.AddRange(new object[] {
            "Solid",
            "Wireframe",
            "Point"});
            this.cmbFillMode.Name = "cmbFillMode";
            this.cmbFillMode.Size = new System.Drawing.Size(121, 23);
            this.cmbFillMode.SelectedIndexChanged += new System.EventHandler(this.cmbFillMode_SelectedIndexChanged);
            // 
            // cmbCullMode
            // 
            this.cmbCullMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCullMode.Items.AddRange(new object[] {
            "None",
            "Clockwise",
            "Counterclockwise"});
            this.cmbCullMode.Name = "cmbCullMode";
            this.cmbCullMode.Size = new System.Drawing.Size(121, 23);
            this.cmbCullMode.SelectedIndexChanged += new System.EventHandler(this.cmbCullMode_SelectedIndexChanged);
            // 
            // chkNormals
            // 
            this.chkNormals.CheckOnClick = true;
            this.chkNormals.Name = "chkNormals";
            this.chkNormals.Size = new System.Drawing.Size(181, 22);
            this.chkNormals.Text = "Normal Overlay";
            this.chkNormals.Click += new System.EventHandler(this.chkNormals_Click);
            // 
            // chkWireframe
            // 
            this.chkWireframe.Checked = true;
            this.chkWireframe.CheckOnClick = true;
            this.chkWireframe.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWireframe.Name = "chkWireframe";
            this.chkWireframe.Size = new System.Drawing.Size(181, 22);
            this.chkWireframe.Text = "Wireframe Overlay";
            this.chkWireframe.Click += new System.EventHandler(this.chkWireframe_Click);
            // 
            // chkGrid
            // 
            this.chkGrid.Checked = true;
            this.chkGrid.CheckOnClick = true;
            this.chkGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGrid.Name = "chkGrid";
            this.chkGrid.Size = new System.Drawing.Size(181, 22);
            this.chkGrid.Text = "Display Grid";
            this.chkGrid.Click += new System.EventHandler(this.chkGrid_Click);
            // 
            // cmbModelColor
            // 
            this.cmbModelColor.BackColor = System.Drawing.Color.Gray;
            this.cmbModelColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModelColor.Name = "cmbModelColor";
            this.cmbModelColor.Size = new System.Drawing.Size(121, 23);
            this.cmbModelColor.Click += new System.EventHandler(this.cmbModelColor_Click);
            // 
            // tabInformation
            // 
            this.tabInformation.Controls.Add(this.txtInformation);
            this.tabInformation.Location = new System.Drawing.Point(4, 22);
            this.tabInformation.Name = "tabInformation";
            this.tabInformation.Padding = new System.Windows.Forms.Padding(3);
            this.tabInformation.Size = new System.Drawing.Size(663, 492);
            this.tabInformation.TabIndex = 1;
            this.tabInformation.Text = "Information";
            this.tabInformation.UseVisualStyleBackColor = true;
            // 
            // txtInformation
            // 
            this.txtInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInformation.Location = new System.Drawing.Point(3, 3);
            this.txtInformation.Multiline = true;
            this.txtInformation.Name = "txtInformation";
            this.txtInformation.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInformation.Size = new System.Drawing.Size(657, 486);
            this.txtInformation.TabIndex = 0;
            this.txtInformation.WordWrap = false;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusRenderInfo});
            this.statusStrip.Location = new System.Drawing.Point(0, 548);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(875, 22);
            this.statusStrip.TabIndex = 3;
            // 
            // statusFaceCount
            // 
            this.statusRenderInfo.Name = "statusFaceCount";
            this.statusRenderInfo.Size = new System.Drawing.Size(0, 17);
            // 
            // mnuStrip
            // 
            this.mnuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuAbout});
            this.mnuStrip.Location = new System.Drawing.Point(0, 0);
            this.mnuStrip.Name = "mnuStrip";
            this.mnuStrip.Size = new System.Drawing.Size(875, 24);
            this.mnuStrip.TabIndex = 4;
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpen,
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "File";
            // 
            // mnuOpen
            // 
            this.mnuOpen.Name = "mnuOpen";
            this.mnuOpen.Size = new System.Drawing.Size(103, 22);
            this.mnuOpen.Text = "Open";
            this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(103, 22);
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // mnuAbout
            // 
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(52, 20);
            this.mnuAbout.Text = "About";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // ctxMenuSelectiveExport
            // 
            this.ctxMenuSelectiveExport.Name = "ctxMenuSelectiveExport";
            this.ctxMenuSelectiveExport.Size = new System.Drawing.Size(156, 22);
            this.ctxMenuSelectiveExport.Text = "Selective Export";
            this.ctxMenuSelectiveExport.Click += new System.EventHandler(this.ctxMenuSelectiveExport_Click);
            // 
            // ForzaStudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 570);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mnuStrip);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "ForzaStudioForm";
            this.Text = "Forza Studio";
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseWheel);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ctxTreeViewMenu.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.grpLayout.ResumeLayout(false);
            this.tabCtrl.ResumeLayout(false);
            this.tabViewport.ResumeLayout(false);
            this.ctxViewportMenu.ResumeLayout(false);
            this.tabInformation.ResumeLayout(false);
            this.tabInformation.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mnuStrip.ResumeLayout(false);
            this.mnuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // visual editor doesn't like when we manually edit some stuff, so lets throw it in here instead...
        private void ManuallyUpdateComponent()
        {
            this.splitContainer1.Panel2MinSize = 250;
            this.cmbCullMode.SelectedIndex = 0;
            this.cmbFillMode.SelectedIndex = 0;

        }

        #endregion

        private System.Windows.Forms.TreeView tvCarStructure;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.MenuStrip mnuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.GroupBox grpLayout;
        private System.Windows.Forms.ToolStripStatusLabel statusRenderInfo;
        private System.Windows.Forms.Panel pnlVisual;
        private System.Windows.Forms.TabControl tabCtrl;
        private System.Windows.Forms.TabPage tabViewport;
        private System.Windows.Forms.TabPage tabInformation;
        private System.Windows.Forms.TextBox txtInformation;
        private System.Windows.Forms.ContextMenuStrip ctxTreeViewMenu;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuExport;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.ContextMenuStrip ctxViewportMenu;
        private System.Windows.Forms.ToolStripComboBox cmbFillMode;
        private System.Windows.Forms.ToolStripMenuItem chkNormals;
        private System.Windows.Forms.ToolStripMenuItem chkWireframe;
        private System.Windows.Forms.ToolStripComboBox cmbCullMode;
        private System.Windows.Forms.ToolStripMenuItem chkGrid;
        private System.Windows.Forms.ToolStripComboBox cmbModelColor;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuSelectiveExport;
    }
}


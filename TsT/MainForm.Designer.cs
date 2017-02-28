/*
 * Created by SharpDevelop.
 * User: denblo
 * Date: 16.08.2016
 * Time: 19:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace TsT
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.SplitContainer MainSplit;
		private System.Windows.Forms.SplitContainer VerticallySplit;
		private System.Windows.Forms.ListView PluginsList;
		private System.Windows.Forms.TextBox LogView;
		private System.Windows.Forms.ColumnHeader KeyHeader;
		private System.Windows.Forms.ColumnHeader StatusHeader;
		private System.Windows.Forms.ColumnHeader BranchHeader;
		private System.Windows.Forms.ColumnHeader StateHeader;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
	
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		void InitializeComponent()
		{
			this.MainSplit = new System.Windows.Forms.SplitContainer();
			this.VerticallySplit = new System.Windows.Forms.SplitContainer();
			this.PluginsList = new System.Windows.Forms.ListView();
			this.KeyHeader = new System.Windows.Forms.ColumnHeader();
			this.StatusHeader = new System.Windows.Forms.ColumnHeader();
			this.BranchHeader = new System.Windows.Forms.ColumnHeader();
			this.StateHeader = new System.Windows.Forms.ColumnHeader();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LogView = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.MainSplit)).BeginInit();
			this.MainSplit.Panel1.SuspendLayout();
			this.MainSplit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VerticallySplit)).BeginInit();
			this.VerticallySplit.Panel1.SuspendLayout();
			this.VerticallySplit.Panel2.SuspendLayout();
			this.VerticallySplit.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainSplit
			// 
			this.MainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplit.Location = new System.Drawing.Point(0, 0);
			this.MainSplit.Name = "MainSplit";
			// 
			// MainSplit.Panel1
			// 
			this.MainSplit.Panel1.Controls.Add(this.VerticallySplit);
			this.MainSplit.Size = new System.Drawing.Size(585, 491);
			this.MainSplit.SplitterDistance = 499;
			this.MainSplit.TabIndex = 0;
			// 
			// VerticallySplit
			// 
			this.VerticallySplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VerticallySplit.Location = new System.Drawing.Point(0, 0);
			this.VerticallySplit.Name = "VerticallySplit";
			this.VerticallySplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// VerticallySplit.Panel1
			// 
			this.VerticallySplit.Panel1.Controls.Add(this.PluginsList);
			this.VerticallySplit.Panel1.Controls.Add(this.menuStrip1);
			// 
			// VerticallySplit.Panel2
			// 
			this.VerticallySplit.Panel2.Controls.Add(this.LogView);
			this.VerticallySplit.Size = new System.Drawing.Size(499, 491);
			this.VerticallySplit.SplitterDistance = 336;
			this.VerticallySplit.TabIndex = 0;
			// 
			// PluginsList
			// 
			this.PluginsList.CheckBoxes = true;
			this.PluginsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.KeyHeader,
			this.StatusHeader,
			this.BranchHeader,
			this.StateHeader});
			this.PluginsList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PluginsList.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PluginsList.FullRowSelect = true;
			this.PluginsList.Location = new System.Drawing.Point(0, 24);
			this.PluginsList.Name = "PluginsList";
			this.PluginsList.Size = new System.Drawing.Size(499, 312);
			this.PluginsList.TabIndex = 0;
			this.PluginsList.UseCompatibleStateImageBehavior = false;
			this.PluginsList.View = System.Windows.Forms.View.Details;
			// 
			// KeyHeader
			// 
			this.KeyHeader.Text = "Key";
			this.KeyHeader.Width = 140;
			// 
			// StatusHeader
			// 
			this.StatusHeader.Text = "Status";
			this.StatusHeader.Width = 76;
			// 
			// BranchHeader
			// 
			this.BranchHeader.Text = "Branch";
			this.BranchHeader.Width = 100;
			// 
			// StateHeader
			// 
			this.StateHeader.Text = "State";
			this.StateHeader.Width = 72;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(499, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.openToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.openToolStripMenuItem.Text = "Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
			// 
			// LogView
			// 
			this.LogView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogView.Location = new System.Drawing.Point(0, 0);
			this.LogView.Multiline = true;
			this.LogView.Name = "LogView";
			this.LogView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.LogView.Size = new System.Drawing.Size(499, 151);
			this.LogView.TabIndex = 0;
			// 
			// MainForm
			// 
			this.ClientSize = new System.Drawing.Size(585, 491);
			this.Controls.Add(this.MainSplit);
			this.Name = "MainForm";
			this.MainSplit.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplit)).EndInit();
			this.MainSplit.ResumeLayout(false);
			this.VerticallySplit.Panel1.ResumeLayout(false);
			this.VerticallySplit.Panel1.PerformLayout();
			this.VerticallySplit.Panel2.ResumeLayout(false);
			this.VerticallySplit.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.VerticallySplit)).EndInit();
			this.VerticallySplit.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);

		}
	}
}

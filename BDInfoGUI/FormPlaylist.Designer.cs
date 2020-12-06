//============================================================================
// BDInfo - Blu-ray Video and Audio Analysis Tool
// Copyright © 2010 Cinema Squid
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

namespace BDInfoGUI
{
    partial class FormPlaylist
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
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
            this.groupBoxSourcePlaylist = new System.Windows.Forms.GroupBox();
            this.buttonAddAll = new System.Windows.Forms.Button();
            this.splitContainerInner = new System.Windows.Forms.SplitContainer();
            this.listViewPlaylistFiles = new System.Windows.Forms.ListView();
            this.columnHeaderPlaylistName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderPlaylistLength = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderPlaylistEstimatedBytes = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderPlaylistMeasuredBytes = new System.Windows.Forms.ColumnHeader();
            this.listViewStreamFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderFileLength = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderFileEstimatedBytes = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderFileMeasuredBytes = new System.Windows.Forms.ColumnHeader();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.groupBoxCustomPlaylist = new System.Windows.Forms.GroupBox();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewTargetFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.checkBoxFilterIncompatible = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).BeginInit();
            this.splitContainerOuter.Panel1.SuspendLayout();
            this.splitContainerOuter.Panel2.SuspendLayout();
            this.splitContainerOuter.SuspendLayout();
            this.groupBoxSourcePlaylist.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInner)).BeginInit();
            this.splitContainerInner.Panel1.SuspendLayout();
            this.splitContainerInner.Panel2.SuspendLayout();
            this.splitContainerInner.SuspendLayout();
            this.groupBoxCustomPlaylist.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(14, 15);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(465, 23);
            this.textBoxName.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(250, 530);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(88, 27);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(344, 530);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 27);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // splitContainerOuter
            // 
            this.splitContainerOuter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerOuter.Location = new System.Drawing.Point(14, 45);
            this.splitContainerOuter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainerOuter.Name = "splitContainerOuter";
            this.splitContainerOuter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerOuter.Panel1
            // 
            this.splitContainerOuter.Panel1.Controls.Add(this.groupBoxSourcePlaylist);
            // 
            // splitContainerOuter.Panel2
            // 
            this.splitContainerOuter.Panel2.Controls.Add(this.groupBoxCustomPlaylist);
            this.splitContainerOuter.Size = new System.Drawing.Size(653, 478);
            this.splitContainerOuter.SplitterDistance = 296;
            this.splitContainerOuter.SplitterWidth = 5;
            this.splitContainerOuter.TabIndex = 21;
            // 
            // groupBoxSourcePlaylist
            // 
            this.groupBoxSourcePlaylist.Controls.Add(this.buttonAddAll);
            this.groupBoxSourcePlaylist.Controls.Add(this.splitContainerInner);
            this.groupBoxSourcePlaylist.Controls.Add(this.buttonAdd);
            this.groupBoxSourcePlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSourcePlaylist.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSourcePlaylist.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxSourcePlaylist.Name = "groupBoxSourcePlaylist";
            this.groupBoxSourcePlaylist.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxSourcePlaylist.Size = new System.Drawing.Size(653, 296);
            this.groupBoxSourcePlaylist.TabIndex = 21;
            this.groupBoxSourcePlaylist.TabStop = false;
            this.groupBoxSourcePlaylist.Text = "Source Playlist:";
            // 
            // buttonAddAll
            // 
            this.buttonAddAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddAll.Enabled = false;
            this.buttonAddAll.Location = new System.Drawing.Point(330, 262);
            this.buttonAddAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAddAll.Name = "buttonAddAll";
            this.buttonAddAll.Size = new System.Drawing.Size(88, 27);
            this.buttonAddAll.TabIndex = 23;
            this.buttonAddAll.Text = "Add All";
            this.buttonAddAll.UseVisualStyleBackColor = true;
            this.buttonAddAll.Click += new System.EventHandler(this.buttonAddAll_Click);
            // 
            // splitContainerInner
            // 
            this.splitContainerInner.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerInner.Location = new System.Drawing.Point(7, 22);
            this.splitContainerInner.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainerInner.Name = "splitContainerInner";
            this.splitContainerInner.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerInner.Panel1
            // 
            this.splitContainerInner.Panel1.Controls.Add(this.listViewPlaylistFiles);
            // 
            // splitContainerInner.Panel2
            // 
            this.splitContainerInner.Panel2.Controls.Add(this.listViewStreamFiles);
            this.splitContainerInner.Size = new System.Drawing.Size(639, 233);
            this.splitContainerInner.SplitterDistance = 114;
            this.splitContainerInner.SplitterWidth = 5;
            this.splitContainerInner.TabIndex = 22;
            // 
            // listViewPlaylistFiles
            // 
            this.listViewPlaylistFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPlaylistName,
            this.columnHeaderPlaylistLength,
            this.columnHeaderPlaylistEstimatedBytes,
            this.columnHeaderPlaylistMeasuredBytes});
            this.listViewPlaylistFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPlaylistFiles.FullRowSelect = true;
            this.listViewPlaylistFiles.HideSelection = false;
            this.listViewPlaylistFiles.Location = new System.Drawing.Point(0, 0);
            this.listViewPlaylistFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listViewPlaylistFiles.MultiSelect = false;
            this.listViewPlaylistFiles.Name = "listViewPlaylistFiles";
            this.listViewPlaylistFiles.Size = new System.Drawing.Size(639, 114);
            this.listViewPlaylistFiles.TabIndex = 2;
            this.listViewPlaylistFiles.UseCompatibleStateImageBehavior = false;
            this.listViewPlaylistFiles.View = System.Windows.Forms.View.Details;
            this.listViewPlaylistFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewPlaylistFiles_ColumnClick);
            this.listViewPlaylistFiles.SelectedIndexChanged += new System.EventHandler(this.listViewPlaylistFiles_SelectedIndexChanged);
            // 
            // columnHeaderPlaylistName
            // 
            this.columnHeaderPlaylistName.Text = "Playlist File";
            this.columnHeaderPlaylistName.Width = 103;
            // 
            // columnHeaderPlaylistLength
            // 
            this.columnHeaderPlaylistLength.Text = "Length";
            this.columnHeaderPlaylistLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderPlaylistLength.Width = 73;
            // 
            // columnHeaderPlaylistEstimatedBytes
            // 
            this.columnHeaderPlaylistEstimatedBytes.Text = "Estimated Size";
            this.columnHeaderPlaylistEstimatedBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderPlaylistEstimatedBytes.Width = 98;
            // 
            // columnHeaderPlaylistMeasuredBytes
            // 
            this.columnHeaderPlaylistMeasuredBytes.Text = "Measured Size";
            this.columnHeaderPlaylistMeasuredBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderPlaylistMeasuredBytes.Width = 125;
            // 
            // listViewStreamFiles
            // 
            this.listViewStreamFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listViewStreamFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName,
            this.columnHeaderFileLength,
            this.columnHeaderFileEstimatedBytes,
            this.columnHeaderFileMeasuredBytes});
            this.listViewStreamFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewStreamFiles.FullRowSelect = true;
            this.listViewStreamFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewStreamFiles.HideSelection = false;
            this.listViewStreamFiles.Location = new System.Drawing.Point(0, 0);
            this.listViewStreamFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listViewStreamFiles.MultiSelect = false;
            this.listViewStreamFiles.Name = "listViewStreamFiles";
            this.listViewStreamFiles.Size = new System.Drawing.Size(639, 114);
            this.listViewStreamFiles.TabIndex = 3;
            this.listViewStreamFiles.UseCompatibleStateImageBehavior = false;
            this.listViewStreamFiles.View = System.Windows.Forms.View.Details;
            this.listViewStreamFiles.SelectedIndexChanged += new System.EventHandler(this.listViewStreamFiles_SelectedIndexChanged);
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "Stream File";
            this.columnHeaderFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileName.Width = 82;
            // 
            // columnHeaderFileLength
            // 
            this.columnHeaderFileLength.Text = "Length";
            this.columnHeaderFileLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileLength.Width = 77;
            // 
            // columnHeaderFileEstimatedBytes
            // 
            this.columnHeaderFileEstimatedBytes.Text = "Estimated Size";
            this.columnHeaderFileEstimatedBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileEstimatedBytes.Width = 119;
            // 
            // columnHeaderFileMeasuredBytes
            // 
            this.columnHeaderFileMeasuredBytes.Text = "Measured Size";
            this.columnHeaderFileMeasuredBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileMeasuredBytes.Width = 125;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Enabled = false;
            this.buttonAdd.Location = new System.Drawing.Point(236, 262);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(88, 27);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // groupBoxCustomPlaylist
            // 
            this.groupBoxCustomPlaylist.Controls.Add(this.buttonDown);
            this.groupBoxCustomPlaylist.Controls.Add(this.buttonUp);
            this.groupBoxCustomPlaylist.Controls.Add(this.buttonRemove);
            this.groupBoxCustomPlaylist.Controls.Add(this.listViewTargetFiles);
            this.groupBoxCustomPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCustomPlaylist.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCustomPlaylist.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxCustomPlaylist.Name = "groupBoxCustomPlaylist";
            this.groupBoxCustomPlaylist.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxCustomPlaylist.Size = new System.Drawing.Size(653, 177);
            this.groupBoxCustomPlaylist.TabIndex = 20;
            this.groupBoxCustomPlaylist.TabStop = false;
            this.groupBoxCustomPlaylist.Text = "Custom Playlist:";
            // 
            // buttonDown
            // 
            this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDown.Enabled = false;
            this.buttonDown.Location = new System.Drawing.Point(102, 141);
            this.buttonDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(88, 27);
            this.buttonDown.TabIndex = 7;
            this.buttonDown.Text = "Move Down";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonUp
            // 
            this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUp.Enabled = false;
            this.buttonUp.Location = new System.Drawing.Point(7, 141);
            this.buttonUp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(88, 27);
            this.buttonUp.TabIndex = 6;
            this.buttonUp.Text = "Move Up";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Location = new System.Drawing.Point(284, 141);
            this.buttonRemove.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(88, 27);
            this.buttonRemove.TabIndex = 8;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listViewTargetFiles
            // 
            this.listViewTargetFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listViewTargetFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTargetFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewTargetFiles.FullRowSelect = true;
            this.listViewTargetFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewTargetFiles.HideSelection = false;
            this.listViewTargetFiles.Location = new System.Drawing.Point(7, 22);
            this.listViewTargetFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listViewTargetFiles.MultiSelect = false;
            this.listViewTargetFiles.Name = "listViewTargetFiles";
            this.listViewTargetFiles.Size = new System.Drawing.Size(639, 111);
            this.listViewTargetFiles.TabIndex = 5;
            this.listViewTargetFiles.UseCompatibleStateImageBehavior = false;
            this.listViewTargetFiles.View = System.Windows.Forms.View.Details;
            this.listViewTargetFiles.SelectedIndexChanged += new System.EventHandler(this.listViewTargetFiles_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Stream File";
            this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader1.Width = 82;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Length";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 77;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Estimated Size";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader3.Width = 119;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Measured Size";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader4.Width = 125;
            // 
            // checkBoxFilterIncompatible
            // 
            this.checkBoxFilterIncompatible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFilterIncompatible.AutoSize = true;
            this.checkBoxFilterIncompatible.Checked = true;
            this.checkBoxFilterIncompatible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFilterIncompatible.Location = new System.Drawing.Point(490, 18);
            this.checkBoxFilterIncompatible.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxFilterIncompatible.Name = "checkBoxFilterIncompatible";
            this.checkBoxFilterIncompatible.Size = new System.Drawing.Size(170, 19);
            this.checkBoxFilterIncompatible.TabIndex = 22;
            this.checkBoxFilterIncompatible.Text = "Filter incombatible playlists";
            this.checkBoxFilterIncompatible.UseVisualStyleBackColor = true;
            this.checkBoxFilterIncompatible.CheckedChanged += new System.EventHandler(this.checkBoxFilterIncompatible_CheckedChanged);
            // 
            // FormPlaylist
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(681, 568);
            this.Controls.Add(this.checkBoxFilterIncompatible);
            this.Controls.Add(this.splitContainerOuter);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxName);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormPlaylist";
            this.Text = "BDInfo Custom Playlist";
            this.Load += new System.EventHandler(this.FormPlaylist_Load);
            this.Resize += new System.EventHandler(this.FormPlaylist_Resize);
            this.splitContainerOuter.Panel1.ResumeLayout(false);
            this.splitContainerOuter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).EndInit();
            this.splitContainerOuter.ResumeLayout(false);
            this.groupBoxSourcePlaylist.ResumeLayout(false);
            this.splitContainerInner.Panel1.ResumeLayout(false);
            this.splitContainerInner.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInner)).EndInit();
            this.splitContainerInner.ResumeLayout(false);
            this.groupBoxCustomPlaylist.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.SplitContainer splitContainerOuter;
        private System.Windows.Forms.GroupBox groupBoxSourcePlaylist;
        private System.Windows.Forms.SplitContainer splitContainerInner;
        private System.Windows.Forms.ListView listViewPlaylistFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistName;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistLength;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistEstimatedBytes;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistMeasuredBytes;
        private System.Windows.Forms.ListView listViewStreamFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderFileLength;
        private System.Windows.Forms.ColumnHeader columnHeaderFileEstimatedBytes;
        private System.Windows.Forms.ColumnHeader columnHeaderFileMeasuredBytes;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.GroupBox groupBoxCustomPlaylist;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListView listViewTargetFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button buttonAddAll;
        private System.Windows.Forms.CheckBox checkBoxFilterIncompatible;
    }
}
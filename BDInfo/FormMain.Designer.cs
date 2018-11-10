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

namespace BDInfo
{
    partial class FormMain
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
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.labelSource = new System.Windows.Forms.Label();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.labelTimeRemainingElapsed = new System.Windows.Forms.Label();
            this.labelTimeDivider = new System.Windows.Forms.Label();
            this.labelTimeElapsed = new System.Windows.Forms.Label();
            this.labelTimeRemaining = new System.Windows.Forms.Label();
            this.progressBarScan = new System.Windows.Forms.ProgressBar();
            this.labelPlaylistFiles = new System.Windows.Forms.Label();
            this.buttonViewReport = new System.Windows.Forms.Button();
            this.buttonScan = new System.Windows.Forms.Button();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.labelProgress = new System.Windows.Forms.Label();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonUnselectAll = new System.Windows.Forms.Button();
            this.buttonCustomPlaylist = new System.Windows.Forms.Button();
            this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
            this.splitContainerInner = new System.Windows.Forms.SplitContainer();
            this.listViewPlaylistFiles = new System.Windows.Forms.ListView();
            this.columnHeaderPlaylistName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistGroup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistEstimatedBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistMeasuredBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewStreamFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileEstimatedBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileMeasuredBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewStreams = new System.Windows.Forms.ListView();
            this.columnHeaderStreamCodec = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStreamLanguage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBitrate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRescan = new System.Windows.Forms.Button();
            this.buttonIsoBrowse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).BeginInit();
            this.splitContainerOuter.Panel1.SuspendLayout();
            this.splitContainerOuter.Panel2.SuspendLayout();
            this.splitContainerOuter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInner)).BeginInit();
            this.splitContainerInner.Panel1.SuspendLayout();
            this.splitContainerInner.Panel2.SuspendLayout();
            this.splitContainerInner.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(598, 22);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(62, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBoxSource
            // 
            this.textBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSource.Location = new System.Drawing.Point(17, 24);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(575, 20);
            this.textBoxSource.TabIndex = 0;
            this.textBoxSource.TextChanged += new System.EventHandler(this.textBoxSource_TextChanged);
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(14, 8);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(141, 13);
            this.labelSource.TabIndex = 3;
            this.labelSource.Text = "Select the Source BD-ROM:";
            // 
            // buttonSettings
            // 
            this.buttonSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSettings.Location = new System.Drawing.Point(663, 548);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(108, 23);
            this.buttonSettings.TabIndex = 13;
            this.buttonSettings.Text = "Settings...";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.buttonSettings_Click);
            // 
            // labelTimeRemainingElapsed
            // 
            this.labelTimeRemainingElapsed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTimeRemainingElapsed.AutoSize = true;
            this.labelTimeRemainingElapsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeRemainingElapsed.Location = new System.Drawing.Point(510, 503);
            this.labelTimeRemainingElapsed.Name = "labelTimeRemainingElapsed";
            this.labelTimeRemainingElapsed.Size = new System.Drawing.Size(135, 13);
            this.labelTimeRemainingElapsed.TabIndex = 35;
            this.labelTimeRemainingElapsed.Text = "Time Remaining / Elapsed:";
            // 
            // labelTimeDivider
            // 
            this.labelTimeDivider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTimeDivider.AutoSize = true;
            this.labelTimeDivider.Location = new System.Drawing.Point(703, 503);
            this.labelTimeDivider.Name = "labelTimeDivider";
            this.labelTimeDivider.Size = new System.Drawing.Size(12, 13);
            this.labelTimeDivider.TabIndex = 34;
            this.labelTimeDivider.Text = "/";
            // 
            // labelTimeElapsed
            // 
            this.labelTimeElapsed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTimeElapsed.AutoSize = true;
            this.labelTimeElapsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeElapsed.Location = new System.Drawing.Point(721, 503);
            this.labelTimeElapsed.Name = "labelTimeElapsed";
            this.labelTimeElapsed.Size = new System.Drawing.Size(49, 13);
            this.labelTimeElapsed.TabIndex = 33;
            this.labelTimeElapsed.Text = "00:00:00";
            // 
            // labelTimeRemaining
            // 
            this.labelTimeRemaining.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTimeRemaining.AutoSize = true;
            this.labelTimeRemaining.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeRemaining.Location = new System.Drawing.Point(651, 503);
            this.labelTimeRemaining.Name = "labelTimeRemaining";
            this.labelTimeRemaining.Size = new System.Drawing.Size(49, 13);
            this.labelTimeRemaining.TabIndex = 32;
            this.labelTimeRemaining.Text = "00:00:00";
            // 
            // progressBarScan
            // 
            this.progressBarScan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarScan.Location = new System.Drawing.Point(17, 519);
            this.progressBarScan.Name = "progressBarScan";
            this.progressBarScan.Size = new System.Drawing.Size(754, 23);
            this.progressBarScan.TabIndex = 31;
            // 
            // labelPlaylistFiles
            // 
            this.labelPlaylistFiles.AutoSize = true;
            this.labelPlaylistFiles.Location = new System.Drawing.Point(14, 55);
            this.labelPlaylistFiles.Name = "labelPlaylistFiles";
            this.labelPlaylistFiles.Size = new System.Drawing.Size(86, 13);
            this.labelPlaylistFiles.TabIndex = 28;
            this.labelPlaylistFiles.Text = "Select Playlist(s):";
            // 
            // buttonViewReport
            // 
            this.buttonViewReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewReport.Enabled = false;
            this.buttonViewReport.Location = new System.Drawing.Point(485, 548);
            this.buttonViewReport.Name = "buttonViewReport";
            this.buttonViewReport.Size = new System.Drawing.Size(108, 23);
            this.buttonViewReport.TabIndex = 12;
            this.buttonViewReport.Text = "View Report...";
            this.buttonViewReport.UseVisualStyleBackColor = true;
            this.buttonViewReport.Click += new System.EventHandler(this.buttonViewReport_Click);
            // 
            // buttonScan
            // 
            this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScan.Enabled = false;
            this.buttonScan.Location = new System.Drawing.Point(368, 548);
            this.buttonScan.Name = "buttonScan";
            this.buttonScan.Size = new System.Drawing.Size(108, 23);
            this.buttonScan.TabIndex = 11;
            this.buttonScan.Text = "Scan Bitrates";
            this.buttonScan.UseVisualStyleBackColor = true;
            this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
            // 
            // textBoxDetails
            // 
            this.textBoxDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDetails.Enabled = false;
            this.textBoxDetails.Location = new System.Drawing.Point(16, 438);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.ReadOnly = true;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDetails.Size = new System.Drawing.Size(754, 59);
            this.textBoxDetails.TabIndex = 10;
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(14, 503);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(0, 13);
            this.labelProgress.TabIndex = 37;
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Enabled = false;
            this.buttonSelectAll.Location = new System.Drawing.Point(106, 50);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 4;
            this.buttonSelectAll.Text = "Select All";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonUnselectAll
            // 
            this.buttonUnselectAll.Enabled = false;
            this.buttonUnselectAll.Location = new System.Drawing.Point(187, 50);
            this.buttonUnselectAll.Name = "buttonUnselectAll";
            this.buttonUnselectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonUnselectAll.TabIndex = 5;
            this.buttonUnselectAll.Text = "Unselect All";
            this.buttonUnselectAll.UseVisualStyleBackColor = true;
            this.buttonUnselectAll.Click += new System.EventHandler(this.buttonUnselectAll_Click);
            // 
            // buttonCustomPlaylist
            // 
            this.buttonCustomPlaylist.Enabled = false;
            this.buttonCustomPlaylist.Location = new System.Drawing.Point(268, 50);
            this.buttonCustomPlaylist.Name = "buttonCustomPlaylist";
            this.buttonCustomPlaylist.Size = new System.Drawing.Size(75, 23);
            this.buttonCustomPlaylist.TabIndex = 6;
            this.buttonCustomPlaylist.Text = "Custom...";
            this.buttonCustomPlaylist.UseVisualStyleBackColor = true;
            this.buttonCustomPlaylist.Click += new System.EventHandler(this.buttonCustomPlaylist_Click);
            // 
            // splitContainerOuter
            // 
            this.splitContainerOuter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerOuter.Location = new System.Drawing.Point(17, 79);
            this.splitContainerOuter.Name = "splitContainerOuter";
            this.splitContainerOuter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerOuter.Panel1
            // 
            this.splitContainerOuter.Panel1.Controls.Add(this.splitContainerInner);
            // 
            // splitContainerOuter.Panel2
            // 
            this.splitContainerOuter.Panel2.Controls.Add(this.listViewStreams);
            this.splitContainerOuter.Size = new System.Drawing.Size(753, 353);
            this.splitContainerOuter.SplitterDistance = 199;
            this.splitContainerOuter.TabIndex = 38;
            // 
            // splitContainerInner
            // 
            this.splitContainerInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInner.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainerInner.Size = new System.Drawing.Size(753, 199);
            this.splitContainerInner.SplitterDistance = 97;
            this.splitContainerInner.TabIndex = 0;
            // 
            // listViewPlaylistFiles
            // 
            this.listViewPlaylistFiles.CheckBoxes = true;
            this.listViewPlaylistFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPlaylistName,
            this.columnHeaderPlaylistGroup,
            this.columnHeaderPlaylistLength,
            this.columnHeaderPlaylistEstimatedBytes,
            this.columnHeaderPlaylistMeasuredBytes});
            this.listViewPlaylistFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPlaylistFiles.Enabled = false;
            this.listViewPlaylistFiles.FullRowSelect = true;
            this.listViewPlaylistFiles.HideSelection = false;
            this.listViewPlaylistFiles.Location = new System.Drawing.Point(0, 0);
            this.listViewPlaylistFiles.MultiSelect = false;
            this.listViewPlaylistFiles.Name = "listViewPlaylistFiles";
            this.listViewPlaylistFiles.Size = new System.Drawing.Size(753, 97);
            this.listViewPlaylistFiles.TabIndex = 7;
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
            // columnHeaderPlaylistGroup
            // 
            this.columnHeaderPlaylistGroup.Text = "Group";
            // 
            // columnHeaderPlaylistLength
            // 
            this.columnHeaderPlaylistLength.Text = "Length";
            this.columnHeaderPlaylistLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderPlaylistLength.Width = 73;
            // 
            // columnHeaderPlaylistEstimatedBytes
            // 
            this.columnHeaderPlaylistEstimatedBytes.Text = "Estimated Bytes";
            this.columnHeaderPlaylistEstimatedBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderPlaylistEstimatedBytes.Width = 98;
            // 
            // columnHeaderPlaylistMeasuredBytes
            // 
            this.columnHeaderPlaylistMeasuredBytes.Text = "Measured Bytes";
            this.columnHeaderPlaylistMeasuredBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderPlaylistMeasuredBytes.Width = 125;
            // 
            // listViewStreamFiles
            // 
            this.listViewStreamFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listViewStreamFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName,
            this.columnHeaderIndex,
            this.columnHeaderFileLength,
            this.columnHeaderFileEstimatedBytes,
            this.columnHeaderFileMeasuredBytes});
            this.listViewStreamFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewStreamFiles.Enabled = false;
            this.listViewStreamFiles.FullRowSelect = true;
            this.listViewStreamFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewStreamFiles.HideSelection = false;
            this.listViewStreamFiles.Location = new System.Drawing.Point(0, 0);
            this.listViewStreamFiles.MultiSelect = false;
            this.listViewStreamFiles.Name = "listViewStreamFiles";
            this.listViewStreamFiles.Size = new System.Drawing.Size(753, 98);
            this.listViewStreamFiles.TabIndex = 8;
            this.listViewStreamFiles.UseCompatibleStateImageBehavior = false;
            this.listViewStreamFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "Stream File";
            this.columnHeaderFileName.Width = 82;
            // 
            // columnHeaderIndex
            // 
            this.columnHeaderIndex.Text = "Index";
            // 
            // columnHeaderFileLength
            // 
            this.columnHeaderFileLength.Text = "Length";
            this.columnHeaderFileLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileLength.Width = 77;
            // 
            // columnHeaderFileEstimatedBytes
            // 
            this.columnHeaderFileEstimatedBytes.Text = "Estimated Bytes";
            this.columnHeaderFileEstimatedBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileEstimatedBytes.Width = 119;
            // 
            // columnHeaderFileMeasuredBytes
            // 
            this.columnHeaderFileMeasuredBytes.Text = "Measured Bytes";
            this.columnHeaderFileMeasuredBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileMeasuredBytes.Width = 125;
            // 
            // listViewStreams
            // 
            this.listViewStreams.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStreamCodec,
            this.columnHeaderStreamLanguage,
            this.columnHeaderBitrate,
            this.columnHeaderDescription});
            this.listViewStreams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewStreams.Enabled = false;
            this.listViewStreams.FullRowSelect = true;
            this.listViewStreams.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewStreams.HideSelection = false;
            this.listViewStreams.Location = new System.Drawing.Point(0, 0);
            this.listViewStreams.MultiSelect = false;
            this.listViewStreams.Name = "listViewStreams";
            this.listViewStreams.Size = new System.Drawing.Size(753, 150);
            this.listViewStreams.TabIndex = 9;
            this.listViewStreams.UseCompatibleStateImageBehavior = false;
            this.listViewStreams.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderStreamCodec
            // 
            this.columnHeaderStreamCodec.Text = "Codec";
            this.columnHeaderStreamCodec.Width = 103;
            // 
            // columnHeaderStreamLanguage
            // 
            this.columnHeaderStreamLanguage.Text = "Language";
            this.columnHeaderStreamLanguage.Width = 151;
            // 
            // columnHeaderBitrate
            // 
            this.columnHeaderBitrate.Text = "Bit Rate";
            this.columnHeaderBitrate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeaderDescription
            // 
            this.columnHeaderDescription.Text = "Description";
            this.columnHeaderDescription.Width = 306;
            // 
            // buttonRescan
            // 
            this.buttonRescan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRescan.Enabled = false;
            this.buttonRescan.Location = new System.Drawing.Point(706, 22);
            this.buttonRescan.Name = "buttonRescan";
            this.buttonRescan.Size = new System.Drawing.Size(66, 23);
            this.buttonRescan.TabIndex = 3;
            this.buttonRescan.Text = "Rescan";
            this.buttonRescan.UseVisualStyleBackColor = true;
            this.buttonRescan.Click += new System.EventHandler(this.buttonRescan_Click);
            // 
            // buttonIsoBrowse
            // 
            this.buttonIsoBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIsoBrowse.Location = new System.Drawing.Point(666, 22);
            this.buttonIsoBrowse.Name = "buttonIsoBrowse";
            this.buttonIsoBrowse.Size = new System.Drawing.Size(34, 23);
            this.buttonIsoBrowse.TabIndex = 2;
            this.buttonIsoBrowse.Text = "ISO";
            this.buttonIsoBrowse.UseVisualStyleBackColor = true;
            this.buttonIsoBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 575);
            this.Controls.Add(this.buttonIsoBrowse);
            this.Controls.Add(this.buttonRescan);
            this.Controls.Add(this.splitContainerOuter);
            this.Controls.Add(this.buttonCustomPlaylist);
            this.Controls.Add(this.buttonUnselectAll);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.buttonSettings);
            this.Controls.Add(this.labelTimeRemainingElapsed);
            this.Controls.Add(this.labelTimeDivider);
            this.Controls.Add(this.labelTimeElapsed);
            this.Controls.Add(this.labelTimeRemaining);
            this.Controls.Add(this.progressBarScan);
            this.Controls.Add(this.labelPlaylistFiles);
            this.Controls.Add(this.buttonViewReport);
            this.Controls.Add(this.buttonScan);
            this.Controls.Add(this.textBoxDetails);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxSource);
            this.Controls.Add(this.labelSource);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "BDInfo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.LocationChanged += new System.EventHandler(this.FormMain_LocationChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormMain_DragEnter);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.splitContainerOuter.Panel1.ResumeLayout(false);
            this.splitContainerOuter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).EndInit();
            this.splitContainerOuter.ResumeLayout(false);
            this.splitContainerInner.Panel1.ResumeLayout(false);
            this.splitContainerInner.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInner)).EndInit();
            this.splitContainerInner.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.Label labelTimeRemainingElapsed;
        private System.Windows.Forms.Label labelTimeDivider;
        private System.Windows.Forms.Label labelTimeElapsed;
        private System.Windows.Forms.Label labelTimeRemaining;
        private System.Windows.Forms.ProgressBar progressBarScan;
        private System.Windows.Forms.Label labelPlaylistFiles;
        private System.Windows.Forms.Button buttonViewReport;
        private System.Windows.Forms.Button buttonScan;
        private System.Windows.Forms.TextBox textBoxDetails;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonUnselectAll;
        private System.Windows.Forms.Button buttonCustomPlaylist;
        private System.Windows.Forms.SplitContainer splitContainerOuter;
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
        private System.Windows.Forms.ListView listViewStreams;
        private System.Windows.Forms.ColumnHeader columnHeaderStreamCodec;
        private System.Windows.Forms.ColumnHeader columnHeaderStreamLanguage;
        private System.Windows.Forms.ColumnHeader columnHeaderBitrate;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.Button buttonRescan;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistGroup;
        private System.Windows.Forms.ColumnHeader columnHeaderIndex;
        private System.Windows.Forms.Button buttonIsoBrowse;
    }
}
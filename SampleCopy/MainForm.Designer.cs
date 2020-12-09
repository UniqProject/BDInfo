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

namespace SampleCopy
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.buttonBrowseSource = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxStreamFile = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxTarget = new System.Windows.Forms.TextBox();
            this.buttonBrowseTarget = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxSampleSize = new System.Windows.Forms.ComboBox();
            this.buttonCreateSample = new System.Windows.Forms.Button();
            this.buttonBrowseISOSource = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Directory:";
            // 
            // textBoxSource
            // 
            this.textBoxSource.Location = new System.Drawing.Point(8, 30);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(424, 23);
            this.textBoxSource.TabIndex = 1;
            // 
            // buttonBrowseSource
            // 
            this.buttonBrowseSource.Location = new System.Drawing.Point(438, 30);
            this.buttonBrowseSource.Name = "buttonBrowseSource";
            this.buttonBrowseSource.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseSource.TabIndex = 2;
            this.buttonBrowseSource.Text = "Browse";
            this.buttonBrowseSource.UseVisualStyleBackColor = true;
            this.buttonBrowseSource.Click += new System.EventHandler(this.buttonBrowseSource_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Stream File:";
            // 
            // comboBoxStreamFile
            // 
            this.comboBoxStreamFile.DisplayMember = "Text";
            this.comboBoxStreamFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStreamFile.FormattingEnabled = true;
            this.comboBoxStreamFile.Location = new System.Drawing.Point(8, 85);
            this.comboBoxStreamFile.Name = "comboBoxStreamFile";
            this.comboBoxStreamFile.Size = new System.Drawing.Size(424, 23);
            this.comboBoxStreamFile.TabIndex = 4;
            this.comboBoxStreamFile.ValueMember = "Value";
            this.comboBoxStreamFile.SelectedIndexChanged += new System.EventHandler(this.comboBoxStreamFile_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Target Directory:";
            // 
            // textBoxTarget
            // 
            this.textBoxTarget.Location = new System.Drawing.Point(8, 146);
            this.textBoxTarget.Name = "textBoxTarget";
            this.textBoxTarget.Size = new System.Drawing.Size(424, 23);
            this.textBoxTarget.TabIndex = 6;
            // 
            // buttonBrowseTarget
            // 
            this.buttonBrowseTarget.Location = new System.Drawing.Point(438, 146);
            this.buttonBrowseTarget.Name = "buttonBrowseTarget";
            this.buttonBrowseTarget.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseTarget.TabIndex = 7;
            this.buttonBrowseTarget.Text = "Browse";
            this.buttonBrowseTarget.UseVisualStyleBackColor = true;
            this.buttonBrowseTarget.Click += new System.EventHandler(this.buttonBrowseTarget_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Sample Size";
            // 
            // comboBoxSampleSize
            // 
            this.comboBoxSampleSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSampleSize.FormattingEnabled = true;
            this.comboBoxSampleSize.Items.AddRange(new object[] {
            "100 MB",
            "200 MB",
            "300 MB"});
            this.comboBoxSampleSize.Location = new System.Drawing.Point(8, 203);
            this.comboBoxSampleSize.Name = "comboBoxSampleSize";
            this.comboBoxSampleSize.Size = new System.Drawing.Size(154, 23);
            this.comboBoxSampleSize.TabIndex = 9;
            this.comboBoxSampleSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSampleSize_SelectedIndexChanged);
            // 
            // buttonCreateSample
            // 
            this.buttonCreateSample.Location = new System.Drawing.Point(188, 203);
            this.buttonCreateSample.Name = "buttonCreateSample";
            this.buttonCreateSample.Size = new System.Drawing.Size(244, 23);
            this.buttonCreateSample.TabIndex = 10;
            this.buttonCreateSample.Text = "Create Sample";
            this.buttonCreateSample.UseVisualStyleBackColor = true;
            this.buttonCreateSample.Click += new System.EventHandler(this.buttonCreateSample_Click);
            // 
            // buttonBrowseISOSource
            // 
            this.buttonBrowseISOSource.Location = new System.Drawing.Point(438, 59);
            this.buttonBrowseISOSource.Name = "buttonBrowseISOSource";
            this.buttonBrowseISOSource.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseISOSource.TabIndex = 11;
            this.buttonBrowseISOSource.Text = "ISO";
            this.buttonBrowseISOSource.UseVisualStyleBackColor = true;
            this.buttonBrowseISOSource.Click += new System.EventHandler(this.buttonBrowseSource_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(526, 261);
            this.Controls.Add(this.buttonBrowseISOSource);
            this.Controls.Add(this.buttonCreateSample);
            this.Controls.Add(this.comboBoxSampleSize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonBrowseTarget);
            this.Controls.Add(this.textBoxTarget);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxStreamFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonBrowseSource);
            this.Controls.Add(this.textBoxSource);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "BDInfo Sample Copy";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Button buttonBrowseSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxStreamFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxTarget;
        private System.Windows.Forms.Button buttonBrowseTarget;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxSampleSize;
        private System.Windows.Forms.Button buttonCreateSample;
        private System.Windows.Forms.Button buttonBrowseISOSource;
    }
}


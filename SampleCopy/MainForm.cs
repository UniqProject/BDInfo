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

using BDInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleCopy
{
    public partial class MainForm : Form
    {
        private class CopyFileState
        {
            public long TotalBytes = 0;
            public long FinishedBytes = 0;
            public DateTime TimeStarted = DateTime.Now;
            public string FileCopy = "";
            public Dictionary<BDInfo.IO.IFileInfo, string> FileMap =
                new Dictionary<BDInfo.IO.IFileInfo, string>();
            public Exception Exception = null;
        }

        private string _selectedStream = "";
        private long _sampleSize = 0;

        public MainForm()
        {
            InitializeComponent();
            ValidateButtonCreate();
            comboBoxSampleSize.SelectedIndex = 0;
        }

        private void buttonBrowseSource_Click(object sender, EventArgs e)
        {
            string path = "";

            if (((Button)sender).Name == "buttonBrowseSource")
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select a BluRay BDMV Folder:";
#if NETCOREAPP3_1
                    dialog.UseDescriptionForTitle = true;
#endif
                    if (!string.IsNullOrEmpty(textBoxSource.Text))
                    {
                        dialog.SelectedPath = textBoxSource.Text;
                    }
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.SelectedPath;
                    }
                }
            }
            else
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Select a BluRay .ISO file:";
                    dialog.Filter = "ISO-Image|*.iso";
                    dialog.RestoreDirectory = true;
                    if (!string.IsNullOrEmpty(textBoxSource.Text))
                    {
                        dialog.InitialDirectory = textBoxSource.Text;
                    }
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.FileName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(path))
            {
                textBoxSource.Text = path;
                InitBDROM(path);
            }

            ValidateButtonCreate();
        }

        private void buttonBrowseTarget_Click(object sender, EventArgs e)
        {
            string path = "";
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select target Folder:";
#if NETCOREAPP3_1
                dialog.UseDescriptionForTitle = true;
#endif
                if (!string.IsNullOrEmpty(textBoxTarget.Text))
                {
                    dialog.SelectedPath = textBoxTarget.Text;
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                }
            }
            if (!string.IsNullOrEmpty(path))
            {
                textBoxTarget.Text = path;
            }

            ValidateButtonCreate();
        }

        private void comboBoxStreamFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedStream = (string)comboBoxStreamFile.SelectedValue;
            ValidateButtonCreate();
        }

        private void comboBoxSampleSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _sampleSize = (comboBoxSampleSize.SelectedIndex + 1) * 100 * (int)Math.Pow(1024, 2);
            ValidateButtonCreate();
        }

        private BackgroundWorker copyWorker = null;

        private async void buttonCreateSample_Click(object sender, EventArgs e)
        {
            if (BDROM == null) return;

            string targetPath = Path.Combine(textBoxTarget.Text, BDROM.VolumeLabel);
            DirectoryInfo target = new DirectoryInfo(targetPath);
            if (target.Exists)
            if (MessageBox.Show("This will delete all files in the target directory! Do you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.ServiceNotification) != DialogResult.Yes) return;

            buttonBrowseSource.Enabled = false;
            buttonBrowseISOSource.Enabled = false;
            buttonBrowseTarget.Enabled = false;
            textBoxSource.Enabled = false;
            textBoxTarget.Enabled = false;
            comboBoxSampleSize.Enabled = false;
            comboBoxStreamFile.Enabled = false;
            buttonCreateSample.Text = "Copying";
            buttonCreateSample.Enabled = false;
            
            try
            {
                if (target.Exists)
                {
                    foreach (var item in target.GetFiles("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch
                        {

                        }
                    }
                    foreach (var item in target.GetDirectories("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch
                        {

                        }
                    }
                }

                CopyFileState copyState = new CopyFileState();

                if (BDROM.DirectorySSIF != null)
                {
                    foreach (var file in BDROM.DirectorySSIF?.GetFiles(_selectedStream.Replace("M2TS","SSIF")))
                    {
                        copyState.TotalBytes += file.Length > _sampleSize ? _sampleSize : file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                if (BDROM.DirectorySTREAM != null)
                {
                    foreach (var file in BDROM.DirectorySTREAM?.GetFiles(_selectedStream))
                    {
                        copyState.TotalBytes += file.Length > _sampleSize ? _sampleSize : file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                if (BDROM.DirectoryBDMV != null)
                {
                    foreach (var file in BDROM.DirectoryBDMV?.GetFiles())
                    {
                        copyState.TotalBytes += file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                if (BDROM.DirectoryBDJO != null)
                {
                    foreach (var file in BDROM.DirectoryBDJO?.GetFiles())
                    {
                        copyState.TotalBytes += file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                if (BDROM.DirectoryCLIPINF != null)
                {
                    foreach (var file in BDROM.DirectoryCLIPINF?.GetFiles())
                    {
                        copyState.TotalBytes += file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                if (BDROM.DirectoryMETA != null)
                {
                    foreach (var file in BDROM.DirectoryMETA?.GetFiles())
                    {
                        copyState.TotalBytes += file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                if (BDROM.DirectoryPLAYLIST != null)
                {
                    foreach (var file in BDROM.DirectoryPLAYLIST?.GetFiles())
                    {
                        copyState.TotalBytes += file.Length;
                        copyState.FileMap.Add(file, file.IsImage ? Path.Combine(targetPath, BDROM.VolumeLabel, file.FullName) : file.FullName.Replace(textBoxSource.Text, targetPath));
                    }
                }

                foreach (var file in copyState.FileMap)
                {
                    using (var fileIn = file.Key.OpenRead())
                    {
                        var fileOut = new FileInfo(file.Value);
                        if (!fileOut.Directory.Exists)
                        {
                            fileOut.Directory.Create();
                        }
                        var cancelToken = new CancellationToken();
                        await CopyFile(fileIn, fileOut, _sampleSize, cancelToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ValidateButtonCreate();

            buttonBrowseSource.Enabled = true;
            buttonBrowseISOSource.Enabled = true;
            buttonBrowseTarget.Enabled = true;
            textBoxSource.Enabled = true;
            textBoxTarget.Enabled = true;
            comboBoxSampleSize.Enabled = true;
            comboBoxStreamFile.Enabled = true;
        }

        private void ValidateButtonCreate()
        {
            buttonCreateSample.Enabled = (!string.IsNullOrEmpty(textBoxSource.Text) &&
                                          !string.IsNullOrEmpty(textBoxTarget.Text) &&
                                          comboBoxStreamFile.SelectedIndex > -1 &&
                                          comboBoxSampleSize.SelectedIndex > -1);
            buttonCreateSample.Text = "Create Sample";
        }

        private BackgroundWorker InitBDROMWorker = null;
        private BDROM BDROM = null;

        private void InitBDROM(string path)
        {
            textBoxSource.Enabled = false;
            buttonBrowseSource.Enabled = false;
            buttonBrowseSource.Text = "Scanning...";
            buttonBrowseISOSource.Enabled = false;
            comboBoxStreamFile.Enabled = false;
            textBoxTarget.Enabled = false;
            buttonBrowseTarget.Enabled = false;
            comboBoxSampleSize.Enabled = false;
            buttonCreateSample.Enabled = false;

            InitBDROMWorker = new BackgroundWorker();
            InitBDROMWorker.WorkerReportsProgress = true;
            InitBDROMWorker.WorkerSupportsCancellation = true;
            InitBDROMWorker.DoWork += InitBDROMWork;
            InitBDROMWorker.ProgressChanged += InitBDROMProgress;
            InitBDROMWorker.RunWorkerCompleted += InitBDROMCompleted;
            InitBDROMWorker.RunWorkerAsync(path);
        }

        private void InitBDROMWork(object sender,
                                   DoWorkEventArgs e)
        {
            try
            {
                BDROM = new BDROM((string)e.Argument);

                BDROM.StreamClipFileScanError += new BDROM.OnStreamClipFileScanError(BDROM_StreamClipFileScanError);
                BDROM.StreamFileScanError += new BDROM.OnStreamFileScanError(BDROM_StreamFileScanError);
                BDROM.PlaylistFileScanError += new BDROM.OnPlaylistFileScanError(BDROM_PlaylistFileScanError);
                BDROM.Scan();
                e.Result = null;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        protected bool BDROM_PlaylistFileScanError(TSPlaylistFile playlistFile,
                                           Exception ex)
        {
            DialogResult result = MessageBox.Show(string.Format(CultureInfo.InvariantCulture,
                "An error occurred while scanning the playlist file {0}.\n\nThe disc may be copy-protected or damaged.\n\nDo you want to continue scanning the playlist files?", playlistFile.Name),
                "BDInfo Scan Error", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) return true;
            else return false;
        }

        protected bool BDROM_StreamFileScanError(TSStreamFile streamFile,
                                                 Exception ex)
        {
            DialogResult result = MessageBox.Show(string.Format(CultureInfo.InvariantCulture,
                "An error occurred while scanning the stream file {0}.\n\nThe disc may be copy-protected or damaged.\n\nDo you want to continue scanning the stream files?", streamFile.Name),
                "BDInfo Scan Error", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) return true;
            else return false;
        }

        protected bool BDROM_StreamClipFileScanError(TSStreamClipFile streamClipFile,
                                                     Exception ex)
        {
            DialogResult result = MessageBox.Show(string.Format(CultureInfo.InvariantCulture,
                "An error occurred while scanning the stream clip file {0}.\n\nThe disc may be copy-protected or damaged.\n\nDo you want to continue scanning the stream clip files?", streamClipFile.Name),
                "BDInfo Scan Error", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) return true;
            else return false;
        }

        private void InitBDROMProgress(object sender,
                               ProgressChangedEventArgs e)
        {
        }

        private void InitBDROMCompleted(object sender,
                                RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture,
                                            "{0}", ((Exception)e.Result).Message);

                MessageBox.Show(msg, "BDInfo Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonBrowseSource.Enabled = true;
                return;
            }

            if (BDROM == null) return;
            comboBoxStreamFile.DisplayMember = "Text";
            comboBoxStreamFile.ValueMember = "Value";

            List<Object> items = new List<Object>();

            foreach (var stream in BDROM.StreamFiles)
            {
                items.Add ( new { Text = $"\"{stream.Value.Name}\"   ({ToolBox.FormatFileSize(stream.Value.FileInfo.Length, true)})", Value = stream.Key });
            }

            comboBoxStreamFile.DataSource = items;

            textBoxSource.Enabled = true;
            buttonBrowseSource.Enabled = true;
            buttonBrowseSource.Text = "Browse";
            buttonBrowseISOSource.Enabled = true;
            comboBoxStreamFile.Enabled = true;
            textBoxTarget.Enabled = true;
            buttonBrowseTarget.Enabled = true;
            comboBoxSampleSize.Enabled = true;

            ValidateButtonCreate();
        }

        public static async Task CopyFile(Stream fileIn, FileInfo fileOut, long maxSize, CancellationToken cancellationToken)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var bufferSize = 81920;

            using (var sourceStream = fileIn)
            {
                using (var destinationStream = new FileStream(fileOut.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions))
                {
                    long bytesLeft = fileIn.Length < maxSize ? fileIn.Length : maxSize;
                    long bytesCopied = 0;
                    byte[] buffer = new byte[bufferSize];
                    while (bytesCopied < bytesLeft)
                    {
                        int count = bytesLeft > bufferSize ? bufferSize : (int)bytesLeft;
                        var read = await sourceStream.ReadAsync(buffer, 0, count, cancellationToken);
                        await destinationStream.WriteAsync(buffer, 0, read);
                        bytesLeft -= read;
                    }
                }
            }
        }

    }
}

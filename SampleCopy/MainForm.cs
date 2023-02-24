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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BDInfoLib.BDROM;

namespace SampleCopy;

public partial class MainForm : Form
{
    private class CopyFileState
    {
        public long TotalBytes = 0;
        public long FinishedBytes = 0;
        public DateTime TimeStarted = DateTime.Now;
        public string FileCopy = "";
        public Dictionary<BDInfoLib.BDROM.IO.IFileInfo, string> FileMap = new();
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
        var path = "";

        if (((Button)sender).Name == "buttonBrowseSource")
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = @"Select a BluRay BDMV Folder:";

#if NETCOREAPP3_1_OR_GREATER
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
        else
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = @"Select a BluRay .ISO file:";
            dialog.Filter = @"ISO-Image|*.iso";
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
        if (!string.IsNullOrEmpty(path))
        {
            textBoxSource.Text = path;
            InitBDROM(path);
        }

        ValidateButtonCreate();
    }

    private void buttonBrowseTarget_Click(object sender, EventArgs e)
    {
        var path = "";
        using var dialog = new FolderBrowserDialog();
        dialog.Description = @"Select target Folder:";

#if NETCOREAPP3_1_OR_GREATER
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

    private async void buttonCreateSample_Click(object sender, EventArgs e)
    {
        if (_bdrom == null) return;

        var targetPath = Path.Combine(textBoxTarget.Text, _bdrom.VolumeLabel);
        System.IO.DirectoryInfo target = new(targetPath);
        if (target.Exists)
            if (MessageBox.Show(@"This will delete all files in the target directory! Do you want to continue?",
                    @"Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2,
                    MessageBoxOptions.ServiceNotification) != DialogResult.Yes)
                return;

        buttonBrowseSource.Enabled = false;
        buttonBrowseISOSource.Enabled = false;
        buttonBrowseTarget.Enabled = false;
        textBoxSource.Enabled = false;
        textBoxTarget.Enabled = false;
        comboBoxSampleSize.Enabled = false;
        comboBoxStreamFile.Enabled = false;
        buttonCreateSample.Text = @"Copying";
        buttonCreateSample.Enabled = false;
            
        try
        {
            if (target.Exists)
            {
                var files = target.GetFiles("*", SearchOption.AllDirectories);
                foreach (var item in files)
                {
                    try
                    {
                        item.Delete();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                var directories = target.GetDirectories("*", SearchOption.AllDirectories);
                Array.Reverse(directories);

                foreach (var item in directories)
                {
                    try
                    {
                        item.Delete();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            CopyFileState copyState = new();

            if (_bdrom.DirectorySSIF != null)
            {
                foreach (var file in _bdrom.DirectorySSIF?.GetFiles(_selectedStream.Replace("M2TS","SSIF")))
                {
                    copyState.TotalBytes += file.Length > _sampleSize ? _sampleSize : file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            if (_bdrom.DirectorySTREAM != null)
            {
                foreach (var file in _bdrom.DirectorySTREAM?.GetFiles(_selectedStream))
                {
                    copyState.TotalBytes += file.Length > _sampleSize ? _sampleSize : file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            if (_bdrom.DirectoryBDMV != null)
            {
                foreach (var file in _bdrom.DirectoryBDMV?.GetFiles())
                {
                    copyState.TotalBytes += file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            if (_bdrom.DirectoryBDJO != null)
            {
                foreach (var file in _bdrom.DirectoryBDJO?.GetFiles())
                {
                    copyState.TotalBytes += file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            if (_bdrom.DirectoryCLIPINF != null)
            {
                foreach (var file in _bdrom.DirectoryCLIPINF?.GetFiles())
                {
                    copyState.TotalBytes += file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            if (_bdrom.DirectoryMeta != null)
            {
                foreach (var file in _bdrom.DirectoryMeta?.GetFiles())
                {
                    copyState.TotalBytes += file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            if (_bdrom.DirectoryPLAYLIST != null)
            {
                foreach (var file in _bdrom.DirectoryPLAYLIST?.GetFiles())
                {
                    copyState.TotalBytes += file.Length;
                    copyState.FileMap.Add(file,
                        file.IsImage
                            ? Path.Combine(targetPath, file.FullName)
                            : file.FullName.Replace(textBoxSource.Text, targetPath));
                }
            }

            foreach (var file in copyState.FileMap)
            {
                await using var fileIn = file.Key.OpenRead();
                var fileOut = new FileInfo(file.Value);
                if (!fileOut.Directory.Exists)
                {
                    fileOut.Directory.Create();
                }
                var cancelToken = new CancellationToken();
                await CopyFile(fileIn, fileOut, _sampleSize, cancelToken);
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
        buttonCreateSample.Enabled = !string.IsNullOrEmpty(textBoxSource.Text) &&
                                     !string.IsNullOrEmpty(textBoxTarget.Text) &&
                                     comboBoxStreamFile.SelectedIndex > -1 &&
                                     comboBoxSampleSize.SelectedIndex > -1;
        buttonCreateSample.Text = @"Create Sample";
    }

    private BackgroundWorker _initBDROMWorker;
    private BDROM _bdrom;

    private void InitBDROM(string path)
    {
        textBoxSource.Enabled = false;
        buttonBrowseSource.Enabled = false;
        buttonBrowseSource.Text = @"Scanning...";
        buttonBrowseISOSource.Enabled = false;
        comboBoxStreamFile.Enabled = false;
        textBoxTarget.Enabled = false;
        buttonBrowseTarget.Enabled = false;
        comboBoxSampleSize.Enabled = false;
        buttonCreateSample.Enabled = false;

        _initBDROMWorker = new BackgroundWorker();
        _initBDROMWorker.WorkerReportsProgress = true;
        _initBDROMWorker.WorkerSupportsCancellation = true;
        _initBDROMWorker.DoWork += InitBDROMWork;
        _initBDROMWorker.ProgressChanged += InitBDROMProgress;
        _initBDROMWorker.RunWorkerCompleted += InitBDROMCompleted;
        _initBDROMWorker.RunWorkerAsync(path);
    }

    private void InitBDROMWork(object sender, DoWorkEventArgs e)
    {
        try
        {
            _bdrom = new BDROM((string)e.Argument);

            _bdrom.StreamClipFileScanError += BDROM_StreamClipFileScanError;
            _bdrom.StreamFileScanError += BDROM_StreamFileScanError;
            _bdrom.PlaylistFileScanError += BDROM_PlaylistFileScanError;
            _bdrom.Scan();
            e.Result = null;
        }
        catch (Exception ex)
        {
            e.Result = ex;
        }
    }

    protected bool BDROM_PlaylistFileScanError(TSPlaylistFile playlistFile, Exception ex)
    {
        var result = MessageBox.Show($"""
            An error occurred while scanning the playlist file {playlistFile.Name}.
            
            The disc may be copy-protected or damaged.
            
            Do you want to continue scanning the playlist files?
            """,
            @"BDInfo Scan Error", MessageBoxButtons.YesNo);

        return result == DialogResult.Yes;
    }

    protected bool BDROM_StreamFileScanError(TSStreamFile streamFile,
        Exception ex)
    {
        var result = MessageBox.Show($"""
            An error occurred while scanning the stream file {streamFile.Name}.
            
            The disc may be copy-protected or damaged.
            
            Do you want to continue scanning the stream files?
            """,
            @"BDInfo Scan Error", MessageBoxButtons.YesNo);

        return result == DialogResult.Yes;
    }

    protected bool BDROM_StreamClipFileScanError(TSStreamClipFile streamClipFile,
        Exception ex)
    {
        var result = MessageBox.Show($"""
            An error occurred while scanning the stream clip file {streamClipFile.Name}.
            
            The disc may be copy-protected or damaged.
            
            Do you want to continue scanning the stream clip files?
            """,
            @"BDInfo Scan Error", MessageBoxButtons.YesNo);

        return result == DialogResult.Yes;
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
            var msg = $"{((Exception)e.Result).Message}";

            MessageBox.Show(msg, @"BDInfo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            buttonBrowseSource.Enabled = true;
            return;
        }

        if (_bdrom == null) return;
        comboBoxStreamFile.DisplayMember = "Text";
        comboBoxStreamFile.ValueMember = "Value";

        List<object> items = new();

        foreach (var stream in _bdrom.StreamFiles)
        {
            items.Add(new
            {
                Text = $"\"{stream.Value.Name}\"   ({ToolBox.FormatFileSize(stream.Value.FileInfo.Length, true)})",
                Value = stream.Key
            });
        }

        comboBoxStreamFile.DataSource = items;

        textBoxSource.Enabled = true;
        buttonBrowseSource.Enabled = true;
        buttonBrowseSource.Text = @"Browse";
        buttonBrowseISOSource.Enabled = true;
        comboBoxStreamFile.Enabled = true;
        textBoxTarget.Enabled = true;
        buttonBrowseTarget.Enabled = true;
        comboBoxSampleSize.Enabled = true;

        ValidateButtonCreate();
    }

    public static async Task CopyFile(Stream fileIn, FileInfo fileOut, long maxSize, CancellationToken cancellationToken)
    {
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        const int bufferSize = 81920;

        await using var sourceStream = fileIn;
        await using var destinationStream = new FileStream(fileOut.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
        var bytesLeft = fileIn.Length < maxSize ? fileIn.Length : maxSize;
        const long bytesCopied = 0;
        var buffer = new byte[bufferSize];
        while (bytesCopied < bytesLeft)
        {
            var count = bytesLeft > bufferSize ? bufferSize : (int)bytesLeft;
            var read = await sourceStream.ReadAsync(buffer, 0, count, cancellationToken);
            await destinationStream.WriteAsync(buffer, 0, read, cancellationToken);
            bytesLeft -= read;
        }
    }

}
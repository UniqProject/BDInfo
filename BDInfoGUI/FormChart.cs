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

using BDInfoLib.BDROM;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZedGraph;

namespace BDInfoGUI
{
    public partial class FormChart : Form
    {
        private string _unitText = "";
        private bool _isHoverDisabled;
        private string _defaultFileName = "";

        private string FixVolumeLabel(string label)
        {
            // TODO: Other Volume Label Tweaks?
            return label.Replace(" ", "_");
        }

        public FormChart()
        {
            InitializeComponent();
            try
            {
                GraphControl.ContextMenuBuilder += GraphControl_ContextMenuBuilder;
                GraphControl.MouseMove += GraphControl_MouseMove;
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                        .OSPlatform.Linux)) return;

                GraphControl.IsEnableHZoom = false;
                GraphControl.IsEnableVZoom = false;
                GraphControl.SelectButtons = MouseButtons.None;
                GraphControl.LinkButtons = MouseButtons.None;
                GraphControl.PanButtons = MouseButtons.None;
                GraphControl.ZoomButtons = MouseButtons.None;
            }
            catch
            {
                // ignored
            }
        }

        private void FormChart_FormClosed(object sender, FormClosedEventArgs e)
        {
            GraphControl.Dispose();
            GC.Collect();
        }

        private void GraphControl_ContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraphControl.ContextMenuObjectState objState)
        {
            for (var i = 0; i < menuStrip.Items.Count; i++)
            {
                var item = (ToolStripMenuItem)menuStrip.Items[i];
                if ((string)item.Tag != "save_as") continue;

                ToolStripMenuItem newItem = new();
                newItem.Name = "save_as";
                newItem.Tag = "save_as";
                newItem.Text = @"Save Image As...";
                newItem.Click += OnSaveGraph;
                menuStrip.Items.Remove(item);
                menuStrip.Items.Insert(i, newItem);
                break;
            }
        }

        private void GraphControl_MouseMove(object sender, MouseEventArgs e)
        {
            var graph = (ZedGraphControl)sender;
            PointF pt = new(e.X, e.Y);
            var pane = graph.MasterPane.FindChartRect(pt);

            if (!_isHoverDisabled && pane != null)
            {
                pane.ReverseTransform(pt, out var x, out var y);

                var time = new TimeSpan(0, 0, 0, 0, (int)Math.Round(x * 1000 * 60));

                toolStripStatus.Text =
                    $@"Time: {$"{x:F3} sec"} ({$"{time:hh\\:mm\\:ss\\.ff}"}) Value: {$"{y:F2} {_unitText}"}";
            }
            else
            {
                toolStripStatus.Text = string.Empty;
            }
        }

        private void OnSaveGraph(object sender, EventArgs args)
        {
            GraphControl.SaveAs(_defaultFileName);
        }

        public void Generate(string chartType, TSPlaylistFile playlist, ushort pid, int angleIndex)
        {
            this.Text = $@"{playlist.Name}: {chartType}";
            GraphControl.GraphPane.Title.Text = chartType;
            GraphControl.IsEnableHEdit = false;
            GraphControl.IsEnableVEdit = false;
            GraphControl.IsEnableVPan = false;
            GraphControl.IsEnableVZoom = false;
            GraphControl.IsShowHScrollBar = true;
            GraphControl.IsAutoScrollRange = true;
            GraphControl.IsEnableHPan = true;
            GraphControl.IsEnableHZoom = true;
            GraphControl.IsEnableSelection = true;
            GraphControl.IsEnableWheelZoom = true;
            GraphControl.GraphPane.Legend.IsVisible = false;
            GraphControl.GraphPane.XAxis.Scale.IsUseTenPower = false;
            GraphControl.GraphPane.YAxis.Scale.IsUseTenPower = false;

            _defaultFileName = BDInfoGuiSettings.UseImagePrefix
                ? BDInfoGuiSettings.UseImagePrefixValue
                : $"{FixVolumeLabel(playlist.BDROM.VolumeLabel)}-{Path.GetFileNameWithoutExtension(playlist.Name)}-";

            switch (chartType)
            {
                case "Video Bitrate: 1-Second Window":
                    GenerateWindowChart(playlist, pid, angleIndex, 1);
                    _defaultFileName += "bitrate-01s";
                    break;
                case "Video Bitrate: 5-Second Window":
                    GenerateWindowChart(playlist, pid, angleIndex, 5);
                    _defaultFileName += "bitrate-05s";
                    break;
                case "Video Bitrate: 10-Second Window":
                    GenerateWindowChart(playlist, pid, angleIndex, 10);
                    _defaultFileName += "bitrate-10s";
                    break;
                case "Video Frame Size (Min / Max)":
                    GenerateFrameSizeChart(playlist, pid, angleIndex);
                    _defaultFileName += "frame-size";
                    break;
                case "Video Frame Type Counts":
                    GenerateFrameTypeChart(playlist, pid, angleIndex, false);
                    _defaultFileName += "frame-type-count";
                    break;
                case "Video Frame Type Sizes":
                    GenerateFrameTypeChart(playlist, pid, angleIndex, true);
                    _defaultFileName += "frame-type-size";
                    break;
            }
            _defaultFileName += ".png";
        }

        public void GenerateWindowChart(TSPlaylistFile playlist, ushort pid, int angleIndex, double windowSize)
        {
            _unitText = "Mbps";

            GraphControl.GraphPane.XAxis.Title.Text = "Time (minutes)";
            GraphControl.GraphPane.YAxis.Title.Text = "Bitrate (Mbps)";


            PointPairList pointsMin = new();
            PointPairList pointsMax = new();
            PointPairList pointsAvg = new();

            Queue<double> windowBits = new();
            Queue<double> windowSeconds = new();
            double windowBitsSum = 0;
            double windowSecondsSum = 0;

            var pointSeconds = 1D;
            var pointMin = double.MaxValue;
            var pointMax = 0D;
            var pointAvg = 0D;
            var pointCount = 0;

            foreach (var clip in playlist.StreamClips)
            {
                if (clip.AngleIndex != angleIndex ||
                    clip.StreamFile?.StreamDiagnostics == null ||
                    !clip.StreamFile.StreamDiagnostics.ContainsKey(pid))
                {
                    continue;
                }

                var diagList =
                    clip.StreamFile.StreamDiagnostics[pid];

                foreach (var diag in diagList)
                {
                    //if (diag.Tag == null) continue;

                    var pointPosition = diag.Marker -
                                        clip.TimeIn +
                                        clip.RelativeTimeIn;

                    var seconds = diag.Interval;
                    var bits = diag.Bytes * 8.0;

                    windowSecondsSum += seconds;
                    windowSeconds.Enqueue(seconds);
                    windowBitsSum += bits;
                    windowBits.Enqueue(bits);

                    if (windowSecondsSum > windowSize)
                    {
                        var bitrate = windowBitsSum / windowSecondsSum / 1000000;

                        if (bitrate < pointMin) pointMin = bitrate;
                        if (bitrate > pointMax) pointMax = bitrate;
                        pointCount++; pointAvg += bitrate;

                        for (var x = pointSeconds; x < (pointPosition - 1); x++)
                        {
                            var pointX = (x - 1) / 60;
                            pointsMin.Add(pointX, 0);
                            pointsAvg.Add(pointX, 0);
                            pointsMax.Add(pointX, 0);
                            pointSeconds += 1;
                        }

                        if (pointPosition >= pointSeconds)
                        {
                            var pointMinutes = (pointSeconds - 1) / 60;
                            pointsMin.Add(pointMinutes, pointMin);
                            pointsMax.Add(pointMinutes, pointMax);
                            pointsAvg.Add(pointMinutes, pointAvg / pointCount);
                            pointMin = double.MaxValue;
                            pointMax = 0;
                            pointAvg = 0;
                            pointCount = 0;
                            pointSeconds += 1;
                        }

                        windowBitsSum -= windowBits.Dequeue();
                        windowSecondsSum -= windowSeconds.Dequeue();
                    }

                    if (pointPosition >= pointSeconds)
                    {
                        for (var x = pointSeconds; x < (pointPosition - 1); x++)
                        {
                            var pointX = (x - 1) / 60;
                            pointsMin.Add(pointX, 0);
                            pointsAvg.Add(pointX, 0);
                            pointsMax.Add(pointX, 0);
                            pointSeconds += 1;
                        }
                        var pointMinutes = (pointSeconds - 1) / 60;
                        pointsMin.Add(pointMinutes, pointMin);
                        pointsAvg.Add(pointMinutes, pointAvg / pointCount);
                        pointsMax.Add(pointMinutes, pointMax);
                        pointMin = double.MaxValue;
                        pointMax = 0;
                        pointAvg = 0;
                        pointCount = 0;
                        pointSeconds += 1;
                    }
                }
            }

            for (var x = pointSeconds; x < playlist.TotalLength; x++)
            {
                var pointX = (x - 1) / 60;
                pointsMin.Add(pointX, 0);
                pointsAvg.Add(pointX, 0);
                pointsMax.Add(pointX, 0);
            }

            var avgCurve = GraphControl.GraphPane.AddCurve("Avg", pointsAvg, Color.Gray, SymbolType.None);
            avgCurve.Line.IsSmooth = true;

            var minCurve = GraphControl.GraphPane.AddCurve("Min", pointsMin, Color.LightGray, SymbolType.None);
            minCurve.Line.IsSmooth = true;
            minCurve.Line.Fill = new Fill(Color.White);

            var maxCurve = GraphControl.GraphPane.AddCurve("Max", pointsMax, Color.LightGray, SymbolType.None);
            maxCurve.Line.IsSmooth = true;
            maxCurve.Line.Fill = new Fill(Color.LightGray);

            GraphControl.GraphPane.XAxis.Scale.Min = 0;
            GraphControl.GraphPane.XAxis.Scale.Max = playlist.TotalLength / 60;
            GraphControl.GraphPane.YAxis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.IsVisible = true;

            GraphControl.AxisChange();
        }

        public void GenerateFrameSizeChart(TSPlaylistFile playlist, ushort pid, int angleIndex)
        {
            _unitText = "KB";

            GraphControl.GraphPane.XAxis.Title.Text = "Time (minutes)";
            GraphControl.GraphPane.YAxis.Title.Text = "Size (KB)";

            PointPairList pointsMin = new();
            PointPairList pointsMax = new();
            PointPairList pointsAvg = new();

            var pointSeconds = 1D;
            var pointMin = double.MaxValue;
            var pointMax = 0D;
            var pointAvg = 0D;
            var pointCount = 0;
            var overallMax = 0D;

            foreach (var clip in playlist.StreamClips)
            {
                if (clip.AngleIndex != angleIndex ||
                    clip.StreamFile?.StreamDiagnostics == null ||
                    !clip.StreamFile.StreamDiagnostics.ContainsKey(pid))
                {
                    continue;
                }

                var diagList = clip.StreamFile.StreamDiagnostics[pid];

                foreach (var diag in diagList)
                {
                    if (diag.Tag == null) continue;

                    var frameType = diag.Tag;
                    var frameSize = diag.Bytes / 1024D;

                    var pointPosition = diag.Marker -
                                        clip.TimeIn +
                                        clip.RelativeTimeIn;

                    if (frameSize > overallMax) overallMax = frameSize;
                    if (frameSize < pointMin) pointMin = frameSize;
                    if (frameSize > pointMax) pointMax = frameSize;

                    pointCount++; 
                    pointAvg += frameSize;

                    if (pointPosition >= pointSeconds)
                    {
                        for (var x = pointSeconds; x < (pointPosition - 1); x++)
                        {
                            var pointX = (x - 1) / 60;
                            pointsMin.Add(pointX, 0);
                            pointsAvg.Add(pointX, 0);
                            pointsMax.Add(pointX, 0);
                            pointSeconds += 1;
                        }
                        var pointMinutes = (pointSeconds - 1) / 60;
                        pointsMin.Add(pointMinutes, pointMin);
                        pointsAvg.Add(pointMinutes, pointAvg / pointCount);
                        pointsMax.Add(pointMinutes, pointMax);
                        pointMin = double.MaxValue;
                        pointMax = 0;
                        pointAvg = 0;
                        pointCount = 0;
                        pointSeconds += 1;
                    }
                }
            }

            for (var x = pointSeconds; x < playlist.TotalLength; x++)
            {
                var pointX = (x - 1) / 60;
                pointsMin.Add(pointX, 0);
                pointsAvg.Add(pointX, 0);
                pointsMax.Add(pointX, 0);
            }

            var avgCurve = GraphControl.GraphPane.AddCurve("Avg", pointsAvg, Color.Gray, SymbolType.None);
            avgCurve.Line.IsSmooth = true;

            var minCurve = GraphControl.GraphPane.AddCurve("Min", pointsMin, Color.LightGray, SymbolType.None);
            minCurve.Line.IsSmooth = true;
            minCurve.Line.Fill = new Fill(Color.White);

            var maxCurve = GraphControl.GraphPane.AddCurve("Max", pointsMax, Color.LightGray, SymbolType.None);
            maxCurve.Line.IsSmooth = true;
            maxCurve.Line.Fill = new Fill(Color.LightGray);

            GraphControl.GraphPane.XAxis.Scale.Min = 0;
            GraphControl.GraphPane.XAxis.Scale.Max = playlist.TotalLength / 60;
            GraphControl.GraphPane.YAxis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.IsVisible = true;

            GraphControl.AxisChange();
        }

        public void GenerateFrameTypeChart(TSPlaylistFile playlist, ushort pid, int angleIndex, bool isSizes)
        {
            _isHoverDisabled = true;

            GraphControl.GraphPane.XAxis.Title.Text = "Frame Type";

            if (isSizes)
            {
                _unitText = "KB";
                GraphControl.GraphPane.YAxis.Title.Text = "Average / Peak Size (KB)";
            }
            else
            {
                _unitText = "";
                GraphControl.GraphPane.YAxis.Title.Text = "Count";
            }

            Dictionary<string, double> frameCount = new();
            Dictionary<string, double> frameSizes = new();
            Dictionary<string, double> framePeaks = new();

            foreach (var clip in playlist.StreamClips)
            {
                if (clip.AngleIndex != angleIndex ||
                    clip.StreamFile?.StreamDiagnostics == null ||
                    !clip.StreamFile.StreamDiagnostics.ContainsKey(pid))
                {
                    continue;
                }

                var diagList = clip.StreamFile.StreamDiagnostics[pid];

                foreach (var diag in diagList)
                {
                    if (diag.Tag == null) continue;

                    var frameType = diag.Tag;
                    var frameSize = diag.Bytes / 1024D;

                    if (!framePeaks.ContainsKey(frameType))
                    {
                        framePeaks[frameType] = frameSize;
                    }
                    else if (frameSize > framePeaks[frameType])
                    {
                        framePeaks[frameType] = frameSize;
                    }
                    if (!frameCount.ContainsKey(frameType))
                    {
                        frameCount[frameType] = 0;
                    }
                    frameCount[frameType]++;

                    if (!frameSizes.ContainsKey(frameType))
                    {
                        frameSizes[frameType] = 0;
                    }
                    frameSizes[frameType] += frameSize;
                }
            }

            var labels = new string[frameCount.Keys.Count];
            var values = new double[frameCount.Keys.Count];
            var peaks = new double[frameCount.Keys.Count];
            Dictionary<string, int> frameTypes = new();

            frameCount.Keys.CopyTo(labels, 0);

            double totalFrameCount = 0;
            for (var i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                frameTypes[label] = i;
                if (isSizes)
                {
                    values[i] = frameSizes[label] / frameCount[label];
                    peaks[i] = framePeaks[label];
                }
                else
                {
                    values[i] = frameCount[label];
                }
                totalFrameCount += frameCount[label];
            }

            if (isSizes)
            {
                var barItem = GraphControl.GraphPane.AddBar("Average", null, values, Color.Black);
                barItem.Bar.Fill.Type = FillType.Solid;

                var barItemMax = GraphControl.GraphPane.AddBar("Peak", null, peaks, Color.Black);
                barItemMax.Bar.Fill.Type = FillType.None;

                GraphControl.GraphPane.XAxis.MajorTic.IsBetweenLabels = true;
                GraphControl.GraphPane.XAxis.Scale.TextLabels = labels;
                GraphControl.GraphPane.XAxis.Type = AxisType.Text;
                GraphControl.AxisChange();

                GraphControl.GraphPane.YAxis.Scale.Max += 
                    GraphControl.GraphPane.YAxis.Scale.MajorStep;

                BarItem.CreateBarLabels(GraphControl.GraphPane, false, "f0");
                GraphControl.GraphPane.Legend.IsVisible = true;
            }
            else
            {
                GraphControl.GraphPane.Chart.Fill.Type = FillType.None;
                GraphControl.GraphPane.XAxis.IsVisible = false;
                GraphControl.GraphPane.YAxis.IsVisible = false;

                var drgb = (int)Math.Truncate(255.0 / labels.Length);
                var rgb = 0;

                var sortedFrameCounts = new List<SortableFrameCount>();
                foreach (var frameType in frameCount.Keys)
                {
                    sortedFrameCounts.Add(new SortableFrameCount(frameType, frameCount[frameType]));
                }
                sortedFrameCounts.Sort();

                var j = sortedFrameCounts.Count;
                for (var i = 0; i < j; i++)
                {
                    AddPieSlice(sortedFrameCounts[i].Name, sortedFrameCounts[i].Count, totalFrameCount, rgb);
                    rgb += drgb;
                    if (--j <= i) continue;
                    AddPieSlice(sortedFrameCounts[j].Name, sortedFrameCounts[j].Count, totalFrameCount, rgb);
                    rgb += drgb;
                }
                GraphControl.GraphPane.AxisChange();
            }

            GraphControl.IsShowHScrollBar = false;
            GraphControl.IsAutoScrollRange = false;
            GraphControl.IsEnableHPan = false;
            GraphControl.IsEnableHZoom = false;
            GraphControl.IsEnableSelection = false;
            GraphControl.IsEnableWheelZoom = false;
        }

        private void AddPieSlice(string frameType, double frameCount, double totalFrameCount, int rgb)
        {
            var label = $" {frameType} Frames \n {frameCount:N0} \n ({frameCount / totalFrameCount * 100:F2}%) ";

            var color = Color.FromArgb(rgb, rgb, rgb);
            var pieItem = GraphControl.GraphPane.AddPieSlice(frameCount, color, color, 0, 0, label);
            pieItem.Border.IsVisible = false;
        }

        private class SortableFrameCount : IComparable
        {
            public readonly string Name;
            public readonly double Count;

            public SortableFrameCount(string name,
                                      double count)
            {
                Name = name;
                Count = count;
            }

            public int CompareTo(object o)
            {
                var frameType = (SortableFrameCount)o;
                if (Math.Abs(frameType.Count - Count) > 0D)
                {
                    return (int)(Count - frameType.Count);
                }

                return string.CompareOrdinal(Name, frameType.Name);
            }
        }
    }
}

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
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace BDInfo
{
    public partial class FormChart : Form
    {
        string UnitText = "";
        bool IsHoverDisabled = false;
        string DefaultFileName = "";

        public FormChart()
        {
            InitializeComponent();
            try
            {
                GraphControl.ContextMenuBuilder += new ZedGraphControl.ContextMenuBuilderEventHandler(GraphControl_ContextMenuBuilder);
                GraphControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphControl_MouseMove);
            }
            catch { }
        }

        private void GraphControl_ContextMenuBuilder(
            ZedGraphControl sender, 
            ContextMenuStrip menuStrip, 
            Point mousePt, 
            ZedGraphControl.ContextMenuObjectState objState)
        {
            for (int i = 0; i < menuStrip.Items.Count; i++)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)menuStrip.Items[i];
                if ((string)item.Tag == "save_as")
                {
                    ToolStripMenuItem newItem = new ToolStripMenuItem();
                    newItem.Name = "save_as";
                    newItem.Tag = "save_as";
                    newItem.Text = "Save Image As...";
                    newItem.Click += new System.EventHandler(OnSaveGraph);
                    menuStrip.Items.Remove(item);
                    menuStrip.Items.Insert(i, newItem);
                    break;
                }
            }
        }

        private void OnSaveGraph(
            object sender,
            EventArgs args)
        {
            GraphControl.SaveAs(DefaultFileName);
        }

        private void FormChart_FormClosed(
            object sender, 
            FormClosedEventArgs e)
        {
            GraphControl.Dispose();
            GC.Collect();
        }

        public void Generate(
            string chartType,
            TSPlaylistFile playlist,
            ushort PID,
            int angleIndex)
        {
            this.Text = string.Format("{0}: {1}", playlist.Name, chartType);
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

            if (BDInfoSettings.UseImagePrefix)
            {
                DefaultFileName = BDInfoSettings.UseImagePrefixValue;
            }
            else
            {
                DefaultFileName = string.Format(
                    "{0}-{1}-",
                    FixVolumeLabel(playlist.BDROM.VolumeLabel),
                    Path.GetFileNameWithoutExtension(playlist.Name));
            }

            switch (chartType)
            {
                case "Video Bitrate: 1-Second Window":
                    GenerateWindowChart(playlist, PID, angleIndex, 1);
                    DefaultFileName += "bitrate-01s";
                    break;
                case "Video Bitrate: 5-Second Window":
                    GenerateWindowChart(playlist, PID, angleIndex, 5);
                    DefaultFileName += "bitrate-05s";
                    break;
                case "Video Bitrate: 10-Second Window":
                    GenerateWindowChart(playlist, PID, angleIndex, 10);
                    DefaultFileName += "bitrate-10s";
                    break;
                case "Video Frame Size (Min / Max)":
                    GenerateFrameSizeChart(playlist, PID, angleIndex);
                    DefaultFileName += "frame-size";
                    break;
                case "Video Frame Type Counts":
                    GenerateFrameTypeChart(playlist, PID, angleIndex, false);
                    DefaultFileName += "frame-type-count";
                    break;
                case "Video Frame Type Sizes":
                    GenerateFrameTypeChart(playlist, PID, angleIndex, true);
                    DefaultFileName += "frame-type-size";
                    break;
            }
            DefaultFileName += ".png";
        }

        private string FixVolumeLabel(string label)
        {
            // TODO: Other Volume Label Tweaks?
            return label.Replace(" ", "_");
        }

        public void GenerateWindowChart(
            TSPlaylistFile playlist,
            ushort PID,
            int angleIndex,
            double windowSize)
        {
            UnitText = "Mbps";

            GraphControl.GraphPane.XAxis.Title.Text = "Time (minutes)";
            GraphControl.GraphPane.YAxis.Title.Text = "Bitrate (Mbps)";


            PointPairList pointsMin = new PointPairList();
            PointPairList pointsMax = new PointPairList();
            PointPairList pointsAvg = new PointPairList();

            Queue<double> windowBits = new Queue<double>();
            Queue<double> windowSeconds = new Queue<double>();
            double windowBitsSum = 0;
            double windowSecondsSum = 0;

            double pointPosition = 0;
            double pointSeconds = 1.0;
            double pointMin = double.MaxValue;
            double pointMax = 0;
            double pointAvg = 0;
            int pointCount = 0;

            foreach (TSStreamClip clip in playlist.StreamClips)
            {
                if (clip.AngleIndex != angleIndex ||
                    clip.StreamFile == null ||
                    clip.StreamFile.StreamDiagnostics == null ||
                    !clip.StreamFile.StreamDiagnostics.ContainsKey(PID))
                {
                    continue;
                }

                List<TSStreamDiagnostics> diagList =
                    clip.StreamFile.StreamDiagnostics[PID];

                for (int i = 0; i < diagList.Count; i++)
                {
                    TSStreamDiagnostics diag = diagList[i];
                    //if (diag.Tag == null) continue;

                    pointPosition =
                        diag.Marker -
                        clip.TimeIn +
                        clip.RelativeTimeIn;

                    double seconds = diag.Interval;
                    double bits = diag.Bytes * 8.0;

                    windowSecondsSum += seconds;
                    windowSeconds.Enqueue(seconds);
                    windowBitsSum += bits;
                    windowBits.Enqueue(bits);

                    if (windowSecondsSum > windowSize)
                    {
                        double bitrate = windowBitsSum / windowSecondsSum / 1000000;

                        if (bitrate < pointMin) pointMin = bitrate;
                        if (bitrate > pointMax) pointMax = bitrate;
                        pointCount++; pointAvg += bitrate;

                        for (double x = pointSeconds; x < (pointPosition - 1); x++)
                        {
                            double pointX = (x - 1) / 60;
                            pointsMin.Add(pointX, 0);
                            pointsAvg.Add(pointX, 0);
                            pointsMax.Add(pointX, 0);
                            pointSeconds += 1;
                        }

                        if (pointPosition >= pointSeconds)
                        {
                            double pointMinutes = (pointSeconds - 1) / 60;
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
                        for (double x = pointSeconds; x < (pointPosition - 1); x++)
                        {
                            double pointX = (x - 1) / 60;
                            pointsMin.Add(pointX, 0);
                            pointsAvg.Add(pointX, 0);
                            pointsMax.Add(pointX, 0);
                            pointSeconds += 1;
                        }
                        double pointMinutes = (pointSeconds - 1) / 60;
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

            for (double x = pointSeconds; x < playlist.TotalLength; x++)
            {
                double pointX = (x - 1) / 60;
                pointsMin.Add(pointX, 0);
                pointsAvg.Add(pointX, 0);
                pointsMax.Add(pointX, 0);
            }

            LineItem avgCurve = GraphControl.GraphPane.AddCurve(
                "Avg", pointsAvg, Color.Gray, SymbolType.None);
            avgCurve.Line.IsSmooth = true;

            LineItem minCurve = GraphControl.GraphPane.AddCurve(
                "Min", pointsMin, Color.LightGray, SymbolType.None);
            minCurve.Line.IsSmooth = true;
            minCurve.Line.Fill = new Fill(Color.White);

            LineItem maxCurve = GraphControl.GraphPane.AddCurve(
                "Max", pointsMax, Color.LightGray, SymbolType.None);
            maxCurve.Line.IsSmooth = true;
            maxCurve.Line.Fill = new Fill(Color.LightGray);

            GraphControl.GraphPane.XAxis.Scale.Min = 0;
            GraphControl.GraphPane.XAxis.Scale.Max = playlist.TotalLength / 60;
            GraphControl.GraphPane.YAxis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.IsVisible = true;

            GraphControl.AxisChange();
        }

        public void GenerateFrameSizeChart(
            TSPlaylistFile playlist,
            ushort PID,
            int angleIndex)
        {
            UnitText = "KB";

            GraphControl.GraphPane.XAxis.Title.Text = "Time (minutes)";
            GraphControl.GraphPane.YAxis.Title.Text = "Size (KB)";

            PointPairList pointsMin = new PointPairList();
            PointPairList pointsMax = new PointPairList();
            PointPairList pointsAvg = new PointPairList();

            double pointPosition = 0;
            double pointSeconds = 1.0;
            double pointMin = double.MaxValue;
            double pointMax = 0;
            double pointAvg = 0;
            int pointCount = 0;
            double overallMax = 0;

            foreach (TSStreamClip clip in playlist.StreamClips)
            {
                if (clip.AngleIndex != angleIndex ||
                    clip.StreamFile == null ||
                    clip.StreamFile.StreamDiagnostics == null ||
                    !clip.StreamFile.StreamDiagnostics.ContainsKey(PID))
                {
                    continue;
                }

                List<TSStreamDiagnostics> diagList =
                    clip.StreamFile.StreamDiagnostics[PID];

                for (int i = 0; i < diagList.Count; i++)
                {
                    TSStreamDiagnostics diag = diagList[i];
                    if (diag.Tag == null) continue;

                    string frameType = diag.Tag;
                    double frameSize = diag.Bytes / 1024;

                    pointPosition =
                        diag.Marker -
                        clip.TimeIn +
                        clip.RelativeTimeIn;

                    if (frameSize > overallMax) overallMax = frameSize;
                    if (frameSize < pointMin) pointMin = frameSize;
                    if (frameSize > pointMax) pointMax = frameSize;

                    pointCount++; 
                    pointAvg += frameSize;

                    if (pointPosition >= pointSeconds)
                    {
                        for (double x = pointSeconds; x < (pointPosition - 1); x++)
                        {
                            double pointX = (x - 1) / 60;
                            pointsMin.Add(pointX, 0);
                            pointsAvg.Add(pointX, 0);
                            pointsMax.Add(pointX, 0);
                            pointSeconds += 1;
                        }
                        double pointMinutes = (pointSeconds - 1) / 60;
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

            for (double x = pointSeconds; x < playlist.TotalLength; x++)
            {
                double pointX = (x - 1) / 60;
                pointsMin.Add(pointX, 0);
                pointsAvg.Add(pointX, 0);
                pointsMax.Add(pointX, 0);
            }

            LineItem avgCurve = GraphControl.GraphPane.AddCurve(
                "Avg", pointsAvg, Color.Gray, SymbolType.None);
            avgCurve.Line.IsSmooth = true;

            LineItem minCurve = GraphControl.GraphPane.AddCurve(
                "Min", pointsMin, Color.LightGray, SymbolType.None);
            minCurve.Line.IsSmooth = true;
            minCurve.Line.Fill = new Fill(Color.White);

            LineItem maxCurve = GraphControl.GraphPane.AddCurve(
                "Max", pointsMax, Color.LightGray, SymbolType.None);
            maxCurve.Line.IsSmooth = true;
            maxCurve.Line.Fill = new Fill(Color.LightGray);

            GraphControl.GraphPane.XAxis.Scale.Min = 0;
            GraphControl.GraphPane.XAxis.Scale.Max = playlist.TotalLength / 60;
            GraphControl.GraphPane.YAxis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.Scale.Min = 0;
            GraphControl.GraphPane.Y2Axis.IsVisible = true;

            GraphControl.AxisChange();
        }

        public void GenerateFrameTypeChart(
            TSPlaylistFile playlist,
            ushort PID,
            int angleIndex,
            bool isSizes)
        {
            IsHoverDisabled = true;

            GraphControl.GraphPane.XAxis.Title.Text = "Frame Type";

            if (isSizes)
            {
                UnitText = "KB";
                GraphControl.GraphPane.YAxis.Title.Text = "Average / Peak Size (KB)";
            }
            else
            {
                UnitText = "";
                GraphControl.GraphPane.YAxis.Title.Text = "Count";
            }

            Dictionary<string, double> frameCount = new Dictionary<string, double>();
            Dictionary<string, double> frameSizes = new Dictionary<string, double>();
            Dictionary<string, double> framePeaks = new Dictionary<string, double>();

            foreach (TSStreamClip clip in playlist.StreamClips)
            {
                if (clip.AngleIndex != angleIndex ||
                    clip.StreamFile == null ||
                    clip.StreamFile.StreamDiagnostics == null ||
                    !clip.StreamFile.StreamDiagnostics.ContainsKey(PID))
                {
                    continue;
                }

                List<TSStreamDiagnostics> diagList =
                    clip.StreamFile.StreamDiagnostics[PID];

                for (int i = 0; i < diagList.Count; i++)
                {
                    TSStreamDiagnostics diag = diagList[i];
                    if (diag.Tag == null) continue;

                    string frameType = diag.Tag;
                    double frameSize = diag.Bytes / 1024;

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

            string[] labels = new string[frameCount.Keys.Count];
            double[] values = new double[frameCount.Keys.Count];
            double[] peaks = new double[frameCount.Keys.Count];
            Dictionary<string, int> frameTypes = new Dictionary<string, int>();

            frameCount.Keys.CopyTo(labels, 0);

            double totalFrameCount = 0;
            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
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
                BarItem barItem = GraphControl.GraphPane.AddBar(
                    "Average", null, values, Color.Black);
                barItem.Bar.Fill.Type = FillType.Solid;

                BarItem barItemMax = GraphControl.GraphPane.AddBar(
                    "Peak", null, peaks, Color.Black);
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

                int drgb = (int)Math.Truncate(255.0 / labels.Length);
                int rgb = 0;

                List<SortableFrameCount> sortedFrameCounts = new List<SortableFrameCount>();
                foreach (string frameType in frameCount.Keys)
                {
                    sortedFrameCounts.Add(new SortableFrameCount(frameType, frameCount[frameType]));
                }
                sortedFrameCounts.Sort();

                int j = sortedFrameCounts.Count;
                for (int i = 0; i < j; i++)
                {
                    AddPieSlice(sortedFrameCounts[i].Name, sortedFrameCounts[i].Count, totalFrameCount, rgb);
                    rgb += drgb;
                    if (--j > i)
                    {
                        AddPieSlice(sortedFrameCounts[j].Name, sortedFrameCounts[j].Count, totalFrameCount, rgb);
                        rgb += drgb;
                    }
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

        private class SortableFrameCount : IComparable
        {
            public string Name;
            public double Count;

            public SortableFrameCount(string name, double count)
            {
                Name = name;
                Count = count;
            }

            public int CompareTo(object o)
            {
                SortableFrameCount frameType = (SortableFrameCount)o;
                if (frameType.Count != Count)
                {
                    return (int)(Count - frameType.Count);
                }
                else return Name.CompareTo(frameType.Name);
            }
        }

        private void AddPieSlice(
            string frameType, 
            double frameCount, 
            double totalFrameCount, 
            int rgb)
        {
            string label = string.Format(
                " {0} Frames \n {1:N0} \n ({2:F2}%) ", 
                frameType, frameCount, frameCount / totalFrameCount * 100);

            Color color = Color.FromArgb(rgb, rgb, rgb);
            PieItem pieItem = GraphControl.GraphPane.AddPieSlice(
                frameCount, color, color, 0, 0, label);
            pieItem.Border.IsVisible = false;
        }

        private void GraphControl_MouseMove(
            object sender, 
            MouseEventArgs e)
        {
            ZedGraphControl graph = (ZedGraphControl)sender;
            PointF pt = new PointF(e.X, e.Y);
            GraphPane pane = graph.MasterPane.FindChartRect(pt);

            if (!IsHoverDisabled && pane != null)
            {
                double x, y;
                pane.ReverseTransform(pt, out x, out y);
                
                TimeSpan time = new TimeSpan(
                    0, 0, 0, 0, (int)Math.Round(x * 1000 * 60));
                
                toolStripStatus.Text = string.Format(
                    "Time: {0} ({1}) Value: {2}",
                    string.Format("{0:F3} sec", x),
                    string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", 
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds),
                    string.Format("{0:F2} {1}", y, UnitText));
            }
            else
            {
                toolStripStatus.Text = string.Empty;
            }
        }
    }
}

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
using OxyPlot;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using BDInfo.DataTypes;
using OxyPlot.Legends;

namespace BDInfo.ViewModels;

public class ChartWindowViewModel : ViewModelBase
{
    private PlotModel _plotViewModel;

    public PlotModel PlotViewModel
    {
        get => _plotViewModel;
        set => this.RaiseAndSetIfChanged(ref _plotViewModel, value);
    }

    public Avalonia.Size WindowSize => BDInfoSettings.WindowSize;

    public ChartWindowViewModel()
    {

    }

    public ChartWindowViewModel(int chartType, TSPlaylistFile playlist, ushort pid, int angleIndex)
    {
        switch (chartType)
        {
            case 0:
                GenerateWindowChart(playlist, pid, angleIndex, 1);
                break;
            case 1:
                GenerateWindowChart(playlist, pid, angleIndex, 5);
                break;
            case 2:
                GenerateWindowChart(playlist, pid, angleIndex, 10);
                break;
            case 3:
                GenerateFrameSizeChart(playlist, pid, angleIndex);
                break;
            case 4:
                GenerateFrameTypeChart(playlist, pid, angleIndex, false);
                break;
            case 5:
                GenerateFrameTypeChart(playlist, pid, angleIndex, true);
                break;
        }
    }

    private void GenerateWindowChart(TSPlaylistFile playlist, ushort pid, int angleIndex, int windowSize)
    {
        PlotViewModel = new PlotModel
        {
            Title = $"Video Bitrate: {windowSize}-Second Window",
            PlotType = PlotType.XY, PlotAreaBorderColor = OxyColors.Black
        };

        List<PlotMeasurement> dataPoints = new();

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

            var diagList = clip.StreamFile.StreamDiagnostics[pid];

            foreach (var diag in diagList)
            {
                //if (diag.Tag == null) continue;

                var pointPosition = diag.Marker - clip.TimeIn + clip.RelativeTimeIn;

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
                        var pointX = (x - 1);
                        dataPoints.Add(new PlotMeasurement
                        {
                            Maximum = pointX, Minimum = pointX,
                            Value = pointX, Time = 0D
                        });
                        pointSeconds += 1;
                    }

                    if (pointPosition >= pointSeconds)
                    {
                        var pointMinutes = (pointSeconds - 1);
                        dataPoints.Add(new PlotMeasurement
                        {
                            Minimum = pointMin, Maximum = pointMax,
                            Value = pointAvg / pointCount, Time = pointMinutes
                        });
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
                        var pointX = (x - 1);
                        dataPoints.Add(new PlotMeasurement
                        {
                            Maximum = pointX, Minimum = pointX,
                            Value = pointX, Time = 0D
                        });
                        pointSeconds += 1;
                    }
                    var pointMinutes = (pointSeconds - 1);
                    dataPoints.Add(new PlotMeasurement
                    {
                        Minimum = pointMin, Maximum = pointMax,
                        Value = pointAvg / pointCount, Time = pointMinutes
                    });
                    pointMin = double.MaxValue;
                    pointMax = 0;
                    pointAvg = 0;
                    pointCount = 0;
                    pointSeconds += 1;
                }
            }
        }

        PlotViewModel.Legends.Add(new Legend()
        {
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.TopLeft,
            LegendOrientation = LegendOrientation.Horizontal
        });

        PlotViewModel.Axes.Add(new OxyPlot.Axes.TimeSpanAxis
        {
            Title = "Time", Key = "X", Position = OxyPlot.Axes.AxisPosition.Bottom, 
            Angle = -45, Selectable = true,
            IsZoomEnabled = true,
            SelectionMode = SelectionMode.All
        });
        PlotViewModel.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Title = "Bitrate (Mbps)", Key = "Y", Position = OxyPlot.Axes.AxisPosition.Left,
            IsZoomEnabled = false,
        });

        OxyPlot.Series.LineSeries avg = new OxyPlot.Series.LineSeries
        {
            Title = "Average", ItemsSource = dataPoints, DataFieldX = "Time",
            DataFieldY = "Value", Color = OxyColors.Gray,
        };

        OxyPlot.Series.AreaSeries max = new OxyPlot.Series.AreaSeries
        {
            Title = "Max / Min", ItemsSource = dataPoints, DataFieldX = "Time",
            DataFieldX2 = "Time", DataFieldY = "Maximum", DataFieldY2 = "Minimum",
            Color = OxyColor.FromAColor(100, OxyColors.LightGray),
            LineStyle = LineStyle.Solid, MarkerFill = OxyColors.LightGray,
            LineJoin = LineJoin.Round
        };

        PlotViewModel.Series.Add(max);
        PlotViewModel.Series.Add(avg);
    }

    private void GenerateFrameSizeChart(TSPlaylistFile playlist, ushort pid, int angleIndex)
    {
        PlotViewModel = new PlotModel
        {
            Title = "Video Frame Size (Min / Max)", 
            PlotType = PlotType.XY, PlotAreaBorderColor = OxyColors.Black
        };

        List<PlotMeasurement> dataPoints = new();

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
                        var pointX = (x - 1);
                        dataPoints.Add(new PlotMeasurement
                        {
                            Minimum = pointX, Maximum = pointX, 
                            Value = pointX, Time = 0
                        });
                        pointSeconds += 1;
                    }
                    var pointMinutes = (pointSeconds - 1);
                    dataPoints.Add(new PlotMeasurement
                    {
                        Minimum = pointMin, Maximum = pointMax, 
                        Value = pointAvg / pointCount, Time = pointMinutes
                    });
                    pointMin = double.MaxValue;
                    pointMax = 0;
                    pointAvg = 0;
                    pointCount = 0;
                    pointSeconds += 1;
                }
            }
        }

        PlotViewModel.Legends.Add(new Legend()
        {
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.TopLeft,
            LegendOrientation = LegendOrientation.Horizontal
        });

        PlotViewModel.Axes.Add(new OxyPlot.Axes.TimeSpanAxis
        {
            Title = "Time", Key = "X", Position = OxyPlot.Axes.AxisPosition.Bottom,
            Angle = -45, Selectable = true, IsZoomEnabled = true,
            SelectionMode = SelectionMode.All
        });
        PlotViewModel.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Title = "Size (KB)", Key = "Y", Position = OxyPlot.Axes.AxisPosition.Left
        });

        OxyPlot.Series.LineSeries avg = new OxyPlot.Series.LineSeries
        {
            Title = "Average", ItemsSource = dataPoints, DataFieldX = "Time",
            DataFieldY = "Value", Color = OxyColors.Gray,
        };

        OxyPlot.Series.AreaSeries maxMin = new OxyPlot.Series.AreaSeries
        {
            Title = "Max / Min", ItemsSource = dataPoints, 
            DataFieldX = "Time", DataFieldY = "Maximum",
            DataFieldX2 = "Time", DataFieldY2 = "Minimum",
            Color = OxyColor.FromAColor(100, OxyColors.LightGray),
            LineStyle = LineStyle.Solid, MarkerFill = OxyColors.White,
        };

        PlotViewModel.Series.Add(maxMin);
        PlotViewModel.Series.Add(avg);
    }

    private void GenerateFrameTypeChart(TSPlaylistFile playlist, ushort pid, int angleIndex, bool isSizes)
    {

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

        var items = new List<PlotBarItem>();

        for (var i = 0; i < frameCount.Keys.Count; i++)
        {
            var label = frameCount.Keys.ElementAt(i);
            var barItem = new PlotBarItem
            {
                Label = label,
                Value = isSizes ? frameSizes[label] / frameCount[label] : frameCount[label],
                Peak = framePeaks[label],
                IsExploded = false
            };
            items.Add(barItem);
        }

        if (isSizes)
        {
            PlotViewModel = new PlotModel
            {
                Title = $"Video Frame Type Sizes",
                PlotType = PlotType.XY,
                PlotAreaBorderColor = OxyColors.Black
            };

            PlotViewModel.Legends.Add(new Legend()
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendOrientation = LegendOrientation.Vertical
            });

            PlotViewModel.Axes.Add(new OxyPlot.Axes.CategoryAxis() { Position = OxyPlot.Axes.AxisPosition.Left, ItemsSource = items, LabelField = "Label" });
            PlotViewModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0 });

            var averageBar = new OxyPlot.Series.BarSeries() { Title = "Average", ItemsSource = items, ValueField = "Value" };
            var peakBar = new OxyPlot.Series.BarSeries() { Title = "Peak", ItemsSource = items, ValueField = "Peak" };

            PlotViewModel.Series.Add(averageBar);
            PlotViewModel.Series.Add(peakBar);
        }
        else
        {
            PlotViewModel = new PlotModel
            {
                Title = $"Video Frame Type Counts",
                PlotType = PlotType.XY,
                PlotAreaBorderColor = OxyColors.Black
            };

            var pieSeries = new OxyPlot.Series.PieSeries
            {
                ItemsSource = items, IsExplodedField = "IsExploded", ValueField = "Value",
                LabelField = "Label",
                Diameter = 0.8,
                ExplodedDistance = 0,
                Stroke = OxyColors.Black,
                StrokeThickness = 1.0,
                AngleSpan = 360,
                StartAngle = 0,
                InsideLabelFormat = "",
                OutsideLabelFormat = "{1} Frames\n{0}\n({2:0} %)",
                TickLabelDistance = 10,
                TickHorizontalLength = 20,
                TickRadialLength = 20
            };
            

            PlotViewModel.Series.Add(pieSeries);
        }
        
    }
}
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
using System.IO;
using Newtonsoft.Json;

namespace BDInfo;

public class SizeConverter : JsonConverter<Avalonia.Size>
{
    public override void WriteJson(JsonWriter writer, Avalonia.Size value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override Avalonia.Size ReadJson(JsonReader reader, Type objectType, Avalonia.Size existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return Avalonia.Size.Parse((string)reader.Value ?? "0, 0");
    }
}

[JsonObject(MemberSerialization.OptIn)]
internal class BDInfoSettingsBase
{
    [JsonProperty]
    internal bool SizeFormatHR { get; set; } = true;
    [JsonProperty]
    internal bool GenerateStreamDiagnostics { get; set; } = true;
    [JsonProperty]
    internal bool DisplayChapterCount { get; set; }
    [JsonProperty]
    internal bool AutosaveReport { get; set; }
    [JsonProperty]
    internal bool UseImagePrefix { get; set; } = true;
    [JsonProperty]
    internal string UseImagePrefixValue { get; set; } = "video-";
    [JsonProperty]
    internal bool GenerateTextSummary { get; set; } = true;
    [JsonProperty]
    internal string LastPath { get; set; } = string.Empty;
    [JsonProperty]
    internal Avalonia.Controls.WindowState WindowState { get; set; } = Avalonia.Controls.WindowState.Normal;
    [JsonProperty]
    internal Avalonia.PixelPoint WindowLocation { get; set; } = new(0,0);
    [JsonProperty]
    internal Avalonia.Size WindowSize { get; set; } = new(1280, 720);
}

public static class BDInfoSettings
{
    private const string FileName = "BDInfoSettings.json";
    private static BDInfoSettingsBase _settings;

    public static void Load()
    {
        Load(false);
    }

    public static void Load(bool forceLoad)
    {
        if (!forceLoad && _settings != null) return;

        try
        {
            using var isoFile = BDInfoLib.ToolBox.GetIsolatedStorageFileStream(FileName, true);
            using var reader = new StreamReader(isoFile);
            _settings = JsonConvert.DeserializeObject<BDInfoSettingsBase>(reader.ReadToEnd(), new SizeConverter());
        }
        catch (Exception)
        {
            // silently ignore

            // On MacOS there is a very high chance this will fail with System.UnauthorizedAccessException,
            // if ~/.local/share is not existing. IsolatedStorageFile might also look at /usr/share/[IsolatedStorage]
            // Might also be a folder permissions problem.
            // Apparrently this was addressed in https://github.com/dotnet/corefx/pull/29514 but is still failing, at least in MacOS Ventura
        }

        if (_settings == null)
        {
            try
            {
                using var reader = BDInfoLib.ToolBox.GetTextReaderWriter(FileName, true) as TextReader;
                if (reader != null)
                    _settings = JsonConvert.DeserializeObject<BDInfoSettingsBase>(reader.ReadToEnd(),
                        new SizeConverter());
            }
            catch (Exception)
            {
                // ignore
            }

            _settings ??= new BDInfoSettingsBase();
        }

        if (!forceLoad)
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                Save();
            };
        }
    }

    public static void Save()
    {
        var json = JsonConvert.SerializeObject(_settings, Formatting.Indented, new SizeConverter());
        try
        {
            using var isoFile = BDInfoLib.ToolBox.GetIsolatedStorageFileStream(FileName, false);
            using var writer = new StreamWriter(isoFile);
            writer.WriteLine(json);
        }
        catch (Exception)
        {
            // silently ignore

            // On MacOS there is a very high chance this will fail with System.UnauthorizedAccessException
            // if ~/.local/share is not existing. IsolatedStorageFile might also look at /usr/share/[IsolatedStorage]
            // This also might be a folder permissions problem.
            // Apparrently this was addressed in https://github.com/dotnet/corefx/pull/29514 but is still failing, at least in MacOS Ventura
        }

        try
        {
            using var writer = BDInfoLib.ToolBox.GetTextReaderWriter(FileName, false) as TextWriter;
            writer!.WriteLine(json);
        }
        catch (Exception)
        {
            // silently ignore

            // Might fail if application does not have permission to write to current folder
        }
    }

    public static void ResetToDefault()
    {
        _settings = new BDInfoSettingsBase();
    }

    public static void RevertChanges()
    {
        Load(true);
    }

    public static bool SizeFormatHR
    {
        get => _settings.SizeFormatHR;

        set => _settings.SizeFormatHR = value;
    }

    public static bool GenerateStreamDiagnostics
    {
        get => _settings.GenerateStreamDiagnostics;

        set => _settings.GenerateStreamDiagnostics = value;
    }

    public static bool DisplayChapterCount
    {
        get => _settings.DisplayChapterCount;

        set => _settings.DisplayChapterCount = value;
    }

    public static bool AutosaveReport
    {
        get => _settings.AutosaveReport;

        set => _settings.AutosaveReport = value;
    }

    public static bool UseImagePrefix
    {
        get => _settings.UseImagePrefix;

        set => _settings.UseImagePrefix = value;
    }

    internal static string UseImagePrefixValue
    {
        get => _settings.UseImagePrefixValue;

        set => _settings.UseImagePrefixValue = value;
    }

    internal static bool GenerateTextSummary
    {
        get => _settings.GenerateTextSummary;

        set => _settings.GenerateTextSummary = value;
    }

    internal static string LastPath
    {
        get => _settings.LastPath;

        set => _settings.LastPath = value;
    }

    internal static Avalonia.Controls.WindowState WindowState
    {
        get => _settings.WindowState;

        set => _settings.WindowState = value;
    }

    internal static Avalonia.Size WindowSize
    {
        get => _settings.WindowSize;

        set => _settings.WindowSize = value;
    }

    internal static Avalonia.PixelPoint WindowLocation
    {
        get => _settings.WindowLocation;

        set => _settings.WindowLocation = value;
    }
}
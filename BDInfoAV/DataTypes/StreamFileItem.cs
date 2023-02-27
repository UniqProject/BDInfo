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

using ReactiveUI;

namespace BDInfo.DataTypes;

public class StreamFileItem : ReactiveObject
{
    private string _codec = string.Empty;
    private string _language = string.Empty;
    private long _bitRate = 0;
    private string _description = string.Empty;
    private int _pid = 0;

    public string Codec
    {
        get => _codec;
        set => this.RaiseAndSetIfChanged(ref _codec, value);
    }

    public string Language
    {
        get => _language;
        set => this.RaiseAndSetIfChanged(ref _language, value);
    }

    public long BitRate
    {
        get => _bitRate;
        set => this.RaiseAndSetIfChanged(ref _bitRate, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public int PID
    {
        get => _pid;
        set => this.RaiseAndSetIfChanged(ref _pid, value);
    }
}
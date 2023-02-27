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

public class PlayListFileItem : BDInfoFileBase
{
    private string _playListName = string.Empty;
    private int _chapters;
    private int _group = 0;
    private bool _isChecked = false;

    public string PlayListName
    {
        get => _playListName;
        set => this.RaiseAndSetIfChanged(ref _playListName, value);
    }

    public int Chapters
    {
        get => _chapters;
        set => this.RaiseAndSetIfChanged(ref _chapters, value);
    }

    public int Group
    {
        get => _group;
        set => this.RaiseAndSetIfChanged(ref _group, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }
}
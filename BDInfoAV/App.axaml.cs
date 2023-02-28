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

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using BDInfo.ViewModels;
using BDInfo.Views;

namespace BDInfo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            FluentTheme theme = null;
            foreach (var style in this.Styles)
            {
                if (style.GetType() == typeof(FluentTheme))
                    theme = style as FluentTheme;
            }

            if (theme != null) 
                theme.Mode = BDInfoSettings.UseDarkTheme ? FluentThemeMode.Dark : FluentThemeMode.Light;

            desktop.MainWindow = new MainWindow
            {
                DataContext = desktop.Args is { Length: > 0 }
                    ? new MainWindowViewModel(desktop.Args)
                    : new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
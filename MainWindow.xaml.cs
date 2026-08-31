using System;
using System.Diagnostics;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using OpenNEL.UI.Bridge;
using OpenNEL.WinUI.Pages;
using WinRT.Interop;
using Windows.Graphics;

namespace OpenNEL.WinUI;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		base.ExtendsContentIntoTitleBar = true;
		SetTitleBar(null);
		TrySetMicaBackdrop();
		try
		{
			OpenNEL.UI.Bridge.AppWindow.WinUIWindowHandle = WindowNative.GetWindowHandle(this);
		}
		catch
		{
		}
		TrySetWindowIcon();
		ContentFrame.Navigate(typeof(PeServersPage), "network");
		NavView.SelectedItem = NavView.MenuItems[0];
		GateFrame.Navigate(typeof(GatePage));
		if (GateFrame.Content is GatePage gatePage)
		{
			gatePage.Verified += delegate(string profile)
			{
				EnterLauncher(profile);
			};
		}
		Microsoft.UI.Windowing.AppWindow? appWindowForCurrentWindow = GetAppWindowForCurrentWindow();
		if (appWindowForCurrentWindow != null)
		{
			appWindowForCurrentWindow.Resize(new SizeInt32(1180, 760));
		}
	}

	private void EnterLauncher(string profile)
	{
		base.DispatcherQueue.TryEnqueue(delegate
		{
			GateFrame.Visibility = Visibility.Collapsed;
			NavView.Visibility = Visibility.Visible;
			base.Title = (string.IsNullOrWhiteSpace(profile) ? "PixelNEL - Minecraft Launcher" : ("PixelNEL - " + profile));
			ContentFrame.Navigate(typeof(PeServersPage), "network");
		});
	}

	private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		if (args.SelectedItem is NavigationViewItem navigationViewItem)
		{
			string? text = navigationViewItem.Tag as string;
			System.Type sourcePageType = text switch
			{
				"PeServers" => typeof(PeServersPage), 
				"Rental" => typeof(PeServersPage), 
				"Accounts" => typeof(AccountsPage), 
				"Settings" => typeof(SettingsPage), 
				"Modules" => typeof(ModulesPage), 
				"Proxies" => typeof(ProxyManagerPage), 
				"Inspector" => typeof(InspectorPage), 
				_ => typeof(PeServersPage), 
			};
			string parameter = ((text == "Rental") ? "rental" : ((text == "PeServers") ? "network" : ""));
			ContentFrame.Navigate(sourcePageType, parameter);
		}
	}

	private void TrySetMicaBackdrop()
	{
		try
		{
			base.SystemBackdrop = new MicaBackdrop
			{
				Kind = MicaKind.BaseAlt
			};
		}
		catch
		{
		}
	}

	private void TrySetWindowIcon()
	{
		try
		{
			Microsoft.UI.Windowing.AppWindow fromWindowId = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this)));
			if (fromWindowId == null)
			{
				return;
			}
			string[] array = new string[3]
			{
				Path.Combine(AppContext.BaseDirectory, "Assets", "PixelNEL.ico"),
				Path.Combine(AppContext.BaseDirectory, "Assets", "StoreLogo.png"),
				Path.Combine(AppContext.BaseDirectory, "PixelNEL.ico")
			};
			foreach (string text in array)
			{
				if (File.Exists(text))
				{
					fromWindowId.SetIcon(text);
					break;
				}
			}
		}
		catch
		{
		}
	}

	private Microsoft.UI.Windowing.AppWindow? GetAppWindowForCurrentWindow()
	{
		try
		{
			return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this)));
		}
		catch
		{
			return null;
		}
	}

}

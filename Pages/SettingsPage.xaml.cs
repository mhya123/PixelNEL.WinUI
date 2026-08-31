using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using OpenNEL.UI.Bridge;
using OpenNEL.WinUI.Services;
using WinRT.Interop;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace OpenNEL.WinUI.Pages;

public sealed partial class SettingsPage : Page
{
	public SettingsPage()
	{
		InitializeComponent();
		base.Loaded += async delegate
		{
			await LoadAsync();
			RefreshSkinPreview();
		};
	}

	private void SkinPath_TextChanged(object sender, TextChangedEventArgs e)
	{
		RefreshSkinPreview();
	}

	private void SlimBox_Changed(object sender, RoutedEventArgs e)
	{
		RefreshSkinPreview();
	}

	private void RefreshSkinPreview_Click(object sender, RoutedEventArgs e)
	{
		RefreshSkinPreview();
	}

	private void OpenSkinFile_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string? text = SkinPathBox.Text?.Trim();
			if (!string.IsNullOrEmpty(text) && File.Exists(text))
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = text,
					UseShellExecute = true
				});
			}
		}
		catch
		{
		}
	}

	private void RefreshSkinPreview()
	{
		try
		{
			string text = SkinPathBox.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(text) || !File.Exists(text))
			{
				SkinPreviewImage.Visibility = Visibility.Collapsed;
				SkinPreviewImage.Source = null;
				SkinPreviewPlaceholder.Visibility = Visibility.Visible;
				SkinPreviewError.Visibility = Visibility.Collapsed;
			}
			else
			{
				BitmapImage source = new BitmapImage(new Uri(text));
				SkinPreviewImage.Source = source;
				SkinPreviewImage.Visibility = Visibility.Visible;
				SkinPreviewPlaceholder.Visibility = Visibility.Collapsed;
				SkinPreviewError.Visibility = Visibility.Collapsed;
				SkinPreviewImage.Tag = ((SlimBox.IsChecked == true) ? "slim" : "classic");
			}
		}
		catch (Exception ex)
		{
			SkinErrorText.Text = ex.Message;
			SkinPreviewError.Visibility = Visibility.Visible;
		}
	}

	private async Task LoadAsync()
	{
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync("settings:get");
			GamePathBox.Text = (jsonElement.TryGetProperty("peGamePath", out var value) ? (value.GetString() ?? "") : "");
			SkinPathBox.Text = (jsonElement.TryGetProperty("peSkinPath", out var value2) ? (value2.GetString() ?? "") : "");
			SlimBox.IsChecked = jsonElement.TryGetProperty("peSkinSlim", out var value3) && value3.ValueKind == JsonValueKind.True;
			MitmCheck.IsChecked = !jsonElement.TryGetProperty("peMitmEnabled", out var value4) || value4.GetBoolean();
			InternationalRelayCheck.IsChecked = jsonElement.TryGetProperty("peInternationalRelayEnabled", out var value5) && value5.GetBoolean();
			int num = ((jsonElement.TryGetProperty("peInternationalRelayProtocol", out var value6) && value6.TryGetInt32(out var value7)) ? value7 : 827);
			RelayV844Radio.IsChecked = num == 844;
			RelayV827Radio.IsChecked = num != 844;
			if (jsonElement.TryGetProperty("launcherMaxGameMemoryMb", out var value8) && value8.TryGetInt32(out var value9))
			{
				MemoryBox.Value = value9;
			}
			else
			{
				MemoryBox.Value = 2048.0;
			}
		}
		catch (Exception ex)
		{
			StatusBar.Message = ex.Message;
			StatusBar.Severity = InfoBarSeverity.Error;
			StatusBar.IsOpen = true;
		}
	}

	private async void Save_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			var data = new
			{
				peGamePath = (GamePathBox.Text?.Trim() ?? ""),
				peSkinPath = (SkinPathBox.Text?.Trim() ?? ""),
				peSkinSlim = (SlimBox.IsChecked == true),
				peMitmEnabled = (MitmCheck.IsChecked == true),
				peInternationalRelayEnabled = (InternationalRelayCheck.IsChecked == true),
				peInternationalRelayProtocol = ((RelayV844Radio.IsChecked == true) ? 844 : 827),
				launcherMaxGameMemoryMb = (int)MemoryBox.Value
			};
			await BridgeService.InvokeAsync("settings:set", data);
			StatusBar.Message = "设置已保存";
			StatusBar.Severity = InfoBarSeverity.Success;
			StatusBar.IsOpen = true;
		}
		catch (Exception ex)
		{
			StatusBar.Message = ex.Message;
			StatusBar.Severity = InfoBarSeverity.Error;
			StatusBar.IsOpen = true;
		}
	}

	private void Reload_Click(object sender, RoutedEventArgs e)
	{
		_ = LoadAsync();
	}

	private async void BrowseGamePath_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			FolderPicker folderPicker = new FolderPicker();
			folderPicker.FileTypeFilter.Add("*");
			folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			nint num = AppWindow.WinUIWindowHandle;
			if (num == IntPtr.Zero)
			{
				try
				{
					Window current = Window.Current;
					if (current != null)
					{
						num = WindowNative.GetWindowHandle(current);
					}
				}
				catch
				{
				}
			}
			if (num != IntPtr.Zero)
			{
				InitializeWithWindow.Initialize(folderPicker, num);
			}
			StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();
			if (storageFolder != null)
			{
				GamePathBox.Text = storageFolder.Path;
				return;
			}
		}
		catch
		{
		}
		try
		{
			if ((await BridgeService.InvokeAsync("system:browseFolder", new
			{
				title = "选择游戏目录"
			})).TryGetProperty("path", out var value))
			{
				string? text = value.GetString();
				if (text != null && !string.IsNullOrEmpty(text))
				{
					GamePathBox.Text = text;
				}
			}
		}
		catch
		{
		}
	}

	private async void BrowseSkin_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			FileOpenPicker fileOpenPicker = new FileOpenPicker();
			fileOpenPicker.FileTypeFilter.Add(".png");
			fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			nint num = AppWindow.WinUIWindowHandle;
			if (num == IntPtr.Zero)
			{
				try
				{
					Window current = Window.Current;
					if (current != null)
					{
						num = WindowNative.GetWindowHandle(current);
					}
				}
				catch
				{
				}
			}
			if (num != IntPtr.Zero)
			{
				InitializeWithWindow.Initialize(fileOpenPicker, num);
			}
			StorageFile storageFile = await fileOpenPicker.PickSingleFileAsync();
			if (storageFile != null)
			{
				SkinPathBox.Text = storageFile.Path;
				return;
			}
		}
		catch
		{
		}
		try
		{
			if ((await BridgeService.InvokeAsync("system:browseFile", new
			{
				filter = "PNG|*.png"
			})).TryGetProperty("path", out var value))
			{
				string? text = value.GetString();
				if (text != null && !string.IsNullOrEmpty(text))
				{
					SkinPathBox.Text = text;
				}
			}
		}
		catch
		{
		}
	}

}

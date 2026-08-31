using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using OpenNEL.WinUI.Services;
using Windows.ApplicationModel.DataTransfer;

namespace OpenNEL.WinUI.Pages;

public sealed partial class ProxyManagerPage : Page
{
	private readonly DispatcherTimer _refreshTimer = new DispatcherTimer
	{
		Interval = TimeSpan.FromSeconds(2L)
	};

	private bool _refreshing;

	public ObservableCollection<ProxyItem> Proxies { get; } = new ObservableCollection<ProxyItem>();

	public ProxyManagerPage()
	{
		InitializeComponent();
		_refreshTimer.Tick += async delegate
		{
			await RefreshAsync(showErrors: false);
		};
		base.Loaded += async delegate
		{
			_refreshTimer.Start();
			await RefreshAsync(showErrors: true);
		};
		base.Unloaded += delegate
		{
			_refreshTimer.Stop();
		};
	}

	private async Task RefreshAsync(bool showErrors)
	{
		if (_refreshing)
		{
			return;
		}
		_refreshing = true;
		if (Proxies.Count == 0)
		{
			LoadingRing.IsActive = true;
		}
		try
		{
			JsonElement data = await BridgeService.InvokeAsync("pe:proxies");
			List<ProxyItem> list = new List<ProxyItem>();
			if (data.TryGetProperty("items", out var value) && value.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					list.Add(ProxyItem.FromJson(item));
				}
			}
			Proxies.Clear();
			foreach (ProxyItem item2 in list)
			{
				Proxies.Add(item2);
			}
			ActiveCountText.Text = ReadCount(data, "count").ToString();
			MitmCountText.Text = ReadCount(data, "mitmCount").ToString();
			ConnectedCountText.Text = ReadCount(data, "connectedCount").ToString();
			EmptyState.Visibility = ((Proxies.Count != 0) ? Visibility.Collapsed : Visibility.Visible);
		}
		catch (Exception ex)
		{
			if (showErrors)
			{
				ShowStatus(ex.Message, InfoBarSeverity.Error);
			}
		}
		finally
		{
			LoadingRing.IsActive = false;
			_refreshing = false;
		}
	}

	private static int ReadCount(JsonElement data, string name)
	{
		if (!data.TryGetProperty(name, out var value) || !value.TryGetInt32(out var value2))
		{
			return 0;
		}
		return value2;
	}

	private async void Refresh_Click(object sender, RoutedEventArgs e)
	{
		await RefreshAsync(showErrors: true);
	}

	private async void StopProxy_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is Button { Tag: string tag }))
		{
			return;
		}
		try
		{
			await BridgeService.InvokeAsync("pe:proxyStop", new
			{
				id = tag
			});
			ShowStatus("代理已关闭，使用该代理的连接会立即断开。", InfoBarSeverity.Success);
			await RefreshAsync(showErrors: true);
		}
		catch (Exception ex)
		{
			ShowStatus(ex.Message, InfoBarSeverity.Error);
		}
	}

	private async void StopAll_Click(object sender, RoutedEventArgs e)
	{
		if (Proxies.Count == 0)
		{
			ShowStatus("当前没有活动代理。", InfoBarSeverity.Informational);
		}
		else if (await new ContentDialog
		{
			XamlRoot = base.XamlRoot,
			Title = "关闭全部 Proxy？",
			Content = $"将关闭 {Proxies.Count} 个代理，所有经过它们的游戏连接都会断开。",
			PrimaryButtonText = "关闭全部",
			CloseButtonText = "取消",
			DefaultButton = ContentDialogButton.Close
		}.ShowAsync() == ContentDialogResult.Primary)
		{
			try
			{
				int value = ReadCount(await BridgeService.InvokeAsync("pe:proxyStopAll"), "stopped");
				ShowStatus($"已关闭 {value} 个代理。", InfoBarSeverity.Success);
				await RefreshAsync(showErrors: true);
			}
			catch (Exception ex)
			{
				ShowStatus(ex.Message, InfoBarSeverity.Error);
			}
		}
	}

	private void CopyEndpoint_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button { Tag: string tag } && !string.IsNullOrWhiteSpace(tag))
		{
			DataPackage dataPackage = new DataPackage();
			dataPackage.SetText(tag);
			Clipboard.SetContent(dataPackage);
			ShowStatus("已复制 " + tag, InfoBarSeverity.Success);
		}
	}

	private void OpenLog_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is Button { Tag: string tag }) || string.IsNullOrWhiteSpace(tag))
		{
			return;
		}
		try
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + tag + "\"")
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			ShowStatus(ex.Message, InfoBarSeverity.Error);
		}
	}

	private void ShowStatus(string message, InfoBarSeverity severity)
	{
		StatusBar.Message = message;
		StatusBar.Severity = severity;
		StatusBar.IsOpen = true;
	}

}

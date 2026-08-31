using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using OpenNEL.WinUI.Services;

namespace OpenNEL.WinUI.Pages;

public sealed partial class AccountsPage : Page
{
	private record AccountRow
	{
		public string Id { get; set; } = "";

		public string Alias { get; set; } = "";

		public string Channel { get; set; } = "";

		public string Status { get; set; } = "";
	}

	public AccountsPage()
	{
		InitializeComponent();
		base.Loaded += async delegate
		{
			await LoadAsync();
		};
	}

	private async Task LoadAsync()
	{
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync("account:list");
			List<AccountRow> list = new List<AccountRow>();
			if (jsonElement.TryGetProperty("accounts", out var value) && value.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					list.Add(new AccountRow
					{
						Id = (item.TryGetProperty("id", out var value2) ? (value2.GetString() ?? "") : (item.TryGetProperty("entityId", out var value3) ? (value3.GetString() ?? "") : "")),
						Alias = (item.TryGetProperty("alias", out var value4) ? (value4.GetString() ?? "") : ""),
						Channel = (item.TryGetProperty("channel", out var value5) ? (value5.GetString() ?? "") : ""),
						Status = (item.TryGetProperty("status", out var value6) ? (value6.GetString() ?? "") : ((item.TryGetProperty("authorized", out var value7) && value7.GetBoolean()) ? "authorized" : "saved"))
					});
				}
			}
			AccountList.ItemsSource = list;
		}
		catch (Exception ex)
		{
			StatusBar.Message = ex.Message;
			StatusBar.Severity = InfoBarSeverity.Error;
			StatusBar.IsOpen = true;
		}
	}

	private async void AddPE_Click(object sender, RoutedEventArgs e)
	{
		string text = CookieBox.Text?.Trim() ?? "";
		if (string.IsNullOrEmpty(text))
		{
			StatusBar.Message = "请输入 Cookie";
			StatusBar.IsOpen = true;
			return;
		}
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync("account:loginAddPE", new
			{
				method = "cookie",
				cookie = text,
				account = text
			});
			StatusBar.Message = (jsonElement.TryGetProperty("message", out var value) ? (value.GetString() ?? "添加成功") : "添加成功");
			StatusBar.Severity = InfoBarSeverity.Success;
			StatusBar.IsOpen = true;
			CookieBox.Text = "";
			await LoadAsync();
		}
		catch (Exception ex)
		{
			StatusBar.Message = ex.Message;
			StatusBar.Severity = InfoBarSeverity.Error;
			StatusBar.IsOpen = true;
		}
	}

	private async void Relogin_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is Button { Tag: string tag }))
		{
			return;
		}
		StatusBar.Message = "正在重新登录...";
		StatusBar.Severity = InfoBarSeverity.Informational;
		StatusBar.IsOpen = true;
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync("account:activate", new
			{
				entityId = tag
			});
			StatusBar.Message = (jsonElement.TryGetProperty("message", out var value) ? (value.GetString() ?? "重登成功") : "重登成功");
			StatusBar.Severity = InfoBarSeverity.Success;
			await LoadAsync();
		}
		catch (Exception ex)
		{
			StatusBar.Message = "重登失败: " + ex.Message + " (请重新粘贴 Cookie)";
			StatusBar.Severity = InfoBarSeverity.Error;
			StatusBar.IsOpen = true;
		}
	}

	private async void Logout_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button { Tag: string tag })
		{
			try
			{
				await BridgeService.InvokeAsync("account:logout", new
				{
					entityId = tag
				});
				await LoadAsync();
			}
			catch (Exception ex)
			{
				StatusBar.Message = ex.Message;
				StatusBar.IsOpen = true;
			}
		}
	}

	private async void Delete_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button { Tag: string tag })
		{
			try
			{
				await BridgeService.InvokeAsync("account:delete", new
				{
					entityId = tag
				});
				await LoadAsync();
			}
			catch (Exception ex)
			{
				StatusBar.Message = ex.Message;
				StatusBar.IsOpen = true;
			}
		}
	}

	private async void Alias_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is Button { Tag: var tag }) || !(tag is string id))
		{
			return;
		}
		ContentDialog obj = new ContentDialog
		{
			Title = "修改备注",
			PrimaryButtonText = "保存",
			CloseButtonText = "取消",
			XamlRoot = base.XamlRoot
		};
		TextBox box = (TextBox)(obj.Content = new TextBox
		{
			PlaceholderText = "新备注"
		});
		if (await obj.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}
		try
		{
			await BridgeService.InvokeAsync("account:updateAlias", new
			{
				entityId = id,
				alias = box.Text
			});
			await LoadAsync();
		}
		catch (Exception ex)
		{
			StatusBar.Message = ex.Message;
			StatusBar.IsOpen = true;
		}
	}

	private void Refresh_Click(object sender, RoutedEventArgs e)
	{
		_ = LoadAsync();
	}

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using OpenNEL.WinUI.Services;
using Windows.Storage;

namespace OpenNEL.WinUI.Pages;

public sealed partial class PeServersPage : Page
{
	private record ServerItem
	{
		public string EntityId { get; set; } = "";

		public string Name { get; set; } = "";

		public string Brief { get; set; } = "";

		public string OnlineCount { get; set; } = "";

		public string ImageUrl { get; set; } = "";
	}

	private record AccountItem
	{
		public string Id { get; set; } = "";

		public string Label { get; set; } = "";

		public string Channel { get; set; } = "";
	}

	private record RoleItem
	{
		public string Name { get; set; } = "";

		public string Id { get; set; } = "";
	}

	private string _mode = "network";

	private string _selectedAccountId = "";

	private string _selectedServerId = "";

	private List<JsonElement> _serversCache = new List<JsonElement>();

	private readonly Dictionary<string, string> _roleByServer = new Dictionary<string, string>();

	private readonly HashSet<string> _favorites = new HashSet<string>(LoadFavorites());

	private readonly List<string> _recent = LoadRecent();

	public PeServersPage()
	{
		InitializeComponent();
		RoleCombo.SelectionChanged += RoleCombo_SelectionChanged;
		base.Loaded += async delegate
		{
			await InitializeAsync();
		};
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		if (e.Parameter is string text && !string.IsNullOrEmpty(text))
		{
			_mode = ((text == "rental") ? "rental" : "network");
			TitleText.Text = ((_mode == "rental") ? "租赁服" : "联机大厅");
			ModeCombo.SelectedIndex = ((_mode == "rental") ? 1 : 0);
			PasswordBox.Visibility = ((!(_mode == "rental")) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private async Task InitializeAsync()
	{
		await LoadAccountsAsync();
		await LoadServersAsync();
	}

	private async Task LoadAccountsAsync()
	{
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync("pe-network:accounts");
			List<AccountItem> list = new List<AccountItem>();
			if (jsonElement.TryGetProperty("accounts", out var value) && value.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					list.Add(new AccountItem
					{
						Id = (item.TryGetProperty("entityId", out var value2) ? (value2.GetString() ?? "") : (item.TryGetProperty("id", out var value3) ? (value3.GetString() ?? "") : "")),
						Label = (item.TryGetProperty("label", out var value4) ? (value4.GetString() ?? "") : ""),
						Channel = (item.TryGetProperty("channel", out var value5) ? (value5.GetString() ?? "") : "")
					});
				}
			}
			AccountCombo.ItemsSource = list;
			AccountCombo.DisplayMemberPath = "Label";
			AccountCombo.SelectedValuePath = "Id";
			if (list.Count > 0)
			{
				AccountCombo.SelectedIndex = 0;
				_selectedAccountId = list[0].Id;
			}
		}
		catch (Exception ex)
		{
			ShowStatus(ex.Message, InfoBarSeverity.Error);
		}
	}

	private async Task LoadServersAsync()
	{
		LoadingRing.IsActive = true;
		EmptyText.Visibility = Visibility.Collapsed;
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync((_mode == "rental") ? "pe-rental:list" : "pe-network:list", new
			{
				pageSize = 0,
				keyword = (SearchBox.Text?.Trim() ?? ""),
				accountId = _selectedAccountId
			});
			List<ServerItem> list = new List<ServerItem>();
			if (jsonElement.TryGetProperty("items", out var value) && value.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					list.Add(new ServerItem
					{
						EntityId = (item.TryGetProperty("entityId", out var value2) ? (value2.GetString() ?? "") : (item.TryGetProperty("id", out var value3) ? (value3.GetString() ?? "") : "")),
						Name = (item.TryGetProperty("name", out var value4) ? (value4.GetString() ?? "") : ""),
						Brief = (item.TryGetProperty("brief", out var value5) ? (value5.GetString() ?? "") : (item.TryGetProperty("description", out var value6) ? (value6.GetString() ?? "") : "")),
						OnlineCount = (item.TryGetProperty("onlineCount", out var value7) ? value7.ToString() : ""),
						ImageUrl = (item.TryGetProperty("imageUrl", out var value8) ? (value8.GetString() ?? "") : "")
					});
				}
			}
			ServerList.ItemsSource = list;
			EmptyText.Visibility = ((list.Count != 0) ? Visibility.Collapsed : Visibility.Visible);
			ShowStatus($"已加载 {list.Count} 个服务器", InfoBarSeverity.Success);
		}
		catch (Exception ex)
		{
			ShowStatus(ex.Message, InfoBarSeverity.Error);
			ServerList.ItemsSource = null;
			EmptyText.Visibility = Visibility.Visible;
		}
		finally
		{
			LoadingRing.IsActive = false;
		}
	}

	private async void ServerList_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
	{
		object selectedItem = ServerList.SelectedItem;
		if (!(selectedItem is ServerItem server))
		{
			return;
		}
		_selectedServerId = server.EntityId;
		DetailText.Text = "加载中...";
		DetailRing.IsActive = true;
		RoleCombo.ItemsSource = null;
		try
		{
			string action = ((_mode == "rental") ? "pe-rental:detail" : "pe-network:detail");
			string action2 = ((_mode == "rental") ? "pe-rental:roles" : "pe-network:roles");
			Task<JsonElement> task = BridgeService.InvokeAsync(action, new
			{
				serverId = server.EntityId
			});
			Task<JsonElement> rolesTask = BridgeService.InvokeAsync(action2, new
			{
				serverId = server.EntityId,
				accountId = _selectedAccountId
			});
			JsonElement jsonElement = await task;
			DetailText.Text = (jsonElement.TryGetProperty("description", out var value) ? (value.GetString() ?? "暂无简介") : "暂无简介");
			JsonElement jsonElement2 = await rolesTask;
			List<RoleItem> list = new List<RoleItem>();
			if (jsonElement2.TryGetProperty("roles", out var value2) && value2.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value2.EnumerateArray())
				{
					string text = (item.TryGetProperty("name", out var value3) ? (value3.GetString() ?? "") : (item.TryGetProperty("roleName", out var value4) ? (value4.GetString() ?? "") : ""));
					string id = (item.TryGetProperty("roleId", out var value5) ? (value5.GetString() ?? "") : (item.TryGetProperty("id", out var value6) ? (value6.GetString() ?? text) : text));
					list.Add(new RoleItem
					{
						Name = text,
						Id = id
					});
				}
			}
			RoleCombo.ItemsSource = list;
			RoleCombo.DisplayMemberPath = "Name";
			RoleCombo.SelectedValuePath = "Id";
			string prev = (_roleByServer.TryGetValue(server.EntityId, out string? value7) ? value7 : "");
			int num = list.FindIndex((RoleItem r) => r.Id == prev);
			if (num >= 0)
			{
				RoleCombo.SelectedIndex = num;
			}
			else if (list.Count > 0)
			{
				RoleCombo.SelectedIndex = 0;
			}
			string message = (jsonElement2.TryGetProperty("message", out var value8) ? (value8.GetString() ?? "") : ((list.Count > 0) ? "角色已同步" : "暂无角色，请创建昵称"));
			ShowStatus(message, InfoBarSeverity.Informational);
		}
		catch (Exception ex)
		{
			DetailText.Text = ex.Message;
			ShowStatus(ex.Message, InfoBarSeverity.Error);
		}
		finally
		{
			DetailRing.IsActive = false;
		}
	}

	private async void CreateRole_Click(object sender, RoutedEventArgs e)
	{
		string text = RoleNameBox.Text?.Trim() ?? "";
		if (string.IsNullOrEmpty(_selectedServerId) || string.IsNullOrEmpty(text))
		{
			ShowStatus("先选择服务器并输入昵称", InfoBarSeverity.Warning);
			return;
		}
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync((_mode == "rental") ? "pe-rental:createRole" : "pe-network:createRole", new
			{
				serverId = _selectedServerId,
				accountId = _selectedAccountId,
				roleName = text,
				name = text
			});
			RoleNameBox.Text = "";
			ShowStatus(jsonElement.TryGetProperty("message", out var value) ? (value.GetString() ?? "昵称已设置") : "昵称已设置", InfoBarSeverity.Success);
			if (ServerList.SelectedItem is ServerItem)
			{
				ServerList_SelectionChanged(null, null);
			}
		}
		catch (Exception ex)
		{
			ShowStatus(ex.Message, InfoBarSeverity.Error);
		}
	}

	private async void Launch_Click(object sender, RoutedEventArgs e)
	{
		if (string.IsNullOrEmpty(_selectedServerId))
		{
			ShowStatus("请先选择服务器", InfoBarSeverity.Warning);
			return;
		}
		string value = "";
		if (RoleCombo.SelectedValue is string text && !string.IsNullOrEmpty(text))
		{
			value = text;
		}
		else if (RoleCombo.SelectedItem is RoleItem roleItem)
		{
			value = roleItem.Id;
		}
		string value2 = PasswordBox.Text ?? "";
		LaunchButton.IsEnabled = false;
		ShowStatus("正在启动...", InfoBarSeverity.Informational);
		try
		{
			string action = ((_mode == "rental") ? "pe-rental:launch" : "pe-network:launch");
			Dictionary<string, object> data = new Dictionary<string, object>
			{
				["serverId"] = _selectedServerId,
				["accountId"] = _selectedAccountId,
				["roleId"] = value,
				["password"] = value2,
				["serverName"] = (ServerList.SelectedItem as ServerItem)?.Name ?? ""
			};
			JsonElement jsonElement = await BridgeService.InvokeAsync(action, data);
			string text2 = (jsonElement.TryGetProperty("message", out var value3) ? (value3.GetString() ?? "启动请求已发送") : "启动请求已发送");
			if (jsonElement.TryGetProperty("type", out var value4) && value4.GetString() == "pe_download_required")
			{
				ShowStatus("需要下载游戏资源，正在下载...", InfoBarSeverity.Warning);
				_ = BridgeService.InvokeAsync("pe-network:download");
				return;
			}
			ShowStatus(text2, InfoBarSeverity.Success);
			LaunchStatusText.Text = text2;
			if (!string.IsNullOrEmpty(_selectedServerId))
			{
				_recent.Remove(_selectedServerId);
				_recent.Insert(0, _selectedServerId);
				SaveRecent(_recent);
			}
		}
		catch (Exception ex)
		{
			ShowStatus(ex.Message, InfoBarSeverity.Error);
			LaunchStatusText.Text = ex.Message;
		}
		finally
		{
			LaunchButton.IsEnabled = true;
		}
	}

	private void Refresh_Click(object sender, RoutedEventArgs e)
	{
		_ = LoadServersAsync();
	}

	private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
	}

	private void AccountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (AccountCombo.SelectedValue is string selectedAccountId)
		{
			_selectedAccountId = selectedAccountId;
		}
		else if (AccountCombo.SelectedItem is AccountItem accountItem)
		{
			_selectedAccountId = accountItem.Id;
		}
	}

	private void RoleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(_selectedServerId) && RoleCombo.SelectedValue is string { Length: >0 } text)
		{
			_roleByServer[_selectedServerId] = text;
		}
	}

	private static HashSet<string> LoadFavorites()
	{
		try
		{
			return (!(ApplicationData.Current.LocalSettings.Values["pe_favorites"] is string text)) ? new HashSet<string>() : new HashSet<string>(text.Split(',', StringSplitOptions.RemoveEmptyEntries));
		}
		catch
		{
			return new HashSet<string>();
		}
	}

	private static List<string> LoadRecent()
	{
		try
		{
			return (!(ApplicationData.Current.LocalSettings.Values["pe_recent"] is string text)) ? new List<string>() : new List<string>(text.Split(',', StringSplitOptions.RemoveEmptyEntries));
		}
		catch
		{
			return new List<string>();
		}
	}

	private static void SaveFavorites(HashSet<string> set)
	{
		try
		{
			ApplicationData.Current.LocalSettings.Values["pe_favorites"] = string.Join(',', set);
		}
		catch
		{
		}
	}

	private static void SaveRecent(List<string> list)
	{
		try
		{
			ApplicationData.Current.LocalSettings.Values["pe_recent"] = string.Join(',', list.Take(10));
		}
		catch
		{
		}
	}

	private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!(ModeCombo == null) && !(TitleText == null) && !(PasswordBox == null) && ModeCombo.SelectedItem is ComboBoxItem { Tag: string tag })
		{
			_mode = ((tag == "rental") ? "rental" : "network");
			TitleText.Text = ((_mode == "rental") ? "租赁服" : "联机大厅");
			PasswordBox.Visibility = ((!(_mode == "rental")) ? Visibility.Collapsed : Visibility.Visible);
			if (base.IsLoaded)
			{
				_ = LoadServersAsync();
			}
		}
	}

	private void ShowStatus(string message, InfoBarSeverity severity)
	{
		StatusBar.Title = severity.ToString();
		StatusBar.Message = message;
		StatusBar.Severity = severity;
		StatusBar.IsOpen = true;
		LaunchStatusText.Text = message;
	}

}

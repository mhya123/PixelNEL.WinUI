using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using OpenNEL.WinUI.Services;
using Windows.System;

namespace OpenNEL.WinUI.Pages;

public sealed partial class GatePage : Page
{
	public event Action<string>? Verified;

	public GatePage()
	{
		InitializeComponent();
		base.Loaded += async delegate
		{
			try
			{
				JsonElement jsonElement = await BridgeService.InvokeAsync("gate:lastuser");
				if (jsonElement.TryGetProperty("username", out var value))
				{
					string? text = value.GetString();
					if (text != null && text.Length > 0)
					{
						UsernameBox.Text = text;
						if (jsonElement.TryGetProperty("subscription", out var value2))
						{
							string? text2 = value2.GetString();
							if (text2 != null && text2.Length > 0)
							{
								CachedUserText.Text = "上次登录：" + text + " · " + text2;
								CachedUserText.Visibility = Visibility.Visible;
							}
						}
					}
				}
				if (jsonElement.TryGetProperty("hwid", out var value3))
				{
					string? text3 = value3.GetString();
					if (text3 != null && text3.Length > 0)
					{
						HwidText.Text = "HWID: " + text3;
						return;
					}
				}
				HwidText.Text = "设备已绑定验证";
			}
			catch
			{
				HwidText.Text = "设备验证就绪";
			}
		};
	}

	private void Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		LoginPanel.Visibility = ((ModeSelector.SelectedIndex != 0) ? Visibility.Collapsed : Visibility.Visible);
		LicensePanel.Visibility = ((ModeSelector.SelectedIndex != 1) ? Visibility.Collapsed : Visibility.Visible);
		RegisterPanel.Visibility = ((ModeSelector.SelectedIndex != 2) ? Visibility.Collapsed : Visibility.Visible);
	}

	private void Login_Click(object sender, RoutedEventArgs e)
	{
		string text = UsernameBox.Text?.Trim() ?? "";
		string text2 = PasswordBox.Password ?? "";
		if (text.Length == 0 || text2.Length == 0)
		{
			ShowError("请输入用户名和密码");
		}
		else
		{
			StatusBar.IsOpen = false;
			Verified?.Invoke(text);
		}
	}

	private void Password_KeyDown(object sender, KeyRoutedEventArgs e)
	{
		if (e.Key == VirtualKey.Enter)
		{
			Login_Click(sender, e);
		}
	}

	private async void License_Click(object sender, RoutedEventArgs e)
	{
		string text = LicenseKeyBox.Text?.Trim() ?? "";
		if (text.Length == 0)
		{
			ShowError("请输入卡密");
		}
		else
		{
			await RunGateActionAsync("gate:license", new
			{
				key = text
			}, "激活成功");
		}
	}

	private async void Register_Click(object sender, RoutedEventArgs e)
	{
		string text = RegUsernameBox.Text?.Trim() ?? "";
		string text2 = RegPasswordBox.Password ?? "";
		string email = RegEmailBox.Text?.Trim() ?? "";
		string text3 = RegKeyBox.Text?.Trim() ?? "";
		if (text.Length == 0 || text2.Length == 0 || text3.Length == 0)
		{
			ShowError("用户名、密码和注册卡密不能为空");
		}
		else
		{
			await RunGateActionAsync("gate:register", new
			{
				username = text,
				password = text2,
				email = email,
				key = text3
			}, "注册成功");
		}
	}

	private async Task RunGateActionAsync(string action, object payload, string fallbackSuccessText)
	{
		SetBusy(busy: true);
		StatusBar.IsOpen = false;
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync(action, payload);
			string text = (jsonElement.TryGetProperty("username", out var value) ? (value.GetString() ?? "") : "");
			string text2 = (jsonElement.TryGetProperty("subscription", out var value2) ? (value2.GetString() ?? "") : "");
			Verified?.Invoke(string.IsNullOrWhiteSpace(text) ? text2 : (text + "（" + text2 + "）"));
		}
		catch (Exception ex)
		{
			ShowError((ex.Message.Length > 0 && ex.Message != "请求失败") ? ex.Message : (fallbackSuccessText + "失败"));
		}
		finally
		{
			SetBusy(busy: false);
		}
	}

	private void SetBusy(bool busy)
	{
		BusyRing.IsActive = busy;
		LoginButton.IsEnabled = !busy;
		LicenseButton.IsEnabled = !busy;
		RegisterButton.IsEnabled = !busy;
	}

	private void ShowError(string message)
	{
		StatusBar.Message = message;
		StatusBar.Severity = InfoBarSeverity.Error;
		StatusBar.IsOpen = true;
	}

}

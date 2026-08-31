using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using OpenNEL.WinUI.Modules;
using OpenNEL.WinUI.Modules.Combat;
using OpenNEL.WinUI.Modules.World;
using OpenNEL.WinUI.Services;

namespace OpenNEL.WinUI.Pages;

public sealed partial class ModulesPage : Page
{
	private DispatcherTimer? _timer;

	private bool _updating;

	private KillauraModule? _killaura;

	private ScaffoldModule? _scaffold;

	public ModulesPage()
	{
		InitializeComponent();
		base.Loaded += OnLoaded;
		base.Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		ModuleManager.Initialize();
		_killaura = ModuleManager.Get("Killaura") as KillauraModule;
		_scaffold = ModuleManager.Get("Scaffold") as ScaffoldModule;
		ModulesList.ItemsSource = ModuleManager.Modules.Where((LuminaModule m) => m.Name != "Killaura" && m.Name != "Scaffold" && m.Name != "NightVision").ToList();
		foreach (LuminaModule module in ModuleManager.Modules)
		{
			module.PropertyChanged += delegate(object? _, PropertyChangedEventArgs args)
			{
				if (args.PropertyName == "IsEnabled")
				{
					base.DispatcherQueue.TryEnqueue(delegate
					{
						SyncControls();
						UpdateEnabledCount();
					});
				}
			};
		}
		SyncControls();
		UpdateEnabledCount();
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1L)
		};
		_timer.Tick += async delegate
		{
			await RefreshAsync();
		};
		_timer.Start();
		_ = RefreshAsync();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		_timer?.Stop();
		_timer = null;
	}

	private void SyncControls()
	{
		_updating = true;
		try
		{
			LuminaModule? luminaModule = ModuleManager.Get("NightVision");
			NightVisionToggle.IsOn = luminaModule?.IsEnabled ?? false;
			if (_killaura != null)
			{
				KillauraToggle.IsOn = _killaura.IsEnabled;
				KillauraPlayersToggle.IsOn = _killaura.PlayerOnly.Value;
				KillauraMobsToggle.IsOn = _killaura.MobsOnly.Value;
				KillauraRangeSlider.Value = _killaura.Range.Value;
				KillauraCpsSlider.Value = _killaura.Cps.Value;
				KillauraRangeText.Text = $"{_killaura.Range.Value:F1} 格";
				KillauraCpsText.Text = $"{_killaura.Cps.Value} CPS";
				KillauraStatusText.Text = (_killaura.IsEnabled ? $"运行中 · 已触发 {_killaura.AttackCount} 次 · 最近目标：{((_killaura.LastTarget.Length == 0) ? "等待实体" : _killaura.LastTarget)}" : "已关闭 · 可先调整范围、目标和 CPS，再开启。");
			}
			if (_scaffold != null)
			{
				ScaffoldToggle.IsOn = _scaffold.IsEnabled;
				ScaffoldRateSlider.Value = _scaffold.Rate.Value;
				ScaffoldRateText.Text = $"{_scaffold.Rate.Value}/秒";
				ScaffoldStatusText.Text = (_scaffold.IsEnabled ? $"运行中 · 已尝试放置 {_scaffold.PlaceCount} 次 · 最近位置：{_scaffold.LastPlacement}" : "已关闭 · 手持方块后再开启。");
			}
		}
		finally
		{
			_updating = false;
		}
	}

	private void UpdateEnabledCount()
	{
		int value = ModuleManager.Modules.Count((LuminaModule m) => m.IsEnabled);
		EnabledCountText.Text = $"{value}/{ModuleManager.Modules.Count} 已启用";
	}

	private void Killaura_Toggled(object sender, RoutedEventArgs e)
	{
		if (!_updating && _killaura != null)
		{
			_killaura.IsEnabled = KillauraToggle.IsOn;
			ModuleManager.SaveState();
			SyncControls();
			UpdateEnabledCount();
		}
	}

	private void KillauraTargets_Toggled(object sender, RoutedEventArgs e)
	{
		if (!_updating && _killaura != null)
		{
			_killaura.PlayerOnly.Value = KillauraPlayersToggle.IsOn;
			_killaura.MobsOnly.Value = KillauraMobsToggle.IsOn;
			ModuleManager.SaveState();
		}
	}

	private void KillauraRange_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (!_updating && _killaura != null)
		{
			_killaura.Range.Value = (float)e.NewValue;
			KillauraRangeText.Text = $"{e.NewValue:F1} 格";
			ModuleManager.SaveState();
		}
	}

	private void KillauraCps_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (!_updating && _killaura != null)
		{
			int value = (int)Math.Round(e.NewValue);
			_killaura.Cps.Value = value;
			KillauraCpsText.Text = $"{value} CPS";
			ModuleManager.SaveState();
		}
	}

	private void ModuleToggle_Toggled(object sender, RoutedEventArgs e)
	{
		if (!_updating && sender is ToggleSwitch { Tag: string tag } toggleSwitch)
		{
			LuminaModule? luminaModule = ModuleManager.Get(tag);
			if (luminaModule != null)
			{
				luminaModule.IsEnabled = toggleSwitch.IsOn;
			}
			UpdateEnabledCount();
		}
	}

	private void Scaffold_Toggled(object sender, RoutedEventArgs e)
	{
		if (!_updating && _scaffold != null)
		{
			_scaffold.IsEnabled = ScaffoldToggle.IsOn;
			ModuleManager.SaveState();
			SyncControls();
			UpdateEnabledCount();
		}
	}

	private void ScaffoldParameters_Click(object sender, RoutedEventArgs e)
	{
		bool flag = ScaffoldParametersPanel.Visibility != Visibility.Visible;
		ScaffoldParametersPanel.Visibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		ScaffoldParametersButton.Content = (flag ? "收起参数" : "展开参数");
	}

	private void ScaffoldRate_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (!_updating && _scaffold != null)
		{
			int value = (int)Math.Round(e.NewValue);
			_scaffold.Rate.Value = value;
			ScaffoldRateText.Text = $"{value}/秒";
			ModuleManager.SaveState();
		}
	}

	private void NightVision_Toggled(object sender, RoutedEventArgs e)
	{
		if (!_updating)
		{
			LuminaModule? luminaModule = ModuleManager.Get("NightVision");
			if (luminaModule != null)
			{
				luminaModule.IsEnabled = NightVisionToggle.IsOn;
			}
			UpdateEnabledCount();
		}
	}

	private async Task RefreshAsync()
	{
		try
		{
			JsonElement jsonElement = await BridgeService.InvokeAsync("pe:modulesStatus");
			BridgeStateText.Text = (jsonElement.TryGetProperty("bridge", out var _) ? "模块桥在线" : "等待游戏数据");
			if (jsonElement.TryGetProperty("traffic", out var value2) && value2.TryGetProperty("lastPacket", out var value3))
			{
				string value4 = (value3.TryGetProperty("direction", out var value5) ? (value5.GetString() ?? "") : "");
				string value6 = (value3.TryGetProperty("packetName", out var value7) ? (value7.GetString() ?? "") : "");
				int value8 = (value3.TryGetProperty("packetId", out var value9) ? value9.GetInt32() : 0);
				LastPacketText.Text = $"最新解密包：{value4}  {value6}  ·  0x{value8:X}";
			}
			SyncControls();
		}
		catch
		{
			BridgeStateText.Text = "等待后端";
		}
	}

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.DataTransfer;

namespace OpenNEL.WinUI.Pages;

public sealed partial class InspectorPage : Page
{
	private record FileItem
	{
		public string Name { get; set; } = "";

		public string Path { get; set; } = "";
	}

	private record PacketRow
	{
		public string PacketName { get; set; } = "";

		public string PacketIdHex { get; set; } = "";

		public string Direction { get; set; } = "";

		public JsonElement Raw { get; set; }
	}

	private List<JsonElement> _all = new List<JsonElement>();

	private JsonElement _selected;

	private string _capturesDir = Path.Combine(AppContext.BaseDirectory, "data", "pe_captures");

	public InspectorPage()
	{
		InitializeComponent();
		base.Loaded += async delegate
		{
			await LoadFileListAsync();
		};
	}

	private async Task LoadFileListAsync()
	{
		try
		{
			string[] obj = new string[3]
			{
				_capturesDir,
				Path.Combine(AppContext.BaseDirectory, "..", "data", "pe_captures"),
				Path.GetFullPath(Path.Combine(_capturesDir, "..", "..", "..", "data", "pe_captures"))
			};
			List<FileItem> list = (from f in (from f in Directory.GetFiles(_capturesDir = obj.FirstOrDefault(Directory.Exists) ?? _capturesDir, "plain_*.jsonl")
					orderby f descending
					select f).Take(20).ToList()
				select new FileItem
				{
					Name = Path.GetFileName(f),
					Path = f
				}).ToList();
			FileCombo.ItemsSource = list;
			FileCombo.DisplayMemberPath = "Name";
			if (list.Count > 0)
			{
				FileCombo.SelectedIndex = 0;
			}
		}
		catch
		{
		}
	}

	private async void FileCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!(FileCombo.SelectedItem is FileItem fileItem))
		{
			return;
		}
		try
		{
			List<JsonElement> list = new List<JsonElement>();
			string[] array = await File.ReadAllLinesAsync(fileItem.Path);
			foreach (string text in array)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					try
					{
						list.Add(JsonDocument.Parse(text).RootElement.Clone());
					}
					catch
					{
					}
					if (list.Count > 5000)
					{
						break;
					}
				}
			}
			_all = list;
			ApplyFilter();
		}
		catch (Exception ex)
		{
			EmptyText.Text = ex.Message;
		}
	}

	private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		ApplyFilter();
	}

	private void ApplyFilter()
	{
		string q = FilterBox.Text?.Trim() ?? "";
		List<JsonElement> list = (string.IsNullOrEmpty(q) ? _all : _all.Where(delegate(JsonElement j)
		{
			string obj = (j.TryGetProperty("PacketName", out var value) ? (value.GetString() ?? "") : "");
			string text = (j.TryGetProperty("PacketId", out var value2) ? $"0x{value2.GetInt32():X}" : "");
			return obj.Contains(q, StringComparison.OrdinalIgnoreCase) || text.Contains(q, StringComparison.OrdinalIgnoreCase);
		}).ToList());
		PacketList.ItemsSource = list.Select((JsonElement j) => new PacketRow
		{
			PacketName = (j.TryGetProperty("PacketName", out var value) ? (value.GetString() ?? "") : ""),
			PacketIdHex = (j.TryGetProperty("PacketId", out var value2) ? $"0x{value2.GetInt32():X2}" : ""),
			Direction = (j.TryGetProperty("Direction", out var value3) ? (value3.GetString() ?? "") : ""),
			Raw = j
		}).ToList();
		EmptyText.Visibility = ((list.Count != 0) ? Visibility.Collapsed : Visibility.Visible);
		if (list.Count == 0)
		{
			EmptyText.Text = (string.IsNullOrEmpty(q) ? "无数据" : ("无匹配: " + q));
		}
	}

	private void PacketList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (PacketList.SelectedItem is PacketRow packetRow)
		{
			_selected = packetRow.Raw;
			DetailHeader.Text = $"{packetRow.PacketName} {packetRow.PacketIdHex} {packetRow.Direction}  BodyLength={(_selected.TryGetProperty("BodyLength", out var value) ? value.GetInt32().ToString() : "?")}";
			DetailBox.Text = JsonSerializer.Serialize(_selected, new JsonSerializerOptions
			{
				WriteIndented = true
			});
		}
	}

	private void CopyJson_Click(object sender, RoutedEventArgs e)
	{
		if (_selected.ValueKind != JsonValueKind.Undefined)
		{
			DataPackage dataPackage = new DataPackage();
			dataPackage.SetText(DetailBox.Text);
			Clipboard.SetContent(dataPackage);
		}
	}

	private void CopyBody_Click(object sender, RoutedEventArgs e)
	{
		if (_selected.TryGetProperty("BodyBase64", out var value))
		{
			DataPackage dataPackage = new DataPackage();
			dataPackage.SetText(value.GetString() ?? "");
			Clipboard.SetContent(dataPackage);
		}
	}

	private void Refresh_Click(object sender, RoutedEventArgs e)
	{
		_ = LoadFileListAsync();
	}

	private void OpenFolder_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = _capturesDir,
				UseShellExecute = true
			});
		}
		catch
		{
		}
	}

}

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenNEL.WinUI.Modules;

public abstract class LuminaModule : INotifyPropertyChanged
{
	private bool _isEnabled;

	protected LuminaModule(string name, ModuleCategory category, string displayName, string description)
	{
		Name = name;
		Category = category;
		DisplayName = displayName;
		Description = description;
	}

	public string Name { get; }

	public ModuleCategory Category { get; }

	public string DisplayName { get; }

	public string Description { get; }

	public bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled == value)
			{
				return;
			}

			_isEnabled = value;
			OnPropertyChanged();
			ModuleManager.SaveState();
		}
	}

	public List<ModuleSetting> Settings { get; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	protected T AddSetting<T>(T setting) where T : ModuleSetting
	{
		Settings.Add(setting);
		return setting;
	}

	public JsonElement ToJson()
	{
		Dictionary<string, object> values = new();
		foreach (ModuleSetting setting in Settings)
		{
			values[setting.Name] = setting.ToJsonValue();
		}

		return JsonSerializer.SerializeToElement(new { enabled = IsEnabled, values });
	}

	public void FromJson(JsonElement value)
	{
		if (value.TryGetProperty("enabled", out JsonElement enabled) &&
			enabled.ValueKind is JsonValueKind.True or JsonValueKind.False)
		{
			_isEnabled = enabled.GetBoolean();
			OnPropertyChanged(nameof(IsEnabled));
		}

		if (!value.TryGetProperty("values", out JsonElement values) || values.ValueKind != JsonValueKind.Object)
		{
			return;
		}

		foreach (ModuleSetting setting in Settings)
		{
			if (values.TryGetProperty(setting.Name, out JsonElement settingValue))
			{
				setting.FromJson(settingValue);
			}
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

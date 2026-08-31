using System.Text.Json;

namespace OpenNEL.WinUI.Modules;

public sealed class BoolSetting : ModuleSetting
{
	private bool _value;

	public BoolSetting(string name, bool defaultValue, string displayName)
		: base(name, displayName)
	{
		_value = defaultValue;
	}

	public bool Value
	{
		get => _value;
		set
		{
			if (_value == value)
			{
				return;
			}

			_value = value;
			OnPropertyChanged();
		}
	}

	public override object ToJsonValue() => Value;

	public override void FromJson(JsonElement value)
	{
		if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
		{
			Value = value.GetBoolean();
		}
	}
}

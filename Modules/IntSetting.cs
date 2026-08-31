using System;
using System.Text.Json;

namespace OpenNEL.WinUI.Modules;

public sealed class IntSetting : ModuleSetting
{
	private int _value;

	public IntSetting(string name, int defaultValue, int minimum, int maximum, string displayName)
		: base(name, displayName)
	{
		Minimum = minimum;
		Maximum = maximum;
		_value = Math.Clamp(defaultValue, minimum, maximum);
	}

	public int Minimum { get; }

	public int Maximum { get; }

	public int Value
	{
		get => _value;
		set
		{
			int clamped = Math.Clamp(value, Minimum, Maximum);
			if (_value == clamped)
			{
				return;
			}

			_value = clamped;
			OnPropertyChanged();
		}
	}

	public override object ToJsonValue() => Value;

	public override void FromJson(JsonElement value)
	{
		if (value.TryGetInt32(out int parsed))
		{
			Value = parsed;
		}
	}
}

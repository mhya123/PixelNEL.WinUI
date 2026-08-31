using System;
using System.Text.Json;

namespace OpenNEL.WinUI.Modules;

public sealed class FloatSetting : ModuleSetting
{
	private float _value;

	public FloatSetting(string name, float defaultValue, float minimum, float maximum, string displayName)
		: base(name, displayName)
	{
		Minimum = minimum;
		Maximum = maximum;
		_value = Math.Clamp(defaultValue, minimum, maximum);
	}

	public float Minimum { get; }

	public float Maximum { get; }

	public float Value
	{
		get => _value;
		set
		{
			float clamped = Math.Clamp(value, Minimum, Maximum);
			if (Math.Abs(_value - clamped) < 0.0001f)
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
		if (value.TryGetSingle(out float parsed))
		{
			Value = parsed;
		}
	}
}

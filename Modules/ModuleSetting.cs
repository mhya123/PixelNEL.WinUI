using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenNEL.WinUI.Modules;

public abstract class ModuleSetting : INotifyPropertyChanged
{
	protected ModuleSetting(string name, string displayName)
	{
		Name = name;
		DisplayName = displayName;
	}

	public string Name { get; }

	public string DisplayName { get; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public abstract object ToJsonValue();

	public abstract void FromJson(JsonElement value);

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

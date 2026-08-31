using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenNEL.WinUI.Modules.Combat;
using OpenNEL.WinUI.Modules.Misc;
using OpenNEL.WinUI.Modules.Motion;
using OpenNEL.WinUI.Modules.Visual;
using OpenNEL.WinUI.Modules.World;

namespace OpenNEL.WinUI.Modules;

public static class ModuleManager
{
	private static readonly List<LuminaModule> RegisteredModules = new();
	private static readonly object StateLock = new();
	private static bool _initialized;

	public static IReadOnlyList<LuminaModule> Modules => RegisteredModules;

	public static void Initialize()
	{
		lock (StateLock)
		{
			if (_initialized)
			{
				return;
			}

			_initialized = true;
			RegisteredModules.AddRange(new LuminaModule[]
			{
				new KillauraModule(),
				new AutoArmorModule(),
				new NightVisionModule(),
				new FullBrightModule(),
				new NoHurtCameraModule(),
				new FreeCameraModule(),
				new ESPModule(),
				new NoFireModule(),
				new FlyModule(),
				new SpeedModule(),
				new BhopModule(),
				new AirJumpModule(),
				new SpiderModule(),
				new StepModule(),
				new ScaffoldModule(),
				new JesusModule(),
				new NoClipModule(),
				new ChestStealerModule(),
				new InvHelperModule(),
				new AntiKickModule(),
				new PingSpoofModule(),
				new PacketLoggerModule()
			});
		}

		LoadState();
	}

	public static LuminaModule? Get(string name)
	{
		Initialize();
		return RegisteredModules.FirstOrDefault(module =>
			string.Equals(module.Name, name, StringComparison.Ordinal));
	}

	public static void SaveState()
	{
		if (!_initialized)
		{
			return;
		}

		try
		{
			lock (StateLock)
			{
				string dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
				Directory.CreateDirectory(dataDirectory);
				Dictionary<string, JsonElement> state = RegisteredModules.ToDictionary(
					module => module.Name,
					module => module.ToJson(),
					StringComparer.Ordinal);
				string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(Path.Combine(dataDirectory, "winui_modules.json"), json);
			}
		}
		catch
		{
		}
	}

	public static void LoadState()
	{
		try
		{
			string statePath = Path.Combine(AppContext.BaseDirectory, "data", "winui_modules.json");
			if (!File.Exists(statePath))
			{
				return;
			}

			using JsonDocument document = JsonDocument.Parse(File.ReadAllText(statePath));
			foreach (JsonProperty property in document.RootElement.EnumerateObject())
			{
				RegisteredModules.FirstOrDefault(module =>
					string.Equals(module.Name, property.Name, StringComparison.Ordinal))?.FromJson(property.Value);
			}
		}
		catch
		{
		}
	}
}

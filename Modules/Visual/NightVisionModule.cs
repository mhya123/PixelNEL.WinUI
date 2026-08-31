namespace OpenNEL.WinUI.Modules.Visual;

public sealed class NightVisionModule : LuminaModule
{
	public NightVisionModule()
		: base("NightVision", ModuleCategory.Visual, "Night Vision", "夜视模块的界面配置。核心协议执行逻辑已移除。")
	{
		Amplifier = AddSetting(new IntSetting("Amplifier", 1, 1, 5, "Amplifier"));
	}

	public IntSetting Amplifier { get; }
}

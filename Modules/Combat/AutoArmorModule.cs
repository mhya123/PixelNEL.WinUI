namespace OpenNEL.WinUI.Modules.Combat;

public sealed class AutoArmorModule : LuminaModule
{
	public AutoArmorModule()
		: base("AutoArmor", ModuleCategory.Combat, "自动穿甲", "自动穿甲的界面配置。核心协议执行逻辑已移除。")
	{
		Delay = AddSetting(new IntSetting("Delay", 100, 50, 1000, "每次装备间隔 ms"));
		PreferProtection = AddSetting(new BoolSetting("Prefer Protection", true, "优先高阶材质"));
	}

	public IntSetting Delay { get; }
	public BoolSetting PreferProtection { get; }
	public int EquippedCount { get; private set; }
}

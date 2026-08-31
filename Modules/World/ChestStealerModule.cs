namespace OpenNEL.WinUI.Modules.World;

public sealed class ChestStealerModule : LuminaModule
{
	public ChestStealerModule()
		: base("ChestStealer", ModuleCategory.World, "自动偷箱", "容器转移的界面配置。核心协议执行逻辑已移除。")
	{
		Delay = AddSetting(new IntSetting("Delay", 75, 25, 1000, "每格间隔 ms"));
		AutoClose = AddSetting(new BoolSetting("Auto Close", true, "清空后关闭容器"));
	}

	public IntSetting Delay { get; }
	public BoolSetting AutoClose { get; }
	public int StolenCount { get; private set; }
}

namespace OpenNEL.WinUI.Modules.World;

public sealed class ScaffoldModule : LuminaModule
{
	public ScaffoldModule()
		: base("Scaffold", ModuleCategory.World, "Scaffold", "自动搭路模块的界面配置。核心协议执行逻辑已移除。")
	{
		Rate = AddSetting(new IntSetting("Rate", 12, 1, 20, "每秒放置次数"));
		Extend = AddSetting(new IntSetting("Extend", 3, 0, 6, "前向延伸格数"));
	}

	public IntSetting Rate { get; }
	public IntSetting Extend { get; }
	public int PlaceCount { get; private set; }
	public string LastPlacement { get; private set; } = "等待游戏状态";
}

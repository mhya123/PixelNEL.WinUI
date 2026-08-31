namespace OpenNEL.WinUI.Modules.World;

public sealed class InvHelperModule : LuminaModule
{
	public InvHelperModule()
		: base("InvHelper", ModuleCategory.World, "背包助手", "背包整理的界面配置。核心协议执行逻辑已移除。")
	{
		Delay = AddSetting(new IntSetting("Delay", 3000, 250, 10000, "整理间隔 ms"));
		OrganizeTools = AddSetting(new BoolSetting("Organize Tools", true, "工具移入快捷栏"));
		OrganizeBlocks = AddSetting(new BoolSetting("Organize Blocks", true, "方块移入快捷栏"));
		DropUseless = AddSetting(new BoolSetting("Drop Useless", false, "丢弃常见无用物品"));
	}

	public IntSetting Delay { get; }
	public BoolSetting OrganizeTools { get; }
	public BoolSetting OrganizeBlocks { get; }
	public BoolSetting DropUseless { get; }
	public int Actions { get; private set; }
}

namespace OpenNEL.WinUI.Modules.Misc;

public sealed class PacketLoggerModule : LuminaModule
{
	public PacketLoggerModule()
		: base("PacketLogger", ModuleCategory.Misc, "抓包日志", "抓包日志的界面状态。核心协议执行逻辑已移除。")
	{
	}
}

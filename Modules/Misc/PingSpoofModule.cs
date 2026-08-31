namespace OpenNEL.WinUI.Modules.Misc;

public sealed class PingSpoofModule : LuminaModule
{
	public PingSpoofModule()
		: base("PingSpoof", ModuleCategory.Misc, "延迟伪装", "延迟伪装的界面配置。核心协议执行逻辑已移除。")
	{
		Delay = AddSetting(new IntSetting("DelayMs", 100, 0, 1000, "延迟"));
	}

	public IntSetting Delay { get; }
}

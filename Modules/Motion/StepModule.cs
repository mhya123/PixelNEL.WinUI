namespace OpenNEL.WinUI.Modules.Motion;

public sealed class StepModule : LuminaModule
{
	public StepModule()
		: base("Step", ModuleCategory.Motion, "高跳台阶", "台阶高度的界面配置。核心协议执行逻辑已移除。")
	{
		Height = AddSetting(new IntSetting("Height", 1, 1, 10, "高度"));
	}

	public IntSetting Height { get; }
}

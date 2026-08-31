namespace OpenNEL.WinUI.Modules.Motion;

public sealed class FlyModule : LuminaModule
{
	public FlyModule()
		: base("Fly", ModuleCategory.Motion, "飞行", "飞行模块的界面配置。核心协议执行逻辑已移除。")
	{
		Speed = AddSetting(new IntSetting("Speed", 1, 1, 5, "速度"));
	}

	public IntSetting Speed { get; }
}

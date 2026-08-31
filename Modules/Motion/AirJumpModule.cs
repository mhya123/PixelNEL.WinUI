namespace OpenNEL.WinUI.Modules.Motion;

public sealed class AirJumpModule : LuminaModule
{
	public AirJumpModule()
		: base("AirJump", ModuleCategory.Motion, "空中跳跃", "空中跳跃的界面配置。核心协议执行逻辑已移除。")
	{
		VerticalVelocity = AddSetting(new FloatSetting("Velocity", 0.42f, 0.1f, 1.2f, "上升速度"));
		Cooldown = AddSetting(new IntSetting("Cooldown", 120, 40, 1000, "触发冷却 ms"));
	}

	public FloatSetting VerticalVelocity { get; }
	public IntSetting Cooldown { get; }
	public int AppliedJumps { get; private set; }
}

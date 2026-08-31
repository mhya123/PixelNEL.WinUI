namespace OpenNEL.WinUI.Modules.Combat;

public sealed class KillauraModule : LuminaModule
{
	public KillauraModule()
		: base("Killaura", ModuleCategory.Combat, "Killaura", "自动攻击模块的界面配置。核心协议执行逻辑已移除。")
	{
		PlayerOnly = AddSetting(new BoolSetting("Players", true, "攻击玩家"));
		MobsOnly = AddSetting(new BoolSetting("Mobs", true, "攻击生物"));
		Range = AddSetting(new FloatSetting("Range", 3.7f, 2f, 7f, "范围"));
		Delay = AddSetting(new IntSetting("Delay", 1, 1, 20, "节流"));
		Cps = AddSetting(new IntSetting("CPS", 12, 1, 20, "每秒攻击次数"));
		Packets = AddSetting(new IntSetting("Packets", 1, 1, 10, "每次操作数"));
		TpAura = AddSetting(new BoolSetting("TP Aura", false, "传送光环"));
		Strafe = AddSetting(new BoolSetting("Strafe", false, "环绕"));
		TpBehind = AddSetting(new BoolSetting("Teleport Behind", false, "传送到背后"));
		KeepDistance = AddSetting(new FloatSetting("Keep Distance", 2f, 1f, 5f, "保持距离"));
		TpSpeed = AddSetting(new IntSetting("TP Speed", 500, 100, 2000, "传送冷却 ms"));
		StrafeSpeed = AddSetting(new FloatSetting("Strafe Speed", 1f, 0.1f, 2f, "环绕速度"));
		StrafeRadius = AddSetting(new FloatSetting("Strafe Radius", 1f, 0.1f, 5f, "环绕半径"));
		MultiTarget = AddSetting(new BoolSetting("Multi Target", false, "多目标"));
	}

	public BoolSetting PlayerOnly { get; }
	public BoolSetting MobsOnly { get; }
	public FloatSetting Range { get; }
	public IntSetting Delay { get; }
	public IntSetting Cps { get; }
	public IntSetting Packets { get; }
	public BoolSetting TpAura { get; }
	public BoolSetting Strafe { get; }
	public BoolSetting TpBehind { get; }
	public FloatSetting KeepDistance { get; }
	public IntSetting TpSpeed { get; }
	public FloatSetting StrafeSpeed { get; }
	public FloatSetting StrafeRadius { get; }
	public BoolSetting MultiTarget { get; }
	public int AttackCount { get; private set; }
	public string LastTarget { get; private set; } = "";
}

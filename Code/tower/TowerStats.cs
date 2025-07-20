using Sandbox;
using System;

public struct TowerUpgrade
{
	public int Cost { get; set; }
	public int AddDamage { get; set; }
	public int ReduceFireRate { get; set; }
	public float AddRange { get; set; }
}


public sealed class TowerStats : Component
{
	[Header("Basic")]
	[Property] public string DisplayName { get; set; } = "Basic Tower";
	[Property] public string Description { get; set; } = "A basic tower that shoots targets";
	[Property] public int Cost { get; set; } = 1;

	[Header("Attacks")]
	[Property] public float Damage { get; set; } = 1.0f;
	[Property] public float FireRate { get; set; } = 1.0f;
	[Property] public float Range { get; set; } = 48.0f;

	public enum AttackMethod
	{
		Single, //Attacks one target
		Area, //Attacks in area
		Chained //Attacks one then to the other
	}

	[Flags]
	public enum Ability
	{
		CanSeeHidden = 1 << 0, //Can see cloaked targets
		CanTargetFlying = 1 << 1, //Can target flying enemies
	}

	[Property] public AttackMethod Method { get; set; } = AttackMethod.Single;
	[Property] public Ability Abilities { get; set; }

	[Property] public bool ChargesAttack { get; set; } = false;

	[Property, ShowIf("ChargesAttack", true)] public float ChargeTime { get; set; } = 1.0f;

	[Header("Upgrades")]
	[Property, InlineEditor] public TowerUpgrade[] Upgrades { get; set; }

	[Header( "Sounds" )]
	[Property] public SoundEvent FireSound { get; set; }
	[Property] public SoundEvent ChargeSound { get; set; }
	[Property] public SoundEvent UpgradeSound { get; set; }

	protected override void DrawGizmos()
	{
		DebugOverlay.Sphere(new Sphere(WorldPosition, Range), Color.White);
	}
}

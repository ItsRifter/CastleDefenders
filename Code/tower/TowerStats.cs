using Sandbox;
using System;

public struct TowerUpgrade
{
	public int Cost { get; set; }
	public int AddDamage { get; set; }
	public int IncreaseFireRate { get; set; }
	public float AddRange { get; set; }


	[Description("Adds an additional target for a chain attack")] public int AddChainAttacks { get; set; }
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
		Area, //Attacks in area, ideal for explosions
		Chained //Attacks one then to the other
	}

	[Flags]
	public enum Ability
	{
		CanSeeHidden = 1 << 0, //Can see cloaked targets
		CanTargetFlying = 1 << 1, //Can target flying enemies
	}

	[Property] public AttackMethod FireType { get; set; } = AttackMethod.Single;
	[Property] public Ability Abilities { get; set; }

	[Property] public bool ChargesAttack { get; set; } = false;

	[Property, ShowIf("ChargesAttack", true)] public float ChargeTime { get; set; } = 1.0f;

	[Header("Upgrades")]
	[Property, InlineEditor] public TowerUpgrade[] Upgrades { get; set; }

	[Header( "Sounds" )]
	[Property] public SoundEvent FireSound { get; set; }
	[Property, ShowIf( "ChargesAttack", true )] public SoundEvent ChargeSound { get; set; }
	[Property] public SoundEvent UpgradeSound { get; set; }

	protected override void DrawGizmos()
	{
		DebugOverlay.Sphere(new Sphere(WorldPosition, Range), Color.White);
	}
}

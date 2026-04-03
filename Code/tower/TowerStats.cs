using Sandbox;
using System;

public struct TowerUpgrade
{
	[Description("Cost of this upgrade")]
	public int Cost { get; set; }

	[Description("Damage to add to existing value")]
	public int AddDamage { get; set; }

	[Description("Fire rate to add to existing value")]
	public int IncreaseFireRate { get; set; }

	[Description("Range to add to existing value")]
	public float AddRange { get; set; }

	[Description("Adds an additional target for a chain attack"), Category( "Special" )] 
	public int AddChainAttacks { get; set; }

	[Description("Adds an additional target for a chain attack"), Category( "Special" )] 
	public int AddExplosionRadius { get; set; }

	[Description("Adds additional time to freeze duration"), Category( "Special" )]
	public float AddFreezeDuration { get; set; }
}

public sealed class TowerStats : Component
{
	[Header("Basic")]
	[Property] public string DisplayName { get; set; } = "Basic Tower";
	[Property] public string Description { get; set; } = "A basic tower that attacks targets";
	[Property] public int Cost { get; set; } = 1;
	[Property, ImageAssetPath, Description("CSS class name used to display this tower's icon in the selection UI (e.g. 'pistol', 'cannon')")]
	public string Icon { get; set; } = "default";

	[Property, Description("Is this tower a radar scanning type? this will lock the tower from performing attacks")] public bool IsRadar { get; set; } = false;

	[Header("Attacks")]
	[Property, HideIf( "IsRadar", true )] public int Damage { get; set; } = 1;
	[Property, HideIf( "IsRadar", true )] public float FireRate { get; set; } = 1.0f;
	[Property] public float Range { get; set; } = 48.0f;
	[Property, HideIf( "IsRadar", true ), Description("The attack explosion radius of this tower, provided its attack method is 'Area'")] 
	public float ExplosionRange { get; set; } = 48.0f;

	[Property, HideIf( "IsRadar", true ), Description("How many chain attacks can this tower initially perform, provided its attack method is 'Chained'")] 
	public int ChainCount { get; set; } = 0;
	
	[Property, HideIf( "IsRadar", true ), Description( "The range of each chained attack, provided its attack method is 'Chained'" )] 
	public int ChainRange { get; set; } = 0;

	public enum AttackMethod
	{
		Single, //Attacks one target
		Area, //Attacks in area, ideal for explosions
		Chained //Attacks one then to the other
	}

	[Flags]
	public enum Ability
	{
		CanSeeHidden = 1 << 0,
		CanRevealHidden = 1 << 1,
		CanFreezeTargets = 2 << 1,

		//CanTargetFlying = 1 << 1,
	}

	[Property] public AttackMethod FireType { get; set; } = AttackMethod.Single;
	[Property] public Ability Abilities { get; set; }

	[Property] public bool ChargesAttack { get; set; } = false;

	[Property, ShowIf("ChargesAttack", true)] public float ChargeTime { get; set; } = 1.0f;
	[Property, ShowIf("IsRadar", true)] public float AddRadarRange { get; set; }

	[Header("Upgrades")]
	[Property, InlineEditor( Label = false )] public TowerUpgrade[] Upgrades { get; set; }

	[Header( "Sounds" )]
	[Property] public SoundEvent FireSound { get; set; }
	[Property, ShowIf( "ChargesAttack", true )] public SoundEvent ChargeSound { get; set; }
	[Property] public SoundEvent UpgradeSound { get; set; }
	protected override void DrawGizmos()
	{
		DebugOverlay.Sphere(new Sphere(WorldPosition, Range), Color.White);
	}
}

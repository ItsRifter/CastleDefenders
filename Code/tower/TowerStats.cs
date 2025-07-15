using Sandbox;
using System;

public sealed class TowerStats : Component
{
	[Header("Basic")]
	[Property] public string DisplayName { get; set; } = "Basic Tower";
	[Property] public string Description { get; set; } = "A basic tower that shoots targets";
	[Property] public int Cost { get; set; } = 1;

	[Header("Statistics")]
	[Property] public float Damage { get; set; } = 1.0f;
	[Property] public float FireRate { get; set; } = 1.0f;
	[Property] public float Range { get; set; } = 48.0f;

	[Header("Sounds")]
	[Property] public SoundEvent FireSound { get; set; }

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

	protected override void DrawGizmos()
	{
		DebugOverlay.Sphere(new Sphere(WorldPosition, Range), Color.White);
	}
}

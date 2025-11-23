using Sandbox;
using System;

public sealed class EnemyStats : Component
{
	[Header( "Basic" )]
	[Property] public string DisplayName { get; set; } = "Basic Enemy";
	[Property, TextArea] public string Description { get; set; } = "A basic enemy whose purpose is to destroy the castle";

	[Header( "Statistics" )]
	[Property] public int Damage { get; set; } = 1;
	[Property] public int Health { get; set; } = 5;
	[Property] public float Speed { get; set; } = 1.0f;
	[Property] public int CashReward { get; set; } = 10;
	[Property] public bool IsBossType { get; set; } = false;

	[Flags]
	public enum EnemyAbility
	{
		//CanFly = 1 << 0,
		Regenerative = 1 << 0,
		Cloaked = 1 << 1,
		SplitsOnDeath = 1 << 2,
		Armoured = 1 << 3,
	}

	[Property] public EnemyAbility Abilities { get; set; }

	[Property, MinMax(1, 5)] public int SplitCount { get; set; } = 2;

	[Property] public int ArmourValue { get; set; } = 5;

	[Property, Description("The pieces of armor that when broken will remove these clothing")] 
	public ClothingContainer.ClothingEntry[] ArmouredClothing { get; set; }
}

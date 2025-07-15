using Sandbox;

public sealed class EnemyStats : Component
{
	[Header( "Basic" )]
	[Property] public string DisplayName { get; set; } = "Basic Enemy";
	[Property] public string Description { get; set; } = "A basic enemy whose purpose is to destroy the castle";
	[Property] public int Damage { get; set; } = 1;


	[Property] public float Health { get; set; } = 5.0f;
	[Property] public float Speed { get; set; } = 1.0f;
}

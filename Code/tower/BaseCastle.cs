using Sandbox;

public sealed class BaseCastle : Component
{
	public static BaseCastle Castle { get; private set; }

	public int Health { get; private set; }

	protected override void OnStart()
	{
		Castle = this;
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;

		if ( Health <= 0 )
		{
			GameObject.Destroy();
			//TODO: Game end logic
		}
	}
}

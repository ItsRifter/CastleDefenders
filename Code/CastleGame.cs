using Sandbox;

public sealed class CastleGame : Component
{
	public static CastleGame Instance { get; private set; }

	[Property] public GameObject PistolPrefab { get; set; }
	[Property] public GameObject SMGPrefab { get; set; }
	[Property] public GameObject ShotgunPrefab { get; set; }

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{

	}
}

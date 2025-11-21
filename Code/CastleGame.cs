using Sandbox;
using System;

public sealed class CastleGame : Component
{
	public static CastleGame Instance { get; private set; }

	[Property] public GameObject PistolPrefab { get; set; }
	[Property, Title("SMG Prefab")] public GameObject SmgPrefab { get; set; }
	[Property] public GameObject ShotgunPrefab { get; set; }
	[Property] public GameObject ElectricPrefab { get; set; }

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{

	}

	public static async void AwaitAction(float time, Action action)
	{
		await GameTask.DelaySeconds(time);
		action?.Invoke();
	}

	[ConCmd("cd.npc.spawn", ConVarFlags.Cheat)]
	public static void CMD_SpawnNPC(string name = "Dummy", int count = 1, float delay = 1.0f)
	{
		var npc = PrefabScene.GetPrefab($"prefabs/enemy/{name}.prefab");

		for ( int i = 0; i < count; i++ )
		{
			AwaitAction( i * delay, () => npc.Clone() );
		}
	}

	[ConCmd("cd.player.money", ConVarFlags.Cheat)]
	public static void CMD_GiveMoney(int amount = 1)
	{
		foreach ( var ply in Instance.Scene.GetAllObjects(true).Where(p => p.GetComponent<CastlePlayer>() != null).ToList())
		{
			ply.GetComponent<CastlePlayer>().AddMoney( amount );
			Log.Info( $"Added {amount} money to {ply.Network?.Owner?.DisplayName ?? "HOST"}" );
		}
	}
}

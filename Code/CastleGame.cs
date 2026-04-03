using Sandbox;
using System;

public sealed class CastleGame : Component
{
	public static CastleGame Instance { get; private set; }

	[Header("Towers")]
	[Property, Description("All placeable tower prefabs, in the order they appear in the selection bar (slot 1, 2, 3...)")]
	public List<GameObject> TowerPrefabs { get; set; } = new();

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

	[ConCmd("cd.game.start", ConVarFlags.Cheat)]
	public static void CMD_StartGame()
	{
		WaveManager.Instance.Wave = WaveManager.WaveState.Idle;
	}

	[ConCmd("cd.game.wave.start", ConVarFlags.Cheat)]
	public static void CMD_StartWave()
	{
		if ( WaveManager.Instance.Wave == WaveManager.WaveState.Inactive ) return;

		WaveManager.Instance.Wave = WaveManager.WaveState.Active;
	}
}

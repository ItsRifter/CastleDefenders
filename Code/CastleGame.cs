using Sandbox;
using System;

public sealed class CastleGame : Component
{
	public static CastleGame Instance { get; private set; }

	[Property] public GameObject PistolPrefab { get; set; }
	[Property, Title("SMG Prefab")] public GameObject SmgPrefab { get; set; }
	[Property] public GameObject ShotgunPrefab { get; set; }

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
	public static void CMD_SpawnNPC(string name = "Dummy")
	{
		var npc = PrefabScene.GetPrefab($"prefabs/Enemy/{name}.prefab");
		npc.Clone();
	}
}

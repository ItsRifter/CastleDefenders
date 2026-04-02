using Sandbox;
using System;
using static Sandbox.Gizmo;

public struct EnemyInfo
{
	[Property] public GameObject Enemy { get; set; }
	[Property] public int SpawnCount { get; set; }
	[Property] public float SpawnInterval { get; set; }
	[Property] public float InitialSpawnDelay { get; set; }
}

public struct WaveInfo
{
	[Description("Helper field to identify this wave")] public string Name { get; set; } = "[Wave Identifier here]";
	[Property, Title("Enemies"), Group("", StartFolded = true)] public EnemyInfo[] EnemyInfo { get; set; }

	public WaveInfo()
	{
	}
}

public sealed class WaveManager : Component
{
	[Property, InlineEditor(Label = false)] public WaveInfo[] Waves { get; set; } = new WaveInfo[0];
	
	public static WaveManager Instance { get; private set; }

	TimeUntil timer;
	TimeSince timeSinceLastState;

	PathNode startNode;

	public enum WaveState
	{
		Inactive,
		Idle,
		Active,
		Completed
	}

	public WaveState Wave;
	WaveState lastWaveState;

	int curWave;

	protected override void OnStart()
	{
		Instance = this;

		Wave = WaveState.Inactive;
		lastWaveState = Wave;

		startNode = Scene.GetAll<PathNode>().FirstOrDefault(p => p.IsStartNode);

		curWave = -1;

		if (startNode == null)
		{
			Log.Error("[Castle Defenders] No start node found");
			return;
		}

		WaveDisplay.Instance.SetStatus( Wave );
		WaveDisplay.Instance.SetWave( curWave + 1, Waves.Length );
	}

	protected override void OnUpdate()
	{
		if ( Wave == WaveState.Inactive ) return;

		if( lastWaveState != Wave )
		{
			Log.Info( $"Updating wave from {lastWaveState} to {Wave}" );
			
			timeSinceLastState = 0;
			lastWaveState = Wave;

			UpdateGame();
			timer = GetNewTime();
		}

		if( timer <= 0.0f )
			ChangeState();
	}

	void ChangeState()
	{
		switch ( Wave )
		{
			case WaveState.Idle:
				Wave = WaveState.Active;
				break;
			case WaveState.Completed:
				Wave = WaveState.Idle;
				break;
		}

		WaveDisplay.Instance.UpdateText();
	}

	void UpdateGame()
	{
		switch ( Wave )
		{
			case WaveState.Idle:
				WaveIdle();
				break;

			case WaveState.Active:
				WaveActive();
				break;

			case WaveState.Completed:
				WaveFinish();
				break;
		}
	}

	void WaveIdle()
	{
		curWave++;
	}

	int spawnsLeft = 0;

	void WaveActive()
	{
		WaveInfo waveInfo = Waves[curWave];

		spawnsLeft += waveInfo.EnemyInfo.Sum( enemy => enemy.SpawnCount );

		foreach ( var enemyInfo in waveInfo.EnemyInfo )
		{
			SpawnEnemy( enemyInfo );
		}
	}

	void WaveFinish()
	{
		if ( timer <= 0.0f )
			Wave = WaveState.Idle;
	}

	float GetNewTime()
	{
		switch( Wave )
		{
			case WaveState.Idle:
				return 45.0f;
			case WaveState.Completed:
				return 5.0f;
			
			default:
				return 0.0f;
		}
	}

	async void SpawnEnemy( EnemyInfo enemyInfo )
	{
		if (enemyInfo.Enemy == null || enemyInfo.SpawnCount <= 0 || enemyInfo.SpawnInterval <= 0)
			return;

		await GameTask.DelaySeconds(enemyInfo.InitialSpawnDelay);

		for (int i = 0; i < enemyInfo.SpawnCount; i++)
		{
			var enemy = enemyInfo.Enemy.Clone();

			enemy.WorldPosition = startNode.WorldPosition;
			enemy.WorldRotation = Rotation.LookAt(Vector3.Forward, Vector3.Up);

			spawnsLeft--;

			await GameTask.DelaySeconds(enemyInfo.SpawnInterval);
		}
	}

	public void OnEnemyDeath(CastleNPC npc)
	{
		if ( Wave == WaveState.Inactive ) return;

		bool finished = spawnsLeft <= 0 && Scene.GetAll<CastleNPC>().Count() <= 1;

		if( finished )
			Wave = WaveState.Completed;
	}
}

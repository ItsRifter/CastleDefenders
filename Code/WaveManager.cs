using Sandbox;
using static Sandbox.Gizmo;

public struct EnemyInfo
{
	[Property] public GameObject Enemy { get; set; }
	[Property] public int SpawnCount { get; set; }
	[Property] public float SpawnInterval { get; set; }
	[Property] public float InitialSpawnDelay { get; set; }

	public bool IsFinished { get; set; }
}

public struct WaveInfo
{
	[Description("Helper field to identify this wave")] public string Name { get; set; } = "[Wave Identifier here]";
	[Property, InlineEditor] public EnemyInfo[] EnemyInfo { get; set; }

	public WaveInfo()
	{
	}
}

public sealed class WaveManager : Component
{
	[Property, InlineEditor] public WaveInfo[] Waves { get; set; } = new WaveInfo[0];

	public static WaveManager Instance { get; private set; }

	TimeUntil timer;
	TimeSince timeSinceLastState;

	PathNode startNode;

	public enum WaveState
	{
		Idle,
		Active,
		Completed
	}

	WaveState waveState;
	WaveState lastWaveState;

	protected override void OnStart()
	{
		Instance = this;

		waveState = WaveState.Idle;
		lastWaveState = waveState;

		startNode = Scene.GetAll<PathNode>().FirstOrDefault(p => p.IsStartNode);
		
		if (startNode == null)
		{
			Log.Error("[Castle Defenders] No start node found");
			return;
		}
	}

	protected override void OnUpdate()
	{
		if(lastWaveState != waveState)
		{
			timeSinceLastState = 0;
			lastWaveState = waveState;

			timer = GetNewTime();
		}

		switch ( waveState )
		{
			case WaveState.Idle:
				HandleIdleState();
				break;

			case WaveState.Active:
				HandleActiveState();
				break;

			case WaveState.Completed:
				HandleCompletedState();
				break;
		}
	}

	void HandleIdleState()
	{
		if(timer <= 0.0f )
			waveState = WaveState.Active;
	}

	void HandleActiveState()
	{

	}

	void HandleCompletedState()
	{
		if ( timer <= 0.0f )
			waveState = WaveState.Idle;
	}

	float GetNewTime()
	{
		switch( waveState )
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

			await GameTask.DelaySeconds(enemyInfo.SpawnInterval);

			if(i == enemyInfo.SpawnCount-1)
				enemyInfo.IsFinished = true;
		}
	}

	public void OnEnemyDeath()
	{
		bool test = Waves.All(wave => wave.EnemyInfo.All(enemy => !enemy.IsFinished));
		Log.Info( test );
	}
}

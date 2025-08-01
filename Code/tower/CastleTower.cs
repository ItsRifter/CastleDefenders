using Sandbox;

public sealed class CastleTower : Component
{
	public TowerStats Statistics { get; private set; }

	public CastlePlayer Owner;

	public int Level { get; set; } = 1;

	TimeSince lastAttack;
	CastleNPC target;

	protected override void OnStart()
	{
		lastAttack = 0;
		Statistics = GetComponent<TowerStats>();
	}

	protected override void OnUpdate()
	{
		if( target == null )
			ScanForTarget();
		else
		{
			ValidateTarget();

			if( target != null && CanAttack() )
				Attack();
		}
	}

	public void SetOwner(CastlePlayer player) => Owner = player;

	SceneTraceResult DoRangeTrace()
	{
		var trace = Scene.Trace.Sphere( Statistics.Range, WorldPosition, WorldPosition )
			.WithTag( "Enemy" )
			.Run();

		return trace;
	}

	IEnumerable<SceneTraceResult> DoRangeTraceList()
	{
		var trace = Scene.Trace.Sphere( Statistics.Range, WorldPosition, WorldPosition )
			.WithTag( "Enemy" )
			.RunAll();

		return trace;
	}

	void ScanForTarget()
	{
		if (target != null) return;

		var trace = DoRangeTrace();

		if ( !trace.Hit ) return;

		if(trace.GameObject.GetComponent<CastleNPC>() != null)
			target = trace.GameObject.GetComponent<CastleNPC>();
	}

	void ValidateTarget()
	{
		if( !target.IsValid || target.GameObject.IsDestroyed || target.Health <= 0.0f )
			RemoveTarget();

		if( !IsTargetInRange() )
			RemoveTarget();

		if ( !CanSeeTarget() )
			RemoveTarget();
	}

	void RemoveTarget()
	{
		target = null;
	}

	bool IsTargetInRange()
	{
		if ( target == null || !target.IsValid ) return false;

		return Vector3.DistanceBetween(WorldPosition, target.WorldPosition) < Statistics.Range;
	}

	bool CanSeeTarget()
	{
		if ( target == null || !target.IsValid ) return false;

		var trace = Scene.Trace.Ray( WorldPosition + Vector3.Up * 8, target.WorldPosition + Vector3.Up * 2 )
			.IgnoreGameObject( GameObject )
			.UsePhysicsWorld()
			.Run();

		return trace.Hit && trace.GameObject == target.GameObject;
	}

	void Attack()
	{
		lastAttack = 0;

		GameObject.PlaySound(Statistics.FireSound);

		target.TakeDamage(Statistics.Damage);
	}

	bool CanAttack() => lastAttack >= Statistics.FireRate;
}

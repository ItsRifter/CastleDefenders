using Sandbox;

public sealed class CastleTower : Component
{
	public TowerStats Statistics { get; private set; }

	public CastlePlayer Owner;

	public int Level { get; set; } = 1;

	TimeSince lastAttack;

	TimeUntil chargeAttack;
	bool chargeReady;
	bool isCharging;

	CastleNPC target;

	protected override void OnStart()
	{
		lastAttack = 0;
		
		chargeAttack = 0;
		chargeReady = false;

		Statistics = GetComponent<TowerStats>();
	}

	protected override void OnUpdate()
	{
		if( target == null )
			ScanForTarget();
		else
		{
			ValidateTarget();

			if( target != null )
			{
				if ( !CanAttack() ) return;

				if ( Statistics.ChargesAttack )
				{
					if( chargeAttack <= 0 && isCharging )
					{
						chargeReady = true;
					}

					if (!chargeReady && !isCharging)
					{
						chargeAttack = Statistics.ChargeTime;
						isCharging = true;

						GameObject.PlaySound( Statistics.ChargeSound );
					}

					if ( chargeReady )
					{
						Attack();
						chargeReady = false;
						isCharging = false;
					}
				} 
				else
				{
					Attack();
				}
			}
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

		isCharging = false;
		chargeReady = false;
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

		if(Statistics.FireType == TowerStats.AttackMethod.Area)
		{

		} 
		else if ( Statistics.FireType == TowerStats.AttackMethod.Chained )
		{
			var hitTargets = new List<CastleNPC>();
			var toHit = new Queue<CastleNPC>();
			toHit.Enqueue( target );

			while ( toHit.Count > 0 && hitTargets.Count < Statistics.ChainCount )
			{
				var currentTarget = toHit.Dequeue();

				if ( currentTarget == null || !currentTarget.IsValid ) continue;
				if ( hitTargets.Contains( currentTarget ) ) continue;

				currentTarget.TakeDamage( Statistics.Damage );
				hitTargets.Add( currentTarget );

				var nearbyTraces = Scene.Trace.Sphere( Statistics.ChainRange, currentTarget.WorldPosition, currentTarget.WorldPosition )
					.WithTag( "Enemy" )
					.RunAll();

				foreach ( var trace in nearbyTraces )
				{
					var npc = trace.GameObject.GetComponent<CastleNPC>();
					if ( npc != null && !hitTargets.Contains( npc ) )
					{
						toHit.Enqueue( npc );
					}
				}
			}
		}
		else
		{
			GameObject.PlaySound( Statistics.FireSound );
			target.TakeDamage(Statistics.Damage);
		}
	}

	bool CanAttack() => lastAttack >= (1.0f / Statistics.FireRate);
}

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

	bool canRevealCloaked;
	bool canSpotCloaked;

	CastleNPC target;

	public int attackDmg;
	float fireRate;
	float range;

	protected override void OnStart()
	{
		lastAttack = 0;
		
		chargeAttack = 0;
		chargeReady = false;

		Statistics = GetComponent<TowerStats>();

		canRevealCloaked = Statistics.Abilities.HasFlag( TowerStats.Ability.CanRevealHidden );
		canSpotCloaked = Statistics.Abilities.HasFlag( TowerStats.Ability.CanSeeHidden );

		attackDmg = Statistics.Damage;
		fireRate = Statistics.FireRate;
		range = Statistics.Range;
	}

	protected override void OnUpdate()
	{
		if(Statistics.IsRadar)
		{
			DoRadarScanning();
			return;
		}

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
					DoChargeAttack();
				}
				else
				{
					Attack();
				}
			}
		}
	}

	void DoChargeAttack()
	{
		if ( chargeAttack <= 0 && isCharging )
		{
			chargeReady = true;
		}

		if ( !chargeReady && !isCharging )
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

	public void SetOwner(CastlePlayer player) => Owner = player;

	public void Upgrade(CastlePlayer player)
	{
		if ( Owner != player ) return;
		int level = Level - 1;

		if ( Statistics.Upgrades.Length <= level ) return;

		TowerUpgrade curUpgrade = Statistics.Upgrades[level];
		if ( player.Money < curUpgrade.Cost ) return;

		player.TakeMoney( curUpgrade.Cost );

		attackDmg += curUpgrade.AddDamage;
		fireRate += curUpgrade.IncreaseFireRate;
		range += curUpgrade.AddRange;

		Level++;
	}

	SceneTraceResult DoRangeTrace()
	{
		var trace = Scene.Trace.Sphere( range, WorldPosition, WorldPosition )
			.WithTag( "Enemy" )
			.Run();

		return trace;
	}

	IEnumerable<SceneTraceResult> DoRangeTraceList()
	{
		var trace = Scene.Trace.Sphere( range, WorldPosition, WorldPosition )
			.WithTag( "Enemy" )
			.RunAll();

		return trace;
	}

	void DoRadarScanning()
	{
		var traces = DoRangeTraceList();
		
		foreach ( var trace in traces )
		{
			if ( !trace.Hit ) continue;
			var npc = trace.GameObject.GetComponent<CastleNPC>();
			
			if ( npc != null )
			{
				if ( npc.HasAbility( EnemyStats.EnemyAbility.Cloaked ) && !npc.IsRevealed )
					npc.Reveal();
			}
		}
	}

	void ScanForTarget()
	{
		if (target != null) return;

		var trace = DoRangeTrace();

		if ( !trace.Hit ) return;

		if(trace.GameObject.GetComponent<CastleNPC>() != null)
		{
			CastleNPC npc = trace.GameObject.GetComponent<CastleNPC>();

			if ( npc.HasAbility( EnemyStats.EnemyAbility.Cloaked ) && !npc.IsRevealed )
			{	
				if (canRevealCloaked)
					npc.Reveal();

				//Can't shoot cloaked targets
				if ( !canSpotCloaked ) return;
			}

			target = npc;
		}
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

		return Vector3.DistanceBetween(WorldPosition, target.WorldPosition) < range;
	}

	bool CanSeeTarget()
	{
		if ( target == null || !target.IsValid ) return false;

		var trace = Scene.Trace.Ray( WorldPosition + Vector3.Up * 8, target.WorldPosition + Vector3.Up * 2 )
			.IgnoreGameObject( GameObject )
			.WithAnyTags( "Solid", "Enemy" )
			.UsePhysicsWorld()
			.Run();

		return trace.Hit && trace.GameObject == target.GameObject;
	}

	void Attack()
	{
		lastAttack = 0;

		#region Area
		if ( Statistics.FireType == TowerStats.AttackMethod.Area)
		{
			GameObject.PlaySound( Statistics.FireSound );

			var areaTargets = new List<CastleNPC>();
			var nearbyTraces = Scene.Trace.Sphere( Statistics.ExplosionRange, target.WorldPosition, target.WorldPosition )
				.WithTag( "Enemy" )
				.RunAll();

			foreach ( var trace in nearbyTraces )
			{
				var npc = trace.GameObject.GetComponent<CastleNPC>();
				
				if ( npc != null )
					areaTargets.Add( npc );
			}

			areaTargets.ForEach(npc => npc.TakeDamage( attackDmg ));
		}
		#endregion
		#region Chained
		else if ( Statistics.FireType == TowerStats.AttackMethod.Chained )
		{
			GameObject.PlaySound( Statistics.FireSound );

			var toHit = new List<CastleNPC>();
			toHit.Add( target );

			var nearbyTraces = Scene.Trace.Sphere( Statistics.ChainRange, target.WorldPosition, target.WorldPosition )
				.WithTag( "Enemy" )
				.RunAll();

			int totalChain = Statistics.ChainCount;

			foreach ( var trace in nearbyTraces )
			{
				if ( totalChain <= 0 ) break;

				var npc = trace.GameObject.GetComponent<CastleNPC>();
				if ( npc != null && !toHit.Contains( npc ) )
				{
					toHit.Add( npc );
					totalChain--;
				}
			}

			foreach ( var enemy in toHit )
				enemy.TakeDamage( attackDmg );
		}
		#endregion
		else
		{
			GameObject.PlaySound( Statistics.FireSound );
			target.TakeDamage( attackDmg );
		}

		if( HasAbility(TowerStats.Ability.CanFreezeTargets) )
		{
			target.Freeze( 3.0f );
		}
	}

	public bool HasAbility(TowerStats.Ability ability)
	{
		if ( Statistics == null ) return false;
		return Statistics.Abilities.HasFlag( ability );
	}

	bool CanAttack() => lastAttack > (1.0f / fireRate);
}

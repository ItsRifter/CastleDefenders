using Sandbox;
using Sandbox.Citizen;

public sealed class CastleNPC : Component
{
	public int Health { get; set; }
	public EnemyStats Statistics { get; private set; }
	public string DisplayName;

	public bool IsSplitted;
	public bool IsRevealed;

	[RequireComponent] public CitizenAnimationHelper AnimationHelper { get; set; }

	PathNode targetNode;
	float speed;
	int armorPoints;
	int splitCount;

	SkinnedModelRenderer modelRenderer;
	Color orgColor;
	float cloakAlpha = -1;

	TimeUntil timeTillCloaked;
	TimeUntil timeTillThaw;
	bool isFrozen;

	const float CloakDelay = 3.0f;

	protected override void OnStart()
	{
		Statistics = GetComponent<EnemyStats>();
		modelRenderer = GetComponent<SkinnedModelRenderer>();
		AnimationHelper = GetComponent<CitizenAnimationHelper>();
		AnimationHelper?.Target = modelRenderer;

		if ( IsSplitted ) return;

		DisplayName = Statistics.DisplayName;
		Health = Statistics.Health;
		speed = Statistics.Speed;
		splitCount = Statistics.SplitCount;
		armorPoints = Statistics.ArmourValue;

		orgColor = modelRenderer.Tint;
		cloakAlpha = modelRenderer.Tint.a;

		targetNode = Scene.GetAll<PathNode>().Where( p => p.IsStartNode ).FirstOrDefault();
		WorldPosition = targetNode.WorldPosition;
	}

	protected override void OnUpdate()
	{
		MoveToNode();
		UpdateAnimation();

		if ( IsRevealed && timeTillCloaked <= 0 )
			Cloak();

		if ( isFrozen && timeTillThaw <= 0 )
			Thaw();
	}

	// --- Movement ---

	void MoveToNode()
	{
		if ( targetNode == null ) return;

		var direction = (targetNode.WorldPosition - WorldPosition).Normal;
		WorldPosition += direction * speed * Time.Delta;
		WorldRotation = Rotation.LookAt( direction, Vector3.Up );

		if ( !IsAtNode() ) return;

		if ( targetNode.IsEndNode )
		{
			OnHitGoal();
			return;
		}

		if ( targetNode.IsTeleporter )
			WorldPosition = targetNode.NextNode.WorldPosition;

		targetNode = targetNode.NextNode;
	}

	bool IsAtNode() => Vector3.DistanceBetween( WorldPosition, targetNode.WorldPosition ) < 0.1f;

	void OnHitGoal()
	{
		// TODO: deal damage to castle / trigger game-over logic
	}

	// --- Animation ---

	void UpdateAnimation()
	{
		if ( AnimationHelper == null ) return;

		var velocity = targetNode != null
			? (targetNode.WorldPosition - WorldPosition).Normal * speed
			: Vector3.Zero;

		AnimationHelper.WithVelocity( velocity );
		AnimationHelper.IsGrounded = true;
	}

	// --- Splitting ---

	void CreateSplitVariant( int index )
	{
		var splitEnemy = Scene.CreateObject();
		splitEnemy.Name = $"{GameObject.Name}_Lesser{index}";
		splitEnemy.Tags.Add( "Enemy" );
		splitEnemy.LocalScale = GameObject.LocalScale * 0.5f;

		var collider = splitEnemy.AddComponent<BoxCollider>();
		collider.Scale = GameObject.GetComponent<BoxCollider>().Scale * 0.5f;

		var npcModel = splitEnemy.AddComponent<SkinnedModelRenderer>();
		npcModel.Model = modelRenderer.Model;
		npcModel.Tint = modelRenderer.Tint;

		var npc = splitEnemy.AddComponent<CastleNPC>();
		npc.IsSplitted = true;
		npc.DisplayName = $"Lesser {DisplayName}";
		npc.Statistics = Statistics;
		npc.Health = Statistics.Health / splitCount;
		npc.speed = Statistics.Speed * 1.2f;
		npc.targetNode = targetNode;
		npc.WorldPosition = WorldPosition + (Vector3.Random * 10.0f).WithZ( 0 );
	}

	// --- Combat ---

	void OnDeath()
	{
		if ( HasAbility( EnemyStats.EnemyAbility.SplitsOnDeath ) && !IsSplitted )
		{
			for ( int i = 0; i < splitCount; i++ )
				CreateSplitVariant( i );
		}

		WaveManager.Instance.OnEnemyDeath( this );
		GameObject.Destroy();
	}

	// TODO: Make armor visually break
	void BreakArmor()
	{
		armorPoints = 0;
		var dresser = GameObject.GetComponent<Dresser>();
		if ( dresser == null ) return;

		foreach ( var clothing in Statistics.ArmouredClothing )
			dresser.Clothing.Remove( clothing );

		dresser.Apply();
	}

	public void TakeDamage( int amount )
	{
		if ( HasAbility( EnemyStats.EnemyAbility.Armoured ) && armorPoints > 0 )
		{
			armorPoints -= amount;
			if ( armorPoints <= 0 )
				amount = -armorPoints;
			else
				return;
		}

		Health -= amount;

		if ( Health <= 0 )
			OnDeath();
	}

	public bool HasAbility( EnemyStats.EnemyAbility ability ) =>
		Statistics != null && Statistics.Abilities.HasFlag( ability );

	// --- Stealth ---

	public void Reveal()
	{
		timeTillCloaked = CloakDelay;
		if ( IsRevealed ) return;

		IsRevealed = true;
		modelRenderer.Tint = modelRenderer.Tint.WithAlpha( 1.0f );
	}

	public void Cloak()
	{
		IsRevealed = false;
		modelRenderer.Tint = modelRenderer.Tint.WithAlpha( cloakAlpha );
	}

	// --- Status Effects ---

	public void Freeze( float duration )
	{
		timeTillThaw = duration;
		speed = 0.0f;
		isFrozen = true;
		modelRenderer.Tint = Color.Cyan;
	}

	public void Thaw()
	{
		speed = Statistics.Speed;
		isFrozen = false;
		modelRenderer.Tint = orgColor;

		if ( cloakAlpha >= 0 && !IsRevealed )
			modelRenderer.Tint = modelRenderer.Tint.WithAlpha( cloakAlpha );
	}
}

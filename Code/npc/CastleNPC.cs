using Sandbox;
using System;

public sealed class CastleNPC : Component
{
	public int Health { get; set; }
	public EnemyStats Statistics { get; private set; }
	public string DisplayName;

	public bool IsSplitted;
	public bool IsRevealed;

	PathNode targetNode;
	float speed;

	int armorPoints;
	int splitCount;

	TimeUntil timeTillCloaked;
	float timeToCloak = 3.0f;

	float cloakAlpha;

	Color orgColor;
	TimeUntil timeTillThaw;

	ModelRenderer modelRenderer;

	protected override void OnStart()
	{
		Statistics = GetComponent<EnemyStats>();

		if ( IsSplitted ) return;

		DisplayName = Statistics.DisplayName;
		Health = Statistics.Health;
		speed = Statistics.Speed;

		targetNode = Scene.GetAll<PathNode>().Where( p => p.IsStartNode ).FirstOrDefault();
		WorldPosition = targetNode.WorldPosition;

		splitCount = Statistics.SplitCount;
		armorPoints = Statistics.ArmourValue;

		IsRevealed = false;

		modelRenderer = GetComponent<ModelRenderer>();
		cloakAlpha = GetComponent<ModelRenderer>().Tint.a;
	}

	protected override void OnUpdate()
	{
		MoveToNode();

		if ( IsRevealed && timeTillCloaked <= 0.0f )
			Cloak();
	}

	void MoveToNode()
	{
		if ( targetNode == null )
			return;

		var direction = (targetNode.WorldPosition - WorldPosition).Normal;
		var distance = speed * Time.Delta;
		
		WorldPosition += direction * distance;
		WorldRotation = Rotation.LookAt( direction, Vector3.Up );

		if ( IsAtNode() )
		{
			if( targetNode.IsEndNode )
			{
				OnHitGoal();
				return;
			}

			if ( targetNode.IsTeleporter )
				WorldPosition = targetNode.NextNode.WorldPosition;

			targetNode = GetNextNode();
		}
	}

	PathNode GetNextNode() => targetNode.NextNode ?? null;

	bool IsAtNode() => Vector3.DistanceBetween( WorldPosition, targetNode.WorldPosition ) < 0.1f;

	void OnHitGoal()
	{

	}

	void CreateSplitVariant(int i)
	{
		var splitEnemy = Scene.CreateObject();
		splitEnemy.Name = $"{GameObject.Name}_Lesser{i}";
		splitEnemy.Tags.Add( "Enemy" );

		splitEnemy.LocalScale = GameObject.LocalScale * 0.5f;

		var collider = splitEnemy.AddComponent<BoxCollider>();
		collider.Scale = GameObject.GetComponent<BoxCollider>().Scale * 0.5f;

		var npcModel = splitEnemy.AddComponent<ModelRenderer>();

		npcModel.Model = modelRenderer.Model;
		npcModel.Tint = modelRenderer.Tint;

		var npcComponent = splitEnemy.AddComponent<CastleNPC>();
		npcComponent.IsSplitted = true;
		npcComponent.DisplayName = $"Lesser {DisplayName}";

		npcComponent.Statistics = Statistics;
		npcComponent.Statistics.DisplayName = $"Lesser {Statistics.DisplayName}";

		npcComponent.Health = Statistics.Health / splitCount;
		npcComponent.speed = Statistics.Speed * 1.2f;

		npcComponent.targetNode = this.targetNode;
		npcComponent.WorldPosition = this.WorldPosition + (Vector3.Random * 10.0f).WithZ(0);
	}

	void OnDeath()
	{
		if( Statistics != null && HasAbility( EnemyStats.EnemyAbility.SplitsOnDeath ) && !IsSplitted )
		{	
			for( int i = 0; i < splitCount; i++ )
			{
				CreateSplitVariant(i);
			}
		}

		WaveManager.Instance.OnEnemyDeath( this );
		GameObject.Destroy();
	}

	//TODO: Make armor visually break
	void BreakArmor()
	{
		armorPoints = 0;
		var clothingComp = GameObject.GetComponent<Dresser>();
		if ( clothingComp == null ) return;

		foreach ( var clothing in Statistics.ArmouredClothing )
		{
			clothingComp.Clothing.Remove( clothing );
			clothingComp.Apply();	
		}
	}

	public void Reveal()
	{
		timeTillCloaked = timeToCloak;
		IsRevealed = true;

		modelRenderer.Tint = modelRenderer.Tint.WithAlpha( 1.0f );
	}

	public void Cloak()
	{
		IsRevealed = false;
		modelRenderer.Tint = modelRenderer.Tint.WithAlpha( cloakAlpha );
	}

	public void Freeze( float duration )
	{
		timeTillThaw = duration;
		speed = 0.0f;

		modelRenderer.Tint = Color.Cyan;
	}

	public void Thaw()
	{
		speed = Statistics.Speed;
		modelRenderer.Tint = orgColor;
	}

	public void TakeDamage(int amount)
	{
		if( Statistics != null && HasAbility( EnemyStats.EnemyAbility.Armoured ) && armorPoints > 0 )
		{
			armorPoints -= amount;
			
			if( armorPoints <= 0 )
			{
				amount = -armorPoints;
				//BreakArmor();
			}
			else
				return;
		}

		Health -= amount;

		if ( Health <= 0 )
			OnDeath();
	}

	public bool HasAbility(EnemyStats.EnemyAbility ability )
	{
		if ( Statistics == null ) return false;
		return Statistics.Abilities.HasFlag( ability );
	}
}

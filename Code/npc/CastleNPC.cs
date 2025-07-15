using Sandbox;

public sealed class CastleNPC : Component
{
	public float Health { get; set; }
	EnemyStats Statistics;

	PathNode targetNode;
	float speed;

	protected override void OnStart()
	{
		Statistics = GetComponent<EnemyStats>();

		Health = Statistics.Health;
		speed = Statistics.Speed;

		targetNode = Scene.GetAll<PathNode>().Where( p => p.IsStartNode ).FirstOrDefault();
		WorldPosition = targetNode.WorldPosition;
	}

	protected override void OnUpdate()
	{
		MoveToNode();
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

	public void TakeDamage(float amount)
	{
		Health -= amount;

		if ( Health <= 0 )
			GameObject.Destroy();
	}
}

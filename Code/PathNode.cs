using Sandbox;

public sealed class PathNode : Component
{
	[Property] public PathNode NextNode { get; set; }
	[Property, Title("Is starting node")] public bool IsStartNode { get; set; }
	[Property, Title("Is last node")] public bool IsEndNode { get; set; }
	[Property, Title("Can teleport")] public bool IsTeleporter { get; set; }

	protected override void DrawGizmos()
	{
		Color baseColour = Color.White;
		Color lineColour = Color.White;

		if (IsStartNode && !IsEndNode)
			baseColour = Color.Green;

		if(IsEndNode && !IsStartNode)
			baseColour = Color.Red;

		if ( IsTeleporter )
		{
			baseColour = Color.Orange;
			lineColour = Color.Orange;
		}

		DebugOverlay.Box( BBox.FromPositionAndSize( WorldPosition + Vector3.Up * 4, 8.0f ), baseColour );

		if( NextNode != null )
		{
			Vector3 offset = Vector3.Up * 4;

			Vector3 pathA = GameObject.WorldPosition + offset;
			Vector3 pathB = NextNode.GameObject.WorldPosition + offset;

			DebugOverlay.Line( pathA, pathB, lineColour );
		}
	}
}

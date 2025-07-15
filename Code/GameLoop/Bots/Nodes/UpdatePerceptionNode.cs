namespace Facepunch
{
	public class UpdatePerceptionNode : BaseBehaviorNode
	{
		private readonly float _range;
		private const string ENEMIES_KEY = "visible_enemies";
		private const string TEAMMATES_KEY = "visible_teammates";
		private const string ITEMS_KEY = "visible_items";

		public UpdatePerceptionNode( float range = 2048f )
		{
			_range = range;
		}

		protected override NodeResult OnEvaluate( BotContext context )
		{
			var pawn = context.Pawn;
			var origin = pawn.WorldPosition;

			var found = pawn.Scene.FindInPhysics( new Sphere( origin, _range ) );

			var enemies = new List<Pawn>();
			var teammates = new List<Pawn>();
			var items = new List<Component>();

			foreach ( var go in found )
			{
				// Look for Pawn component
				var otherPawn = go.Root.GetComponentInChildren<Pawn>();
				if ( otherPawn is not null )
				{
					if ( otherPawn == pawn )
						continue;

					if ( !otherPawn.HealthComponent.IsValid() || otherPawn.HealthComponent.Health <= 0 )
						continue;

					if ( !IsInLineOfSight( pawn, otherPawn ) )
						continue;

					if ( otherPawn.Team != pawn.Team )
						enemies.Add( otherPawn );
					else
						teammates.Add( otherPawn );

					continue; // already classified
				}

				// Look for Pickup or other item components
				var pickup = go.Root.GetComponentInChildren<DroppedEquipment>(); // change type as needed
				if ( pickup is not null )
				{
					items.Add( pickup );
					continue;
				}

				// Add other object types if needed
			}

			context.SetData( ENEMIES_KEY, enemies );
			context.SetData( TEAMMATES_KEY, teammates );
			context.SetData( ITEMS_KEY, items );

			return (enemies.Count > 0 || teammates.Count > 0 || items.Count > 0)
				? NodeResult.Success
				: NodeResult.Failure;
		}

		private bool IsInLineOfSight( Pawn from, Component obj )
		{
			var targetPos = obj is Pawn pawn ? pawn.EyePosition : obj.WorldPosition;

			var trace = from.Scene.Trace.Ray( from.EyePosition, targetPos + Vector3.Down * 32f )
				.IgnoreGameObjectHierarchy( from.GameObject.Root )
				.Run();

			if ( obj is not Pawn )
			{
				from.Scene.DebugOverlay.Line( from.EyePosition, targetPos, trace.Hit ? Color.Green : Color.Red );
			}

			return trace.Hit && trace.GameObject.Root == obj.GameObject.Root;
		}
	}
}

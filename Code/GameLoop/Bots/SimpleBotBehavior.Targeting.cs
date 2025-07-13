using System.Text.Json.Serialization;

namespace Facepunch;

public partial class SimpleBotBehavior
{
	private HashSet<Pawn> _relevantEnemies = new();
	private Dictionary<Pawn, TimeSince> _lastSeenEnemies = new();
	private Pawn _currentTarget;
	private TimeSince _timeSinceTargetSwitch = 0;

	[Property, JsonIgnore, ReadOnly]
	public int EnemyCount => _relevantEnemies.Count;

	[Property] public float EngagementRange { get; set; } = 2048f;

	private float engagementRangeSqr = 2048 * 2048;

	protected override void OnStart()
	{
		// Cached
		engagementRangeSqr = EngagementRange * EngagementRange;
	}

	private void FindEnemies()
	{
		var allPlayers = Scene.GetAllComponents<PlayerPawn>();
		var playersInLOS = allPlayers.Where( potentialEnemy =>
		{
			if ( !BasicEnemyFilter( potentialEnemy ) )
				return false;

			if ( TeamFilter( potentialEnemy ) )
				return false;

			return IsInLineOfSight( potentialEnemy );
		} ).ToHashSet();

		UpdateEnemyList( playersInLOS );
	}

	private bool BasicEnemyFilter( PlayerPawn potentialEnemy )
	{
		return potentialEnemy != Player &&
			   potentialEnemy.HealthComponent.Health > 0 &&
			   potentialEnemy.WorldPosition.DistanceSquared( WorldPosition ) < engagementRangeSqr;
	}

	private bool TeamFilter( PlayerPawn potentialEnemy )
	{
		if ( potentialEnemy.Team == Team.Unassigned ) return false;
		if ( potentialEnemy.Team == Player.Team ) return true;
		return false;
	}

	private bool IsInLineOfSight( PlayerPawn potentialEnemy )
	{
		var trace = Scene.Trace.Ray( Pawn.EyePosition, potentialEnemy.EyePosition + Vector3.Down * 32f )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.Run();

		return trace.Hit && trace.GameObject.Root == potentialEnemy.GameObject;
	}

	private void UpdateEnemyList( HashSet<PlayerPawn> playersInLOS )
	{
		foreach ( var enemy in playersInLOS )
		{
			_lastSeenEnemies[enemy] = 0;
			_relevantEnemies.Add( enemy );
		}

		// Remove old enemies
		foreach ( var enemy in _lastSeenEnemies.Where( x => x.Value > 5 ).ToList() )
		{
			_relevantEnemies.Remove( enemy.Key );
		}
	}

	private Pawn FindAndUpdateTarget()
	{
		// select target
		var shouldTrySwitchTarget = _timeSinceTargetSwitch > 5.0f || !IsCurrentTargetValid();
		if ( shouldTrySwitchTarget )
		{
			if ( _relevantEnemies.Count > 0 )
			{
				_timeSinceTargetSwitch = 0;
				// pick last seen enemy
				_currentTarget = _relevantEnemies.OrderBy( x => _lastSeenEnemies[x].Relative ).First();
			}
		}

		if ( !IsCurrentTargetValid() )
		{
			_currentTarget = null;
		}

		return _currentTarget;
	}

	bool IsCurrentTargetValid()
	{
		if ( !_currentTarget.IsValid() ) return false;
		if ( !_relevantEnemies.Contains( _currentTarget ) ) return false;
		if ( !_lastSeenEnemies.TryGetValue( _currentTarget, out TimeSince value ) || value > 5f ) return false;
		if ( _currentTarget.HealthComponent.Health <= 0 ) return false;

		return true;
	}
}

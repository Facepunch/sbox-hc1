namespace Facepunch;

/// <summary>
/// Selects the best target from visible enemies and stores it in context
/// </summary>
public class SelectTargetNode : BaseBehaviorNode
{
	private const string ENEMIES_KEY = "visible_enemies";
	private const string CURRENT_TARGET_KEY = "current_target";
	private const float TARGET_SWITCH_TIME = 5.0f;

	private TimeSince _timeSinceTargetSwitch = 0;
	private Pawn _currentTarget;

	protected override NodeResult OnEvaluate( BotContext context )
	{
		// Must have a visible enemies list
		if ( !context.HasData( ENEMIES_KEY ) )
			return NodeResult.Failure;

		var enemies = context.GetData<List<Pawn>>( ENEMIES_KEY );
		if ( enemies == null || enemies.Count == 0 )
			return NodeResult.Failure;

		// Keep current target if still valid and within switch time
		if ( _currentTarget != null &&
			enemies.Contains( _currentTarget ) &&
			_timeSinceTargetSwitch < TARGET_SWITCH_TIME )
		{
			context.SetData( CURRENT_TARGET_KEY, _currentTarget );
			return NodeResult.Success;
		}

		// Select a new target (closest to aim)
		var pawn = context.Pawn;
		var aimRay = new Ray( pawn.EyePosition, pawn.EyeAngles.Forward );

		_currentTarget = enemies
			.OrderBy( x => GetTargetScore( x, aimRay ) )
			.FirstOrDefault();

		if ( _currentTarget == null )
			return NodeResult.Failure;

		_timeSinceTargetSwitch = 0;
		context.SetData( CURRENT_TARGET_KEY, _currentTarget );
		return NodeResult.Success;
	}

	private float GetTargetScore( Pawn target, Ray aimRay )
	{
		var toTarget = target.WorldPosition - aimRay.Position;
		var dot = Vector3.Dot( toTarget.Normal, aimRay.Forward );
		var aimScore = 1 - ((dot + 1) * 0.5f); // 0 = best aligned, 1 = worst

		var distanceScore = toTarget.Length / 1000f;

		return aimScore + distanceScore * 0.5f;
	}
}

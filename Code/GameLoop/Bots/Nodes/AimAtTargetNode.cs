namespace Facepunch;

/// <summary>
/// Aims the bot’s view at a weighted random hitbox on the current target.
/// </summary>
public class AimAtTargetNode : BaseBehaviorNode
{
	private const string CURRENT_TARGET_KEY = "current_target";

	private Pawn _currentTarget;
	private PlayerPawn _currentPlayer;
	private HitboxSet _cachedHitBoxSet;
	private Dictionary<string, float> _hitboxWeights;

	// TODO: configurable skill
	private const float Accuracy = 0.25f;

	protected override NodeResult OnEvaluate( BotContext context )
	{
		// Must have a current target
		if ( !context.HasData( CURRENT_TARGET_KEY ) )
			return NodeResult.Failure;

		var target = context.GetData<Pawn>( CURRENT_TARGET_KEY );
		if ( target == null || !target.IsValid() )
			return NodeResult.Failure;

		// Cache target if changed
		if ( _currentTarget != target )
		{
			_currentTarget = target;

			if ( _currentTarget is PlayerPawn player )
			{
				_currentPlayer = player;
			}

			_cachedHitBoxSet = null;
			_hitboxWeights = null;
		}

		// Calculate local aim target
		var localAimTarget = FindLocalAimTarget();
		if ( localAimTarget == Vector3.Zero )
			return NodeResult.Failure;

		// Convert local aim target to world space
		var worldAimTarget = _currentTarget.WorldPosition + localAimTarget;

		// Aim bot toward target
		var pawn = context.Pawn;
		var direction = (worldAimTarget - pawn.EyePosition).Normal;
		var angles = Rotation.LookAt( direction ).Angles();

		// Lerp eye angles for smoothness
		pawn.EyeAngles = pawn.EyeAngles.LerpTo( angles, Time.Delta * 10f );

		return NodeResult.Running;
	}

	private Vector3 FindLocalAimTarget()
	{
		InitializeHitboxData();

		float totalWeight = _hitboxWeights.Values.Sum();
		float randomValue = Random.Shared.Float( 0, totalWeight );
		float cumulativeWeight = 0f;

		if ( !_currentPlayer.IsValid() ) return Vector3.Zero;
		if ( !_currentPlayer.BodyRenderer.IsValid() ) return Vector3.Zero;

		foreach ( var hitbox in _cachedHitBoxSet.All )
		{
			cumulativeWeight += _hitboxWeights[hitbox.Name];

			if ( randomValue <= cumulativeWeight )
			{

				if ( !_currentPlayer.BodyRenderer.TryGetBoneTransform( hitbox.Bone, out var boneTransform ) )
					continue;

				var spread = 5f * (1f / MathF.Max( Accuracy, 0.1f ));
				var localAimTarget =
					boneTransform.Position +
					GetRandomPointInHitbox( hitbox, spread ) -
					_currentTarget.WorldPosition;

				return localAimTarget;
			}
		}

		Log.Warning( "AimAtTargetNode: Failed to find aim target" );
		return Vector3.Zero;
	}

	private Vector3 GetRandomPointInHitbox( HitboxSet.Box hitbox, float scale )
	{
		var shape = hitbox.Shape;
		if ( shape is Sphere sphere )
		{
			var scaled = new Sphere( sphere.Center, sphere.Radius * scale );
			return scaled.RandomPointInside;
		}

		if ( shape is BBox box )
		{
			var scaled = box * scale;
			return scaled.RandomPointInside;
		}

		if ( shape is Capsule capsule )
		{
			var capsuleCenter = (capsule.CenterA + capsule.CenterB) * 0.5f;
			var direction = capsule.CenterB - capsule.CenterA;
			var halfLength = direction.Length * 0.5f;
			var normalizedDir = direction.Normal;

			var scaledRadius = capsule.Radius * scale;
			var scaledHalfLength = halfLength * scale;

			var scaledCenterA = capsuleCenter - normalizedDir * scaledHalfLength;
			var scaledCenterB = capsuleCenter + normalizedDir * scaledHalfLength;

			var scaledCapsule = new Capsule( scaledCenterA, scaledCenterB, scaledRadius );
			return scaledCapsule.RandomPointInside;
		}

		throw new NotImplementedException( "Unsupported hitbox shape" );
	}

	private void InitializeHitboxData()
	{
		if ( _cachedHitBoxSet != null )
			return;

		if ( !_currentPlayer.IsValid() )
			return;

		_cachedHitBoxSet = _currentPlayer.BodyRenderer.Model.HitboxSet;

		_hitboxWeights = new Dictionary<string, float>
		{
			{ "pelvis", 0.2f },
			{ "spine1", 0.3f },
			{ "spine2", 0.4f },
			{ "spine3", 0.5f },
			{ "head", 0.6f }
		};

		const float defaultWeight = 0.02f;

		foreach ( var hitbox in _cachedHitBoxSet.All )
		{
			if ( !_hitboxWeights.ContainsKey( hitbox.Name ) )
				_hitboxWeights[hitbox.Name] = defaultWeight;
		}
	}
}

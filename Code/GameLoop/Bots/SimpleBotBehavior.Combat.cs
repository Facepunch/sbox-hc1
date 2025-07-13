using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

public partial class SimpleBotBehavior
{
	/// <summary>
	/// Reaction speed bias (scale)
	/// </summary>
	[Property] public float ReactionSpeedBias { get; set; } = 0.5f;

	/// <summary>
	/// How accurate are we
	/// </summary>
	[Property] public float Accuracy { get; set; } = 0.25f;

	/// <summary>
	/// When a bot shoots, they won't keep shooting forever, they'll shoot in controllable bursts
	/// TODO: Expose per-weapon, or have the bot dictate based on mag size, semi auto, etc..
	/// </summary>
	[Property] public RangedFloat BurstRange { get; set; } = new RangedFloat( 1, 5 );

	private async Task<bool> ValidateTarget( CancellationToken token )
	{
		const float validationInterval = 0.5f; // Check every half second
		var nextValidation = 0f;

		while ( !token.IsCancellationRequested )
		{
			if ( Time.Now >= nextValidation )
			{
				if ( !IsCurrentTargetValid() )
				{
					_currentTarget = null;
					return false;
				}
				nextValidation = Time.Now + validationInterval;
			}
			await Task.FixedUpdate();
		}

		return false;
	}

	private async Task<bool> HandleMovement( CancellationToken token )
	{
		TimeSince lastCombatMovementUpdate = 0f;

		while ( !token.IsCancellationRequested )
		{
			if ( lastCombatMovementUpdate > 1f )
			{
				lastCombatMovementUpdate = 0f;

				// If target not visible, chase it
				if ( !_lastSeenEnemies.ContainsKey( _currentTarget ) || _lastSeenEnemies[_currentTarget] > 0.1f )
				{
					MeshAgent.MoveTo( _currentTarget.WorldPosition );
				}
				else
				{
					MeshAgent.MoveTo( _currentTarget.WorldPosition + Vector3.Random * 500f );
				}
			}

			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}

	private bool IsValidTarget( Pawn target )
	{
		if ( !target.IsValid() || target.HealthComponent.State != LifeState.Alive )
			return false;
		return true;
	}

	private async Task<bool> Combat( CancellationToken token )
	{
		_currentTarget = FindAndUpdateTarget();

		if ( !IsValidTarget( _currentTarget ) )
			return false;

		while ( !token.IsCancellationRequested )
		{
			_currentTarget = FindAndUpdateTarget();

			if ( !_currentTarget.IsValid() )
				return true;

			bool result = await RunParallel(
				token,
				ValidateTarget,
				UpdateAim,
				HandleMovement,
				HandleReload,
				HandleShooting
			);

			if ( !result && !_currentTarget.IsValid() )
				return true;

			await Task.DelayRealtimeSeconds( 0.1f );
		}

		return true;
	}

	private async Task<bool> HandleReload( CancellationToken token )
	{
		while ( !token.IsCancellationRequested )
		{
			var w = Player.CurrentEquipment;

			if ( w.IsValid() && w.GetComponentInChildren<Reloadable>() is { } c )
			{
				if ( !c.AmmoComponent.HasAmmo && !c.IsReloading )
				{
					c.StartReload();
				}
			}

			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}

	private async Task<bool> HandleShooting( CancellationToken token )
	{
		var burstCount = 0;
		var targetBurstSize = 0;
		var timeSinceLastShot = 0f;
		const float burstBreakTime = 0.5f; // Time between bursts in seconds

		while ( !token.IsCancellationRequested )
		{
			// Can see target => consider shooting at it
			if ( _lastSeenEnemies.TryGetValue( _currentTarget, out TimeSince value ) && value < 0.1f )
			{
				var w = Player.CurrentEquipment;
				if ( w.IsValid() && w.GetComponentInChildren<Shootable>() is { } shootable )
				{
					// If we're not in a burst, decide if we should start one
					if ( burstCount == 0 )
					{
						if ( timeSinceLastShot > burstBreakTime )
						{
							// Start new burst
							targetBurstSize = BurstRange.GetValue().CeilToInt();
							burstCount = 0;
						}
					}

					if ( burstCount < targetBurstSize && shootable.CanShoot() )
					{
						shootable.Shoot();
						burstCount++;
						timeSinceLastShot = 0f;
					}
					else if ( burstCount >= targetBurstSize )
					{
						burstCount = 0;
						targetBurstSize = 0;
					}
				}
			}
			else
			{
				// Lost sight of target, reset burst
				burstCount = 0;
				targetBurstSize = 0;
			}

			timeSinceLastShot += Time.Delta;
			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}

	/// <summary>
	/// Our Aim has two components
	/// 1. Which Local Aim Target to aim at (Head, Body, etc)
	/// 2. Track the global movement of the target ( WorldPosition )
	/// </summary>
	private async Task<bool> UpdateAim( CancellationToken token )
	{
		Vector3 localAimTarget = Vector3.Zero;
		Vector3 newLocalAimTarget = Vector3.Zero;
		TimeSince timeSinceLastLocalAimUpdate = 0;

		const float localAimUpdateInterval = 1.5f;

		while ( !token.IsCancellationRequested )
		{
			if ( timeSinceLastLocalAimUpdate > localAimUpdateInterval )
			{
				newLocalAimTarget = FindLocalAimTarget();
				timeSinceLastLocalAimUpdate = 0;
			}

			// Local aim is not as snappy and rather slow
			const float aimLerpFactor = 0.025f;
			localAimTarget = localAimTarget.LerpTo( newLocalAimTarget, aimLerpFactor );

			Vector3 worldAimTarget = _currentTarget.WorldPosition + localAimTarget;
			// Clamp so we don't aim too much in to the ground
			worldAimTarget = worldAimTarget.WithZ( MathF.Max( _currentTarget.WorldPosition.z + 24f, worldAimTarget.z ) );
			Vector3 direction = worldAimTarget - Pawn.EyePosition;

			// If target hasn't been visible for a while stop tracking it
			// otherwhise it looks like the bot has wallhacks. It does but we don't want people to know.
			if ( _lastSeenEnemies.TryGetValue( _currentTarget, out var targetLastSeen ) && targetLastSeen > 1f )
			{
				worldAimTarget = MeshAgent.GetLookAhead( 30.0f ).WithZ( Pawn.EyePosition.z );
			}

			if ( direction.LengthSquared > 0.01f * 0.01f )
			{
				Rotation targetRotation = Rotation.LookAt( direction.Normal );

				// Adjust eye angles towards the target rotation with reaction speed bias
				float lerpFactor = Random.Shared.Float( 0.1f, 0.25f ) * ReactionSpeedBias;
				Pawn.EyeAngles = Pawn.EyeAngles.LerpTo( targetRotation, lerpFactor );
			}

			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}

	// Let's cache these
	private HitboxSet _cachedHitBoxSet;
	private Dictionary<string, float> _hitboxWeights;

	private Vector3 FindLocalAimTarget()
	{
		InitializeHitboxData();

		float totalWeight = _hitboxWeights.Values.Sum();

		float randomValue = Random.Shared.Float( 0, totalWeight );
		float cumulativeWeight = 0;

		var p = _currentTarget as PlayerPawn;

		foreach ( var hitbox in _cachedHitBoxSet.All )
		{
			cumulativeWeight += _hitboxWeights[hitbox.Name];

			if ( randomValue <= cumulativeWeight )
			{
				if ( !p.BodyRenderer.TryGetBoneTransform( hitbox.Bone, out var boneTransform ) )
					continue;

				Vector3 localAimTarget = boneTransform.Position
										 + GetRandomPointInHitbox( hitbox, 5f * (1f / MathF.Max( Accuracy, 0.1f )) )
										 - p.BodyRenderer.WorldPosition;

				return localAimTarget;
			}
		}

		Log.Warning( "Failed to find local aim target" );
		return Vector3.Zero;
	}

	private Vector3 GetRandomPointInHitbox( HitboxSet.Box hitbox, float scale )
	{
		var shape = hitbox.Shape;
		if ( shape is Sphere sphere )
		{
			var scaledSphere = new Sphere( sphere.Center, sphere.Radius * scale );
			return scaledSphere.RandomPointInside;
		}

		if ( shape is BBox box )
		{
			var scaledBox = box * scale;
			return scaledBox.RandomPointInside;
		}

		if ( shape is Capsule capsule )
		{
			var capsuleCenter = (capsule.CenterA + capsule.CenterB) * 0.5f; // Find the midpoint
			var direction = capsule.CenterB - capsule.CenterA; // Get the direction vector
			var halfLength = direction.Length * 0.5f; // Get half the length
			var normalizedDirection = direction.Normal; // Get the normalized direction

			// Scale both radius and length
			var scaledRadius = capsule.Radius * scale;
			var scaledHalfLength = halfLength * scale;

			// Calculate new center points
			var scaledCenterA = capsuleCenter - normalizedDirection * scaledHalfLength;
			var scaledCenterB = capsuleCenter + normalizedDirection * scaledHalfLength;

			// Create the scaled capsule
			var scaledCapsule = new Capsule( scaledCenterA, scaledCenterB, scaledRadius );

			return scaledCapsule.RandomPointInside;
		}

		throw new NotImplementedException( "Unsupported shape" );
	}

	private void InitializeHitboxData()
	{
		if ( _cachedHitBoxSet != null )
			return;

		_cachedHitBoxSet = (_currentTarget as PlayerPawn).BodyRenderer.Model.HitboxSet;

		_hitboxWeights = new Dictionary<string, float>
		{
			{ "pelvis", 0.2f },
			{ "spine1", 0.3f },
			{ "spine2", 0.4f },
			{ "spine3", 0.5f },
			{ "head", 0.6f },
		};

		const float defaultWeight = 0.02f;

		// Initialize remaining hitboxes with default weight
		foreach ( var hitbox in _cachedHitBoxSet.All )
		{
			if ( !_hitboxWeights.ContainsKey( hitbox.Name ) )
			{
				_hitboxWeights[hitbox.Name] = defaultWeight;
			}
		}
	}
}

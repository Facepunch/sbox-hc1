namespace Facepunch;

[Icon( "track_changes" )]
[Title( "Melee" ), Group( "Weapon Components" )]
public partial class MeleeWeaponComponent : InputWeaponComponent
{
	[Property, Category( "Config" )] public float BaseDamage { get; set; } = 25.0f;
	[Property, Category( "Config" )] public float FireRate { get; set; } = 0.2f;
	[Property, Category( "Config" )] public float MaxRange { get; set; } = 1024000;
	[Property, Category( "Config" )] public float Size { get; set; } = 1.0f;

	[Property, Group( "Sounds" )] public SoundEvent SwingSound { get; set; }

	public TimeSince TimeSinceSwing { get; private set; }

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected SkinnedModelRenderer EffectsRenderer 
	{
		get
		{
			if ( IsProxy || !Weapon.ViewModel.IsValid() )
			{
				return Weapon.ModelRenderer;
			}

			return Weapon.ViewModel.ModelRenderer;
		}
	}

	/// <summary>
	/// Do shoot effects
	/// </summary>
	[Broadcast]
	protected void DoEffects()
	{
		if ( SwingSound is not null )
		{
			if ( Sound.Play( SwingSound, Weapon.Transform.Position ) is SoundHandle snd )
			{
				snd.ListenLocal = !IsProxy;
			}
		}

		// Third person
		Weapon?.PlayerController?.BodyRenderer?.Set( "b_attack", true );

		// First person
		Weapon?.ViewModel?.ModelRenderer.Set( "b_attack", true );
	}

	private void CreateImpactEffects( GameObject hitObject, Surface surface, Vector3 pos, Vector3 normal )
	{
		var decalPath = Game.Random.FromList( surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );
		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			var decal = Game.Random.FromList( decalResource.Decals );

			var gameObject = Scene.CreateObject();
			gameObject.Transform.Position = pos;
			gameObject.Transform.Rotation = Rotation.LookAt( -normal );

			// Random rotation
			gameObject.Transform.Rotation *= Rotation.FromAxis( Vector3.Forward, decal.Rotation.GetValue() );

			var decalRenderer = gameObject.Components.Create<DecalRenderer>();
			decalRenderer.Material = decal.Material;
			decalRenderer.Size = new( decal.Width.GetValue(), decal.Height.GetValue(), decal.Depth.GetValue() );

			// Creates a destruction component to destroy the gameobject after a while
			gameObject.DestroyAsync( 3f );
		}

		if ( !string.IsNullOrEmpty( surface.Sounds.Bullet ) )
		{
			Sound.Play( surface.Sounds.Bullet, pos );
		}
	}

	public void Swing()
	{
		TimeSinceSwing = 0f;
		
		foreach ( var tr in GetTrace() )
		{
			if ( !tr.Hit )
			{
				DoEffects();
				return;
			}

			DoEffects();
			CreateImpactEffects( tr.GameObject, tr.Surface, tr.EndPosition, tr.Normal );

			// Inflict damage on whatever we find.

			using ( Rpc.FilterInclude( Connection.Host ) )
			{
				InflictKnifeDamage( tr.GameObject.Id, tr.EndPosition, tr.Direction );
			}
		}
	}

	[Broadcast]
	private void InflictKnifeDamage( Guid targetObjectId, Vector3 pos, Vector3 dir )
	{
		var obj = Scene.Directory.FindByGuid( targetObjectId );

		// TODO: backstab detection

		obj?.TakeDamage( new DamageInfo( Weapon.PlayerController, BaseDamage, Weapon, pos, dir * 64f, HitboxTags.UpperBody, DamageFlags.Melee ) );
	}

	protected virtual Ray WeaponRay => Weapon.PlayerController.AimRay;

	/// <summary>
	/// Runs a trace with all the data we have supplied it, and returns the result
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerable<SceneTraceResult> GetTrace()
	{
		var start = WeaponRay.Position;
		var end = WeaponRay.Position + WeaponRay.Forward * MaxRange;

        yield return Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement" )
			.Size( Size )
			.Run();
	}

	/// <summary>
	/// Can we shoot this gun right now?
	/// </summary>
	public bool CanSwing()
	{
		// Delay checks
		return TimeSinceSwing >= FireRate;
	}

    protected override void OnInput()
	{
		if ( CanSwing() )
		{
			Swing();
		}
	}
}

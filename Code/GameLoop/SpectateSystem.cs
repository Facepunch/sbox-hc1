namespace Facepunch;

public sealed class SpectateSystem : SingletonComponent<SpectateSystem>
{
	public CameraMode CameraMode { get; private set; } = CameraMode.FirstPerson;
	// todo: make this not skip deathcams
	public bool IsSpectating => !Client.Local.IsValid() || !Client.Local.PlayerPawn.IsValid() || Client.Local.PlayerPawn != Client.Viewer.Pawn;
	public bool IsFreecam => (FreecamController as Pawn)?.IsPossessed ?? false;

	[Property] public SpectateController FreecamController { get; set; }
	
	private bool _wasSpectating { get; set; }

	private void OnSpectateBegin()
	{
		// try to find someone to spectate
		SpectateNextPlayer( true );
	}

	protected override void OnUpdate()
	{
		// TODO: Fix this, this sucks
		if ( Client.Viewer.IsValid() && Client.Viewer.PlayerPawn.IsValid() )
		{
			if ( Client.Local.IsValid() )
			{
				Client.Local.Possess();
			}
		}

		// Do we have no pawn? Spectate!
		if ( IsSpectating )
		{
			UpdateSpectate();
		}

		_wasSpectating = IsSpectating;
	}

	private void UpdateSpectate()
	{
		if ( !_wasSpectating )
			OnSpectateBegin();

		if ( !Client.Viewer.IsValid() )
			return;

		if ( Input.Pressed( "SpectatorNext" ) || !Client.Viewer.Pawn.IsValid() )
		{
			SpectateNextPlayer( true );
		}
		else if ( Input.Pressed( "SpectatorPrev" ) )
		{
			SpectateNextPlayer( false );
		}
		else if ( Input.Pressed( "SpectatorFreeCam" ) )
		{
			SpectateFreecam();
		}
		else if ( Input.Pressed( "SpectatorMode" ) )
		{
			if ( IsFreecam || Client.Viewer.IsLocalPlayer )
				return;

			const int max = (int)CameraMode.ThirdPerson + 1;
			CameraMode = (CameraMode)((((int)CameraMode) + 1) % max);

			( Client.Viewer.PlayerPawn ).CameraController.Mode = CameraMode;
		}
	}

	private void SpectateNextPlayer( bool direction )
	{
		var players = Scene.GetAllComponents<Client>();
		
		var idxCur = 0;
		for ( var i = 0; i < players.Count(); i++ )
		{
			if ( players.ElementAt( i ).IsViewer )
				idxCur = i;
		}

		var count = players.Count();
		for ( var i = 1; i <= count; i++ )
		{
			var idx = (idxCur + (direction ? i : -i) + count) % count;
			var Client = players.ElementAt( idx );

			var controller = Client.PlayerPawn;

			if ( !controller.IsValid() )
				continue;

			if ( controller.HealthComponent.State != LifeState.Alive )
				continue;

			// Already spectating this guy, no need to reposess (and reset the viewmodel etc)
			if ( idx == idxCur )
				return;

			Client.Possess();
			return;
		}

		// No players to spectate, fallback to freecam
		SpectateFreecam();
	}

	private void SpectateFreecam()
	{
		if ( !FreecamController.IsValid() )
			return;

		if ( Client.Viewer.IsValid() && Client.Viewer.Pawn.IsValid() )
		{
			// Entering freecam, position ourselves at the last guy's POV
			var rotation = Client.Viewer.Pawn.EyeAngles;
			FreecamController.EyeAngles = rotation;
			FreecamController.WorldPosition = Client.Viewer.Pawn.GameObject.WorldPosition + (rotation.Forward * 8.0f);
		}

		FreecamController.Possess();
	}
}

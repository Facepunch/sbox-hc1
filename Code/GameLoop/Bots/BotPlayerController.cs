namespace Facepunch;

public class BotPlayerController : Component, IBotController
{
	[RequireComponent] public PlayerPawn Player { get; set; }

	void IBotController.OnControl( BotController bot )
	{
		Player.EyeAngles = Player.EyeAngles.WithYaw( Player.EyeAngles.yaw + 25f * Time.Delta );
		Player.EyeAngles = Player.EyeAngles.WithPitch( Player.EyeAngles.pitch - 50f * MathF.Sin( Time.Now * 2f ) * Time.Delta );
	}
}

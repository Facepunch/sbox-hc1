using Sandbox.Utility;

namespace Facepunch;

public abstract class ScreenShake
{
	public class Random : ScreenShake
	{
		public float Progress => Easing.EaseOut( ((float)LifeTime).LerpInverse( 0, Length ) );

		private float Length { get; set; }
		private float Size { get; set; }
		private TimeSince LifeTime { get; set; } = 0f;

		public Random( float length = 1.5f, float size = 1f )
		{
			Length = length;
			Size = size;
		}

		public override bool Update( CameraComponent camera )
		{
			var random = Vector3.Random;
			random.z = 0f;
			random = random.Normal;

			camera.Transform.LocalPosition += (camera.Transform.LocalRotation.Right * random.x + camera.Transform.LocalRotation.Up * random.y) * (1f - Progress) * Size;

			return LifeTime < Length;
		}
	}

	public class Fov : ScreenShake
	{
		public float Progress => ((float)LifeTime).LerpInverse( 0, Length );

		private float Length { get; set; }
		private float Amount { get; set; }
		private TimeSince LifeTime { get; set; } = 0f;
		private Curve Curve { get; set; }

		public Fov( float length = 1.5f, float size = 1f, Curve curve = default )
		{
			Length = length;
			Amount = size;
			Curve = curve;
		}

		public override bool Update( CameraComponent camera )
		{
			var c = Curve.Evaluate( Progress );

			var cc = (GameUtils.CurrentPawn as PlayerPawn)?.CameraController;
			cc.AddFieldOfViewOffset( Amount * c );

			return LifeTime < Length;
		}
	}

	public class Punch : ScreenShake
	{
		public float Progress => ((float)LifeTime).LerpInverse( 0, Length );

		private Vector3 Size { get; set; }
		private Angles PunchAngles { get; set; }
		private float Length { get; set; }
		private TimeSince LifeTime { get; set; } = 0f;
		private Curve Curve { get; set; }

		public Punch( float length = 1.5f, Vector3 size = default, Angles punch = default, Curve curve = default )
		{
			Length = length;
			Size = size;
			PunchAngles = punch;
			Curve = curve;
		}

		public override bool Update( CameraComponent camera )
		{
			var random = Size;

			var c = Curve.Evaluate( Progress );

			camera.Transform.LocalPosition += (camera.Transform.LocalRotation.Right * random.x + camera.Transform.LocalRotation.Up * random.y + camera.Transform.LocalRotation.Backward * random.z) * c * Size;
			camera.Transform.LocalRotation *= (PunchAngles * c).ToRotation();

			return LifeTime < Length;
		}
	}

	public abstract bool Update( CameraComponent camera );
}

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
	
	public abstract bool Update( CameraComponent camera );
}

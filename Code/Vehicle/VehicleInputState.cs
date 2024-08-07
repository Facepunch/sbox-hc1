namespace Facepunch;

public partial class Vehicle
{
	public class VehicleInputState
	{
		public Vector3 direction;
		public bool isBoosting;

		public void Reset()
		{
			direction = Vector3.Zero;
			isBoosting = false;
		}

		public void UpdateFromLocal()
		{
			direction = Input.AnalogMove;
			isBoosting = Input.Down( "Run" );
		}
	}
}

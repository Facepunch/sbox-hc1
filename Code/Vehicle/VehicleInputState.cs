namespace Facepunch;

public partial class Vehicle
{
	public struct VehicleInputState
	{
		public Vector3 direction;
		public bool isBoosting;

		public void Reset()
		{
			direction = Vector3.Zero;
			isBoosting = false;
		}

		public static VehicleInputState CreateFromLocal()
		{
			return new VehicleInputState()
			{
				direction = Input.AnalogMove,
				isBoosting = Input.Down( "Run" )
			};
		}
	}
}

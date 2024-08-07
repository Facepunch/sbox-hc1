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
	}
}

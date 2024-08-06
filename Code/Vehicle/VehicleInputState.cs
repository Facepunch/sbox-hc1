namespace Facepunch;

public partial class Vehicle
{
	public struct VehicleInputState
	{
		public Vector3 direction;

		public void Reset()
		{
			direction = Vector3.Zero;
		}
	}
}

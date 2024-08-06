using Sandbox;

public struct WheelFrictionInfo
{
	public float ExtremumSlip { get; set; }
	public float ExtremumValue { get; set; }
	public float AsymptoteSlip { get; set; }
	public float AsymptoteValue { get; set; }
	public float Stiffness { get; set; }

	public WheelFrictionInfo()
	{
		ExtremumSlip = 1.0f;
		ExtremumValue = 1.0f;
		AsymptoteSlip = 2.0f;
		AsymptoteValue = 0.5f;
		Stiffness = 1.0f;
	}

	public float Evaluate( float slip )
	{
		var value = 0.0f;

		if ( slip <= ExtremumSlip )
		{
			value = (slip / ExtremumSlip) * ExtremumValue;
		}
		else
		{
			value = ExtremumValue - ((slip - ExtremumSlip) / (AsymptoteSlip - ExtremumSlip)) * (ExtremumValue - AsymptoteValue);
		}

		return (value * Stiffness).Clamp( 0, float.MaxValue );
	}
}

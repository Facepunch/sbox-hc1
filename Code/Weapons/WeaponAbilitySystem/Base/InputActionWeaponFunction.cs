namespace Facepunch;

/// <summary>
/// A weapon function that reacts to input actions.
/// </summary>
public abstract class InputActionWeaponFunction : WeaponFunction
{
	public enum InputListenerType
	{
		Pressed,
		Down,
		Released
	}

	/// <summary>
	/// What input action are we going to listen for?
	/// </summary>
	[Property, Category( "Base" )] public List<string> InputActions { get; set; } = new() { "Attack1" };
	
	/// <summary>
	/// Should we perform the action when ALL input actions match, or any?
	/// </summary>
	[Property, Category( "Base" )] public bool RequiresAllInputActions { get; set; }

	/// <summary>
	/// What kind of input are we listening for?
	/// </summary>
	[Property, Category( "Base" )] public InputListenerType InputType { get; set; } = InputListenerType.Down;

	/// <summary>
	/// ActionGraphs action so you can do stuff with visual scripting.
	/// </summary>
	[Property, Category( "Base" )] public Action<InputActionWeaponFunction> OnFunctionExecuteAction { get; set; }

	/// <summary>
	/// Gets the input method
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetInputMethod( string action )
	{
		if ( InputType == InputListenerType.Pressed )
		{
			return Input.Pressed( action );
		}
		else if ( InputType == InputListenerType.Down )
		{
			return Input.Down( action );
		}
		
		return Input.Released( action );
	}

	bool isDown = false;

	protected bool IsDown() => isDown;

	/// <summary>
	/// Called when the input method succeeds.
	/// </summary>
	protected virtual void OnFunctionExecute()
	{
		//
	}

	/// <summary>
	/// When the button is up
	/// </summary>
	protected virtual void OnFunctionUp()
	{
		//
	}

	/// <summary>
	/// When the button is down
	/// </summary>
	protected virtual void OnFunctionDown()
	{
		//
	}

	protected override void OnFixedUpdate()
	{
		// We only care about input actions coming from the owning object.
		if ( !Weapon.PlayerController.IsLocallyControlled )
			return;

		bool matched = false;

		foreach ( var action in InputActions )
		{
			var success = GetInputMethod( action );

			if ( RequiresAllInputActions && !success )
			{
				matched = false;
				break;
			}
			if ( success )
			{
				matched = true;
			}
		}

		if ( matched )
		{
			OnFunctionExecute();
			OnFunctionExecuteAction?.Invoke( this );

			if ( !isDown )
			{
				OnFunctionDown();
				isDown = true;
			}
		}
		else
		{
			if ( isDown )
			{
				OnFunctionUp();
				isDown = false;
			}
		}
	}
}

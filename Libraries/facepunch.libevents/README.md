# facepunch.libevents
Easily dispatch events in your scene when stuff happens.

## Basics
Declare an event type with all the properties you want to pass around.
```csharp
public record DamagedEventArgs(
    GameObject Attacker,
    GameObject Victim,
    int Damage );
```
Implement `IGameEventHandler<T>` for your custom event type in a `Component`.
```csharp
public sealed class MyComponent
    : Component, IGameEventHandler<DamagedEventArgs>
{
    public void OnGameEvent( DamagedEventArgs eventArgs )
    {
        Log.Info( $"{eventArgs.Victim.Name} says \"Ouch!\"" );
    }
}
```
Dispatch the event on a `GameObject` or the `Scene`, which will notify any components in its descendants.
```csharp
GameObject.Dispatch( new DamagedEventArgs( attacker, victim, 50 ) );
```

## Invocation order
You can control the order that handlers are invoked using attributes on the handler method.
* `Early`: run this first
* `Late`: run this last
* `Before<T>`: run this before T's handler
* `After<T>`: run this after T's handler
```csharp
[Early, After<SomeOtherComponent>]
public void OnGameEvent( DamagedEventArgs eventArgs )
{
    Log.Info( $"{eventArgs.Victim.Name} says \"Ouch!\"" );
}
```

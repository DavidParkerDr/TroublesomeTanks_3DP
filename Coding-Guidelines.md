# Coding Guidelines

## General Conventions
- Avoid abbrevations (e.g. use SoundManager instead of SM)
- Avoid shortening names (e.g. use Utilities instead of Utils)

## Namespaces & Folders
- Every class should reside within a namespace.
- Namespace name/structure should match folder name/structure.
- Try to use existing folder structure, or create a new folder if necessary/logical.

Example: A Tankotroller project class within the "Managers" folder should be in the `Tankontroller.Managers` namespace

## Classes
- By default, make classes internal, increase visibility only if necessary/logical.
- Add summaries to classes that include their core responsibilities.
- If a class can be static, make it static.
- Classes should be in `PascalCase`.

Example:
```C#
/// <summary>
/// Singleton reponsible for collision detection logic and some collision responses
/// </summary>
internal class CollisionManager
```

## Methods
- By default, make methods private, increase visibility only when necessary/logical.
- Add summaries to methods, including what they return. Include a summary for any parameters that are not self-explanetory.
- If a method can be static, make it static. 
- Methods should be in `PascalCase`

Example:
```C#
/// <summary>
/// Checks if the tank's corners are within a certain radius of a point.
/// Transforms the point into tank-local space and checks against an AABB.
/// </summary>
/// <returns> True if the bullet radius intersects the tank's collision shape (corners) </returns>
public bool TankInRadius(float pRadius, Vector2 pPoint)
```

## Variables
- All variables should be in `camelCase`, except
- Constants or `readonly` variables should be in `MACRO_CASE`
- Member variables should have the `m` prefix 
- Method arguments/parameters should have the `p` prefix
- Properties should be in 'PascalCase' and have no prefix

Example:
```C#
public class Tank
{
    // Member readonly variable 
    public static readonly int MAX_HEALTH = DGS.Instance.GetInt("MAX_TANK_HEALTH");
    
    // Member variable
    private Vector3 mPosition;

    // Property
    public Vector3 Position => mPosition;

    // Parameter variables with 'p' prefix
    private void AdvancedTrackRotation(float pAngle, bool pForwards)
    {
        // Standard local variable
        float offsetSquared = TRACK_OFFSET * TRACK_OFFSET;
        // ... rest of method
    }
    // ... rest of class
}
```

## Incremental Refactoring Strategy
- Leave the code better than you found it
- When changing a method, apply these guidelines to the method
- When tweaking a variable, apply these guidelines to the variable
- When doing major work on a class, refactor whole class to use these convetions

## Metadata
### Changelog
- 19/01/2026 - Created by Piotr Moskala
- 26/01/2026 - Added a bullet point on properties by Piotr Moskala

### Approvals
- 19/01/2026 - Scott White
- 19/01/2026 - Gray Farshidi
- 19/01/2026 - Adam Szlamp
- 19/01/2026 - Piotr Moskala
- 22/01/2026 - David Parker (client)

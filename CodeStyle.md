# Code style
This document notes the code style we will use

## Classes / Namespaces
- Style: ```UpperCamelCase```
- Example:
```c#
public class ExampleClass : MonoBehaviour {

}
```

## Variables
Try to avoid ```var``` as much as possible in the code

#### Private
- Style: ```_lowerCamelCase```
- Example:
```c#
public class ExampleClass {
    private float _maxRayDist = 12.5f;
    [SerializeField] [Range(0, 15)] private int _maxRetries;
}
```

#### Public
- Style: ```UpperCamelCase```
- Example:
```c#
public class ExampleClass {
    public int CurrentHoverHeight;
}
```

#### Local Scope & parameters
- Style: ```lowerCamelCase```
- Example:
```c#
public class ExampleClass {
    private float vecDist(Vector2 vector) {
        float dist = sqrt(vector.x*vector.x + vector.y*vector.y);
        return dist;
    }
}
```

## File Buildup
Code files should be ordered in this order:
1. Usings (e.g. ```using UnityEngine.Random```)
2. serialized private variables
3. public variables
4. private, protected and internal variables
5. public methods
6. Unity methods
7. private methods

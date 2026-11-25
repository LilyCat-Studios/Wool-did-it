using UnityEngine;

[CreateAssetMenu]
public class ClueBase : ScriptableObject
{
    public int id;
    public int points;

    public string clueName;

    public bool destroyable;
    public bool visible;
    public bool movable;
    public bool directional;
}
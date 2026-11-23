using System;
using UnityEngine;

// Must be a struct: SyncVar only detects changes when the whole value is reassigned.
[Serializable]
public struct SheepData
{
    public Color woolColor;
    public int hatID;
    public int woolShapeID;
    public int footprintID;

    public static SheepData Default => new() { woolColor = Color.white };
}
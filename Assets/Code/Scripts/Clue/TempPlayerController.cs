using UnityEngine;

public class TempPlayerController : MonoBehaviour
{
    public Clue clue;

    private void OnInteract()
    {
        var res = clue.Interact();

        Debug.Log(res);
    }
}
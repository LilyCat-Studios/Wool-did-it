using UnityEngine;

public class Clue : MonoBehaviour
{
    [SerializeField] private ClueBase clueBase;

    public int Interact()
    {
        Destroy(gameObject);

        return clueBase.id;
    }
}
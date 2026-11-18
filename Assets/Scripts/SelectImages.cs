using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectImages : MonoBehaviour {
    [SerializeField] private List<Texture2D> possibleClues;

    private RawImage clueImage;
    public int index;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        index = GetRandomIndex(possibleClues.Count);
        clueImage = GetComponentInChildren<RawImage>();
        clueImage.texture = possibleClues[index];
    }

    public void NextClue() {
        index = GetNextIndex(index, possibleClues.Count);
        clueImage.texture = possibleClues[index];
    }

    public void PrevClue() {
        index = GetPrevIndex(index, possibleClues.Count);
        clueImage.texture = possibleClues[index];
    }
    
    internal static int GetRandomIndex(int count) {
        return UnityEngine.Random.Range(0, count - 1);
    }

    internal static int GetNextIndex(int currentIndex, int count)
    {
        int nextIndex = currentIndex + 1;
        if(nextIndex >= count) 
        {
            nextIndex = 0;
        }
        return nextIndex;
    }

    internal static int GetPrevIndex(int currentIndex, int count)
    {
        int prevIndex = currentIndex - 1;
        if(prevIndex < 0) 
        {
            prevIndex = count - 1;
        }
        return prevIndex;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}

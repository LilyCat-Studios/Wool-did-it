using UnityEngine;

public class CheckGuess : MonoBehaviour {
    [SerializeField] private int goodWhoIndex;
    [SerializeField] private int goodWhatIndex;
    [SerializeField] private int goodWhereIndex;

    [SerializeField] private Transform winPOI;

    private bool _guessGood = true;
    private ParticleSystem system;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        system = GetComponentInChildren<ParticleSystem>();
    }

    public void CheckGuesses() {
        SelectImages[] guessPanels = GetComponentsInChildren<SelectImages>(true);
        _guessGood = AreAllGuessesCorrect(guessPanels);

        playerController controller = gameObject.transform.parent.parent.parent.parent.GetComponentInChildren<playerController>();
        controller.OnPickup(null);

        if(_guessGood) {
            system.Play();
            SheepAI[] ais = gameObject.transform.parent.parent.parent.parent.GetComponentsInChildren<SheepAI>();
            foreach(SheepAI ai in ais) {
                SendToWinPoint(ai, winPOI);
            }
        }
    }

    internal bool AreAllGuessesCorrect(SelectImages[] guessPanels) {
        bool guessGood = true;
        foreach(SelectImages guess in guessPanels) {
            switch(guess.gameObject.name) {
            case "What":
                guessGood = guess.index == goodWhatIndex ? guessGood : false;
                break;
            case "Where":
                guessGood = guess.index == goodWhereIndex ? guessGood : false;
                break;
            case "Who":
                guessGood = guess.index == goodWhoIndex ? guessGood : false;
                break;
                
            }
        }
        return guessGood;
    }

    internal static void SendToWinPoint(SheepAI ai, Transform winPoint) {
        ai.interestPoints.RemoveRange(0, ai.interestPoints.Count);
        ai.interestPoints.Add(winPoint);
        ai.maxWanderDist = 5f;
        ai.destArrivedDist = 1f;
    }

    internal void SetCorrectAnswers(int who, int what, int where) {
        goodWhoIndex = who;
        goodWhatIndex = what;
        goodWhereIndex = where;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
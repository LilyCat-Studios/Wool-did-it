using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[Category("Guessing")]
public class CheckGuessTests
{
    private GameObject board;
    private CheckGuess checkGuess;

    [SetUp]
    public void SetUp()
    {
        board = new GameObject("GuessBoard");
        checkGuess = board.AddComponent<CheckGuess>();
        checkGuess.SetCorrectAnswers(who: 1, what: 2, where: 0);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(board);
    }

    private SelectImages AddPanel(string name, int index)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(board.transform);

        SelectImages selectImages = panel.AddComponent<SelectImages>();
        selectImages.index = index;
        return selectImages;
    }

    private SelectImages[] GetPanels()
    {
        return board.GetComponentsInChildren<SelectImages>(true);
    }

    [Test]
    public void AreAllGuessesCorrect_AllPanelsCorrect_ReturnsTrue()
    {
        AddPanel("Who", 1);
        AddPanel("What", 2);
        AddPanel("Where", 0);

        Assert.That(checkGuess.AreAllGuessesCorrect(GetPanels()), Is.True);
    }

    [TestCase(0, 2, 0)]   // Who wrong
    [TestCase(1, 0, 0)]   // What wrong
    [TestCase(1, 2, 2)]   // Where wrong
    public void AreAllGuessesCorrect_OnePanelWrong_ReturnsFalse(int who, int what, int where)
    {
        AddPanel("Who", who);
        AddPanel("What", what);
        AddPanel("Where", where);

        Assert.That(checkGuess.AreAllGuessesCorrect(GetPanels()), Is.False);
    }

    [Test]
    public void AreAllGuessesCorrect_AllPanelsWrong_ReturnsFalse()
    {
        AddPanel("Who", 0);
        AddPanel("What", 0);
        AddPanel("Where", 2);

        Assert.That(checkGuess.AreAllGuessesCorrect(GetPanels()), Is.False);
    }

    [Test]
    public void AreAllGuessesCorrect_InactivePanel_IsStillChecked()
    {
        AddPanel("Who", 1);
        AddPanel("What", 2);
        AddPanel("Where", 1).gameObject.SetActive(false); // wrong and inactive

        Assert.That(checkGuess.AreAllGuessesCorrect(GetPanels()), Is.False);
    }

    [Test]
    public void AreAllGuessesCorrect_PanelWithOtherName_IsIgnored()
    {
        AddPanel("Who", 1);
        AddPanel("What", 2);
        AddPanel("Where", 0);
        AddPanel("Background", 99);

        Assert.That(checkGuess.AreAllGuessesCorrect(GetPanels()), Is.True);
    }

    [Test]
    public void AreAllGuessesCorrect_NoPanels_ReturnsTrue()
    {
        Assert.That(checkGuess.AreAllGuessesCorrect(GetPanels()), Is.True);
    }


    [Test]
    public void SendToWinPoint_ReplacesAllInterestPoints()
    {
        GameObject sheep = new GameObject("Sheep");
        SheepAI ai = sheep.AddComponent<SheepAI>();
        GameObject pointA = new GameObject("PointA");
        GameObject winPoint = new GameObject("WinPoint");
        sheep.transform.SetParent(board.transform);
        pointA.transform.SetParent(board.transform);
        winPoint.transform.SetParent(board.transform);

        ai.interestPoints = new List<Transform> { pointA.transform };

        CheckGuess.SendToWinPoint(ai, winPoint.transform);

        Assert.That(ai.interestPoints, Has.Count.EqualTo(1));
        Assert.That(ai.interestPoints, Does.Contain(winPoint.transform));
    }

    [Test]
    public void SendToWinPoint_SetsWanderAndArrivalDistance()
    {
        GameObject sheep = new GameObject("Sheep");
        GameObject winPoint = new GameObject("WinPoint");
        sheep.transform.SetParent(board.transform);
        winPoint.transform.SetParent(board.transform);

        SheepAI ai = sheep.AddComponent<SheepAI>();
        ai.interestPoints = new List<Transform>();
        ai.maxWanderDist = 20f;
        ai.destArrivedDist = 3f;

        CheckGuess.SendToWinPoint(ai, winPoint.transform);

        Assert.That(ai.maxWanderDist, Is.EqualTo(5f));
        Assert.That(ai.destArrivedDist, Is.EqualTo(1f));
    }
}
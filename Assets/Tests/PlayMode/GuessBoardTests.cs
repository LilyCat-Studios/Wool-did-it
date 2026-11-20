using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

[Category("Guessing")]
public class GuessBoardTests
{
    private GameObject board;
    private CheckGuess checkGuess;
    private List<Texture2D> clues;
    private SelectImages who;
    private SelectImages what;
    private SelectImages where;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        clues = new List<Texture2D> { new Texture2D(1, 1), new Texture2D(1, 1), new Texture2D(1, 1) };

        board = new GameObject("GuessBoard");
        checkGuess = board.AddComponent<CheckGuess>();
        checkGuess.SetCorrectAnswers(who: 1, what: 2, where: 0);

        who = CreatePanel("Who");
        what = CreatePanel("What");
        where = CreatePanel("Where");
        
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(board);

        foreach (Texture2D clue in clues)
        {
            Object.Destroy(clue);
        }

        yield return null;
    }

    private SelectImages CreatePanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(board.transform);
        
        GameObject image = new GameObject("Image");
        image.transform.SetParent(panel.transform);
        image.AddComponent<RawImage>();

        SelectImages selectImages = panel.AddComponent<SelectImages>();
        selectImages.SetClues(clues);
        return selectImages;
    }

    private bool BoardIsCorrect()
    {
        return checkGuess.AreAllGuessesCorrect(board.GetComponentsInChildren<SelectImages>(true));
    }

    [UnityTest]
    public IEnumerator Start_SelectsAClueWithinTheList()
    {
        Assert.That(who.index, Is.GreaterThanOrEqualTo(0).And.LessThan(clues.Count));
        yield break;
    }

    [UnityTest]
    public IEnumerator Start_ShowsTheSelectedClueImage()
    {
        RawImage image = who.GetComponentInChildren<RawImage>();

        Assert.That(image.texture, Is.EqualTo(clues[who.index]));
        yield break;
    }

    [UnityTest]
    public IEnumerator NextClue_ShowsTheNextClueImage()
    {
        RawImage image = what.GetComponentInChildren<RawImage>();
        
        what.NextClue();
        yield return null;

        Assert.That(image.texture, Is.EqualTo(clues[what.index]));
    }

    [UnityTest]
    public IEnumerator PrevClue_FromTheFirstClue_WrapsAroundToTheLastClue()
    {
        RawImage image = where.GetComponentInChildren<RawImage>();
        where.index = 0;

        where.PrevClue();
        yield return null;

        Assert.That(where.index, Is.EqualTo(2));
        Assert.That(image.texture, Is.EqualTo(clues[2]));
    }

    [UnityTest]
    public IEnumerator WrongSelection_IsRejected()
    {
        who.index = 1;
        what.index = 2;
        where.index = 1;

        Assert.That(BoardIsCorrect(), Is.False);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CyclingToTheCorrectClue_MakesTheGuessCorrect()
    {
        who.index = 0;
        what.index = 2;
        where.index = 0;
        Assert.That(BoardIsCorrect(), Is.False);

        who.NextClue();
        yield return null;

        Assert.That(who.index, Is.EqualTo(1));
        Assert.That(BoardIsCorrect(), Is.True);
    }
}
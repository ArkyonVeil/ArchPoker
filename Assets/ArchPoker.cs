using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSetData
{
    public string setName;
    public Sprite symbolSprite;
    public Sprite aceSprite;
    public Color typeColour;
    public CardType cardType;

    public CardObject[] SetInstancedCardSet;

    public GameObject Two;
    public GameObject Three;
    public GameObject Four;
    public GameObject Five;
    public GameObject Six;
    public GameObject Seven;
    public GameObject Eight;
    public GameObject Nine;
    public GameObject Ten;
    public GameObject Jack;
    public GameObject Queen;
    public GameObject King;
    public GameObject Ace;
    

    public void RegisterCardOfDeck(CardObject cObj)
    {
        if (SetInstancedCardSet == null ||SetInstancedCardSet.Length == 0)
        {
            SetInstancedCardSet = new CardObject[13];
        }

        SetInstancedCardSet[(int)cObj.cardValueType-1] = cObj;
        switch (cObj.cardValueType)
        {
            case CardValue.Two:
                Two = cObj.gameObject;
                break;
            case CardValue.Three:
                Three = cObj.gameObject;
                break;
            case CardValue.Four:
                Four = cObj.gameObject;
                break;
            case CardValue.Five:
                Five = cObj.gameObject;
                break;
            case CardValue.Six:
                Six = cObj.gameObject;
                break;
            case CardValue.Seven:
                Seven = cObj.gameObject;
                break;
            case CardValue.Eight:
                Eight = cObj.gameObject;
                break;
            case CardValue.Nine:
                Nine = cObj.gameObject;
                break;
            case CardValue.Ten:
                Ten = cObj.gameObject;
                break;
            case CardValue.Jack:
                Jack = cObj.gameObject;
                break;
            case CardValue.Queen:
                Queen = cObj.gameObject;
                break;
            case CardValue.King:
                King = cObj.gameObject;
                break;
            case CardValue.Ace:
                Ace = cObj.gameObject;
                break;
            default:
                break;
        }
    }
}

public enum CardType
{
    Eyes,
    Worlds,
    Doors,
    Order,
}

public enum CardValue
{
    Two = 1,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}


[System.Serializable]
public enum ContextualDialogue
{
    Random,
    NewGame,
    NewRound,
    JustBetted,
    Raised,
    Checked,
    Called,
    Folded,
    AllIn,    
    SomeoneElseRaised,
    SomeoneElseChecked,
    SomeoneElseCalled,
    SomeoneElseFolded,
    SomeoneElseAllIn,
    SomeoneElseBetted,
    LostTheRound,
    LostTheGame,
    GoingToExpire,
}

[System.Serializable]
public class DialogueOptions
{
    public List<string> RandomComment;
    public List<string> NewGame;
    public List<string> NewRound;
    public List<string> JustBetted;
    public List<string> Raised;
    public List<string> Checked;
    public List<string> Called;
    public List<string> Folded;
    public List<string> AllIn;
    public List<string> SomeoneElseRaised;
    public List<string> SomeoneElseChecked;
    public List<string> SomeoneElseCalled;
    public List<string> SomeoneElseFolded;
    public List<string> SomeoneElseAllIn;
    public List<string> SomeoneElseBetted;
    public List<string> LostTheRound;
    public List<string> LostTheGame;
    public List<string> GoingToExpire;

    public string GetRandomStringFromContext(ContextualDialogue context)
    {
        return "text";
        switch (context)
        {
            case ContextualDialogue.Random:
                return RandomComment[Random.Range(0, RandomComment.Count)];
            case ContextualDialogue.NewGame:
                return NewGame[Random.Range(0, NewGame.Count)];
            case ContextualDialogue.NewRound:
                return NewRound[Random.Range(0, NewRound.Count)];
            case ContextualDialogue.JustBetted:
                return JustBetted[Random.Range(0, JustBetted.Count)];
            case ContextualDialogue.Raised:
                return Raised[Random.Range(0, Raised.Count)];
            case ContextualDialogue.Checked:
                return Checked[Random.Range(0, Checked.Count)];
            case ContextualDialogue.Called:
                return Called[Random.Range(0, Called.Count)];
            case ContextualDialogue.Folded:
                return Folded[Random.Range(0, Folded.Count)];
            case ContextualDialogue.AllIn:
                return AllIn[Random.Range(0, AllIn.Count)];
            case ContextualDialogue.SomeoneElseRaised:
                return SomeoneElseRaised[Random.Range(0, SomeoneElseRaised.Count)];
            case ContextualDialogue.SomeoneElseChecked:
                return SomeoneElseChecked[Random.Range(0, SomeoneElseChecked.Count)];
            case ContextualDialogue.SomeoneElseCalled:
                return SomeoneElseCalled[Random.Range(0, SomeoneElseCalled.Count)];
            case ContextualDialogue.SomeoneElseFolded:
                return SomeoneElseFolded[Random.Range(0, SomeoneElseFolded.Count)];
            case ContextualDialogue.SomeoneElseAllIn:
                return SomeoneElseAllIn[Random.Range(0, SomeoneElseAllIn.Count)];
            case ContextualDialogue.SomeoneElseBetted:
                return SomeoneElseBetted[Random.Range(0, SomeoneElseBetted.Count)];
            case ContextualDialogue.LostTheRound:
                return LostTheRound[Random.Range(0, LostTheRound.Count)];
            case ContextualDialogue.LostTheGame:
                return LostTheGame[Random.Range(0, LostTheGame.Count)];
            case ContextualDialogue.GoingToExpire:
                return GoingToExpire[Random.Range(0, GoingToExpire.Count)];
        }
        return " . . . ";
    }
}

public class PokerHand
{
    public Vector3 PokerHandLocation;
    public float PokerHandRotation;
    public float CardSpread;
    Transform pokerHandTransform;
    public List<CardObject> cards;
    //Recalculates the cards so that they're paralel to each other according to card spread, as well as the correct location and rotation.
    public void RecalculateCardLocations()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].GoToLocation(PokerHandLocation + pokerHandTransform.right *  ((i - (cards.Count/2)) * (CardSpread*30)));

            cards[i].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, PokerHandRotation);
        }
    }
    
    public void Setup(Transform locationData)
    {
        locationData.SetParent(CardCreator.I.cardTransformParent);
        PokerHandLocation = locationData.GetComponent<RectTransform>().anchoredPosition;
        PokerHandRotation = locationData.transform.rotation.eulerAngles.z;
        pokerHandTransform = locationData;
        cards = new List<CardObject>();
        RecalculateCardLocations();
    }

    public void ShuffleHand()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(0, cards.Count);
            CardObject temp = cards[randomIndex];
            cards[randomIndex] = cards[i];
            cards[i] = temp;
        }
    }
}

public enum PlayerAction
{
    None,//Player couldn't act. Skips the turn and sends an error.
    WaitHuman,//Essentially stops the game until a proper decision, usable by the humans playing this.
    Fold,//Nope, out of this. Player is out of the round and will not play until the next round.
    Check,//0$ bet, 
    Call,//Betting the same amount as the last bet.
    Bet,//Bigger than call, but of arbitrary size. Usable by AI only.
    Raise,//Doubles the last bet.
    AllIn,//Oh boy, here it comes.
    ShowHand,//Reveals the hand of the player, always used if still in the game during the River phase.
}

public enum PlayerState
{
    StaringIntoSpace,//You fucked up Ark.
    Waiting,//New round, waiting to do something
    Dead,//Has no money.
    Folded,//Has folded, will no longer participate,
    Checked,
    Bet,
    Call,
    Raise,
    AllIn,
    HandRevealed,
}
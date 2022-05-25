using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCreator : MonoBehaviour
{
    public static CardCreator I;
    public GameObject cardPrefab;

    public Transform cardTransformParent;

    public List<GameObject> cardArtTemplatePrefabs;

    //Potential improvements include creating a class of 'card type' which includes all this information.
    public CardSetData setOfEyes;
    public CardSetData setOfWorlds;
    public CardSetData setOfDoors;
    public CardSetData setOfOrder;

    public List<CardObject> cardCompleteDeck;

    public void Awake()
    {
        I = this;
    }
    private void Start()
    {
        cardCompleteDeck = new List<CardObject>();
        CreateDeck(); //Create the deck of cards.

        for (int i = 0; i < cardCompleteDeck.Count; i++)
        {
            GameManager.I.dealerDeck.cards.Add(cardCompleteDeck[i]);
            cardCompleteDeck[i].FlipState(true);
            GameManager.I.dealerDeck.RecalculateCardLocations();
        }
        //StartCoroutine(queueDeckAssembly());
        //StartCoroutine(MassFlipAnimation());
    }

    IEnumerator queueDeckAssembly()
    {
        yield return new WaitForSeconds(1.5f);
        DebugAssembleDeck();
    }

    //Spawns 52 cards and puts them in 1 place.
    void CreateDeck()
    {
        cardCompleteDeck.AddRange(CreateCardSet(CardType.Order));
        cardCompleteDeck.AddRange(CreateCardSet(CardType.Doors));
        cardCompleteDeck.AddRange(CreateCardSet(CardType.Worlds));
        cardCompleteDeck.AddRange(CreateCardSet(CardType.Eyes));       
    }

    /// <summary>
    /// Assembles the deck into an easily readable format for checking if it looks good.
    /// </summary>
    void DebugAssembleDeck()
    {
        int yD = 0;//Y displacement
        int sC = 0;//set count
        for (int i = 0; i < cardCompleteDeck.Count; i++)
        {
            Vector3 CardLoc = new Vector3(-(150*7)+150*sC, -400+200*yD, 0);
            cardCompleteDeck[i].GoToLocation(CardLoc);
            sC++;
            if (sC == 13)
            {
                sC = 0;
                yD++;
            }

        }
    }

    IEnumerator MassFlipAnimation()
    {
        yield return new WaitForSeconds(2f);
        int i1 = 0;
        int i2 = 15;
        int i3 = 30;
        int i4 = 45;
        while (true)
        {
            cardCompleteDeck[i1].FlipState(true);
            cardCompleteDeck[i2].FlipState(false);
            cardCompleteDeck[i3].FlipState(true);
            cardCompleteDeck[i4].FlipState(false);
            i1++;
            i2++;
            i3++;
            i4++;

            if (i1 == 52)
            {
                i1 = 0;
            }
            if (i2 == 52)
            {
                i2 = 0;
            }
            if (i3 == 52)
            {
                i3 = 0;
            }
            if (i4 == 52)
            {
                i4 = 0;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
       
    }

    

    public void DebugFlipDeckTest()
    {
        for (int i = 0; i < cardCompleteDeck.Count; i++)
        {
            cardCompleteDeck[i].FlipState(true);
        }
    }

    List<CardObject> CreateCardSet(CardType cTyp)
    {
        List<CardObject> cardSet = new List<CardObject>();
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Two));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Three));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Four));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Five));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Six));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Seven));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Eight));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Nine));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Ten));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Jack));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Queen));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.King));
        cardSet.Add(Instantiate(cardPrefab, cardTransformParent).GetComponent<CardObject>().SetupCard(cTyp, CardValue.Ace));

        CardSetData currentSet = null;
        switch (cTyp)
        {
            case CardType.Eyes:
                currentSet = setOfEyes;
                break;
            case CardType.Worlds:
                currentSet = setOfWorlds;
                break;
            case CardType.Doors:
                currentSet = setOfDoors;
                break;
            case CardType.Order:
                currentSet = setOfOrder;
                break;
            default:
                break;
        }

        for (int i = 0; i < cardSet.Count; i++)
        {
            currentSet.RegisterCardOfDeck(cardSet[i]);
        }
        return cardSet;
    }



    public CardSetData GetSetOfCardType(CardType ctype)
    {
        switch (ctype)
        {
            case CardType.Eyes:
                return setOfEyes;
            case CardType.Doors:
                return setOfDoors;
            case CardType.Worlds:
                return setOfWorlds;
            case CardType.Order:
                return setOfOrder;
            default:
                return null;
        }
    }

    public string GetTextOfCardValue(CardValue cardValue)
    {
        switch (cardValue)
        {
            case CardValue.Two:
                return "2";
            case CardValue.Three:
                return "3";
            case CardValue.Four:
                return "4";
            case CardValue.Five:
                return "5";
            case CardValue.Six:
                return "6";
            case CardValue.Seven:
                return "7";
            case CardValue.Eight:
                return "8";
            case CardValue.Nine:
                return "9";
            case CardValue.Ten:
                return "10";
            case CardValue.Jack:
                return "J";
            case CardValue.Queen:
                return "Q";
            case CardValue.King:
                return "K";
            case CardValue.Ace:
                return "A";
            default:
                break;
        }
        return "Oops";
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PokerCharacterObject : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI CharacterNameText;
    public Image characterImage;
    public bool isAI;//Auto decides stuff.
    public Transform speechBubbleLocation;
    public Transform HandLocation;

    public string CharacterName
    {
        get { return charName; }
        set { { charName = value; CharacterNameText.text = charName; } }
    }
    string charName;
    //Stats: 1-10
    public int Aggressiveness;//Increases chance of more aggressive actions like raising or All In
    public int Logic;//How logical should their actions be? On failure, they will act randomly.
    public int Talkiness;//How often they will commit random banter. 
    public int Money { get { return pMoney; } set{ pMoney = value; moneyText.text = pMoney.ToString() + " $"; } }
    int pMoney;
    public PlayerState playerState { get { return pPlayerState; } set { pPlayerState = value; actionText.text = pStateToText(value); } }
    PlayerState pPlayerState;
    
    public int HandTier;//Determined at victory.
    public float KickerValue;//Determined at victory, only used in HighCards due to time contraints.

    public PokerHand hand;
    [SerializeField]
    public DialogueOptions dialogueOptions;

    string pStateToText(PlayerState pState)
    {
        switch (pState)
        {
            case PlayerState.Folded:
                return "Fold";
            case PlayerState.Checked:
                return "Check";
            case PlayerState.Call:
                return "Call";
            case PlayerState.Raise:
                return "Raise";
            case PlayerState.AllIn:
                return "All In";
            case PlayerState.Dead:
                return "Out";
            case PlayerState.StaringIntoSpace:
                return "Staring into Space";
            case PlayerState.Waiting:
                return "";
            case PlayerState.Bet:
                break;
            case PlayerState.HandRevealed:
                return "";
            default:
                return "";
        }
        return "";
    }

    public void Start()
    {
        hand = new PokerHand();
        hand.Setup(HandLocation);
        hand.CardSpread = 2;
    }

    public void AskForComment(ContextualDialogue context)
    {
        if (!isAI)
            return;

        dialogueOptions.GetRandomStringFromContext(context);
    }

    public PlayerAction GetPlayerAction()
    {
        if (!isAI)
            return PlayerAction.WaitHuman;


        switch (playerState)
        {
            case PlayerState.StaringIntoSpace:
                return PlayerAction.None;
            case PlayerState.Waiting:
                break;        
            case PlayerState.Checked:
                break;
            case PlayerState.Bet:
                break;
            case PlayerState.Call:
                break;
            case PlayerState.Raise:
                break;
            case PlayerState.AllIn:
                break;
            case PlayerState.HandRevealed:
                return PlayerAction.ShowHand;
            default:
                break;
        }

        //If finale, show our hand.
        if (GameManager.I.gameProgress == GameManager.GameProgress.River)
        {
            playerState = PlayerState.HandRevealed;
            return PlayerAction.ShowHand;
        }

        bool wantIn = false;
        float handValue = GetHandWinEyeballProbability();
        if (handValue > Random.Range(3f- Logic/3f, 5f))
            wantIn = true;


        if (wantIn)
        {
            {
                float AggressionRoll = Random.Range(-5, 5f) + Aggressiveness;

                if (AggressionRoll > 10)
                {
                    playerState = PlayerState.AllIn;
                    return PlayerAction.AllIn;
                }


                if (AggressionRoll > 6 && Money > GameManager.I.PrevBet * 2)
                {
                    playerState = PlayerState.Raise;
                    return PlayerAction.Raise;
                }
                else if (AggressionRoll > 6 && Money <= GameManager.I.PrevBet)
                {
                    playerState = PlayerState.AllIn;
                    return PlayerAction.AllIn;
                }

                if (AggressionRoll > 3 && Money > GameManager.I.PrevBet && GameManager.I.PrevBet > 0)
                {
                    playerState = PlayerState.Call;
                    return PlayerAction.Call;
                }
                else if (AggressionRoll > 3 && GameManager.I.PrevBet == 0)
                {
                    playerState = PlayerState.Checked;
                    return PlayerAction.Check;
                }
                if (AggressionRoll > 0 && Money > GameManager.I.PrevBet * 2)
                {
                    playerState = PlayerState.Call;
                    return PlayerAction.Call;
                }
                else if (AggressionRoll > 0 && GameManager.I.PrevBet == 0)
                {
                    playerState = PlayerState.Checked;
                    return PlayerAction.Check;
                }
                    

                AggressionRoll = Random.Range(0, Aggressiveness);
                if (AggressionRoll > 4)
                {
                    playerState = PlayerState.AllIn;
                    return PlayerAction.AllIn;
                }
                else
                {
                    playerState = PlayerState.Folded;
                    return PlayerAction.Fold;
                }
            }
        }
        else
        {
            { 
            if (GameManager.I.PrevBet == 0)
                {
                    playerState = PlayerState.Checked;
                    return PlayerAction.Check;
                }                   
                else
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        playerState = PlayerState.Folded;
                        return PlayerAction.Fold;
                    }
                    else
                    {//Mostly so the AI will be more aggressive.
                        if (Money > GameManager.I.PrevBet)
                        {
                            playerState = PlayerState.Call;
                            return PlayerAction.Call;
                        }
                        else
                        {
                            playerState = PlayerState.Folded;
                            return PlayerAction.Fold;
                        }                                                
                    }
                        
                }
                    
            }
        }

        return PlayerAction.Fold;
    }

    
    //Determine hand tier.
    //Determine kicker value.
    public void GetHandFinalValue()
    {
        
        List<CardObject> cards = new List<CardObject>();
        cards.AddRange(hand.cards);
        cards.AddRange(GameManager.I.communityHand.cards);
        cards = cards.OrderBy(x => x.cardType).ThenBy(x => x.cardValueType).ToList();

        //Debug
        Debug.Log(CharacterName + "'s Hand: ");
        for (int i = 0; i < cards.Count; i++)
        {
            Debug.Log(cards[i].CardName);
        }
        Debug.Log("----------------");

        //Highcard
        HandTier = 1;
        KickerValue = cards[cards.Count-1].cardValue;

        //Pair
        float PrevValue = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardValue == PrevValue)
            {
                HandTier = 2;
                break;
            }
            PrevValue = cards[i].cardValue;
        }
        //Full disclosure, due to time constraints, this later section was written by Codex.

        //Two Pairs
        PrevValue = 0;
        int Pairs = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardValue == PrevValue)
            {
                Pairs++;
            }
            PrevValue = cards[i].cardValue;
        }
        if (Pairs == 2)
        {
            HandTier = 3;
        }
        //Three of a Kind
        PrevValue = 0;
        int ThreeOfAKind = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardValue == PrevValue)
            {
                ThreeOfAKind++;
            }
            PrevValue = cards[i].cardValue;
        }
        if (ThreeOfAKind == 3)
        {
            HandTier = 4;
        }
        //Straight
        bool Straight = true;
        for (int i = 0; i < cards.Count - 1; i++)
        {
            if (cards[i].cardValue != cards[i + 1].cardValue + 0.1f)
            {
                Straight = false;
                break;
            }
        }
        if (Straight)
        {
            HandTier = 5;
        }
        //Flush
        bool Flush = true;
        for (int i = 0; i < cards.Count - 1; i++)
        {
            if (cards[i].cardType != cards[i + 1].cardType)
            {
                Flush = false;
                break;
            }
        }
        if (Flush)
        {
            HandTier = 6;
        }
        //Full House
        if (ThreeOfAKind == 3 && Pairs == 2)
        {
            HandTier = 7;
        }
        //Four of a Kind
        PrevValue = 0;
        int FourOfAKind = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardValue == PrevValue)
            {
                FourOfAKind++;
            }
            PrevValue = cards[i].cardValue;
        }
        if (FourOfAKind == 4)
        {
            HandTier = 8;
        }
        //Straight Flush
        if (Straight && Flush)
        {
            HandTier = 9;
        }
        //Royal Flush
        if (Straight && Flush)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardValueType == CardValue.Ace)
                {
                    HandTier = 10;
                    break;
                }
            }
        }
    }

    /* There is probably a way to calculate this legitimately in order in order to get
     * mathematical accuracy in how likely a hand is to win the pot.
     * 
     * Because this is a short 2 day project, I'm going to make it quick and dirty, just watch.
     */
    public float GetHandWinEyeballProbability()
    {
        List<CardObject> cards = new List<CardObject>();
        cards.AddRange(hand.cards);
        cards.AddRange(GameManager.I.communityHand.cards);
        cards = cards.OrderBy(x => x.cardValue).ToList();

        float handValue = 0;

        //What is the value of all the cards combined?
        for (int i = 0; i < hand.cards.Count; i++)
        {
            handValue += hand.cards[i].cardValue;
        }
        //Do any of them match each other?
        float lastValue = 0;
        float bonus = 0;
        for (int i = 0; i < hand.cards.Count; i++)
        {
            if (hand.cards[i].cardValue == lastValue)
            {
                bonus += 1;
                handValue += bonus;
            }
            lastValue = hand.cards.Count;
        }

        //Are they in order?
        bonus = 0;
        for (int i = 0; i < hand.cards.Count; i++)
        {
            lastValue = hand.cards[i].cardValue;
            if (hand.cards[i].cardValue == lastValue + 0.1f)
            {
                bonus += 1;
                handValue += bonus;
            }
        }

        //Are they of the same suit?
        bonus = 0;
        cards.OrderBy(x => x.cardType);
        CardType prevCardType = CardType.Eyes;
        for (int i = 0; i < hand.cards.Count; i++)
        {
            prevCardType = hand.cards[i].cardType;
            if (hand.cards[i].cardType == prevCardType)
            {
                bonus += 1;
                handValue += bonus;
            }
        }
        //Compensate with unknown community cards, just so the AI is a bit more aggressive.
        handValue += (7 - cards.Count) / 2.25f;
        return handValue;
    }
}

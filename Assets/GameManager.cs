using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public TextMeshProUGUI MoneyInPotText;
    public TextMeshProUGUI CurrentPhaseText;
    public float beatsPerSecond = 5;
    public float cardFlySpeed;
    float beatTime;
    public int StartingMoney = 100;
    public int MinimumBet = 10;
    public int SmallBlindValue = 10;
    public int PrevBet = 0;
    public int Ante = 5;//How much money is deduced from every player at every new round.
    public PokerCharacterObject[] Players;

    public PokerHand communityHand;//Available to all players to make their hands.
    public PokerHand dealerDeck;//The whole deck of cards

    public Transform communityHandLoc;
    public Transform dealerDeckLoc;

    public int beatTimeWait;//Freezes the game for this amount of beats.
    public float secondsTimeWait;//Freezes the game for this amount of seconds.

    int dealer;
    int currentPlayerTurn;
    public GameProgress gameProgress;
    public int MoneyInPot { get { return moneyInPot; } set { moneyInPot = value; MoneyInPotText.text = moneyInPot + " $"; } }
    int moneyInPot = 0;

    bool isGameLive = false;

    public enum GameProgress
    {
        Preflop,
        Flop,
        Turn,
        River
    }

    private void Awake()
    {
        I = this;
        communityHand = new PokerHand();
        communityHand.Setup(communityHandLoc);
        communityHand.CardSpread = 4.5f;

        dealerDeck = new PokerHand();
        dealerDeck.Setup(dealerDeckLoc);
    }

    public void Start()
    {


        beatTimeWait = 2;

        MoneyInPot = 0;
    }

    public void Update()
    {
        if (secondsTimeWait > 0)
        {
            secondsTimeWait -= Time.deltaTime;
        }
        else
        {
            beatTime += Time.deltaTime;
        }

        if (beatTime > 1f / beatsPerSecond)
        {
            beatTime -= 1f / beatsPerSecond;
            GameBeat();
        }
    }


    /// <summary>
    /// A 'tick' in the game logic
    /// </summary>
    public void GameBeat()
    {
        if (beatTimeWait > 0)
        {
            beatTimeWait--;
            return;
        }
        if (currentPlayerTurn > Players.Length - 1)
        {
            currentPlayerTurn = 0;
        }

        if (!isGameLive)
        {
            isGameLive = true;
            BeginGame();
            return;
        }


        if (Players[currentPlayerTurn].playerState == PlayerState.Folded
            || Players[currentPlayerTurn].playerState == PlayerState.Dead
            || Players[currentPlayerTurn].playerState == PlayerState.AllIn)
        {
            currentPlayerTurn++;
            return;
        }

        PlayerAction playerAction = Players[currentPlayerTurn].GetPlayerAction();

        switch (playerAction)
        {
            case PlayerAction.None:
                Debug.LogError("Player couldn't do anything, mnwhat?");
                currentPlayerTurn++;
                break;
            case PlayerAction.WaitHuman:
                //Human is not yet done playing.
                return;
            case PlayerAction.Fold:
                currentPlayerTurn++;
                return;
            case PlayerAction.Check:
                currentPlayerTurn++;
                break;
            case PlayerAction.Call:
                if (PrevBet > Players[currentPlayerTurn].Money)
                {
                    MoneyInPot += Players[currentPlayerTurn].Money;
                    Players[currentPlayerTurn].Money = 0;
                    Players[currentPlayerTurn].playerState = PlayerState.AllIn;
                    currentPlayerTurn++;
                    return;
                }
                else
                {
                    Players[currentPlayerTurn].Money -= PrevBet;
                    MoneyInPot += PrevBet;
                    currentPlayerTurn++;
                }
                currentPlayerTurn++;
                break;
            case PlayerAction.Bet:
                //Cut for time.
                currentPlayerTurn++;
                break;
            case PlayerAction.Raise:
                if (PrevBet * 2 > Players[currentPlayerTurn].Money)
                {
                    MoneyInPot += Players[currentPlayerTurn].Money * 2;
                    Players[currentPlayerTurn].Money = 0;
                    Players[currentPlayerTurn].playerState = PlayerState.AllIn;
                    PrevBet *= 2;
                    currentPlayerTurn++;
                    return;
                }
                else
                {
                    Players[currentPlayerTurn].Money -= PrevBet * 2;
                    MoneyInPot += PrevBet * 2;
                    PrevBet *= 2;
                    currentPlayerTurn++;
                }
                currentPlayerTurn++;
                break;
            case PlayerAction.AllIn:
                MoneyInPot += Players[currentPlayerTurn].Money;
                if (Players[currentPlayerTurn].Money > PrevBet)
                {
                    PrevBet = Players[currentPlayerTurn].Money;
                }
                Players[currentPlayerTurn].Money = 0;
                currentPlayerTurn++;
                break;
            case PlayerAction.ShowHand:
                Players[currentPlayerTurn].playerState = PlayerState.HandRevealed;              
                Players[currentPlayerTurn].hand.cards[0].FlipState(false);
                Players[currentPlayerTurn].hand.cards[1].FlipState(false);
                currentPlayerTurn++;
                break;
            default:
                break;
        }

        //Check for instant win.

        if (GetNumPlayersOutofRound() == Players.Length - 1)
        {
            DefaultWin();
        }

        //Check for next Phase

        if (GetNumPlayersThatNeedAct() == 0)
        {
            NextPhase();
        }
    }



    public void NextPhase()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerState != PlayerState.Folded || Players[i].playerState != PlayerState.Dead || Players[i].playerState != PlayerState.AllIn)
            {
                Players[i].playerState = PlayerState.Waiting;
            }
        }
        switch (gameProgress)
        {
            case GameProgress.Preflop:
                gameProgress = GameProgress.Flop;//It's now the flop, first 3 cards are revealed.
                TransferCards(dealerDeck, communityHand, 3, false);
                CurrentPhaseText.text = "Flop";
                break;
            case GameProgress.Flop:
                gameProgress = GameProgress.Turn;//Another reveal, last betting round.
                TransferCards(dealerDeck, communityHand, 1, false);
                CurrentPhaseText.text = "Turn";
                break;
            case GameProgress.Turn://The river landed, all players must show their hands.
                TransferCards(dealerDeck, communityHand, 1, false);
                gameProgress = GameProgress.River;
                CurrentPhaseText.text = "River";
                break;
            case GameProgress.River:
                Showdown();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Last player still standing while everyone folded before the showdown.
    /// </summary>
    void DefaultWin()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerState != PlayerState.Dead || Players[i].playerState != PlayerState.Folded)
            {
                TransferMoney(false, MoneyInPot, Players[i]);
                StartNewRound();
            }
        }
    }

    void Showdown()
    {

    }

    /// <summary>
    /// New game, everyone has money, and is also alive and well.
    /// </summary>
    public void BeginGame()
    {
        dealerDeck.ShuffleHand();
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Money = 100;
            Players[i].playerState = PlayerState.Waiting;
            TransferCards(dealerDeck, Players[i].hand, 2);
        }
        secondsTimeWait += 2;
    }

    public void StartNewRound()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerState != PlayerState.Dead)
            {
                Players[i].playerState = PlayerState.Waiting;
                TransferCards(dealerDeck, Players[i].hand, 2);
            }
        };
        secondsTimeWait += 2;
    }

    /// <summary>
    /// Transfers a card between two hands and updates them.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="sourceHand"></param>
    /// <param name="destinationHand"></param>
    public void TransferCard(CardObject card, PokerHand sourceHand, PokerHand destinationHand)
    {
        sourceHand.cards.Remove(card);
        destinationHand.cards.Add(card);
        sourceHand.RecalculateCardLocations();
        destinationHand.RecalculateCardLocations();
    }
    /// <summary>
    /// Transfers cards from the top of a pile;
    /// </summary>
    /// <param name="sourceHand"></param>
    /// <param name="destinationHand"></param>
    /// <param name="Number"></param>
    public void TransferCards(PokerHand sourceHand, PokerHand destinationHand, int Number, bool cardisFaceDownState = true)
    {
        for (int i = 0; i < Number; i++)
        {
            var card = sourceHand.cards[sourceHand.cards.Count - 1];
            sourceHand.cards.RemoveAt(sourceHand.cards.Count - 1);
            destinationHand.cards.Add(card);
            card.FlipState(cardisFaceDownState);
        }

        sourceHand.RecalculateCardLocations();
        destinationHand.RecalculateCardLocations();
    }

    public void TransferMoney(bool ToPot, int Amount, PokerCharacterObject player)
    {
        if (ToPot)
        {
            MoneyInPot += Amount;
            player.Money -= Amount;
        }
        else
        {
            MoneyInPot -= Amount;
            player.Money += Amount;
        }
    }

    public void DetermineWinner()
    {

    }


    /// <summary>
    /// Folded, or dead.
    /// </summary>
    /// <returns></returns>
    public int GetNumPlayersOutofRound()
    {
        int v = 0;
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerState == PlayerState.Folded || Players[i].playerState == PlayerState.Dead)
            {
                v++;
            }
        }
        return v;
    }

    /// <summary>
    /// Called, checked or all in.
    /// </summary>
    /// <returns></returns>
    public int GetNumPlayersReadyForNextPhase()
    {
        int v = 0;
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerState == PlayerState.Call || Players[i].playerState == PlayerState.Checked || Players[i].playerState == PlayerState.AllIn)
            {
                v++;
            }
        }
        return v;
    }

    /// <summary>
    /// Waiting, Betting or Raised.
    /// </summary>
    /// <returns></returns>
    public int GetNumPlayersThatNeedAct()
    {
        int v = 0;
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerState == PlayerState.Waiting || Players[i].playerState == PlayerState.Bet || Players[i].playerState == PlayerState.Raise)
            {
                v++;
            }
        }
        return v;
    }


}

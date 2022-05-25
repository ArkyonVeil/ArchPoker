using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardObject : MonoBehaviour
{
    public TextMeshProUGUI cardType1;
    public TextMeshProUGUI cardType2;

    public Image cardTypeSymbol1;
    public Image cardTypeSymbol2;

    public Image cardBorder;
    public Image cardFlipOverlay;
    public Image flipArt;

    public List<GameObject> cardFaceUpContents;

    public bool isFaceDown { get { return faceDown; } }
    bool faceDown;
    
    public CardType cardType;//Eyes,Worlds,Doors,Order
    public CardValue cardValueType;//2,3,4,5(...),queen,king,ace
    public float cardValue { get { return (float)cardValueType * 0.1f; } }

    Sprite cardSprite;
    string cardValuestring;

    public Transform cardArtContainer;
    RectTransform rTransform;

    Vector3 goal;
    float speed;

    CardSetData cardSet;
    public string CardName { get{ return CardCreator.I.GetTextOfCardValue(cardValueType) + " of " + cardType; } }

    public void Start()
    {
        rTransform = GetComponent<RectTransform>();
        speed = GameManager.I.cardFlySpeed;
        flipArt.enabled = false;
    }

    public CardObject SetupCard(CardType cType, CardValue cValue)
    {
        cardType = cType;
        cardValueType = cValue;
        cardSet = CardCreator.I.GetSetOfCardType(cType);
        cardSprite = cardSet.symbolSprite;
        cardValuestring = CardCreator.I.GetCharOfCardValue(cValue);

        cardTypeSymbol1.sprite = cardSprite;
        cardTypeSymbol2.sprite = cardSprite;

        cardType1.text = cardValuestring;
        cardType2.text = cardValuestring;

        GameObject Pf = CardCreator.I.cardArtTemplatePrefabs[(int)cValue - 1];
        Instantiate(Pf, cardArtContainer);
        FinishCardArt();

        return this;
    }

    public void FinishCardArt()
    {
        var CCount = cardArtContainer.GetChild(0).childCount;
        List<Transform> childTransforms = new List<Transform>();
        for (int i = 0; i < CCount; i++)
        {
            childTransforms.Add(cardArtContainer.GetChild(0).GetChild(i));
        }

        foreach (var child in childTransforms)
        {
            if (child.name.Contains("symbImag"))
            {
                child.GetComponent<Image>().sprite = cardSprite;
                Vector3 clocalScale = child.transform.localScale;

                if (cardType != CardType.Eyes)
                    clocalScale = Vector3.one * 0.75f;

                if (child.GetComponent<RectTransform>().anchoredPosition.y < -0.1f)
                    clocalScale.y = clocalScale.y * -1;

                child.GetComponent<RectTransform>().localScale = clocalScale;
            }
            else if (child.name.Contains("aceImag"))
                child.GetComponent<Image>().sprite = cardSet.aceSprite;       
        }

        Color cardColor = cardSet.typeColour;
        //Get all the images in the transform and color them
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<Image>() != null && child.name != "Background")
                child.GetComponent<Image>().color = cardColor;
        }
        cardBorder.color = Color.white;
        cardFlipOverlay.color = Color.clear;
        flipArt.color = Color.white;
    }

    public void FlipState(bool newFlipState)
    {
        faceDown = newFlipState;
        flipArt.enabled = newFlipState;
        for (int i = 0; i < cardFaceUpContents.Count; i++)
        {
            cardFaceUpContents[i].SetActive(!newFlipState);
        }
        //Flip animation
        StartCoroutine(FlipAnimation());
    }

    IEnumerator FlipAnimation()
    {

        for (int i = 0; i < 10; i++)
        {
            cardFlipOverlay.color = Color.Lerp(Color.white,Color.clear , (float)i / 10f);
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void Update()
    {   

        if (Vector3.Distance(transform.position, goal) > 0.05f)
        {
            rTransform.anchoredPosition = Vector3.Lerp(rTransform.anchoredPosition, goal, speed * Time.deltaTime);
        }
        else
        {
            rTransform.anchoredPosition = goal;
        }
    }

    public void GoToLocation(Vector3 newGoal)
    {
        goal = newGoal;
    }

    
}

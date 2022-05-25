using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGroup : MonoBehaviour
{
    public List<CardObject> cardsInGroup;

    public void Start()
    {
        cardsInGroup = new List<CardObject>();
    }
}

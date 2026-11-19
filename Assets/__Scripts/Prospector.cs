using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(JsonParseLayout))]
public class Prospector : MonoBehaviour
{
    private static Prospector S;

    [Header("Dynamic")]
    public List<CardProspector> drawPile;
    public List<CardProspector> discardPile;
    public List<CardProspector> mine;
    public CardProspector target;

    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout jsonLayout;

    private Dictionary<int, CardProspector> mineIdToCardDict;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (S != null) Debug.LogError("Attempted to set S more than once!");
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;
        deck = GetComponent<Deck>();

        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);

        drawPile = ConvertCards(deck.cards);

        LayoutMine();
        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    private List<CardProspector> ConvertCards(List<Card> cardList)
    {
        List<CardProspector> cardProsList = new();
        foreach (Card card in cardList)
        {
            CardProspector cp = card as CardProspector;
            cardProsList.Add(cp);
        }
        return cardProsList;
    }

    private CardProspector Draw()
    {
        CardProspector card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    private void LayoutMine()
    {
        if (layoutAnchor == null)
        {
            GameObject go = new("_LayoutAnchor");
            layoutAnchor = go.transform;
        }

        mineIdToCardDict = new();

        foreach(JsonLayoutSlot slot in jsonLayout.slots)
        {
            CardProspector card = Draw();
            card.FaceUp = slot.faceUp;
            card.transform.SetParent(layoutAnchor);

            int z = int.Parse(slot.layer[^1].ToString());
            card.SetLocalPos(new Vector3(
                jsonLayout.multiplier.x * slot.x,
                jsonLayout.multiplier.y * slot.y,
                -z
                ));

            card.layoutID = slot.id;
            card.layoutSlot = slot;
            card.state = eCardState.mine;

            card.SetSpriteSortingLayer(slot.layer);

            mine.Add(card);
            mineIdToCardDict.Add(slot.id, card);
        }
    }

    private void MoveToDiscard(CardProspector card)
    {
        card.state = eCardState.discard;
        discardPile.Add(card);
        card.transform.SetParent(layoutAnchor);

        card.SetLocalPos(new Vector3(
            jsonLayout.multiplier.x * jsonLayout.discardPile.x,
            jsonLayout.multiplier.y * jsonLayout.discardPile.y,
            0));

        card.FaceUp = true;

        card.SetSpriteSortingLayer(jsonLayout.discardPile.layer);
        card.SetSpriteSortingOrder(-200 + discardPile.Count * 3);
    }

    private void MoveToTarget(CardProspector card)
    {
        if (target != null) MoveToDiscard(target);

        MoveToDiscard(card);

        target = card;
        card.state = eCardState.target;

        card.SetSpriteSortingLayer("Target");
        card.SetSpriteSortingOrder(0);
    }

    private void UpdateDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardProspector card = drawPile[i];
            card.transform.SetParent(layoutAnchor);

            Vector3 cardPos = new()
            {
                x = jsonLayout.multiplier.x * jsonLayout.drawPile.x,
                y = jsonLayout.multiplier.y * jsonLayout.drawPile.y,
                z = 0.1f * i
            };
            cardPos.x += jsonLayout.drawPile.xStagger * i;
            card.SetLocalPos(cardPos);

            card.FaceUp = false;
            card.state = eCardState.drawpile;

            card.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            card.SetSpriteSortingOrder(-10 * i);
        }
    }

    public void SetMineFaceUps()
    {
        foreach(CardProspector card in mine)
        {
            bool faceUp = true;
            foreach(int coverID in card.layoutSlot.hiddenBy)
            {
                CardProspector coverCard = mineIdToCardDict[coverID];
                if (coverCard == null || coverCard.state == eCardState.mine)
                {
                    faceUp = false;
                }
            }
            card.FaceUp = faceUp;
        }
    }

    public static void CARD_CLICKED(CardProspector card)
    {
        switch (card.state)
        {
            case eCardState.drawpile:
                S.MoveToTarget(S.Draw());
                S.UpdateDrawPile();
                break;
            case eCardState.mine:
                bool validMatch = card.FaceUp && card.AdjacentTo(S.target);
                if (validMatch)
                {
                    S.mine.Remove(card);
                    S.MoveToTarget(card);
                    S.SetMineFaceUps();
                }
                break;
            case eCardState.target:
            case eCardState.discard:
            default:
                break;
        }
    }
}

using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(JsonParseDeck))]
public class Deck : MonoBehaviour
{
    [Header("Inscribed")]
    public CardSpritesSO cardSprites;
    public GameObject prefabCard;
    public GameObject prefabSprite;
    public bool startFaceUp = true;

    [Header("Dynamic")]
    public Transform deckAnchor;
    public List<Card> cards;

    private JsonParseDeck jsonDeck;

    static public GameObject SPRITE_PREFAB { get; private set; }

    //private void Start()
    //{
    //    InitDeck();
    //    Shuffle(ref cards);
    //}

    public void InitDeck()
    {
        SPRITE_PREFAB = prefabSprite;
        cardSprites.Init();
        jsonDeck = GetComponent<JsonParseDeck>();

        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new("_Deck");
            deckAnchor = anchorGO.transform;
        }

        MakeCards();  
    }

    private void MakeCards()
    {
        cards = new List<Card>();
        Card c;

        string suits = "CDHS";

        for (int i = 0; i < suits.Length; i++)
        {
            for (int j = 1; j <= jsonDeck.deck.cards.Count; j++)
            {
                c = MakeCard(suits[i], j);
                cards.Add(c);
                // Test position
                c.transform.position = new Vector3((j - 7) * 3, (i - 1.5f) * 4, 0);
            }
        }
    }

    private Card MakeCard(char suit, int rank)
    {
        GameObject go = Instantiate(prefabCard, deckAnchor);
        Card card = go.GetComponent<Card>();
        card.Init(suit, rank, startFaceUp);
        return card;
    }

    static public void Shuffle(ref List<Card> refCards)
    {
        List<Card> shuffledCards = new();

        while (refCards.Count > 0)
        {
            int i = Random.Range(0, refCards.Count);
            shuffledCards.Add(refCards[i]);
            refCards.RemoveAt(i);
        }

        refCards = shuffledCards;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Dynamic")]
    public char suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public GameObject back;
    public JsonCard def;

    public List<GameObject> decoGOs = new();
    public List<GameObject> pipGOs = new();

    private Quaternion _flipRot = Quaternion.Euler(0, 0, 180);
    private SpriteRenderer[] spriteRenderers;

    public bool FaceUp
    {
        get { return !back.activeSelf; }
        set { back.SetActive(!value); }
    }

    public void Init(char eSuit, int eRank, bool startFaceUp = true)
    {
        gameObject.name = name = $"{eSuit}{eRank}";
        suit = eSuit;
        rank = eRank;
        if (suit == 'D' || suit == 'H')
        {
            color = Color.red;
            colS = "Red";
        }

        def = JsonParseDeck.GET_CARD_DEF(rank);

        AddDecorators();
        AddPips();
        AddFace();
        AddBack();
        FaceUp = startFaceUp;
    }

    public virtual void SetLocalPos(Vector3 v)
    {
        transform.localPosition = v;
    }

    private void AddDecorators()
    {
        foreach(JsonPip pip in JsonParseDeck.DECORATORS)
        {
            GameObject decoGO = Instantiate(Deck.SPRITE_PREFAB, transform);
            SpriteRenderer sRend = decoGO.GetComponent<SpriteRenderer>();

            if (pip.type == "suit")
            {
                sRend.sprite = CardSpritesSO.SUITS[suit];
            }
            else
            {
                sRend.sprite = CardSpritesSO.RANKS[rank];
                sRend.color = color;
            }
            sRend.sortingOrder = 1;

            decoGO.transform.localPosition = pip.loc;
            if (pip.flip) decoGO.transform.rotation = _flipRot;
            if (pip.scale != 1) decoGO.transform.localScale = Vector3.one * pip.scale;

            decoGO.name = pip.type;
            decoGOs.Add(decoGO);
        }
    }

    private void AddPips()
    {
        int pipNum = 0;
        foreach (JsonPip pip in def.pips)
        {
            GameObject pipGO = Instantiate(Deck.SPRITE_PREFAB, transform);
            SpriteRenderer sRend = pipGO.GetComponent<SpriteRenderer>();
            sRend.sprite = CardSpritesSO.SUITS[suit];
            sRend.sortingOrder = 1;

            pipGO.transform.localPosition = pip.loc;
            if (pip.flip) pipGO.transform.rotation = _flipRot;
            if (pip.scale != 1) pipGO.transform.localScale = Vector3.one * pip.scale;

            pipGO.name = $"pip_{pipNum++}";
            pipGOs.Add(pipGO);
        }
    }

    private void AddFace()
    {
        if (def.face == "") return;

        string faceName = $"{def.face}{suit}";
        Sprite faceSprite = CardSpritesSO.GET_FACE(faceName);
        if (faceSprite == null)
        {
            Debug.LogError($"Face sprite {faceName} not found.");
            return;
        }

        GameObject faceGO = Instantiate(Deck.SPRITE_PREFAB, transform);
        SpriteRenderer sRend = faceGO.GetComponent<SpriteRenderer>();
        sRend.sprite = faceSprite;
        sRend.sortingOrder = 1;

        faceGO.transform.localPosition = Vector3.zero;

        faceGO.name = faceName;
    }

    private void AddBack()
    {
        GameObject backGO = Instantiate(Deck.SPRITE_PREFAB, transform);
        SpriteRenderer sRend = backGO.GetComponent<SpriteRenderer>();
        sRend.sprite = CardSpritesSO.BACK;
        sRend.sortingOrder = 2;

        backGO.transform.localPosition = Vector3.zero;

        backGO.name = "back";
        back = backGO;
    }

    private void PopulateSpriteRenderers()
    {
        if (spriteRenderers != null) return;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetSpriteSortingLayer(string layerName)
    {
        PopulateSpriteRenderers();
        foreach(SpriteRenderer sr in spriteRenderers)
        {
            sr.sortingLayerName = layerName;
        }
    }

    public void SetSpriteSortingOrder(int order)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr.gameObject == gameObject)
            {
                sr.sortingOrder = order;
            }
            else if (sr.gameObject.name == "back")
            {
                sr.sortingOrder = order + 2;
            }
            else
            {
                sr.sortingOrder = order + 1;
            }
        }
    }

    public bool AdjacentTo(Card other, bool wrap = true)
    {
        if (!FaceUp || !other.FaceUp) return false;
        if (Mathf.Abs(rank - other.rank) == 1) return true;
        if (wrap)
        {
            if (rank == 1 && other.rank == 13) return true; // Please make dynamic!
            if (rank == 13 && other.rank == 1) return true;
        }
        return false;
    }

    public virtual void OnMouseUpAsButton()
    {
        print(name);
    }
}

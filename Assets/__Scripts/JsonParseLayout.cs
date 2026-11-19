using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonLayout
{
    public Vector2 multiplier;
    public List<JsonLayoutSlot> slots;
    public JsonLayoutPile drawPile;
    public JsonLayoutPile discardPile;
}

[Serializable]
public class JsonLayoutSlot : ISerializationCallbackReceiver
{
    public int id;
    public int x, y;
    public bool faceUp;
    public string layer;
    public string hiddenByString;

    [NonSerialized]
    public List<int> hiddenBy;

    public void OnAfterDeserialize()
    {
        hiddenBy = new List<int>();
        if (hiddenByString.Length == 0) return;
        string[] bits = hiddenByString.Split(',');
        for (int i = 0; i < bits.Length; i++)
        {
            hiddenBy.Add(int.Parse(bits[i]));
        }
    }

    public void OnBeforeSerialize() { }
}

[Serializable]
public class JsonLayoutPile
{
    public int x, y;
    public string layer;
    public float xStagger;
}


public class JsonParseLayout : MonoBehaviour
{
    public static JsonParseLayout S { get; private set; }

    [Header("Inscribed")]
    public TextAsset jsonLayoutFile;

    [Header("Dynamic")]
    public JsonLayout layout;

    private void Awake()
    {
        layout = JsonUtility.FromJson<JsonLayout>(jsonLayoutFile.text);
        S = this;
    }
}

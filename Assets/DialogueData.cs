using UnityEngine;

[System.Serializable]
public class DialogueCharacter
{
    public string displayName;
    public Sprite portrait; // ไม่ใช้ได้
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter speaker;
    [TextArea(2, 5)] public string text;
}

[CreateAssetMenu(menuName = "Dialogue/SimpleDialogue")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

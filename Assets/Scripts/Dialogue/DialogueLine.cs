using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("🧍 Character Info")]
    public string characterName;
    public Sprite portrait;

    [Header("💬 Dialogue Text")]
    [TextArea(2, 6)]
    public string lineText;

    [Header("🔘 UI Settings (optional)")]
    public bool showButton = true;
    public string buttonText = "Next";
}

using UnityEngine;

[CreateAssetMenu(menuName = "Streamer/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Ayarlar")]
    public float bubbleDuration = 3.0f; 

    [Header("Oda Replikleri (Intro)")]
    [TextArea] public string[] roomQuotes; 

    [Header("Yayın İçi Replikler (Chat)")]
    [TextArea] public string[] chatQuotes; 
}
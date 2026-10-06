using UnityEngine;

// [CreateAssetMenu] adds an entry to the right-click menu in the Project window
[CreateAssetMenu(fileName = "NewCandy", menuName = "Twick o Tweat/Candy")]
public class CandyData : ScriptableObject
{
    public string displayName = "Candy";
    public Sprite icon;
}
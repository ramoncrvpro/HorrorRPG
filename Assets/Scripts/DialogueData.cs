namespace HorrorRPG.Dialogue
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [TextArea(3, 10)]
    public string[] sentences;

    [Header("Dialogue Behavior")]
    public bool stuckDialogue = false;
    public bool fastText = false;

    [Header("Confirmation Settings")]
    public bool requiresConfirmation = false;
}


}

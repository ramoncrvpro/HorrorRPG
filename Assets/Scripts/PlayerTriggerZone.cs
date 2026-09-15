namespace HorrorRPG.Player
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerZone : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;

    [Header("Settings")]
    [SerializeField] private bool triggerOnce = false;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (!IsPlayer(other))
            return;

        hasTriggered = true;
        onPlayerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (triggerOnce && hasTriggered)
            return;

        onPlayerExit?.Invoke();
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        return false;
    }
}


}

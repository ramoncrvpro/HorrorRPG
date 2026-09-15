namespace HorrorRPG.Presentation
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIClickProbe : MonoBehaviour
{
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    void Awake()
    {
        if (!eventSystem) eventSystem = EventSystem.current;
        if (!raycaster) raycaster = Object.FindFirstObjectByType<GraphicRaycaster>();
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            var data = new PointerEventData(eventSystem)
            {
                position = Mouse.current.position.ReadValue()
            };

            var results = new List<RaycastResult>();
            raycaster.Raycast(data, results);

            if (results.Count == 0)
            {
                Debug.Log("Clique: nenhum elemento de UI foi atingido.");
                return;
            }

            Debug.Log("Clique UI (topo): " + results[0].gameObject.name);

            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log($"{i}: {results[i].gameObject.name} | depth:{results[i].depth} | sorting:{results[i].sortingOrder}");
            }
        }
    }
}


}

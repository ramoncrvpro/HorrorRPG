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

[CreateAssetMenu(fileName = "New UI Sprite Animation", menuName = "UI/Sprite Animation")]
public class UISpriteAnimation : ScriptableObject
{
    [Header("Animation Properties")]
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool loop = true;

    public List<Sprite> Sprites => sprites;
    public float FPS => fps;
    public bool Loop => loop;
    public int FrameCount => sprites.Count;

    public Sprite GetSprite(int index)
    {
        if (sprites == null || sprites.Count == 0)
            return null;

        if (index < 0 || index >= sprites.Count)
            return null;

        return sprites[index];
    }
}


}

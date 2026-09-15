namespace HorrorRPG.Presentation
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private new UISpriteAnimation animation;
    [SerializeField] private bool playOnAwake = true;

    private Image targetImage;
    private int currentFrame;
    private float timer;
    private bool isPlaying;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (playOnAwake && animation != null)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || animation == null || animation.FrameCount == 0)
            return;

        timer += Time.deltaTime;

        float frameDuration = 1f / animation.FPS;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            NextFrame();
        }
    }

    public void Play()
    {
        if (animation == null)
        {
            Debug.LogWarning("UISpriteAnimator: No animation assigned!");
            return;
        }

        if (animation.FrameCount == 0)
        {
            Debug.LogWarning("UISpriteAnimator: Animation has no sprites!");
            return;
        }

        isPlaying = true;
        currentFrame = 0;
        timer = 0f;
        UpdateSprite();
    }

    public void Play(UISpriteAnimation newAnimation)
    {
        animation = newAnimation;
        Play();
    }

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        timer = 0f;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        if (animation != null && animation.FrameCount > 0)
        {
            isPlaying = true;
        }
    }

    public void SetAnimation(UISpriteAnimation newAnimation)
    {
        bool wasPlaying = isPlaying;
        animation = newAnimation;
        
        if (wasPlaying)
        {
            Play();
        }
    }

    private void NextFrame()
    {
        currentFrame++;

        if (currentFrame >= animation.FrameCount)
        {
            if (animation.Loop)
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = animation.FrameCount - 1;
                isPlaying = false;
                return;
            }
        }

        UpdateSprite();
    }

    private void UpdateSprite()
    {
        Sprite sprite = animation.GetSprite(currentFrame);
        if (sprite != null)
        {
            targetImage.sprite = sprite;
        }
    }
}


}

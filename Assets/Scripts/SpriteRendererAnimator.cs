using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Presentation
{
    /// <summary>Plays a sprite-list animation on a Unity SpriteRenderer.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererAnimator : MonoBehaviour
    {
        private const float MinimumFps = 0.01f;

        [Header("Animation Settings")]
        [SerializeField] private new UISpriteAnimation animation;
        [SerializeField] private bool playOnAwake = true;

        private SpriteRenderer targetRenderer;
        private int currentFrame;
        private float timer;
        private bool isPlaying;

        /// <summary>Returns the animation currently assigned to this renderer.</summary>
        public UISpriteAnimation Animation => animation;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
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
            {
                return;
            }

            timer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(animation.FPS, MinimumFps);

            if (timer >= frameDuration)
            {
                timer -= frameDuration;
                NextFrame();
            }
        }

        /// <summary>Starts the assigned animation from its first frame.</summary>
        public void Play()
        {
            if (animation == null)
            {
                Debug.LogWarning($"{nameof(SpriteRendererAnimator)}: No animation assigned.", this);
                return;
            }

            if (animation.FrameCount == 0)
            {
                Debug.LogWarning($"{nameof(SpriteRendererAnimator)}: Animation has no sprites.", this);
                return;
            }

            isPlaying = true;
            currentFrame = 0;
            timer = 0f;
            UpdateSprite();
        }

        /// <summary>Assigns an animation and starts it immediately.</summary>
        public void Play(UISpriteAnimation newAnimation)
        {
            animation = newAnimation;
            Play();
        }

        /// <summary>Stops playback and resets the renderer to the first frame.</summary>
        public void Stop()
        {
            isPlaying = false;
            currentFrame = 0;
            timer = 0f;
            UpdateSprite();
        }

        /// <summary>Pauses playback on the current frame.</summary>
        public void Pause()
        {
            isPlaying = false;
        }

        /// <summary>Resumes playback when a valid animation is assigned.</summary>
        public void Resume()
        {
            if (animation != null && animation.FrameCount > 0)
            {
                isPlaying = true;
            }
        }

        /// <summary>Changes the assigned animation while preserving the playback state.</summary>
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
            if (targetRenderer == null || animation == null)
            {
                return;
            }

            Sprite sprite = animation.GetSprite(currentFrame);
            if (sprite != null)
            {
                targetRenderer.sprite = sprite;
            }
        }
    }
}

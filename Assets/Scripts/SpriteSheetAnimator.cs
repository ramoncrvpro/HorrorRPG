namespace HorrorRPG.Presentation
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public class SpriteSheetAnimator : MonoBehaviour
{
    [Header("Spritesheet Settings")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 4;
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private bool loop = true;
    
    [Header("Animation")]
    [SerializeField] private int startFrame = 0;
    [SerializeField] private int endFrame = 15;
    [SerializeField] private bool playOnAwake = true;
    
    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private float currentFrame;
    private bool isPlaying = true;
    
    private static readonly int ColumnsProperty = Shader.PropertyToID("_Columns");
    private static readonly int RowsProperty = Shader.PropertyToID("_Rows");
    private static readonly int CurrentFrameProperty = Shader.PropertyToID("_CurrentFrame");
    private static readonly int HitEffectProperty = Shader.PropertyToID("_HitEffect");

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        currentFrame = startFrame;
        
        InitializePropertyBlock();
        
        isPlaying = playOnAwake;
    }

    private void InitializePropertyBlock()
    {
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(ColumnsProperty, columns);
        propertyBlock.SetFloat(RowsProperty, rows);
        propertyBlock.SetFloat(CurrentFrameProperty, currentFrame);
        propertyBlock.SetFloat(HitEffectProperty, 0f);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private void Update()
    {
        if (!isPlaying) return;

        currentFrame += frameRate * Time.deltaTime;

        if (currentFrame > endFrame)
        {
            if (loop)
            {
                currentFrame = startFrame;
            }
            else
            {
                currentFrame = endFrame;
                isPlaying = false;
            }
        }

        UpdateFrame();
    }

    private void UpdateFrame()
    {
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(CurrentFrameProperty, Mathf.Floor(currentFrame));
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public void PlayAnimation(int start, int end, bool shouldLoop = true)
    {
        startFrame = start;
        endFrame = end;
        loop = shouldLoop;
        currentFrame = start;
        isPlaying = true;
        UpdateFrame();
    }

    public void SetFrame(int frame)
    {
        currentFrame = Mathf.Clamp(frame, 0, (columns * rows) - 1);
        isPlaying = false;
        UpdateFrame();
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        isPlaying = false;
        currentFrame = startFrame;
        UpdateFrame();
    }

    public void TriggerHitEffect(float duration = 0.1f)
    {
        StopAllCoroutines();
        StartCoroutine(HitEffectCoroutine(duration));
    }

    private System.Collections.IEnumerator HitEffectCoroutine(float duration)
    {
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(HitEffectProperty, 1f);
        meshRenderer.SetPropertyBlock(propertyBlock);

        yield return new WaitForSeconds(duration);

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(HitEffectProperty, 0f);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}


}

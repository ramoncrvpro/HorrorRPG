namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using System;

public class AttackTimingBar : MonoBehaviour
{
    public static AttackTimingBar Instance { get; private set; }
    
    private const float INPUT_DELAY = 0.15f;
    
    private WeaponData currentWeapon;
    private float currentPosition = 0f;
    private float direction = 1f;
    private bool isActive = false;
    private Action<AttackResult> onComplete;
    private float speedModifier = 1f;
    private float inputDelayTimer = 0f;
    private GameInputReader inputReader;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        inputReader = FindFirstObjectByType<GameInputReader>();
        if (inputReader != null)
        {
            inputReader.TimingConfirmPerformed += HandleTimingConfirm;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.TimingConfirmPerformed -= HandleTimingConfirm;
        }
    }

    private void HandleTimingConfirm()
    {
        if (isActive && inputDelayTimer <= 0f)
        {
            EvaluateAndComplete();
        }
    }

    public void StartTiming(WeaponData weapon, Action<AttackResult> callback)
    {
        currentWeapon = weapon;
        onComplete = callback;
        currentPosition = 0f;
        direction = 1f;
        isActive = true;
        inputDelayTimer = INPUT_DELAY;
        
        if (AttackTimingUI.Instance != null)
        {
            AttackTimingUI.Instance.Show();
            AttackTimingUI.Instance.SetupZones(weapon);
        }
    }
    
    public void SetSpeedModifier(float modifier)
    {
        speedModifier = Mathf.Clamp(modifier, 0.1f, 2f);
    }
    
    public void ResetSpeedModifier()
    {
        speedModifier = 1f;
    }
    
    private void Update()
    {
        if (!isActive || currentWeapon == null)
            return;
        
        if (inputDelayTimer > 0f)
        {
            inputDelayTimer -= Time.deltaTime;
        }
        
        currentPosition += direction * currentWeapon.markerSpeed * speedModifier * Time.deltaTime;
        
        if (currentPosition >= 1f)
        {
            currentPosition = 1f;
            direction = -1f;
        }
        else if (currentPosition <= 0f)
        {
            currentPosition = 0f;
            direction = 1f;
        }
        
        if (AttackTimingUI.Instance != null)
        {
            AttackTimingUI.Instance.UpdateMarkerPosition(currentPosition);
        }
        
        if (inputDelayTimer <= 0f)
        {
            inputDelayTimer = 0f;
        }

    }
    
    private void EvaluateAndComplete()
    {
        isActive = false;
        
        AttackResult result = currentWeapon.EvaluateTimingPosition(currentPosition);
        
        if (AttackTimingUI.Instance != null)
        {
            AttackTimingUI.Instance.Hide();
        }
        
        ResetSpeedModifier();
        
        onComplete?.Invoke(result);
    }
    
    public float GetCurrentPosition()
    {
        return currentPosition;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
}


}

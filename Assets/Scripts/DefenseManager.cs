namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;



using UnityEngine;
using System.Collections.Generic;

public class DefenseManager : MonoBehaviour
{
    public static DefenseManager Instance { get; private set; }

    [Header("Defense Settings")]
    [SerializeField] private float globalCooldown = 1.5f;

    [Header("Input Settings")]
    private Dictionary<DefensePosition, DefenseRegionData> defenseRegions;
    private float globalCooldownTimer;
    private bool defenseEnabled;
    private GameInputReader inputReader;

    public bool IsDefenseEnabled => defenseEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeDefenseRegions();
    }

    private void OnEnable()
    {
        inputReader = FindFirstObjectByType<GameInputReader>();
        if (inputReader == null)
        {
            return;
        }

        inputReader.DefendLeftPerformed += HandleLeftDefense;
        inputReader.DefendUpPerformed += HandleUpDefense;
        inputReader.DefendRightPerformed += HandleRightDefense;
    }

    private void OnDisable()
    {
        if (inputReader == null)
        {
            return;
        }

        inputReader.DefendLeftPerformed -= HandleLeftDefense;
        inputReader.DefendUpPerformed -= HandleUpDefense;
        inputReader.DefendRightPerformed -= HandleRightDefense;
    }

    private void InitializeDefenseRegions()
    {
        defenseRegions = new Dictionary<DefensePosition, DefenseRegionData>
        {
            { DefensePosition.Left, new DefenseRegionData(DefensePosition.Left) },
            { DefensePosition.Up, new DefenseRegionData(DefensePosition.Up) },
            { DefensePosition.Right, new DefenseRegionData(DefensePosition.Right) }
        };
    }

    private void Update()
    {
        if (!defenseEnabled)
            return;

        UpdateGlobalCooldown();
        UpdateDefenseTimers();
    }

    private void UpdateGlobalCooldown()
    {
        if (globalCooldownTimer > 0f)
        {
            globalCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateDefenseTimers()
    {
        foreach (var region in defenseRegions.Values)
        {
            region.UpdateTimer(Time.deltaTime);
        }
    }

    private void HandleLeftDefense()
    {
        TryActivateDefense(DefensePosition.Left);
    }

    private void HandleUpDefense()
    {
        TryActivateDefense(DefensePosition.Up);
    }

    private void HandleRightDefense()
    {
        TryActivateDefense(DefensePosition.Right);
    }

    private void TryActivateDefense(DefensePosition position)
    {
        if (!defenseEnabled || globalCooldownTimer > 0f)
        {
            return;
        }

        DefenseRegionData region = defenseRegions[position];
        if (region.isActive)
        {
            return;
        }

        region.Activate();
        globalCooldownTimer = globalCooldown;
        PlayDefenseAnimation(position);
        Debug.Log($"Defesa {position} ativada! Timer iniciado.");
    }

    private void PlayDefenseAnimation(DefensePosition position)
    {
        DefenseEffectsManager.Instance?.TriggerEffect(position);
        
        if (HandAnimationManager.Instance != null)
        {
            switch (position)
            {
                case DefensePosition.Left:
                    HandAnimationManager.Instance.PlayReachAnimationLeftHand();
                    break;
                case DefensePosition.Up:
                    HandAnimationManager.Instance.PlayReachAnimation();
                    break;
                case DefensePosition.Right:
                    HandAnimationManager.Instance.PlayReachAnimationRightHand();
                    break;
            }
        }
    }

    public void EnableDefense(bool enable)
    {
        defenseEnabled = enable;
        
        if (!enable)
        {
            ResetAllDefenses();
        }
        
        Debug.Log($"Sistema de defesa {(enable ? "ativado" : "desativado")}");
    }

    public int OnProjectileHit(DefensePosition position, int baseDamage, out DefenseType defenseType)
    {
        if (!defenseRegions.ContainsKey(position))
        {
            Debug.LogError($"Posição de defesa inválida: {position}");
            defenseType = DefenseType.None;
            return baseDamage;
        }

        DefenseRegionData region = defenseRegions[position];
        
        if (!region.HasDefense())
        {
            Debug.Log($"Projétil atingiu {position} - Sem defesa! Dano total: {baseDamage}");
            defenseType = DefenseType.None;
            return baseDamage;
        }

        float damageMultiplier = region.GetDamageMultiplier();
        int finalDamage = Mathf.CeilToInt(baseDamage * damageMultiplier);
        
        float normalizedTime = region.defenseTimer / 1.0f;
        float defensePercent = (1.0f - damageMultiplier) * 100f;
        
        defenseType = GetDefenseType(damageMultiplier);
        
        string timingQuality = GetTimingQuality(normalizedTime);
        Debug.Log($"Projétil atingiu {position} - {timingQuality} (Timer: {normalizedTime:P0}) - Bloqueio {defensePercent:F0}% - Dano: {finalDamage}/{baseDamage}");
        
        region.Reset();
        
        return finalDamage;
    }

    private DefenseType GetDefenseType(float damageMultiplier)
    {
        if (damageMultiplier <= 0.0f)
            return DefenseType.Perfect;
        else if (damageMultiplier < 1.0f)
            return DefenseType.Partial;
        else
            return DefenseType.None;
    }

    private string GetTimingQuality(float normalizedTime)
    {
        if (normalizedTime < 0.15f) return "MUITO CEDO";
        if (normalizedTime < 0.375f) return "CEDO";
        if (normalizedTime < 0.625f) return "PERFEITO";
        if (normalizedTime < 0.85f) return "TARDE";
        return "MUITO TARDE";
    }

    public void ResetAllDefenses()
    {
        foreach (var region in defenseRegions.Values)
        {
            region.Reset();
        }
        globalCooldownTimer = 0f;
    }

    public DefenseRegionData GetRegionData(DefensePosition position)
    {
        if (defenseRegions.ContainsKey(position))
        {
            return defenseRegions[position];
        }
        return null;
    }

    public float GetGlobalCooldownRemaining()
    {
        return Mathf.Max(0f, globalCooldownTimer);
    }
}


}

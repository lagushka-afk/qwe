using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] actionButtons;           
    public Button startBattleButton;         
    public Button restartButton;             
    public Text battleLogText;               

    [Header("Settings")]
    public float turnDelay = 1.5f;          

    [Header("Scene Settings")]
    public string[] bossSceneNames = new string[]
    {
        "Scene_Boss1",
        "Scene_Boss2",
        "Scene_Boss3"
    };
    public int currentBossIndex = 0;         

   
    private string[] bossNames = { "Босс 1", "Босс 2", "Босс 3" };
    private int[] bossHPValues = { 10, 12, 15 };
    private List<List<int>> bossPatterns = new List<List<int>>();

   
    private int playerHP = 10;
    private int bossHP = 10;
    private string bossName = "Босс";
    private List<int> currentBossPattern = new List<int>();
    private int bossPatternIndex = 0;
    private List<int> playerActions = new List<int>();
    private int playerActionIndex = 0;
    private bool battleActive = false;
    private bool waitingForTurn = false;

    void Start()
    {
       
        bossPatterns.Add(new List<int> { 0, 1, 2, 3, 0 });
       
        bossPatterns.Add(new List<int> { 2, 3, 1, 3, 0 });
        
        bossPatterns.Add(new List<int> { 3, 1, 3, 2, 3 });

        LoadBossData();

        
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int index = i;
            actionButtons[i].onClick.AddListener(() => AddPlayerAction(index));
        }

        startBattleButton.onClick.AddListener(StartBattle);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartBattle);

        LogMessage($"ВЫБЕРИ 4 ПРИЁМА, ПОТОМ НАЖМИ 'НАЧАТЬ БОЙ'");
        LogMessage($"БОСС: {bossName} (HP: {bossHP})");
        LogMessage($"0=Удар(2) | 1=Блок | 2=Отхил(+3) | 3=СильныйУдар(4)");
    }

    void LoadBossData()
    {
        bossName = bossNames[currentBossIndex];
        bossHP = bossHPValues[currentBossIndex];
        currentBossPattern = new List<int>(bossPatterns[currentBossIndex]);
    }

    void AddPlayerAction(int actionIndex)
    {
        if (battleActive)
        {
            LogMessage("Бой уже идёт, нельзя менять приёмы");
            return;
        }

        if (playerActions.Count >= 4)
        {
            LogMessage("Уже выбрано 4 приёма! Нажми 'Начать бой'");
            return;
        }

        int selectedAction = actionIndex;
        playerActions.Add(selectedAction);
        LogMessage($"Выбрано: {GetActionName(selectedAction)}. Осталось: {4 - playerActions.Count}");
    }

    void StartBattle()
    {
        if (playerActions.Count != 4)
        {
            LogMessage("Нужно выбрать ровно 4 приёма!");
            return;
        }

        battleActive = true;
        playerHP = 10;
        bossHP = bossHPValues[currentBossIndex];
        playerActionIndex = 0;
        bossPatternIndex = 0;
        waitingForTurn = false;

        startBattleButton.interactable = false;
        foreach (Button btn in actionButtons)
            btn.interactable = false;

        LogMessage($"\n=== БОЙ ПРОТИВ {bossName} НАЧАЛСЯ ===");

        string patternStr = "";
        foreach (var a in currentBossPattern)
            patternStr += GetActionName(a) + " → ";
        LogMessage($"ПАТТЕРН БОССА: {patternStr.TrimEnd(' ', '→')} (повторяется по кругу)");

        Invoke(nameof(ExecuteTurn), turnDelay);
    }

    void ExecuteTurn()
    {
        if (!battleActive || waitingForTurn)
            return;

        if (playerHP <= 0)
        {
            LogMessage($"\n=== ТЫ ПРОИГРАЛ {bossName}! ===");
            LogMessage("НАЖМИ 'НАЧАТЬ ЗАНОВО' ЧТОБЫ ПОПРОБОВАТЬ СНОВА");
            battleActive = false;
            return;
        }

        if (bossHP <= 0)
        {
            LogMessage($"\n=== ПОБЕДА! {bossName} ПОВЕРЖЕН! ===");
            battleActive = false;

            if (currentBossIndex >= 2)
            {
                LogMessage("\n=== ТЫ ПРОШЁЛ ВСЮ ИГРУ! ПОЗДРАВЛЯЮ! ===");
            }
            else
            {
                LogMessage($"\nЧЕРЕЗ 2 СЕКУНДЫ ПЕРЕХОД К СЛЕДУЮЩЕМУ БОССУ...");
                Invoke(nameof(GoToNextBoss), 2f);
            }
            return;
        }

        waitingForTurn = true;

        int playerAction = playerActions[playerActionIndex];
        int bossAction = currentBossPattern[bossPatternIndex];

        LogMessage($"\n--- ХОД {playerActionIndex + 1} ---");
        LogMessage($"ТЫ: {GetActionName(playerAction)}");
        LogMessage($"{bossName}: {GetActionName(bossAction)}");

        int playerDamageToBoss = 0;
        int bossDamageToPlayer = 0;
        int playerHeal = 0;
        int bossHeal = 0;

        ResolveCombat(playerAction, bossAction, ref playerDamageToBoss, ref bossDamageToPlayer, ref playerHeal, ref bossHeal);

        if (playerDamageToBoss > 0)
        {
            bossHP -= playerDamageToBoss;
            LogMessage($"→ ТЫ НАНЁС {playerDamageToBoss} УРОНА!");
        }
        if (bossDamageToPlayer > 0)
        {
            playerHP -= bossDamageToPlayer;
            LogMessage($"→ {bossName} НАНЁС {bossDamageToPlayer} УРОНА ТЕБЕ!");
        }
        if (playerHeal > 0)
        {
            playerHP = Mathf.Min(10, playerHP + playerHeal);
            LogMessage($"→ ТЫ ВОССТАНОВИЛ {playerHeal} HP!");
        }
        if (bossHeal > 0)
        {
            bossHP = Mathf.Min(10, bossHP + bossHeal);
            LogMessage($"→ {bossName} ВОССТАНОВИЛ {bossHeal} HP!");
        }

        playerHP = Mathf.Clamp(playerHP, 0, 10);
        bossHP = Mathf.Clamp(bossHP, 0, 10);

        HealthBar[] allHealthBars = FindObjectsOfType<HealthBar>();
        foreach (HealthBar hb in allHealthBars)
        {
            if (hb.isPlayer)
                hb.SetHealth(playerHP);
            else
                hb.SetHealth(bossHP);
        }

        LogMessage($"ТВОЁ HP: {playerHP}/10 | HP {bossName}: {bossHP}/{bossHPValues[currentBossIndex]}");

        playerActionIndex = (playerActionIndex + 1) % playerActions.Count;
        bossPatternIndex = (bossPatternIndex + 1) % currentBossPattern.Count;

        waitingForTurn = false;

        if (battleActive && playerHP > 0 && bossHP > 0)
        {
            Invoke(nameof(ExecuteTurn), turnDelay);
        }
    }

    void GoToNextBoss()
    {
        currentBossIndex++;

        if (currentBossIndex <= 2)
        {
            SceneManager.LoadScene(bossSceneNames[currentBossIndex]);
        }
    }

    void RestartBattle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ResolveCombat(int p, int b, ref int pDmg, ref int bDmg, ref int pHeal, ref int bHeal)
    {
        int pBaseDmg = GetBaseDamage(p);
        int bBaseDmg = GetBaseDamage(b);
        int pBaseHeal = GetHealValue(p);
        int bBaseHeal = GetHealValue(b);

        
        if (p == 1 && (b == 0 || b == 3))
        {
            bDmg = 0;
            pDmg = 0;
            LogMessage("⚡ ТВОЙ БЛОК ОСТАНОВИЛ АТАКУ!");
            return;
        }
        if (b == 1 && (p == 0 || p == 3))
        {
            bDmg = 0;
            pDmg = 0;
            LogMessage($"⚡ {bossName} ЗАБЛОКИРОВАЛ ТВОЮ АТАКУ!");
            return;
        }

        pDmg = pBaseDmg;
        bDmg = bBaseDmg;
        pHeal = pBaseHeal;
        bHeal = bBaseHeal;

       
        if (p == 2 && (b == 0 || b == 3))
        {
            pDmg = 0;
            bDmg = bBaseDmg;
            pHeal = pBaseHeal;
            bHeal = 0;
        }
        else if (b == 2 && (p == 0 || p == 3))
        {
            bDmg = 0;
            pDmg = pBaseDmg;
            bHeal = bBaseHeal;
            pHeal = 0;
        }
    }

    int GetBaseDamage(int action)
    {
        switch (action)
        {
            case 0: return 2;      
            case 3ы: return 4;      
            default: return 0;
        }
    }

    int GetHealValue(int action)
    {
        return action == 2 ? 3 : 0;
    }

    string GetActionName(int action)
    {
        switch (action)
        {
            case 0: return "УДАР (2)";
            case 1: return "БЛОК";
            case 2: return "ОТХИЛ (+3)";
            case 3: return "СИЛЬНЫЙ УДАР (4)";
            default: return "?";
        }
    }

    void LogMessage(string msg)
    {
        battleLogText.text += msg + "\n";
        Debug.Log(msg);
    }
}
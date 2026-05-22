using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
   
    public Button[] actionButtons;       
    public Button startBattleButton;     
    public Text battleLogText;           

  
    public float turnDelay = 1.5f;
    private int i = 0;


    private List<ActionType> bossPattern = new List<ActionType>
    {
        ActionType.Punch,
        ActionType.Block,
        ActionType.Heal,
        ActionType.StrongPunch,
        ActionType.Punch
    };
    private int bossPatternIndex = 0;

   
    public int playerHP = 10;         
    public int bossHP = 10;
    private List<ActionType> playerActions = new List<ActionType>();
    private int playerActionIndex = 0;
    private bool battleActive = false;
    private bool waitingForTurn = false;

    void Start()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int index = i;
            actionButtons[i].onClick.AddListener(() => AddPlayerAction(index));
        }

        startBattleButton.onClick.AddListener(StartBattle);
        LogMessage("Выбери 4 приёма , потом нажми 'Начать бой'");
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

        ActionType selectedAction = (ActionType)actionIndex;
        playerActions.Add(selectedAction);
        LogMessage($"Выбран приём: {GetActionName(selectedAction)}. Осталось: {4 - playerActions.Count}");
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
        bossHP = 10;
        playerActionIndex = 0;
        bossPatternIndex = 0;
        waitingForTurn = false;

        startBattleButton.interactable = false;
        foreach (Button btn in actionButtons)
            btn.interactable = false;

        LogMessage("=== БОЙ НАЧАЛСЯ ===");
        LogMessage($"Твой порядок: {GetActionListString(playerActions)}");
        LogMessage($"Босс: Удар → Блок → Отхил → Сильный удар → Удар (по кругу)");

        Invoke(nameof(ExecuteTurn), turnDelay);
    }

    void ExecuteTurn()
    {
        if (!battleActive || waitingForTurn)
            return;

        if (playerHP <= 0)
        {
            LogMessage("\n=== ТЫ ПРОИГРАЛ! ===");
            battleActive = false;
            return;
        }

        if (bossHP <= 0)
        {
            LogMessage("\n=== ПОБЕДА! Босс повержен! ===");
            battleActive = false;
            return;
        }

        waitingForTurn = true;

        ActionType playerAction = playerActions[playerActionIndex];
        ActionType bossAction = bossPattern[bossPatternIndex];

        LogMessage($"\n--- Ход {playerActionIndex + 1} ---");
        LogMessage($"Ты: {GetActionName(playerAction)}");
        LogMessage($"Босс: {GetActionName(bossAction)}");

        int playerDamageToBoss = 0;
        int bossDamageToPlayer = 0;
        int playerHeal = 0;
        int bossHeal = 0;

        ResolveCombat(playerAction, bossAction, ref playerDamageToBoss, ref bossDamageToPlayer, ref playerHeal, ref bossHeal);

        if (playerDamageToBoss > 0)
        {
            bossHP -= playerDamageToBoss;
            LogMessage($" Ты нанёс {playerDamageToBoss} урона боссу!");
        }
        if (bossDamageToPlayer > 0)
        {
            playerHP -= bossDamageToPlayer;
            LogMessage($" Босс нанёс {bossDamageToPlayer} урона тебе!");
        }
        if (playerHeal > 0)
        {
            playerHP = Mathf.Min(10, playerHP + playerHeal);
            LogMessage($" Ты восстановил {playerHeal} HP!");
        }
        if (bossHeal > 0)
        {
            bossHP = Mathf.Min(10, bossHP + bossHeal);
            LogMessage($" Босс восстановил {bossHeal} HP!");
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

        LogMessage($"Твоё HP: {playerHP}/10 | HP босса: {bossHP}/10");

        playerActionIndex = (playerActionIndex + 1) % playerActions.Count;
        bossPatternIndex = (bossPatternIndex + 1) % bossPattern.Count;

        waitingForTurn = false;

        if (battleActive && playerHP > 0 && bossHP > 0)
        {
            Invoke(nameof(ExecuteTurn), turnDelay);
        }
        else if (playerHP <= 0)
        {
            LogMessage(" ТЫ ПРОИГРАЛ! ");
            battleActive = false;
        }
        else if (bossHP <= 0)
        {
            LogMessage("\n ПОБЕДА! Босс повержен! ");
            battleActive = false;
        }
    }

    void ResolveCombat(ActionType p, ActionType b, ref int pDmg, ref int bDmg, ref int pHeal, ref int bHeal)
    {
        int pBaseDmg = GetBaseDamage(p);
        int bBaseDmg = GetBaseDamage(b);
        int pBaseHeal = GetHealValue(p);
        int bBaseHeal = GetHealValue(b);

        
        if (p == ActionType.Block && (b == ActionType.Punch || b == ActionType.StrongPunch))
        {
            bDmg = 0;
            pDmg = 0;
            LogMessage(" Твой блок остановил атаку босса!");
            return;
        }
        if (b == ActionType.Block && (p == ActionType.Punch || p == ActionType.StrongPunch))
        {
            bDmg = 0;
            pDmg = 0;
            LogMessage(" Босс заблокировал твою атаку!");
            return;
        }

        pDmg = pBaseDmg;
        bDmg = bBaseDmg;
        pHeal = pBaseHeal;
        bHeal = bBaseHeal;

        
        if (p == ActionType.Heal && (b == ActionType.Punch || b == ActionType.StrongPunch))
        {
            pDmg = 0;
            bDmg = bBaseDmg;
            pHeal = pBaseHeal;
            bHeal = 0;
        }
        else if (b == ActionType.Heal && (p == ActionType.Punch || p == ActionType.StrongPunch))
        {
            bDmg = 0;
            pDmg = pBaseDmg;
            bHeal = bBaseHeal;
            pHeal = 0;
        }
    }

    int GetBaseDamage(ActionType action)
    {
        switch (action)
        {
            case ActionType.Punch: return 2;
            case ActionType.StrongPunch: return 4;
            default: return 0;
        }
    }

    int GetHealValue(ActionType action)
    {
        return action == ActionType.Heal ? 3 : 0;
    }

    string GetActionName(ActionType action)
    {
        switch (action)
        {
            case ActionType.Punch: return "Удар (2)";
            case ActionType.Block: return "Блок";
            case ActionType.Heal: return "Отхил (+3)";
            case ActionType.StrongPunch: return "Сильный удар (4)";
            default: return "?";
        }
    }

    string GetActionListString(List<ActionType> actions)
    {
        string s = "";
        foreach (var a in actions)
            s += GetActionName(a) + " → ";
        return s.TrimEnd(' ', '→');
    }

   

    void LogMessage(string msg)
    {
        battleLogText.text += msg + "\n";
        Debug.Log(msg);

        i++;

        if (i > 15)
        {
            
            int firstNewLine = battleLogText.text.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                battleLogText.text = battleLogText.text.Substring(firstNewLine + 1);
                i--; 
            }
        }
    }
}

public enum ActionType
{
    Punch,      
    Block,      
    Heal,       
    StrongPunch 
}

 
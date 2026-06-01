using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public Button[] actionButtons;
    public Button startBattleButton;
    public Text battleLogText;
    public float turnDelay = 1.5f;

    private int playerHP = 10;
    private int bossHP = 10;
    private int bossLevel = 1;
    private List<int> playerMoves = new List<int>();
    private int moveIndex = 0;
    private int bossMoveIndex = 0;
    private bool fighting = false;
    private bool waiting = false;

    private List<int> bossPattern1 = new List<int> { 0, 1, 0, 2, 3, 1, 0 };
    private List<int> bossPattern2 = new List<int> { 3, 1, 3, 2, 0, 3, 1 };
    private List<int> bossPattern3 = new List<int> { 3, 3, 1, 3, 2, 3, 3, 1 };
    private List<int> currentPattern;

    void Start()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int idx = i;
            actionButtons[i].onClick.AddListener(() => AddMove(idx));
        }
        startBattleButton.onClick.AddListener(StartFight);

        Log("Выбери 4 приема. 0=Удар(2) 1=Блок 2=Хил(3) 3=Сильный(4)");
        Log("Босс HP: 10");
    }

    void AddMove(int move)
    {
        if (fighting) { Log("Бой идет"); return; }
        if (playerMoves.Count >= 4) { Log("4 приема выбрано. Жми СТАРТ"); return; }
        playerMoves.Add(move);
        Log($"Выбрал: {MoveName(move)}. Осталось: {4 - playerMoves.Count}");
    }

    void StartFight()
    {
        if (playerMoves.Count != 4) { Log("Надо 4 приема"); return; }

        fighting = true;
        playerHP = 10;
        bossHP = 10;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Boss1")
        {
            bossLevel = 1;
            currentPattern = bossPattern1;
        }
        else if (currentScene == "Boss2")
        {
            bossLevel = 2;
            currentPattern = bossPattern2;
        }
        else
        {
            bossLevel = 3;
            currentPattern = bossPattern3;
        }

        moveIndex = 0;
        bossMoveIndex = 0;
        waiting = false;

        startBattleButton.interactable = false;
        foreach (Button b in actionButtons) b.interactable = false;

        UpdateHealthBars();
        Log($"=== БОЙ С БОССОМ {bossLevel} НАЧАЛСЯ ===");
        Log($"HP БОССА: {bossHP}");
        Invoke(nameof(NextTurn), turnDelay);
    }

    void NextTurn()
    {
        if (!fighting || waiting) return;

        waiting = true;

        int playerMove = playerMoves[moveIndex];
        int bossMove = currentPattern[bossMoveIndex];

        Log($"Ход {moveIndex + 1}: Ты {MoveName(playerMove)} | Босс {MoveName(bossMove)}");

        int playerDmg = 0, bossDmg = 0, playerHeal = 0, bossHeal = 0;
        Fight(playerMove, bossMove, ref playerDmg, ref bossDmg, ref playerHeal, ref bossHeal);

        if (playerDmg > 0) { bossHP -= playerDmg; Log($"Ты нанес {playerDmg} урона"); }
        if (bossDmg > 0) { playerHP -= bossDmg; Log($"Босс нанес {bossDmg} урона"); }
        if (playerHeal > 0) { playerHP = Mathf.Min(10, playerHP + playerHeal); Log($"Ты вылечил {playerHeal} HP"); }
        if (bossHeal > 0) { bossHP = Mathf.Min(10, bossHP + bossHeal); Log($"Босс вылечил {bossHeal} HP"); }

        playerHP = Mathf.Clamp(playerHP, 0, 10);
        bossHP = Mathf.Clamp(bossHP, 0, 10);

        UpdateHealthBars();
        Log($"Твое HP: {playerHP} | Босс HP: {bossHP}");

        moveIndex = (moveIndex + 1) % playerMoves.Count;
        bossMoveIndex = (bossMoveIndex + 1) % currentPattern.Count;

        waiting = false;

        if (playerHP <= 0)
        {
            Log("ТЫ ПРОИГРАЛ. ПЕРЕЗАПУСК ЧЕРЕЗ 2 СЕКУНДЫ...");
            fighting = false;
            Invoke(nameof(RestartScene), 2f);
            return;
        }

        if (bossHP <= 0)
        {
            Log($"БОСС {bossLevel} ПОВЕРЖЕН. ПЕРЕХОД ЧЕРЕЗ 2 СЕКУНДЫ...");
            fighting = false;
            Invoke(nameof(NextScene), 2f);
            return;
        }

        if (fighting)
        {
            Invoke(nameof(NextTurn), turnDelay);
        }
    }

    void Fight(int p, int b, ref int pDmg, ref int bDmg, ref int pH, ref int bH)
    {
        int pBaseDmg = (p == 0) ? 2 : (p == 3) ? 4 : 0;
        int bBaseDmg = (b == 0) ? 2 : (b == 3) ? 4 : 0;
        int pBaseHeal = (p == 2) ? 3 : 0;
        int bBaseHeal = (b == 2) ? 3 : 0;

        if (p == 1 && (b == 0 || b == 3)) { Log("Твой блок"); return; }
        if (b == 1 && (p == 0 || p == 3)) { Log("Блок босса"); return; }

        pDmg = pBaseDmg;
        bDmg = bBaseDmg;
        pH = pBaseHeal;
        bH = bBaseHeal;

        if (p == 2 && (b == 0 || b == 3)) { bDmg = bBaseDmg; pH = pBaseHeal; pDmg = 0; bH = 0; }
        if (b == 2 && (p == 0 || p == 3)) { pDmg = pBaseDmg; bH = bBaseHeal; bDmg = 0; pH = 0; }
    }

    void UpdateHealthBars()
    {
        HealthBar[] bars = FindObjectsOfType<HealthBar>();
        foreach (HealthBar h in bars)
        {
            if (h.isPlayer) h.SetHealth(playerHP);
            else h.SetHealth(bossHP);
        }
    }

    void NextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Boss1")
        {
            SceneManager.LoadScene("Boss2");
        }
        else if (currentScene == "Boss2")
        {
            SceneManager.LoadScene("Boss3");
        }
        else
        {
            Log("ТЫ ПРОШЕЛ ВСЮ ИГРУ");
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    string MoveName(int m)
    {
        if (m == 0) return "Удар(2)";
        if (m == 1) return "Блок";
        if (m == 2) return "Хил(3)";
        return "Сильный(4)";
    }

    void Log(string msg)
    {
        battleLogText.text += msg + "\n";
        Debug.Log(msg);
    }
}
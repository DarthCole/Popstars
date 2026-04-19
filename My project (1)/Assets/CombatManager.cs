using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatManager : MonoBehaviour
{
    [Header("Active Artist")]
    public ArtistData activeArtist;

    [Header("Enemy")]
    public int enemyMaxHP = 200;
    public Slider enemyHealthBar;
    public TMP_Text enemyHPLabel;
    public Slider enemySignatureBar;

    [Header("Player UI")]
    public Slider playerHealthBar;
    public Slider playerSignatureBar;

    [Header("Combat Tuning")]
    [Range(0f, 0.2f)] public float chargePerGem = 0.05f;
    public float synergyMultiplier = 1.5f;
    [SerializeField] private float enemyChargeRate = 0.02f;

    // ── Runtime State ─────────────────────────
    private int _enemyCurrentHP;
    private float _playerSignatureCharge;
    private float _enemySignatureCharge;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Read selected artist from Team Select screen
        if (TeamData.ActiveArtist != null)
            activeArtist = TeamData.ActiveArtist;

        _enemyCurrentHP = enemyMaxHP;
        _playerSignatureCharge = 0f;
        _enemySignatureCharge = 0f;
        RefreshUI();
    }

    void Update()
    {
        // Enemy signature charges automatically over time
        _enemySignatureCharge += enemyChargeRate * Time.deltaTime;
        _enemySignatureCharge = Mathf.Clamp01(_enemySignatureCharge);

        if (enemySignatureBar != null)
            enemySignatureBar.value = _enemySignatureCharge;

        if (_enemySignatureCharge >= 1f)
        {
            Debug.Log("[Combat] Enemy Signature Move is READY!");
            _enemySignatureCharge = 0f;
        }
    }

    // ── PUBLIC API — called by Gibby's board ──
    public void OnGemsCleared(GemColor clearedColor, int count)
    {
        if (activeArtist == null)
        {
            Debug.LogWarning("CombatManager: No active artist assigned!");
            return;
        }

        bool isSynergy = clearedColor == activeArtist.FavoriteGem;

        // Damage Calculation
        float multiplier = isSynergy ? synergyMultiplier : 1f;
        float rawDamage = activeArtist.attackPower * count * multiplier;

        bool isCrit = Random.value < activeArtist.critChance;
        if (isCrit) rawDamage *= 2f;

        int finalDamage = Mathf.RoundToInt(rawDamage);

        _enemyCurrentHP = Mathf.Max(0, _enemyCurrentHP - finalDamage);

        Debug.Log($"[Combat] {clearedColor} x{count} → {finalDamage} dmg" +
                  (isSynergy ? " (SYNERGY)" : "") +
                  (isCrit ? " (CRIT!)" : ""));

        // Player signature charges from matches
        float chargeGained = chargePerGem * count * activeArtist.SignatureChargeMultiplier;
        _playerSignatureCharge = Mathf.Clamp01(_playerSignatureCharge + chargeGained);

        if (_playerSignatureCharge >= 1f)
            OnSignatureReady();

        RefreshUI();

        if (_enemyCurrentHP <= 0)
            OnEnemyDefeated();
    }

    void RefreshUI()
    {
        if (playerHealthBar != null)
            playerHealthBar.value = 1f;

        if (playerSignatureBar != null)
            playerSignatureBar.value = _playerSignatureCharge;

        if (enemyHealthBar != null)
            enemyHealthBar.value = (float)_enemyCurrentHP / enemyMaxHP;

        if (enemyHPLabel != null)
            enemyHPLabel.text = $"{_enemyCurrentHP} / {enemyMaxHP}";
    }

    void OnSignatureReady()
    {
        Debug.Log($"[Combat] {activeArtist.signatureMoveName} is READY!");
    }

    void OnEnemyDefeated()
    {
        ResultData.PlayerWon = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
    }

    void OnPlayerDefeated()
    {
        ResultData.PlayerWon = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
    }

    public static CombatManager Instance { get; private set; }
}
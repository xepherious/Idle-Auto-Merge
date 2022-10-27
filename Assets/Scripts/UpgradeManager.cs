using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using LargeNumbers;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private GameObject _generatorSpeedUpgrade;
    [SerializeField] private GameObject _generatorDoubleUpgrade;
    [SerializeField] private GameObject _generatorMinTierUpgrade;
    [SerializeField] private GameObject _generatorUpOneUpgrade;
    [SerializeField] private GameObject _generatorUpTwoUpgrade;
    [SerializeField] private GameObject _automergeSpeedUpgrade;
    [SerializeField] private GameObject _automergeTwiceUpgrade;
    [SerializeField] private GameObject _automergeThriceUpgrade;
    [SerializeField] private GameObject _mergeUpOneUpgrade;

    // normal upgrades
    public static Upgrade GeneratorSpeed;
    public static Upgrade GeneratorDouble;
    public static Upgrade GeneratorMinTier;
    public static Upgrade GeneratorUpOne;
    public static Upgrade GeneratorUpTwo;

    public static Upgrade AutomergeSpeed;
    public static Upgrade AutomergeTwice;
    public static Upgrade AutomergeThrice;
    public static Upgrade MergeUpOne;

    private static List<Upgrade> _upgrades;

    // TEMP
    public static double DebugMultiplier = 1;

    public static int GridSizeBonusX = 0;
    public static int GridSizeBonusY = 0;
    public static int GeneratorTripleUpgradeLevel = 0;
    public static int GeneratorUpThreeUpgradeLevel = 0;
    public static int AutomergeQuadUpgradeLevel = 0;
    public static int AutomergeUpTwoUpgradeLevel = 0;

    public static float ChanceBonusItemOne = 0;
    public static float ChanceBonusItemTwo = 0;
    public static float ChanceBonusItemThree = 0;

    private void Start()
    {
        GeneratorSpeed = new Upgrade(1, _generatorSpeedUpgrade, "Increase Generator Speed", new Upgrade.CostScaling(4, 15, 15),
            20, null, 0.05f, Upgrade.LockState.AlwaysUnlocked);

        GeneratorDouble = new Upgrade(2, _generatorDoubleUpgrade, "Chance to Generate Twice", new Upgrade.CostScaling(4, 20, 25),
            25, PrestiegeSkillTreeManager.Instance.RaiseCapForDoubleItemSpawn, 0.02f, Upgrade.LockState.AlwaysUnlocked);

        GeneratorMinTier = new Upgrade(3, _generatorMinTierUpgrade, "Raise Generator Output Tier", new Upgrade.CostScaling(8, 5000, 10000),
            25, null, 0.01f, Upgrade.LockState.LockedUnlockable, false);

        GeneratorUpOne = new Upgrade(4, _generatorUpOneUpgrade, "Chance for One Tier Higher", new Upgrade.CostScaling(4, 50, 50),
            25, PrestiegeSkillTreeManager.Instance.RaiseCapForGeneratorPlusOne, 0.01f, Upgrade.LockState.LockedUnlockable);

        GeneratorUpTwo = new Upgrade(5, _generatorUpTwoUpgrade, "Chance for Two Tiers Higher", new Upgrade.CostScaling(8, 5000, 10000),
            25, PrestiegeSkillTreeManager.Instance.RaiseCapForGeneratorPlusTwo, 0.01f, Upgrade.LockState.LockedUnlockable);

        AutomergeSpeed = new Upgrade(6, _automergeSpeedUpgrade, "Increase Auto-Merge Speed", new Upgrade.CostScaling(4, 20, 25),
            20, null, 0.05f, Upgrade.LockState.AlwaysUnlocked);

        AutomergeTwice = new Upgrade(7, _automergeTwiceUpgrade, "Chance to Auto-Merge Twice", new Upgrade.CostScaling(4, 30, 25),
            25, PrestiegeSkillTreeManager.Instance.RaiseCapForDoubleItemMerge, 0.02f, Upgrade.LockState.AlwaysUnlocked);

        AutomergeThrice = new Upgrade(8, _automergeThriceUpgrade, "Chance to Auto-Merge Thrice", new Upgrade.CostScaling(4, 40, 50),
            25, null, 0.01f, Upgrade.LockState.LockedUnlockable);

        MergeUpOne = new Upgrade(9, _mergeUpOneUpgrade, "Chance for +1 Tier on Merge", new Upgrade.CostScaling(4, 50, 50),
            25, null, 0.01f, Upgrade.LockState.LockedUnlockable);

        _upgrades = new List<Upgrade> { GeneratorSpeed, GeneratorDouble, GeneratorMinTier, GeneratorUpOne, GeneratorUpTwo, AutomergeSpeed, AutomergeTwice, AutomergeThrice, MergeUpOne};

        RefreshUpgrades();
    }

    public static void RefreshUpgradeButtonsClickableStatus()
    {
        // refreshing the buttons availbilities, for use when available currency changes

        GeneratorSpeed.RefreshButtonClickableStatus();
        GeneratorDouble.RefreshButtonClickableStatus();
        GeneratorMinTier.RefreshButtonClickableStatus();
        GeneratorUpOne.RefreshButtonClickableStatus();
        GeneratorUpTwo.RefreshButtonClickableStatus();

        AutomergeSpeed.RefreshButtonClickableStatus();
        AutomergeTwice.RefreshButtonClickableStatus();
        AutomergeThrice.RefreshButtonClickableStatus();
        MergeUpOne.RefreshButtonClickableStatus();
    }

    public static void ResetEarnedUnlocks()
    {
        GridSizeBonusX = 0;
        GridSizeBonusY = 0;

        foreach (Upgrade upgrade in _upgrades)
        {
            upgrade.ResetLock();
        }
    }

    public static void ApplyEarnedUpgrades(List<Objective> objectives, int activeObjectiveIndex)
    {
        if (objectives == null || activeObjectiveIndex < 0)
        {
            // Nothing to unlock
            return;
        }

        // Unlock rewards from previous objectives, this includes the current active objective since it hasn't been progressed yet.
        for (int i = activeObjectiveIndex - 1; i >= 0; i--)
        {
            // set grid bonuses
            if (objectives[i].Reward.Type is ObjectiveRewardType.NumberRowsBonus && objectives[i].Reward.Value > GridSizeBonusY)
            {
                GridSizeBonusY = objectives[i].Reward.Value;
            }
            else if (objectives[i].Reward.Type is ObjectiveRewardType.NumberColumnsBonus && objectives[i].Reward.Value > GridSizeBonusX)
            {
                GridSizeBonusX = objectives[i].Reward.Value;
            }
            // unlock standard upgrades
            else if (objectives[i].Reward.Type is ObjectiveRewardType.UnlockStandardUpgrade)
            {
                switch (objectives[i].Reward.Value)
                {
                    case 1:
                        GeneratorMinTier.Unlock();
                        break;
                    case 2:
                        GeneratorUpOne.Unlock();
                        break;
                    case 3:
                        AutomergeThrice.Unlock();
                        break;
                    case 4:
                        GeneratorUpTwo.Unlock();
                        break;
                    case 5:
                        MergeUpOne.Unlock();
                        break;
                }
            }
        }

    }

    public static void RefreshUpgrades()
    {
        foreach (Upgrade upgrade in _upgrades)
        {
            upgrade.Refresh();
        }
    }

    public static void ResetUpgradeLevels()
    {
        foreach(Upgrade upgrade in _upgrades)
        {
            upgrade.ResetLevel();
            upgrade.Refresh();
        }
    }

    public static List<SerializedUpgrade> SaveUpgradeLevels()
    {
        var serializedUpgrades = new List<SerializedUpgrade>();

        foreach (Upgrade upgrade in _upgrades)
        {
            serializedUpgrades.Add(upgrade.Serialize());
        }

        return serializedUpgrades;
    }

    public static void LoadUpgradeLevels(List<SerializedUpgrade> serializedUpgrades)
    {
        foreach(Upgrade upgrade in _upgrades)
        {
            upgrade.SetPurchasedLevelFromData(serializedUpgrades);
            upgrade.Refresh();
        }
    }
}

public class Upgrade
{
    // Identifier for working with saved data.
    private int _saveIndex;

    private string _description;
    private int _purchasedLevel;
    private int _maxUpgradeLevel;
    private PrestiegeSkill _maxOverrideSkill;

    private float _effectPerLevel;
    private CostScaling _costScaling;

    private Button _button;
    private TextMeshProUGUI _buttonTextMesh;
    private TextMeshProUGUI _upgradePreviewTextMesh;
    private TextMeshProUGUI _upgradeDescription;
    private Image _lockImage;

    private LockState _lock;
    private bool _percentageBased;


    public Upgrade(int saveIndex, GameObject gameObject, string description, CostScaling scaling, int maxUpgradeLevel, PrestiegeSkill maxOverrideSkill, float effectPerLevel, LockState locked, bool percentageBased = true)
    {
        _saveIndex = saveIndex;
        _description = description;
        _purchasedLevel = 0;
        _costScaling = scaling;

        if (locked == LockState.LockedUnlockable)
        {
            _lockImage = gameObject.transform.Find("LockImage").gameObject.GetComponent<Image>();
        }

        _upgradeDescription = gameObject.transform.Find("TextDescription").gameObject.GetComponent<TextMeshProUGUI>();
        _upgradePreviewTextMesh = gameObject.transform.Find("TextUpgradeValueChange").gameObject.GetComponent<TextMeshProUGUI>();

        _button = gameObject.transform.Find("Button").gameObject.GetComponent<Button>();
        _buttonTextMesh = _button.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>(); ;
        
        _maxUpgradeLevel = maxUpgradeLevel;
        _maxOverrideSkill = maxOverrideSkill;

        _effectPerLevel = effectPerLevel;
        _lock = locked;
        _percentageBased = percentageBased;

        _button.onClick.AddListener(delegate { UpgradeClick(this); });
    }

    private static void UpgradeClick(Upgrade upgrade)
    {
        if (!upgrade.IsMax() && CurrencyManager.Amount >= upgrade.GetCost())
        {
            CurrencyManager.Spend(upgrade.GetCost());

            upgrade.LevelUp();
        }
    }

    /// <summary>
    /// Refreshes the status of the upgrade button and text values
    /// </summary>
    public void Refresh()
    {
        if (IsMax())
        {
            _upgradePreviewTextMesh.text = $"{(_percentageBased ? "" : "+")}{Mathf.RoundToInt(_purchasedLevel * _effectPerLevel * 100)}{(_percentageBased ? "%" : "")}";
        }
        else
        {
            _buttonTextMesh.text = $"Upgrade\r\n{StringHelper.FormatCurrency(new AlphabeticNotation(_costScaling.GetCost(_purchasedLevel)))}";
            _upgradePreviewTextMesh.text = $"{(_percentageBased ? "" : "+")}{Mathf.RoundToInt(_purchasedLevel * _effectPerLevel * 100)}{(_percentageBased ? "%" : "")} -> <color=#00E000>" +
                                           $"{(_percentageBased ? "" : "+")}{Mathf.RoundToInt((_purchasedLevel + 1) * _effectPerLevel * 100)}{(_percentageBased ? "%" : "")}</color>";
        }

        if (_lock == LockState.LockedUnlockable)
        {
            _upgradeDescription.text = "Complete Objectives to Unlock";
            _buttonTextMesh.text = "Locked";
            _upgradePreviewTextMesh.gameObject.SetActive(false);

            if (_lockImage != null)
            {
                _lockImage.gameObject.SetActive(true);
            }
        }
        else
        {
            _upgradeDescription.text = _description;
            _upgradePreviewTextMesh.gameObject.SetActive(true);

            if (_lockImage != null)
            {
                _lockImage.gameObject.SetActive(false);
            }
        }

        RefreshButtonClickableStatus();
    }

    /// <summary>
    /// Refreshes only the status of the upgrade button
    /// </summary>
    public void RefreshButtonClickableStatus()
    {
        if (_lock == LockState.LockedUnlockable)
        {
            _button.interactable = false;
        }
        else if (IsMax())
        {
            _buttonTextMesh.text = $"Max\r\nLevel";
            _button.interactable = false;
        }
        else if (CurrencyManager.Amount < GetCost())
        {
            _button.interactable = false;
        }
        else
        {
            _button.interactable = true;
        }
    }

    /// <summary>
    /// Gets the bonus percentage for the upgrade
    /// </summary>
    /// <returns>a multiplier which represents a percentage bonus. Example: a 75% bonus from an upgrade will return as 1.75f</returns>
    public float GetBonus()
    {
        // Most upgrade bonuses are percentage
        return 1 + _purchasedLevel * _effectPerLevel;
    }

    /// <summary>
    /// Gets the bonus min item tier for the special case upgrade
    /// </summary>
    /// <returns>The minimum tier of item that can be spawned</returns>
    public int GetMinTierBonus()
    {
        // This is an exception, where the bonus is not a percentage
        return 1 + _purchasedLevel; 
    }

    /// <summary>
    /// Uses the current upgrade level and the cost scaling for this upgrade to determine the cost of the next level
    /// </summary>
    /// <returns>the cost of purchasing the next level of the upgrade</returns>
    public double GetCost()
    {
        return _costScaling.GetCost(_purchasedLevel);
    }

    /// <summary>
    /// Increases the level of the upgrade
    /// </summary>
    /// <param name="numberOfLevelups">The amout to increase the level of the upgrade</param>
    public void LevelUp(int numberOfLevelups = 1)
    {
        _purchasedLevel += numberOfLevelups;

        Refresh();

        // Special handling for the min item tier upgrade, which is the only non-percentage based standard upgrade
        if (!_percentageBased)
        {
            GridManager.EnforceMinItemTier();
        }
    }

    public bool IsMax()
    {
        if (_maxOverrideSkill == null)
        {
            return _purchasedLevel >= _maxUpgradeLevel;
        }
        else
        {
            // Prestiege skills increase the maximum rank of the upgrade by an amout that is derived from the effectiveness of the skill vs. the upgrade.
            return _purchasedLevel >= _maxUpgradeLevel + (_maxOverrideSkill.GetBonusPercent() / _effectPerLevel);
        }
    }

    public void ResetLevel()
    {
        _purchasedLevel = 0;
    }

    public void Unlock()
    {
        if (_lock == LockState.LockedUnlockable)
        {
            _lock = LockState.UnlockedUnlockable;
        }

        Refresh();
    }

    public void ResetLock()
    {
        if (_lock != LockState.AlwaysUnlocked)
        {
            _lock = LockState.LockedUnlockable;
        }
    }

    public SerializedUpgrade Serialize()
    {
        return new SerializedUpgrade(_saveIndex, _purchasedLevel);
    }

    public void SetPurchasedLevelFromData(List<SerializedUpgrade> serializedUpgrades)
    {
        SerializedUpgrade matchingUpgrade = serializedUpgrades.First(x => x.SaveIndex == _saveIndex);
        _purchasedLevel = Math.Min(matchingUpgrade.PurchasedLevel, _maxUpgradeLevel);
    }

    public enum LockState
    {
        LockedUnlockable,
        UnlockedUnlockable,
        AlwaysUnlocked
    }

    public struct CostScaling
    {
        // Cost scaling for upgrades should be able handle the base price, linear and exponential scaling.
        private double ExponentBase;
        private double LinearMulti;
        private int BaseCost;

        public CostScaling(double exponentBase, double linearMulti, int baseCost)
        {
            ExponentBase = exponentBase;
            LinearMulti = linearMulti;
            BaseCost = baseCost;
        }

        public double GetCost(int currentTier)
        {
            if (currentTier != 0)
            {
                return Math.Pow(ExponentBase, currentTier) * LinearMulti * (1 - PrestiegeSkillTreeManager.Instance.UpgradeDiscount.GetBonusPercent());
            }
            else
            {
                return BaseCost;
            }
        }
    }
}

[Serializable]
public class SerializedUpgrade
{
    public int SaveIndex;
    public int PurchasedLevel;

    public SerializedUpgrade(int saveIndex, int purchasedLevel)
    {
        SaveIndex = saveIndex;
        PurchasedLevel = purchasedLevel;
    }
}
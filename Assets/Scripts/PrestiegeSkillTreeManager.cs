using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using LargeNumbers;

public class PrestiegeSkillTreeManager : MonoBehaviour
{
    public static int AvailableSkillPoints
    {
        get { return s_availableSkillPoints; }
        set
        {
            s_availableSkillPoints = value;
            AchievementManager.Instance.PrestiegeSkillPointsTracker.Refresh(AvailableSkillPoints + SpentSkillPoints);
        }
    }
    private static int s_availableSkillPoints;

    public static int SpentSkillPoints { get; set; }

    [SerializeField] private TextMeshProUGUI _availablePointsText;
    [SerializeField] private TextMeshProUGUI _spentPointsText;
    [SerializeField] private TextMeshProUGUI _totalPointsText;
    [SerializeField] private TextMeshProUGUI _creditsBonusText;

    [SerializeField] private GameObject _skillParentPrimary;
    [SerializeField] private GameObject _skillParentSecondary;

    [SerializeField] private TextMeshProUGUI _skillTitleText;
    [SerializeField] private TextMeshProUGUI _skillCurrentRanksText;
    [SerializeField] private TextMeshProUGUI _skillDetailsText;
    [SerializeField] private TextMeshProUGUI _skillNoneSelectedText;
    [SerializeField] private Button _skillPurchaseButton;
    private TextMeshProUGUI _skillPurchaseButtonText;
    [SerializeField] private TextMeshProUGUI _skillCostPreviewText;

    [SerializeField] private GameObject _ObjectiveDisplay;
    [SerializeField] private GameObject _PrestiegeDisplay;
    [SerializeField] private Slider _skillPointProgress;
    [SerializeField] private TextMeshProUGUI _skillPointProgressText;
    [SerializeField] private TextMeshProUGUI _currentSkillPointAwardText;
    [SerializeField] private Button _resetForPrestiegeButton;
    private TextMeshProUGUI _resetForPrestiegeButtonText;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _okCancelMessageBox;

    public PrestiegeSkill GeneratorSpeed { get; private set; }
    public PrestiegeSkill RaiseCapForDoubleItemSpawn { get; private set; }
    public PrestiegeSkill RaiseCapForGeneratorPlusOne { get; private set; }
    public PrestiegeSkill RaiseCapForGeneratorPlusTwo { get; private set; }
    public PrestiegeSkill AddChanceForGenThrice { get; private set; }
    public PrestiegeSkill AddChanceForGenPlusThree { get; private set; }
    public PrestiegeSkill AutoMergerSpeed { get; private set; }
    public PrestiegeSkill RaiseCapForDoubleItemMerge { get; private set; }
    public PrestiegeSkill AutoMergeBonusItems { get; private set; }
    public PrestiegeSkill AddChanceForMergePlusTwo { get; private set; }
    public PrestiegeSkill AddChanceForMergePlusThree { get; private set; }
    public PrestiegeSkill CreditsMultiplier { get; private set; }
    public PrestiegeSkill UpgradeDiscount { get; private set; }
    public PrestiegeSkill PrestiegePointBonus { get; private set; }
    public PrestiegeSkill AdditionalRow { get; private set; }
    public PrestiegeSkill AdditionalColumn { get; private set; }
    public PrestiegeSkill MinimumItemTier { get; private set; }
    public PrestiegeSkill BonusItemSpawnChanceMultiplier { get; private set; }
    public PrestiegeSkill BonusItemSpawnOneHigherChance { get; private set; }
    public PrestiegeSkill BonusItemMergeUpgradeChance { get; private set; }

    private List<PrestiegeSkill> _prestiegeSkillList;

    public PrestiegeSkill SelectedSkill { get; set; }

    // Used for tracking how many prestiege points the player can recieve by resetting.
    public static AlphabeticNotation CreditsProducedThisRun
    {
        get { return _creditsProducedThisRun; }
        set
        {
            _creditsProducedThisRun = value;
        }
    }
    private static AlphabeticNotation _creditsProducedThisRun;
    private static int _currentSkillPointAwardAmount;

    public static PrestiegeSkillTreeManager Instance;

    public static bool PrestiegeModeUnlocked
    {
        get { return s_prestiegeModeUnlocked; }
        set
        {
            s_prestiegeModeUnlocked = value;

            Instance._ObjectiveDisplay.gameObject.SetActive(!s_prestiegeModeUnlocked);
            Instance._PrestiegeDisplay.gameObject.SetActive(s_prestiegeModeUnlocked);
        }
    }
    private static bool s_prestiegeModeUnlocked;



    private void Awake()
    {
        Instance = this;
        _skillPurchaseButtonText = _skillPurchaseButton.GetComponentInChildren<TextMeshProUGUI>();
        _resetForPrestiegeButtonText = _resetForPrestiegeButton.GetComponentInChildren<TextMeshProUGUI>();
        _resetForPrestiegeButtonText.text = "Not Ready";

        PrestiegeModeUnlocked = false;
        _creditsProducedThisRun = new AlphabeticNotation();

        PrestiegeSkill.SetGlobalSkillParent(_skillParentPrimary);

        // First Column - Generator themed skills
        GeneratorSpeed = new PrestiegeSkill("Generator Speed Increase", 1,
            "Increase the speed of the Generator by 5%. This is additive with the normal upgrade to Generator speed.",
            20, 1, 0, new List<PrestiegeSkill>(), 0, 0, 0.05f);

        RaiseCapForDoubleItemSpawn = new PrestiegeSkill("Raise Cap on Chance To Generate Twice", 2,
            "Each Rank of this Skill raises the cap on the upgrade which provides a chance for the Generator to output TWO items by 2%. This allows for 100% chance, instead of 50%",
            25, 2, 0, new List<PrestiegeSkill> { GeneratorSpeed }, 0, 0, 0.02f);

        RaiseCapForGeneratorPlusOne = new PrestiegeSkill("Raise Cap on Chance for One Tier Higher", 3,
            "Each Rank of this Skill raises the cap on the upgrade which provides a chance for the Generator to make an item ONE rank higher than the minimum by 5%. This allows for 90% chance, instead of 50%",
            18, 3, 0, new List<PrestiegeSkill> { RaiseCapForDoubleItemSpawn }, 0, 0, 0.05f);

        RaiseCapForGeneratorPlusTwo = new PrestiegeSkill("Raise Chance for Two Tiers Higher Cap", 4,
            "Each Rank of this Skill raises the cap on the upgrade which provides a chance for the Generator to make an item TWO ranks higher than the minimum by 2%. This allows for 45% chance, instead of 25%",
            20, 4, 0, new List<PrestiegeSkill> { RaiseCapForGeneratorPlusOne }, 0, 0, 0.02f);

        AddChanceForGenThrice = new PrestiegeSkill("Add a Chance to Generate THRICE", 5,
            "Each Rank of this Skill adds a 2% chance for the Generator to output THREE items.",
            15, 5, 0, new List<PrestiegeSkill> { RaiseCapForGeneratorPlusTwo }, 0, 0, 0.02f);

        AddChanceForGenPlusThree = new PrestiegeSkill("Add a Chance to Generate THREE Tiers Higher", 6,
            "Each Rank of this Skill adds a 1% chance for the Generator to make an item FIVE ranks higher than the minimum.",
            10, 5, 1, new List<PrestiegeSkill> { AddChanceForGenThrice }, 0, 0, 0.01f);


        // Second Column - Auto-Merger themed skills
        AutoMergerSpeed = new PrestiegeSkill("Auto-Merge Speed Increase", 7,
            "Increase the speed of Auto-Merge by 5%. This is additive with the normal upgrade to Auto-Merge speed.",
            20, 1, 0, new List<PrestiegeSkill>(), 0, 0, 0.05f);

        RaiseCapForDoubleItemMerge = new PrestiegeSkill("Raise Chance To Auto-Merge Twice Cap", 8,
            "Each Rank of this Skill raises the cap on the upgrade which provides a chance to Auto-Merge two items by 2%. This allows for 100% chance, instead of 50%",
            25, 2, 0, new List<PrestiegeSkill> { AutoMergerSpeed }, 0, 0, 0.02f);

        AutoMergeBonusItems = new PrestiegeSkill("Auto-Merge for Bonus Items", 9,
            "Bonus items will now be Auto-Merged if they are at or below the rank of this Skill.",
            4, 3, 1, new List<PrestiegeSkill> { RaiseCapForDoubleItemMerge }, 0, 0, 1f);

        AddChanceForMergePlusTwo = new PrestiegeSkill("Add Chance for +2 on Merge", 10,
            "Each Rank of this Skill adds a 1% chance for merged items to become TWO tiers higher than normal.",
            15, 4, 1, new List<PrestiegeSkill> { AutoMergeBonusItems }, 0, 0, 0.01f);

        AddChanceForMergePlusThree = new PrestiegeSkill("Add Chance for +3 on Merge", 11,
            "Each Rank of this Skill adds a 1% chance for merged items to become THREE tiers higher than normal.",
            10, 6, 2, new List<PrestiegeSkill> { AddChanceForMergePlusTwo }, 0, 0, 0.01f);

        // Third Column - Credits themed skills
        CreditsMultiplier = new PrestiegeSkill("Credit Bonus Multiplier", 12,
            "Credit earnings are increased by 10% for each Rank of this Skill. This is multiplicative with all other bonuses.",
            25, 1, 0, new List<PrestiegeSkill>(), 0, 0, 0.1f);

        UpgradeDiscount = new PrestiegeSkill("Upgrade Discount", 13,
            "Normal upgrade costs are reduced by 5% for each rank of this Skill. Maxiumum 75% reduction.",
            15, 2, 1, new List<PrestiegeSkill> { CreditsMultiplier }, 0, 0, 0.05f);

        PrestiegePointBonus = new PrestiegeSkill("Prestiege Point Bonus", 14,
            "Earn 5% more Prestiege points for each rank of this Skill.",
            20, 3, 1, new List<PrestiegeSkill> { UpgradeDiscount }, 0, 0, 0.05f);

        // Fourth and Fifth Columns
        AdditionalRow = new PrestiegeSkill("Additional Grid Row", 15,
            "Each Rank of this Skill provides one more row on the grid.",
            4, 1, 0, new List<PrestiegeSkill>(), 0, 0, 1f);

        AdditionalColumn = new PrestiegeSkill("Additional Grid Column", 16,
            "Each Rank of this Skill provides one more column on the grid.",
            4, 1, 0, new List<PrestiegeSkill>(), 0, 0, 1f);

        // The parent of the skill buttons changes here, since they're not part of the main grid spacing
        PrestiegeSkill.SetGlobalSkillParent(_skillParentSecondary);

        // Collapsed column of skills from the 3rd and the combine 4th and 5th
        BonusItemSpawnOneHigherChance = new PrestiegeSkill("Chance for Bonus Items to be One Tier Higher", 17,
            "Each Rank of this Skill adds a 3% chance for Bonus items to spawn ONE tier higher than normal.",
            10, 2, 1, new List<PrestiegeSkill> { PrestiegePointBonus, MinimumItemTier }, 0, 0, 0.03f);

        BonusItemMergeUpgradeChance = new PrestiegeSkill("Chance for +1 Tier to Bonus Item on Merge", 18,
            "Each Rank of this Skill adds a 3% chance for merged Bonus items to become ONE tier higher than normal.",
            10, 3, 2, new List<PrestiegeSkill> { BonusItemSpawnOneHigherChance }, 0, 0, 0.03f);

        // Collapsed columns of skills from 4th and 5th
        MinimumItemTier = new PrestiegeSkill("Increase Minimum Tier", 19,
            "Raises the minimum tier of Generated items by ONE for each Rank of this Skill.",
            25, 3, 2, new List<PrestiegeSkill> { AdditionalRow, AdditionalColumn }, 0, 0, 1f);

        BonusItemSpawnChanceMultiplier = new PrestiegeSkill("Increase Bonus Item Chance", 20,
            "Increases the chance for Bonus items to spawn by 10% for each Rank of this Skill. Allows the overall chance to be raised from 2.5% to 5%.",
            10, 2, 2, new List<PrestiegeSkill> { MinimumItemTier }, 0, 0, 0.1f);

        _prestiegeSkillList = new List<PrestiegeSkill> { GeneratorSpeed, RaiseCapForDoubleItemSpawn, RaiseCapForGeneratorPlusOne, RaiseCapForGeneratorPlusTwo,
                                                         AddChanceForGenThrice, AddChanceForGenPlusThree, AutoMergerSpeed, RaiseCapForDoubleItemMerge, AutoMergeBonusItems,
                                                         AddChanceForMergePlusTwo, AddChanceForMergePlusThree, CreditsMultiplier, UpgradeDiscount, PrestiegePointBonus, AdditionalRow,
                                                         AdditionalColumn, MinimumItemTier, BonusItemSpawnChanceMultiplier, BonusItemSpawnOneHigherChance, BonusItemMergeUpgradeChance };
    }

    public void HardReset()
    {
        AvailableSkillPoints = 0;
        SpentSkillPoints = 0;
        _creditsProducedThisRun = new AlphabeticNotation();
        Instance.RefreshPoints();

        foreach(var skill in _prestiegeSkillList)
        {
            skill.CurrentRank = 0;
        }
        RefreshSkillTree();

        PrestiegeModeUnlocked = false;
    }

    public void RefreshPoints()
    {
        _availablePointsText.text = AvailableSkillPoints.ToString();
        _spentPointsText.text = SpentSkillPoints.ToString();
        _totalPointsText.text = (AvailableSkillPoints + SpentSkillPoints).ToString();
        _creditsBonusText.text = $"Credits Bonus\r\n{(AvailableSkillPoints + SpentSkillPoints) * 10}%";
    }

    public static float GetEarnedSkillPointCreditsBonus()
    {
        // Each earned skill point provides a permanent 10% bonus to all credits earned.
        return 1 + .1f * (AvailableSkillPoints + SpentSkillPoints);
    }

    public void RefreshSkillTree()
    {
        foreach (PrestiegeSkill skill in _prestiegeSkillList)
        {
            skill.RefreshUI();
        }
    }

    public void RefreshSelectedSkillDetails()
    {
        if (SelectedSkill == null)
        {
            _skillTitleText.text = String.Empty;
            _skillCurrentRanksText.text = String.Empty;
            _skillDetailsText.text = "Select a skill from the tree on the left by clicking on it.";
            _skillCostPreviewText.text = String.Empty;
            _skillPurchaseButton.gameObject.SetActive(false);
        }
        else
        {
            _skillTitleText.text = SelectedSkill.SkillName;
            _skillCurrentRanksText.text = $"{SelectedSkill.CurrentRank} / {SelectedSkill.MaxRank}";
            _skillDetailsText.text = SelectedSkill.SkillDescription;
            
            _skillPurchaseButton.gameObject.SetActive(true);

            if (SelectedSkill.CurrentRank == SelectedSkill.MaxRank)
            {
                _skillCostPreviewText.text = String.Empty;
                _skillPurchaseButtonText.text = $"Maximum Rank";
                _skillPurchaseButton.interactable = false;
            }
            else if (SelectedSkill.PrereqSkillsMet())
            {
                _skillCostPreviewText.text = $"(Cost for next rank: {SelectedSkill.GetCost(SelectedSkill.CurrentRank + 1)})";
                _skillPurchaseButtonText.text = $"Purchase for {SelectedSkill.GetCost()} Points";
                _skillPurchaseButton.interactable = AvailableSkillPoints >= SelectedSkill.GetCost();
            }
            else
            {
                _skillCostPreviewText.text = "You must first purchase at least one Rank of each connected Skill above this one in the tree.";
                _skillPurchaseButtonText.text = $"Prerequisite Skill Missing";
                _skillPurchaseButton.interactable = false;
            }
        }
    }

    public void PurchaseButtonClick()
    {
        SelectedSkill.TryPurchase();
        RefreshSelectedSkillDetails();

        // Update other game elements that can change based on Prestiege Skill purchases.
        UpgradeManager.RefreshUpgrades();
        GridManager.RegenerateGrid();
        GridManager.EnforceMinItemTier();
    }

    public Dictionary<int, int> SerializeSkillTree()
    {
        Dictionary<int, int> serializedSkills = new Dictionary<int, int>();

        foreach(PrestiegeSkill skill in Instance._prestiegeSkillList)
        {
            serializedSkills.Add(skill.SkillID, skill.CurrentRank);
        }

        return serializedSkills;
    }

    public void DeserializeSkillTree(Dictionary<int, int> serializedSkills)
    {
        foreach(PrestiegeSkill skill in Instance._prestiegeSkillList)
        {
            skill.CurrentRank = serializedSkills[skill.SkillID];
        }

        RefreshSkillTree();
    }

    public static void UpdateSkillPointReward(AlphabeticNotation earnedCredits)
    {
        // Each time currency earned is updated, also update the prestige skill point reward preview.
        _creditsProducedThisRun += earnedCredits;

        if (_creditsProducedThisRun <= 0)
        {
            Instance._currentSkillPointAwardText.text = $"Prestiege for +0 Skill Points";
            Instance._skillPointProgress.value = 0;
            Instance._skillPointProgressText.text = $"{String.Format("{0:0.00}", 0)}%";
            return;
        }

        // Recalculate the number of prestiege points ready to be awarded, and the progress towards the next point.
        double rawAwardAmount = Math.Log(_creditsProducedThisRun, 4);
        _currentSkillPointAwardAmount = (int)Math.Floor(rawAwardAmount);
        double progressPercentage = rawAwardAmount - _currentSkillPointAwardAmount;

        // Skill point award shouldn't be too easy to reset rapidly for a few points.
        _currentSkillPointAwardAmount -= 15;
        if (_currentSkillPointAwardAmount < 0)
        {
            _currentSkillPointAwardAmount = 0;
            // To compensate for the -15 point adjustment, when no skill points are yet ready to award, calculate based on when the first point will be awarded at 16.
            progressPercentage = rawAwardAmount / 16;
        }
        else if (!Instance._resetForPrestiegeButton.interactable && _currentSkillPointAwardAmount > 0)
        {
            Instance._resetForPrestiegeButton.interactable = true;
            Instance._resetForPrestiegeButtonText.text = "Claim";
        }

        Instance._currentSkillPointAwardText.text = $"Prestiege for +{_currentSkillPointAwardAmount} Skill Points";
        Instance._skillPointProgress.value = (float)progressPercentage;
        Instance._skillPointProgressText.text = $"{String.Format("{0:0.00}", progressPercentage * 100)}%";
    }

    public void PrestiegeResetButtonClick()
    {
        GameObject msgBox = Instantiate(_okCancelMessageBox, _canvas.transform);
        msgBox.GetComponentInChildren<TextMeshProUGUI>().text = $"Are you sure you want to reset the current grid to prestiege?\r\n\r\n" +
                                                                $"This will reward {_currentSkillPointAwardAmount} Skill Points.";

        Button okButton = msgBox.transform.Find("OKButton_Find").gameObject.GetComponent<Button>();
        okButton.GetComponentInChildren<TextMeshProUGUI>().text = "Yes!";
        okButton.onClick.AddListener(delegate { PerformPrestiegeReset(msgBox); });

        Button cancelButton = msgBox.transform.Find("CancelButton_Find").gameObject.GetComponent<Button>();
        cancelButton.onClick.AddListener(delegate { DismissMessagebox(msgBox); });
    }

    private void DismissMessagebox(GameObject msgBox)
    {
        Destroy(msgBox);
    }

    public void PerformPrestiegeReset(GameObject msgBox)
    {
        AvailableSkillPoints += _currentSkillPointAwardAmount;
        _creditsProducedThisRun = new AlphabeticNotation();

        // Currency Reset
        CurrencyManager.Amount = new AlphabeticNotation();

        // Purchased Upgrade Levels Reset, Unlocks Reset
        UpgradeManager.ResetUpgradeLevels();

        // Item/Tile Data
        GridManager.DestroyGrid();
        GridManager.GenerateGrid();
        Item.BestItemTierThisRun = 1;

        SaveDataHandler.SaveGame();

        // Set button state back to uninteractable
        Instance._resetForPrestiegeButton.interactable = false;
        Instance._resetForPrestiegeButtonText.text = "Not Ready";

        // Trigger a quick refresh
        UpdateSkillPointReward(new AlphabeticNotation());
        RefreshPoints();

        Destroy(msgBox);
    }
}

public class PrestiegeSkill
{
    public string SkillName { get; private set; }
    public int SkillID { get; private set; }
    public string SkillDescription { get; private set; }

    public int MaxRank { get; private set; }
    public int CurrentRank { get; set; }

    // The Parent object, used for finding the game object asscociated with each skill
    public static GameObject SkillParent { get; private set; }

    // The skill's game object in the prestiege UI
    public GameObject SkillButtonObj { get; private set; }
    private Image _skillImage;

    private TextMeshProUGUI _skillRankOnIconText;

    public const string SkillNamePrefix = "Skill_ID_";
    private static Color s_skillSelectedColor = new Color(0.5058824f, 0.9254903f, 0.9254903f);
    private static Color s_skillMaxColor = new Color(1f, 0.9176471f, 0.654902f);

    private int _baseCost;
    private int _costIncreasePerRank;

    // Each Skill can have between zero and two prerequiste skills
    private List<PrestiegeSkill> _prereqSkills;

    // The tier of item needed to qualify to purchase the skill
    private int _purchasePrereqTier;

    // The tier of item needed to allow the player to see the skill info
    private int _visibilityPrereqTier;

    // value is the effectiveness of the skill, which is completely dependent on how each skill functions
    private float _effectiveness;

    public PrestiegeSkill(string name, int id, string desc, int maxRank, int cost, int costIncrease, List<PrestiegeSkill> prereqSkills, int purchasePrereqTier, int visPrereqTier, float effectiveness)
    {
        SkillName = name;
        SkillID = id;
        SkillDescription = desc;
        MaxRank = maxRank;

        _baseCost = cost;
        _costIncreasePerRank = costIncrease;

        _prereqSkills = prereqSkills;
        _purchasePrereqTier = purchasePrereqTier;
        _visibilityPrereqTier = visPrereqTier;

        _effectiveness = effectiveness;

        SkillButtonObj = SkillParent.transform.Find($"{SkillNamePrefix}{id}").gameObject;
        SkillButtonObj.GetComponent<Button>().onClick.AddListener(delegate { SkillClick(); });
        _skillImage = SkillButtonObj.GetComponent<Image>();
        _skillRankOnIconText = SkillButtonObj.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SkillClick()
    {
        if (PrestiegeSkillTreeManager.Instance.SelectedSkill != null)
        {
            PrestiegeSkillTreeManager.Instance.SelectedSkill.RefreshUI();
        }

        PrestiegeSkillTreeManager.Instance.SelectedSkill = this;
        PrestiegeSkillTreeManager.Instance.RefreshSelectedSkillDetails();
        _skillImage.color = s_skillSelectedColor;
    }

    public int GetCost()
    {
        return _baseCost + CurrentRank * _costIncreasePerRank;
    }

    public int GetCost(int rank)
    {
        return _baseCost + rank * _costIncreasePerRank;
    }

    public bool PrereqSkillsMet()
    {
        foreach(PrestiegeSkill prereqSkill in _prereqSkills)
        {
            if (prereqSkill.CurrentRank == 0)
            {
                return false;
            }
        }

        return true;
    }

    public void TryPurchase()
    {
        if (CurrentRank < MaxRank && PrestiegeSkillTreeManager.AvailableSkillPoints >= GetCost())
        {
            PrestiegeSkillTreeManager.AvailableSkillPoints -= GetCost();
            PrestiegeSkillTreeManager.SpentSkillPoints += GetCost();
            CurrentRank++;

            RefreshUI();
            PrestiegeSkillTreeManager.Instance.RefreshPoints();
        }
    }

    public static void SetGlobalSkillParent(GameObject parent)
    {
        SkillParent = parent;
    }

    public void RefreshUI()
    {
        if (CurrentRank == MaxRank)
        {
            _skillRankOnIconText.text = "M";
            _skillImage.color = s_skillMaxColor;
        }
        else
        {
            _skillRankOnIconText.text = $"{CurrentRank}";
            _skillImage.color = Color.white;
        }
    }

    public float GetBonusPercent()
    {
        return CurrentRank * _effectiveness;
    }

    public int GetBonusFlat()
    {
        if (_effectiveness == Mathf.Floor(_effectiveness))
        {
            return CurrentRank * (int)_effectiveness;
        }
        else
        {
            throw new Exception("GetBonusInt failed to convert float to int");
        }
    }
}
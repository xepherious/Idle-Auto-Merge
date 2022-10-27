using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LargeNumbers;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _objectiveText;
    [SerializeField] private Button _objectiveCompleteButton;
    [SerializeField] private GameObject _objectivePanel;
    [SerializeField] private GameObject _highlightPrefab;
    [SerializeField] private GameObject _rewardPopup;
    [SerializeField] private Canvas _canvas;

    private static TextMeshProUGUI s_objectiveText;
    private static Button s_objectiveCompleteButton;
    private static TextMeshProUGUI s_objectiveCompleteButtonText;
    private static Image s_objectiveBackgroundImage;

    private static Color s_objectiveBackgroundColorInProgress = new Color(0, 0.7215686f, 0.5803922f);
    private static Color s_objectiveForegroundColorReadyToComplete = new Color(0.3333333f, 0.937255f, 0.7686275f);

    private static List<Objective> s_objectives = new List<Objective>();

    // Using a value for tracking which objective is active, since objectives are serial
    public static int ActiveObjectiveIndex
    {
        get { return s_activeObjectiveIndex; }
        set
        {
            s_activeObjectiveIndex = value;
            SetActiveObjective();
        }
    }
    private static int s_activeObjectiveIndex;

    private static Objective s_activeObjective;

    private void Awake()
    {
        s_objectiveBackgroundImage = _objectivePanel.GetComponent<Image>();
        s_objectiveText = _objectiveText;
        s_objectiveCompleteButton = _objectiveCompleteButton;
        s_objectiveCompleteButtonText = _objectiveCompleteButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        //UseDebugObjectives();
        UseRealObjectives();

        ActiveObjectiveIndex = 0;
    }

    private void UseDebugObjectives()
    {
        s_objectives.Add(new BestItemObjective("Merge into a Tier 3 item", 3,
            new ObjectiveReward("You have gained a fifth row on the grid", ObjectiveRewardType.NumberRowsBonus, 1,
            new HighlightTransform(207f, -143f, 447, 84, 0))));

        s_objectives.Add(new CreditsTotalObjective("Earn a total of 1K credits", new AlphabeticNotation(1, 1),
            new ObjectiveReward("A new Generator upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 1,
            new HighlightTransform(-757f, 74.7f, 406, 88, 0))));

        s_objectives.Add(new MakeMergesObjective("Make a total of 4 merges", 4,
            new ObjectiveReward("You have gained a seventh column on the grid", ObjectiveRewardType.NumberColumnsBonus, 1,
            new HighlightTransform(366f, 1f, 276, 60, 90))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 5 item", 5,
            new ObjectiveReward("A new Generator upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 2,
            new HighlightTransform(-757f, -6.3f, 406, 88, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 3 item", 3,
            new ObjectiveReward("You have gained a sixth row on the grid", ObjectiveRewardType.NumberRowsBonus, 2,
            new HighlightTransform(204f, -135f, 388, 60, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 5 item", 5,
            new ObjectiveReward("A new Auto-Merge upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 3,
            new HighlightTransform(-757f, -413.7f, 406, 88, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 3 item", 3,
            new ObjectiveReward("You have gained an eighth column on the grid", ObjectiveRewardType.NumberColumnsBonus, 2,
            new HighlightTransform(392f, 1f, 334, 60, 90))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 5 item", 5,
            new ObjectiveReward("A new Generator upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 4,
            new HighlightTransform(-757f, -88.3f, 406, 88, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 3 item", 3,
            new ObjectiveReward("You have gained a seventh row on the grid", ObjectiveRewardType.NumberRowsBonus, 3,
            new HighlightTransform(204f, -162, 444, 60, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 5 item", 5,
            new ObjectiveReward("The last standard upgrade has been unlocked.\r\n\r\nPrestiege reset is now available.", ObjectiveRewardType.UnlockStandardUpgrade, 5,
            new HighlightTransform(-757f, -495.7f, 406, 88, 0))));

        s_objectives.Add(new PlaceHolderObjective("None currently remain",
            new ObjectiveReward("Placeholder", ObjectiveRewardType.None, 0,
            new HighlightTransform())));

        UpgradeManager.DebugMultiplier = 100;
    }

    private void UseRealObjectives()
    {
        s_objectives.Add(new BestItemObjective("Merge into a Tier 5 item", 5,
            new ObjectiveReward("You have gained a fifth row on the grid", ObjectiveRewardType.NumberRowsBonus, 1,
            new HighlightTransform(207f, -143f, 447, 84, 0))));

        s_objectives.Add(new CreditsTotalObjective("Earn a total of 25K credits", new AlphabeticNotation(25, 1),
            new ObjectiveReward("A new Generator upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 1,
            new HighlightTransform(-432.5f, 47.7f, 415, 70, 0))));

        s_objectives.Add(new MakeMergesObjective("Make a total of 75 merges", 75,
            new ObjectiveReward("You have gained a seventh column on the grid", ObjectiveRewardType.NumberColumnsBonus, 1,
            new HighlightTransform(422.5f, 0.26f, 372, 84, 90))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 10 item", 10,
            new ObjectiveReward("A new Generator upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 2,
            new HighlightTransform(-432.5f, -6.3f, 415, 70, 0))));

        s_objectives.Add(new CreditsTotalObjective("Earn a total of 5M credits", new AlphabeticNotation(5, 2),
            new ObjectiveReward("You have gained a sixth row on the grid", ObjectiveRewardType.NumberRowsBonus, 2,
            new HighlightTransform(207.5f, -178.2f, 515, 84, 0))));

        s_objectives.Add(new MakeMergesObjective("Make a total of 250 merges", 250,
            new ObjectiveReward("A new Auto-Merge upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 3,
            new HighlightTransform(-432.5f, -276.5f, 415, 70, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 15 item", 15,
            new ObjectiveReward("You have gained an eighth column on the grid", ObjectiveRewardType.NumberColumnsBonus, 2,
            new HighlightTransform(459f, 0.8f, 442, 82, 90))));

        s_objectives.Add(new CreditsTotalObjective("Earn a total of 5B credits", new AlphabeticNotation(5, 3),
            new ObjectiveReward("A new Generator upgrade has been unlocked", ObjectiveRewardType.UnlockStandardUpgrade, 4,
            new HighlightTransform(-432.5f, -60.5f, 415, 70, 0))));

        s_objectives.Add(new MakeMergesObjective("Make a total of 1000 merges", 1000,
            new ObjectiveReward("You have gained a seventh row on the grid", ObjectiveRewardType.UnlockStandardUpgrade, 5,
            new HighlightTransform(206.9f, -178.3f, 588, 84, 0))));

        s_objectives.Add(new BestItemObjective("Merge into a Tier 20 item", 20,
            new ObjectiveReward("Prestiege mode is now available.", ObjectiveRewardType.UnlockPrestiege, 1,
            new HighlightTransform(-432.5f, -329f, 415, 62, 0))));

        s_objectives.Add(new PlaceHolderObjective("None currently remain",
            new ObjectiveReward("Placeholder", ObjectiveRewardType.None, 0,
            new HighlightTransform())));
    }

    public static void ToggleCompletionButton(bool setInteractable)
    {
        s_objectiveCompleteButton.interactable = setInteractable;
        s_objectiveCompleteButtonText.text = setInteractable ? "Complete": "In\r\nProgress";
        s_objectiveBackgroundImage.color = setInteractable ? s_objectiveForegroundColorReadyToComplete: s_objectiveBackgroundColorInProgress;
    }

    public static void UpdateObjectiveProgress()
    {
        s_objectiveText.text = s_activeObjective.GetFullDescription();

        if (s_objectiveCompleteButton.interactable == true)
        {
            return;
        }

        switch (s_activeObjective)
        {
            case BestItemObjective objective:
                objective.CheckCompletion(AchievementManager.BestItemTierEver);
                break;
            case MakeMergesObjective objective:
                objective.CheckCompletion(AchievementManager.TotalMergesEver);
                break;
            case CreditsTotalObjective objective:
                objective.CheckCompletion(AchievementManager.TotalCreditsEver);
                break;
        }
    }

    private static void SetActiveObjective()
    {
        // Ensure there's a next objective, then set it active
        if (s_objectives.Count > ActiveObjectiveIndex)
        {
            s_activeObjective = s_objectives[ActiveObjectiveIndex];
        }
        else
        {
            // If we've overrun the existing objectives, set us back to the last one. This should just be an uncompletelable message about more to come
            ActiveObjectiveIndex = s_objectives.Count;
        }

        ToggleCompletionButton(false);
        s_objectiveText.text = s_activeObjective.GetFullDescription();
    }

    public void ObjectiveCompleteButtonClick()
    {
        GameObject msgBox = Instantiate(_rewardPopup, _canvas.transform);
        msgBox.GetComponentInChildren<TextMeshProUGUI>().text = $"Objective Completed!\r\n\r\n{s_activeObjective.Reward.Description}";
        msgBox.GetComponentInChildren<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        msgBox.GetComponentInChildren<Button>().onClick.AddListener(delegate { RewardDismissButtonClick(msgBox); });

        ShowObjectiveUI(false);
    }

    public void WarningDismissButtonClick(GameObject msgBox)
    {
        Destroy(msgBox);
    }

    public void RewardDismissButtonClick(GameObject msgBox)
    {
        Destroy(msgBox);

        // Display the highlighting for the newly unlocked UI. Do this before advancing to the next objective.
        DisplayHighlight(s_activeObjective.Reward.HighlightTransform);

        // Unhide the UI that we hid to make it look clean.
        ShowObjectiveUI(true);

        // Apply the reward changes, after setting the active objective to the next one. This matters since this index is used to track what is unlocked.
        ActiveObjectiveIndex++;
        UnlockObjectiveRewards();

        GridManager.RegenerateGrid();
    }

    public static void UnlockObjectiveRewards()
    {
        UpgradeManager.ResetEarnedUnlocks();
        UpgradeManager.ApplyEarnedUpgrades(s_objectives, s_activeObjectiveIndex);

        // Prestiege mode unlocking
        if (s_activeObjectiveIndex + 1 >= s_objectives.Count)
        {
            PrestiegeSkillTreeManager.PrestiegeModeUnlocked = true;
        }
    }

    private void ShowObjectiveUI(bool show)
    {
        _objectiveText.gameObject.SetActive(show);
        _objectiveCompleteButton.gameObject.SetActive(show);
    }

    private void DisplayHighlight(HighlightTransform hlt)
    {
        GameObject highlight = Instantiate(_highlightPrefab, new Vector3(hlt.AnchorX, hlt.AnchorY, 0), Quaternion.Euler(0,0, hlt.RotationZ), _canvas.transform);
        highlight.GetComponent<RectTransform>().sizeDelta = new Vector2(hlt.Width, hlt.Height);

        // display a flashing highlight over the newly unlocked upgrade for 5 seconds
        float lowestAlpha = 0.5f;
        float highestAlpha = 1f;
        float cycleDelay = 0.5f;
        for (int i = 0; i < 5; i++)
        {
            LeanTween.alpha(highlight.GetComponent<RectTransform>(), lowestAlpha, cycleDelay).setDelay(i);
            LeanTween.alpha(highlight.GetComponent<RectTransform>(), highestAlpha, cycleDelay).setDelay(i + cycleDelay);
        }
        LeanTween.alpha(highlight.GetComponent<RectTransform>(), 0f, 2 * cycleDelay).setDelay(5);
        //Destroy(highlight);
    }
}

public struct HighlightTransform
{
    public HighlightTransform(float anchorX, float anchorY, int width, int height, int rotationZ)
    {
        AnchorX = anchorX;
        AnchorY = anchorY;
        Width = width;
        Height = height;
        RotationZ = rotationZ;
    }

    public float AnchorX { get; }
    public float AnchorY { get; }
    public int Width { get; }
    public int Height { get; }
    public int RotationZ { get; }
}

public abstract class Objective
{
    public string TargetDescription { get; set; }
    public ObjectiveReward Reward { get; set; }

    public abstract string GetFullDescription();
}

public class BestItemObjective : Objective
{
    public int TargetTier { get; set; }

    public BestItemObjective(string description, int targetTier, ObjectiveReward reward)
    {
        TargetDescription = description;
        TargetTier = targetTier;
        Reward = reward;
    }

    public override string GetFullDescription()
    {
        return $"Goal: {TargetDescription}\r\nCurrent: {Mathf.Min(AchievementManager.BestItemTierEver, TargetTier)}/{TargetTier}";
    }

    public void CheckCompletion(int currentBestTier)
    {
        if (currentBestTier >= TargetTier)
        {
            ObjectiveManager.ToggleCompletionButton(true);
        }
    }
}

public class MakeMergesObjective : Objective
{
    public int TargetMerges { get; set; }

    public MakeMergesObjective(string description, int targetMerges, ObjectiveReward reward)
    {
        TargetDescription = description;
        TargetMerges = targetMerges;
        Reward = reward;
    }

    public override string GetFullDescription()
    {
        return $"Goal: {TargetDescription}\r\nCurrent: {Mathf.Min(AchievementManager.TotalMergesEver, TargetMerges)}/{TargetMerges}";
    }

    public void CheckCompletion(long currentMergesMade)
    {
        if (currentMergesMade >= TargetMerges)
        {
            ObjectiveManager.ToggleCompletionButton(true);
        }
    }
}

public class CreditsTotalObjective : Objective
{
    public AlphabeticNotation TargetCreditsTotal { get; set; }

    public CreditsTotalObjective(string description, AlphabeticNotation targetCreditsTotal, ObjectiveReward reward)
    {
        TargetDescription = description;
        TargetCreditsTotal = targetCreditsTotal;
        Reward = reward;
    }

    public override string GetFullDescription()
    {
        // Prevent showing a larger number on the left side, it's more satisfying to see the numbers match.
        AlphabeticNotation credits = AchievementManager.TotalCreditsEver;
        if (credits > TargetCreditsTotal)
        {
            credits = TargetCreditsTotal;
        }

        return $"Goal: {TargetDescription}\r\nCurrent: {credits}/{TargetCreditsTotal}";
    }

    public void CheckCompletion(AlphabeticNotation amount)
    {
        if (amount >= TargetCreditsTotal)
        {
            ObjectiveManager.ToggleCompletionButton(true);
        }
    }
}

public class PlaceHolderObjective : Objective
{
    public AlphabeticNotation TargetCredits { get; set; }

    public PlaceHolderObjective(string description, ObjectiveReward reward)
    {
        TargetDescription = description;
        Reward = reward;
    }

    public override string GetFullDescription()
    {
        return $"Goal: {TargetDescription}\r\n";
    }

    public void CheckCompletion(AlphabeticNotation amount)
    {
        // Maybe make it so the button can be clicked to open a web page for more information on game updates.
    }
}

public class ObjectiveReward
{
    public string Description { get; set; }
    public ObjectiveRewardType Type { get; set; }
    public int Value { get; set; }
    public HighlightTransform HighlightTransform { get; set; }

    public ObjectiveReward(string description, ObjectiveRewardType type, int value, HighlightTransform hlt)
    {
        Description = description;
        Type = type;
        Value = value;
        HighlightTransform = hlt;
    }
}

public enum ObjectiveRewardType
{
    None = 0,
    NumberRowsBonus = 1,
    NumberColumnsBonus = 2,
    UnlockStandardUpgrade = 3,
    UnlockTokenUpgrade = 4,
    UnlockMarkerUpgrade = 5,
    UnlockPrestiege= 6
}
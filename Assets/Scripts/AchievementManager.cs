using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LargeNumbers;

public class AchievementManager : MonoBehaviour
{
    public static int BestItemTierEver { get; set; }
    public static long TotalMergesEver { get; set; }
    public static AlphabeticNotation TotalCreditsEver
    {
        get { return s_totalCreditsEver; }
        set
        {
            s_totalCreditsEver = value;
            Instance.TotalCreditsTracker.Refresh(s_totalCreditsEver);
        }
    }
    private static AlphabeticNotation s_totalCreditsEver;

    public static long SpecialItemsMergedTotal { get; set; }

    [SerializeField] private GameObject _highestItemTierUI;
    [SerializeField] private GameObject _totalMergesUI;
    [SerializeField] private GameObject _totalCreditsUI;
    [SerializeField] private GameObject _specialItemsMergedUI;
    [SerializeField] private GameObject _prestiegeSkillPointsUI;

    public AchievementTracker HighestItemTierTracker;
    public AchievementTracker TotalMergesTracker;
    public AchievementTracker TotalCreditsTracker;
    public AchievementTracker SpecialItemsMergedTracker;
    public AchievementTracker PrestiegeSkillPointsTracker;

    public static AchievementManager Instance;

    public void Awake()
    {
        Instance = this;

        HighestItemTierTracker = new AchievementTracker(_highestItemTierUI, new AchievementSeries(new long[6] { 20, 30, 40, 50, 60, 70 }));
        TotalMergesTracker = new AchievementTracker(_totalMergesUI, new AchievementSeries(new long[5] { 2000, 4000, 8000, 20000, 40000 }));
        TotalCreditsTracker = new AchievementTracker(_totalCreditsUI, new AchievementSeries(
            new AlphabeticNotation[5]
            {
                                        new AlphabeticNotation(1, 3),
                                        new AlphabeticNotation(1, 4),
                                        new AlphabeticNotation(1, 5),
                                        new AlphabeticNotation(1, 6),
                                        new AlphabeticNotation(1, 7)
            }));
        SpecialItemsMergedTracker = new AchievementTracker(_specialItemsMergedUI, new AchievementSeries(new long[4] { 10, 25, 75, 150 }));
        PrestiegeSkillPointsTracker = new AchievementTracker(_prestiegeSkillPointsUI, new AchievementSeries(new long[5] { 10, 50, 150, 250, 500 }));
    }

    public static void UpdateItemTrackers(int itemTier)
    {

        if (itemTier > BestItemTierEver)
        {
            BestItemTierEver = itemTier;
            Instance.HighestItemTierTracker.Refresh(BestItemTierEver);
        }

        TotalMergesEver++;
        Instance.TotalMergesTracker.Refresh(TotalMergesEver);
    }

    public static void UpdateSpecialItemTrackers()
    {
        TotalMergesEver++;
        SpecialItemsMergedTotal++;
        Instance.SpecialItemsMergedTracker.Refresh(SpecialItemsMergedTotal);
    }

    public static void RefreshAchievementUI()
    {
        Instance.HighestItemTierTracker.Refresh(BestItemTierEver);
        Instance.TotalMergesTracker.Refresh(TotalMergesEver);
        Instance.TotalCreditsTracker.Refresh(s_totalCreditsEver);
        Instance.SpecialItemsMergedTracker.Refresh(SpecialItemsMergedTotal);
        Instance.PrestiegeSkillPointsTracker.Refresh(PrestiegeSkillTreeManager.AvailableSkillPoints + PrestiegeSkillTreeManager.SpentSkillPoints);
    }

    public static void DeserializeData(SerializeableAchievementData data)
    {
        BestItemTierEver = data.BestItemTierEver;
        TotalMergesEver = data.TotalMergesEver;
        TotalCreditsEver = new AlphabeticNotation(data.TotalCreditsEverCoefficient, data.TotalCreditsEverMagnitude);
        SpecialItemsMergedTotal = data.SpecialItemsMergedTotal;
    }

    public static void HardReset()
    {
        BestItemTierEver = 0;
        TotalMergesEver = 0;
        TotalCreditsEver = new AlphabeticNotation();
        SpecialItemsMergedTotal = 0;
    }
}

public class AchievementTracker
{
    public GameObject Tracker { get; set; }
    private Slider _slider;
    private TextMeshProUGUI _progressText;
    private Image[] _starImages;

    public AchievementSeries Series { get; set; }

    private static Color s_starCompleteColor = new Color(0.9921569f, 0.7960785f, 0.4313726f);

    public AchievementTracker(GameObject tracker, AchievementSeries series)
    {
        Tracker = tracker;
        Series = series;

        _slider = Tracker.GetComponentInChildren<Slider>();
        _progressText = _slider.transform.Find("ProgressText").gameObject.GetComponent<TextMeshProUGUI>();

        GameObject starParent = Tracker.transform.Find("StarDisplayParent").gameObject;
        _starImages = starParent.GetComponentsInChildren<Image>();
    }

    public void Refresh(long currentValue)
    {
        Refresh(new AlphabeticNotation(currentValue));
    }

    public void Refresh(AlphabeticNotation currentValue)
    {
        AlphabeticNotation nextThreshold = Series.GetThresholdData(currentValue, out int starsEarned);
        _slider.value = (float)(currentValue / nextThreshold);

        if (currentValue >= Series.FinalValue)
        {
            _progressText.text = "Complete!";
        }
        else
        {
            _progressText.text = $"Progress: {currentValue}/{nextThreshold}";
        }

        foreach (var star in _starImages)
        {
            if (starsEarned <= 0)
            {
                star.color = Color.white;
            }
            else
            {
                star.color = s_starCompleteColor;
            }

            starsEarned--;
        }
    }
}

public struct AchievementSeries
{
    public AlphabeticNotation[] Thresholds { get; set; }
    public AlphabeticNotation FinalValue { get; set; }

    public AchievementSeries(AlphabeticNotation[] thresholds)
    {
        Thresholds = thresholds;
        FinalValue = thresholds[thresholds.Length - 1];
    }

    public AchievementSeries(long[] thresholds)
    {
        Thresholds = new AlphabeticNotation[thresholds.Length];
        
        for (int i = 0; i < thresholds.Length; i++)
        {
            Thresholds[i] = new AlphabeticNotation(thresholds[i]);

        }

        FinalValue = new AlphabeticNotation(thresholds[thresholds.Length - 1]);
    }

    public AlphabeticNotation GetThresholdData(AlphabeticNotation value, out int thresholdsMet)
    {
        thresholdsMet = 0;

        foreach (var threshold in Thresholds)
        {
            // Check each threshold, starting with the lowest, returning the first one to be higher than current progress.
            if (threshold > value)
            {
                return threshold;
            }
            thresholdsMet++;
        }

        // If the player has exceeded all achievement thresholds, just use the last one.
        return FinalValue;
    }
}

[Serializable]
public class SerializeableAchievementData
{
    public int BestItemTierEver { get; set; }
    public long TotalMergesEver { get; set; }
    public double TotalCreditsEverCoefficient { get; set; }
    public int TotalCreditsEverMagnitude { get; set; }
    public long SpecialItemsMergedTotal { get; set; }

    public SerializeableAchievementData()
    {
        BestItemTierEver = AchievementManager.BestItemTierEver;
        TotalMergesEver = AchievementManager.TotalMergesEver;
        TotalCreditsEverCoefficient = AchievementManager.TotalCreditsEver.coefficient;
        TotalCreditsEverMagnitude = AchievementManager.TotalCreditsEver.magnitude;
        SpecialItemsMergedTotal = AchievementManager.SpecialItemsMergedTotal;
    }
}
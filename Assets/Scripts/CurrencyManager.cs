using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LargeNumbers;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currecyAmountText;
    private static TextMeshProUGUI s_currecyAmountText;

    [SerializeField] private TextMeshProUGUI _currecyGainText;
    private static TextMeshProUGUI s_currecyGainText;

    [SerializeField] private TextMeshProUGUI _currecyBonusText;
    private static TextMeshProUGUI s_currecyBonusText;

    // Bonus values for special items
    private float _specialItemBonusMulti = 1f;
    private int _specialItemBonusDuration = 0;

    public static AlphabeticNotation Amount { get; set; }

    public static bool Collecting { get; set; }

    public static CurrencyManager Instance;

    private void Awake()
    {
        Instance = this;

        s_currecyAmountText = _currecyAmountText;
        s_currecyGainText = _currecyGainText;
        s_currecyBonusText = _currecyBonusText;
    }

    private void Start()
    {
        Collecting = true;
        InvokeRepeating("Tick", 0.1f, 1f);

        s_currecyBonusText.gameObject.SetActive(false);
    }

    private void Tick()
    {
        if (Collecting)
        {
            AlphabeticNotation gain = GridManager.TotalItemOutput() * UpgradeManager.DebugMultiplier * PrestiegeSkillTreeManager.GetEarnedSkillPointCreditsBonus() * (1 + PrestiegeSkillTreeManager.Instance.CreditsMultiplier.GetBonusPercent()) * _specialItemBonusMulti;
            s_currecyGainText.text = $"+ {StringHelper.FormatCurrency(gain)} /s";

            Amount += gain;
            AchievementManager.TotalCreditsEver += gain;
            PrestiegeSkillTreeManager.UpdateSkillPointReward(gain);

            Refresh();
        }
    }

    public static void Spend(double amountToSpend)
    {
        Amount -= amountToSpend;

        Refresh();
    }

    private static void Refresh()
    {
        s_currecyAmountText.text = StringHelper.FormatCurrency(Amount);

        ObjectiveManager.UpdateObjectiveProgress();
        UpgradeManager.RefreshUpgradeButtonsClickableStatus();
    }

    public static void ResetValue()
    {
        Amount = new AlphabeticNotation(0, 0);
    }

    public void StartSpecialItemBonus(int tier)
    {
        switch (tier)
        {
            case 1:
                _specialItemBonusMulti = 2f;
                _specialItemBonusDuration = 30;
                break;
            case 2:
                _specialItemBonusMulti = 4f;
                _specialItemBonusDuration = 40;
                break;
            case 3:
                _specialItemBonusMulti = 8f;
                _specialItemBonusDuration = 50;
                break;
            case 4:
                _specialItemBonusMulti = 16f;
                _specialItemBonusDuration = 65;
                break;
            case 5:
                _specialItemBonusMulti = 32f;
                _specialItemBonusDuration = 80;
                break;
        }

        s_currecyBonusText.gameObject.SetActive(true);
        CancelInvoke("BonusCountdown");
        InvokeRepeating("BonusCountdown", 0, 1);
    }

    private void BonusCountdown()
    {
        if (_specialItemBonusDuration <= 0)
        {
            _specialItemBonusMulti = 1f;
            s_currecyBonusText.gameObject.SetActive(false);
            CancelInvoke("BonusCountdown");
        }
        else
        {
            _specialItemBonusDuration--;
            s_currecyBonusText.text = $"({_specialItemBonusMulti}x {StringHelper.FormatTime(_specialItemBonusDuration)})";
        }
    }

    public void ResetBonus()
    {
        _specialItemBonusMulti = 1f;
        _specialItemBonusDuration = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoMerger : MonoBehaviour
{
    [SerializeField] private Slider _progressBar;
    private float _barBaseMoveSpeed = 0.111f;

    [SerializeField] private TextMeshProUGUI _bonusText;

    [SerializeField] private Transform _gridParent;

    public static bool Running;

    private float _specialItemBonusMulti = 1f;
    private int _specialItemBonusDuration = 0;

    private Image _backgroundImage;
    private Color _backgroundNormalColor = new Color(0.1764706f, 0.2039216f, 0.2117647f);
    private Color _backgroundBonusColor = new Color(0.6980392f, 0.7450981f, 0.764706f);

    public static AutoMerger Instance;

    private void Awake()
    {
        Instance = this;
        _backgroundImage = GetComponent<Image>();
    }

    void Start()
    {
        _progressBar.value = 0;
        Running = true;
        _bonusText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!Running)
        {
            _progressBar.value = 0;
        }
        if (_progressBar.value >= 1)
        {
            AutomaticMerge();
            _progressBar.value = 0;
        }
        else
        {
            _progressBar.value += _barBaseMoveSpeed * Time.deltaTime * (UpgradeManager.AutomergeSpeed.GetBonus() + PrestiegeSkillTreeManager.Instance.AutoMergerSpeed.GetBonusPercent()) * _specialItemBonusMulti;
        }
    }

    private void AutomaticMerge()
    {
        int mergesToMake = 1;

        // find out how many merges to make
        if (Random.Range(1f, 2f) < UpgradeManager.AutomergeThrice.GetBonus())
        {
            mergesToMake = 3;
        }
        else if (Random.Range(1f, 2f) < UpgradeManager.AutomergeTwice.GetBonus())
        {
            mergesToMake = 2;
        }

        List<ItemPair> merges = GridManager.GetItemsToMerge(mergesToMake, Item.BestItemTierThisRun);

        Debug.Log($"Performing {merges.Count} merge(s), total automatic merges triggered was {mergesToMake}");

        foreach (ItemPair pair in merges)
        {
            // create a copy of the merging item for animation purposes only
            Item mergeGhost = Instantiate(pair.ItemTwo, pair.ItemTwo.transform.position, Quaternion.identity, _gridParent);
            mergeGhost.name = "mergeGhost";
            mergeGhost.PlayMergeAnimation(pair.ItemOne.transform.localPosition, 0.2f);

            pair.ItemOne.MergeUpgrade();
            pair.ItemOne.UpdateItem();
            pair.ItemOne.PlayUpgradeAnimation();

            Destroy(pair.ItemTwo.gameObject);
        }
    }

    public void StartSpecialItemBonus(int tier)
    {
        switch (tier)
        {
            case 1:
                _specialItemBonusMulti = 3f;
                _specialItemBonusDuration = 30;
                break;
            case 2:
                _specialItemBonusMulti = 4f;
                _specialItemBonusDuration = 60;
                break;
            case 3:
                _specialItemBonusMulti = 5f;
                _specialItemBonusDuration = 120;
                break;
            case 4:
                _specialItemBonusMulti = 6f;
                _specialItemBonusDuration = 240;
                break;
            case 5:
                _specialItemBonusMulti = 8f;
                _specialItemBonusDuration = 480;
                break;
        }

        _bonusText.gameObject.SetActive(true);
        //_backgroundImage.color = _backgroundBonusColor;
        CancelInvoke();
        InvokeRepeating("BonusCountdown", 0, 1);
    }

    private void BonusCountdown()
    {
        if (_specialItemBonusDuration <= 0)
        {
            _specialItemBonusMulti = 1f;
            _bonusText.gameObject.SetActive(false);
            //_backgroundImage.color = _backgroundNormalColor;
            CancelInvoke();
        }
        else
        {
            _specialItemBonusDuration--;
            _bonusText.text = $"{_specialItemBonusMulti}x Bonus ({StringHelper.FormatTime(_specialItemBonusDuration)})";
        }
    }

    public void ResetBonus()
    {
        _specialItemBonusMulti = 1f;
        _specialItemBonusDuration = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Generator : MonoBehaviour
{
    [SerializeField] private Slider _progressBar;
    private float _barBaseMoveSpeed = 0.208f;

    [SerializeField] private TextMeshProUGUI _bonusText;

    [SerializeField] private Item _itemPrefab;
    [SerializeField] private Transform _gridParent;

    [SerializeField] private Canvas _draggingCanvas;

    public static bool Running;

    private float _baseSpecialItemChance = 0.025f;

    private float _specialItemBonusMulti = 1f;
    private int _specialItemBonusDuration = 0;

    private Image _backgroundImage;
    private Color _backgroundNormalColor = new Color(0.1764706f, 0.2039216f, 0.2117647f);
    private Color _backgroundBonusColor = new Color(0.6980392f, 0.7450981f, 0.764706f);

    public static Generator Instance;

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
        else if (_progressBar.value >= 1)
        {
            PlaceNewItems();
            _progressBar.value = 0;
        }
        else
        {
            _progressBar.value += _barBaseMoveSpeed * Time.deltaTime * (UpgradeManager.GeneratorSpeed.GetBonus() + PrestiegeSkillTreeManager.Instance.GeneratorSpeed.GetBonusPercent()) * _specialItemBonusMulti;
        }
    }

    private void PlaceNewItems()
    {
        int itemsToPlace = 1;

        // Triple item generation from prestiege tree, bonus stored as percentages ready to be added, so a 5% double chance is 0.05
        if (Random.Range(0f, 1f) < PrestiegeSkillTreeManager.Instance.AddChanceForGenThrice.GetBonusPercent())
        {
            itemsToPlace += 2;

        }
        // upgrades bonuses are stored as percentages ready to be multiplied, so a 5% double chance is 1.05
        else if (Random.Range(1f, 2f) < UpgradeManager.GeneratorDouble.GetBonus())
        {
            // double item bonus succeeded
            itemsToPlace++;
        }

        var sb = new StringBuilder();
        sb.Append($"Generating {itemsToPlace} item(s) -- ");

        for (int i = 0; i < itemsToPlace; i++)
        {
            Tile openTile = GridManager.GetOpenTile();

            if (openTile == null)
            {
                sb.Append("Grid is full! -- ");
            }
            else
            {
                sb.Append(openTile.name + " is available -- ");

                

                var spawnedItem = Instantiate(_itemPrefab, _gridParent);

                // Handle chance for special item to spawn
                if (Random.Range(0f, 1f) < _baseSpecialItemChance * (1 + PrestiegeSkillTreeManager.Instance.BonusItemSpawnChanceMultiplier.GetBonusPercent()))
                {
                    // A Special Item has been spawned, determine which type
                    int specialItemRoll = Random.Range(1, 5);

                    // Create a 2x chance for the special item to be a merge bonus, for design reasons.
                    specialItemRoll = specialItemRoll == 4 ? 2 : specialItemRoll;

                    spawnedItem.SpecialType = (SpecialItemType)specialItemRoll;
                    sb.Append($"Special Item roll succeeded -- Special Type is {spawnedItem.SpecialType}");

                    if (Random.Range(0f, 1f) < PrestiegeSkillTreeManager.Instance.BonusItemSpawnOneHigherChance.GetBonusPercent())
                    {
                        spawnedItem.Tier = 2;
                    }
                    else
                    {
                        spawnedItem.Tier = 1;
                    }
                }
                else
                {
                    // A Normal Item has been spawned
                    spawnedItem.Tier = Item.GetMinTier();

                    // Handle item spawning at a higher tier from bonuses
                    if (Random.Range(0f, 1f) < PrestiegeSkillTreeManager.Instance.AddChanceForGenPlusThree.GetBonusPercent())
                    {
                        spawnedItem.Tier += 3;
                        sb.Append($"Item spawned at +3 bonus levels, item level is {spawnedItem.Tier}    ");
                    }
                    if (Random.Range(1f, 2f) < UpgradeManager.GeneratorUpTwo.GetBonus())
                    {
                        spawnedItem.Tier += 2;
                        sb.Append($"Item spawned at +2 bonus levels, item level is {spawnedItem.Tier}    ");
                    }
                    else if (Random.Range(1f, 2f) < UpgradeManager.GeneratorUpOne.GetBonus())
                    {
                        spawnedItem.Tier++;
                        sb.Append($"Item spawned at +1 bonus levels, item level is {spawnedItem.Tier}    ");
                    }
                    else
                    {
                        sb.Append($"Item spawned with no bonus levels, item level is {spawnedItem.Tier}    ");
                    }
                }
                

                spawnedItem.transform.position = openTile.transform.position;
                //spawnedItem.name = $"Item.tier-{spawnedItem.Tier}.{openTile.name}";

                spawnedItem.TileLandlord = openTile;
                openTile.ItemTenent = spawnedItem;

                spawnedItem.SetCanvasRefs(_draggingCanvas);

                spawnedItem.UpdateItem();
                spawnedItem.PlaySpawnAnimation();
            }
        }

        Debug.Log(sb.ToString());
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
                _specialItemBonusMulti = 2.5f;
                _specialItemBonusDuration = 60;
                break;
            case 3:
                _specialItemBonusMulti = 3f;
                _specialItemBonusDuration = 120;
                break;
            case 4:
                _specialItemBonusMulti = 3.5f;
                _specialItemBonusDuration = 240;
                break;
            case 5:
                _specialItemBonusMulti = 4f;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LargeNumbers;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject _okOnlyMessageBox;
    [SerializeField] private GameObject _okCancelMessageBox;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _menuPanelOverlay;
    [SerializeField] private GameObject _infoPanelOverlay;
    [SerializeField] private GameObject _prestiegeTree;
    [SerializeField] private GameObject _achievementsPanel;

    private void Start()
    {
        _prestiegeTree.SetActive(false);
        _menuPanelOverlay.SetActive(false);
    }

    public void MenuButtonClick()
    {
        if (_menuPanelOverlay.activeSelf)
        {
            CloseOverlays();
        }
        else
        {
            CloseOverlays();
            _menuPanelOverlay.SetActive(true);
        }
    }

    public void InfoButtonClick()
    {
        if (_infoPanelOverlay.activeSelf)
        {
            CloseOverlays();
        }
        else
        {
            CloseOverlays();
            _infoPanelOverlay.SetActive(true);
        }
    }

    public void AchievementsButtonClick()
    {
        if (_achievementsPanel.activeSelf)
        {
            CloseOverlays();
        }
        else
        {
            CloseOverlays();
            _achievementsPanel.SetActive(true);
            AchievementManager.RefreshAchievementUI();
        } 
    }

    public void CloseOverlays()
    {
        _menuPanelOverlay.SetActive(false);
        _infoPanelOverlay.SetActive(false);
        _achievementsPanel.SetActive(false);
    }

    public void LoadSaveDataButtonClick()
    {
        SaveDataHandler.LoadSaveGame();
    }

    public void ForceSaveDataButtonClick()
    {
        SaveDataHandler.SaveGame();
    }

    public void HardResetButtonClick()
    {
        GameObject msgBox = Instantiate(_okCancelMessageBox, _canvas.transform);
        msgBox.GetComponentInChildren<TextMeshProUGUI>().text = "Are you sure you want to permanently delete your data?\r\n\r\nThis action cannot be undone.";

        Button okButton = msgBox.transform.Find("OKButton_Find").gameObject.GetComponent<Button>();
        okButton.GetComponentInChildren<TextMeshProUGUI>().text = "Yes, I'm Sure";
        okButton.onClick.AddListener(delegate { HardReset(msgBox); });

        Button cancelButton = msgBox.transform.Find("CancelButton_Find").gameObject.GetComponent<Button>();
        cancelButton.onClick.AddListener(delegate { DismissMessagebox(msgBox); });
    }

    private void HardReset(GameObject msgBox)
    {
        // Currency Reset
        CurrencyManager.Amount = new AlphabeticNotation();

        // Objective Progress Reset
        ObjectiveManager.ActiveObjectiveIndex = 0;

        // Purchased Upgrade Levels Reset, Unlocks Reset
        UpgradeManager.ResetUpgradeLevels();
        UpgradeManager.ResetEarnedUnlocks();

        // Prestiege Skills, before grid is reset to make sure the size is correct.
        PrestiegeSkillTreeManager.Instance.HardReset();

        // Item/Tile Data
        GridManager.DestroyGrid();
        GridManager.GenerateGrid();

        // Achievement data goes last
        AchievementManager.HardReset();

        SaveDataHandler.SaveGame();
        Debug.Log("Save data wiped! Game has been Hard Reset.");

        Destroy(msgBox);
    }

    public void PrestiegeButtonClick()
    {
        if (!PrestiegeSkillTreeManager.PrestiegeModeUnlocked)
        {
            GameObject msgBox = Instantiate(_okOnlyMessageBox, _canvas.transform);
            msgBox.GetComponentInChildren<TextMeshProUGUI>().text = "Prestiege Skills are currently locked.\r\n\r\nFinish all basic goals to unlock this feature.";
            msgBox.GetComponentInChildren<Button>().onClick.AddListener(delegate { DismissMessagebox(msgBox); });
        }
        else if (!_prestiegeTree.gameObject.activeSelf)
        {
            _prestiegeTree.SetActive(true);
            PrestiegeSkillTreeManager.Instance.RefreshPoints();
            PrestiegeSkillTreeManager.Instance.RefreshSkillTree();
            PrestiegeSkillTreeManager.Instance.RefreshSelectedSkillDetails();
        }
        else
        {
            _prestiegeTree.SetActive(false);
        }
    }

    private void DismissMessagebox(GameObject msgBox)
    {
        Destroy(msgBox);
    }

    public void LoadDebug1()
    {
        SaveDataHandler.LoadSaveGame("DebugData1.dat");
    }
}

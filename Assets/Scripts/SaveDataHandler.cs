using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using LargeNumbers;
using System.Runtime.InteropServices;

public class SaveDataHandler : MonoBehaviour
{
	// Javascript plugin here to allow a file sync call for the Indexed DB.
    [DllImport("__Internal")]
    private static extern void SyncFiles();

	private static string s_dataPath;

	private void Awake()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			// Specified path is needed here to allow for save data to work between builds for WebGL.
			s_dataPath = "/idbfs/auto_merge_idle";
		}
		else
		{
            s_dataPath = Application.persistentDataPath;
        }
    }

	private void Start()
	{
		Invoke("LoadSaveGame", 0.01f);
        InvokeRepeating("AutoSaveGame", 10f, 10f);
    }

	private void AutoSaveGame()
	{
        //  This exists because InvokRepeating does not allow static methods.
        SaveGame();
    }

	public static void SaveGame()
	{
		BinaryFormatter bf = new BinaryFormatter();

		if (!Directory.Exists(s_dataPath))
		{
			Directory.CreateDirectory(s_dataPath);
		}

        FileStream file = File.Create($"{s_dataPath}/SaveData.dat");
		SaveData data = new SaveData();

		// Currency
		data.CurrencyCoeffecient = CurrencyManager.Amount.coefficient;
		data.CurrencyMagnitude = CurrencyManager.Amount.magnitude;
		 
		// Objective Progress
		data.ActiveObjective = ObjectiveManager.ActiveObjectiveIndex;

		// Achievement data
		data.AchievementData = new SerializeableAchievementData();

		// AutoMerge cap
		data.AutoMergeTierMax = Item.BestItemTierThisRun;

		// Purchased Upgrade Levels
		data.Upgrades = UpgradeManager.SaveUpgradeLevels();

		// Prestiege
		data.PrestiegeModeUnlocked = PrestiegeSkillTreeManager.PrestiegeModeUnlocked;
		data.AvailableSkillPoints = PrestiegeSkillTreeManager.AvailableSkillPoints;
		data.SpentSkillPoints = PrestiegeSkillTreeManager.SpentSkillPoints;
		data.SerializedPrestiegeSkills = PrestiegeSkillTreeManager.Instance.SerializeSkillTree();
		data.CreditsProducedThisRunCoefficient = PrestiegeSkillTreeManager.CreditsProducedThisRun.coefficient;
		data.CreditsProducedThisRunMagnitude = PrestiegeSkillTreeManager.CreditsProducedThisRun.magnitude;

		// Item/Tile Data
		data.Tiles = GridManager.GetSerializedGridData();

		bf.Serialize(file, data);
		file.Close();

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			SyncFiles();
		}

		Debug.Log($"Data saved! ({s_dataPath}/SaveData.dat)");
	}

	private void LoadSaveGame()
	{
		// This exists because Invoke does not allow static methods.
		LoadSaveGame("SaveData.dat");
	}

	public static void LoadSaveGame(string dataFile = "SaveData.dat")
	{
		if (File.Exists($"{s_dataPath}/{dataFile}"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open($"{s_dataPath}/{dataFile}", FileMode.Open);
			SaveData data = (SaveData)bf.Deserialize(file);
			file.Close();

			// Remove Active Bonuses
			CurrencyManager.Instance.ResetBonus();
			Generator.Instance.ResetBonus();
			AutoMerger.Instance.ResetBonus();
			
			// Currency
			CurrencyManager.Amount = new AlphabeticNotation(data.CurrencyCoeffecient, data.CurrencyMagnitude);

			// Objective Progress
			ObjectiveManager.ActiveObjectiveIndex = data.ActiveObjective;
			ObjectiveManager.UnlockObjectiveRewards();

			// Achievement data
			AchievementManager.DeserializeData(data.AchievementData);

			// AutoMerge cap
			Item.BestItemTierThisRun = data.AutoMergeTierMax;

			// Purchased Upgrade Levels
			UpgradeManager.LoadUpgradeLevels(data.Upgrades);

			// Prestiege
			PrestiegeSkillTreeManager.PrestiegeModeUnlocked = data.PrestiegeModeUnlocked;
			PrestiegeSkillTreeManager.AvailableSkillPoints = data.AvailableSkillPoints;
			PrestiegeSkillTreeManager.SpentSkillPoints = data.SpentSkillPoints;
			PrestiegeSkillTreeManager.Instance.DeserializeSkillTree(data.SerializedPrestiegeSkills);
			PrestiegeSkillTreeManager.Instance.RefreshPoints();
			PrestiegeSkillTreeManager.CreditsProducedThisRun = new AlphabeticNotation(data.CreditsProducedThisRunCoefficient, data.CreditsProducedThisRunMagnitude);

			// Item/Tile Data
			GridManager.DestroyGrid();
			GridManager.GenerateGridFromData(data.Tiles);

			Debug.Log("Save data loaded!");
		}
        else
        {
			Debug.LogError("There is no save data to load!");
		}
	}

	void OnApplicationQuit()
	{
		SaveGame();
		Debug.Log("Application ending after " + Time.time + " seconds");
	}
}

[Serializable]
public class SaveData
{
	// Large number stored as serializable types
	public double CurrencyCoeffecient;
	public int CurrencyMagnitude;

	public int ActiveObjective;
	public long TotalMergesThisRun;
	

	public int AutoMergeTierMax;

	public List<SerializedUpgrade> Upgrades;

	public int AvailableSkillPoints;
	public int SpentSkillPoints;
	public Dictionary<int, int> SerializedPrestiegeSkills;

	public Dictionary<SerializableVector2, SerializableTile> Tiles;

	public SerializeableAchievementData AchievementData;

	// Prestiege data
	public bool PrestiegeModeUnlocked;

	public double CreditsProducedThisRunCoefficient;
	public int CreditsProducedThisRunMagnitude;
}
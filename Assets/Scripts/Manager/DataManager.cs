using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class DataManager : MonoBehaviour {

    [Header("References")]
    private Character playerCharacter;
    private LevelManager levelManager;

    private void Start() {

        playerCharacter = FindObjectOfType<PlayerController>().GetComponent<Character>();
        levelManager = FindObjectOfType<LevelManager>();

        LoadData();

    }

    private void OnApplicationQuit() => SaveData();

    public void SaveData() {

        Debug.Log("Saving data...");

        MMGameEvent.Trigger("Save"); // save all data

        #region CHECKPOINT

        MMSaveLoadManager.Save(levelManager.CurrentCheckPoint.GetComponent<UUID>().ID, "checkpoints.dat", "Data/"); // save the current checkpoint guid

        #endregion

        Debug.Log("Data saved.");

    }

    public void LoadData() {

        Debug.Log("Loading data...");

        MMGameEvent.Trigger("Load"); // load all data

        #region CHECKPOINT

        object checkpointObj = MMSaveLoadManager.Load(typeof(string), "checkpoints.dat", "Data/");

        if (checkpointObj != null) {

            string checkpointUUID = (string) checkpointObj;

            foreach (CheckPoint checkpoint in levelManager.Checkpoints) {

                if (checkpoint.GetComponent<UUID>().ID == checkpointUUID) {

                    levelManager.SetCurrentCheckpoint(checkpoint);
                    levelManager.CurrentCheckPoint.SpawnPlayer(playerCharacter);
                    break;

                }
            }
        }

        #endregion

        Debug.Log("Data loaded.");

    }
}

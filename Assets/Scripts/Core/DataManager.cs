using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour {

    [Header("References")]
    private Character playerCharacter;
    private LevelManager levelManager;
    private GameManager gameManager;

    private void Start() {

        playerCharacter = FindObjectOfType<PlayerController>().GetComponent<Character>();
        levelManager = FindObjectOfType<LevelManager>();
        gameManager = FindObjectOfType<GameManager>();

        LoadData();

    }

    private void OnApplicationQuit() => SaveData();

    public void SaveData() {

        Debug.Log("Saving data...");

        MMGameEvent.Trigger("Save"); // save all data

        #region CHECKPOINT
        MMSaveLoadManager.Save(levelManager.CurrentCheckPoint.GetComponent<UUID>().ID, "checkpoints.dat", "Data/" + SceneManager.GetActiveScene().name); // save the current checkpoint guid
        #endregion

        Debug.Log("Data saved.");

    }

    public void LoadData() {

        Debug.Log("Loading data...");

        MMGameEvent.Trigger("Load"); // load all data

        #region CHECKPOINT
        object checkpointObj = MMSaveLoadManager.Load(typeof(string), "checkpoints.dat", "Data/" + SceneManager.GetActiveScene().name);

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

        gameManager.CheckLevelComplete(); // check if the level is complete when data is loaded

        Debug.Log("Data loaded.");

    }
}

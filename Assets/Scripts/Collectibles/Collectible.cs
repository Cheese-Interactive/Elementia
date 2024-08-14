using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System;
using UnityEngine;

public class Collectible : MMPersistentBase {

    [Header("References")]
    protected ItemPicker itemPicker;
    protected bool isCollected;
    private PlayerController playerController;
    private GameManager gameManager;

    [Header("Settings")]
    [SerializeField] private bool isRequired;

    [Header("Feedback")]
    [SerializeField] private MMF_Player onCollectFeedback;

    [Serializable]
    private struct Data {

        public bool isCollected;

    }

    protected void Awake() {

        itemPicker = GetComponent<ItemPicker>();
        gameManager = FindObjectOfType<GameManager>();
        itemPicker.OnCollect += OnCollect;

    }

    protected void Start() => playerController = FindObjectOfType<PlayerController>();

    protected void OnDestroy() => itemPicker.OnCollect -= OnCollect;

    protected virtual void OnCollect(ItemPicker collectible) {

        onCollectFeedback.PlayFeedbacks(playerController.transform.position);
        isCollected = true;
        gameManager.CheckVictory();
        gameManager.RefreshInventoryLayouts();

    }

    public override string OnSave() {

        Data saveData = new() { isCollected = isCollected };
        return JsonUtility.ToJson(saveData); // save data

    }

    public override void OnLoad(string data) {

        Data saveData = JsonUtility.FromJson<Data>(data);
        isCollected = saveData.isCollected; // load data

        // if the collectible has been collected, disable the collectible
        if (isCollected)
            itemPicker.gameObject.SetActive(false);

    }

    public bool IsCollected() => isCollected;

    public bool IsRequired() => isRequired;

}

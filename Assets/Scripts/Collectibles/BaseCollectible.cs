using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System;
using UnityEngine;

public abstract class BaseCollectible : MMPersistentBase {

    [Header("References")]
    protected WeaponSelector weaponSelector;
    protected ItemPicker itemPicker;
    protected bool isCollected;
    private GameManager gameManager;

    [Header("Settings")]
    [SerializeField] private bool isRequired;
    [SerializeField] private bool isKey;

    [Serializable]
    protected struct Data {

        public bool isCollected;

    }

    protected void Awake() {

        gameManager = FindObjectOfType<GameManager>();
        itemPicker = GetComponent<ItemPicker>();
        itemPicker.OnCollect += OnCollect;

        if (isRequired && isKey)
            Debug.LogWarning("A collectible cannot be both required and a key.");

    }

    protected void Start() => weaponSelector = FindObjectOfType<WeaponSelector>();

    protected void OnDestroy() => itemPicker.OnCollect -= OnCollect;

    protected virtual void OnCollect(ItemPicker collectible) {

        isCollected = true;
        gameManager.CheckVictory();

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

    public bool IsKey() => isKey;

}

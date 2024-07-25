using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour {

    [Header("Cooldowns")]
    private Dictionary<Action, CooldownData> cooldownData;

    private void Start() => cooldownData = new Dictionary<Action, CooldownData>();

    public void SetCooldown(Action action, float cooldownTimer, float unequipTime) {

        if (cooldownData.ContainsKey(action))
            cooldownData[action] = new CooldownData(cooldownTimer, unequipTime);
        else
            cooldownData.Add(action, new CooldownData(cooldownTimer, unequipTime));

    }

    public CooldownData GetCooldown(Action action) {

        if (cooldownData.ContainsKey(action))
            return cooldownData[action];
        else
            return null;

    }

    public void ClearCooldownData() => cooldownData.Clear();

}

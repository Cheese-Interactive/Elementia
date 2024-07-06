using UnityEngine;

public class MeterController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Meter meterPrefab;

    public Meter CreateMeter(float cooldownDuration, Sprite icon, Color color) {

        Meter meter = Instantiate(meterPrefab, transform);
        meter.Initialize(cooldownDuration, icon, color);
        return meter;

    }
}

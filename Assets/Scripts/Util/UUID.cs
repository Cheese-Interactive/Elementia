using System.Collections.Generic;
using UnityEngine;

public class UUID : MonoBehaviour, ISerializationCallbackReceiver {

    private static Dictionary<UUID, string> m_ObjToUUID = new Dictionary<UUID, string>();
    private static Dictionary<string, UUID> m_UUIDtoObj = new Dictionary<string, UUID>();

    static void RegisterUUID(UUID aID) {

        string UID;

        if (m_ObjToUUID.TryGetValue(aID, out UID)) {

            // found object instance, update ID
            aID.m_UUID = UID;
            aID.m_IDBackup = aID.m_UUID;
            if (!m_UUIDtoObj.ContainsKey(UID))
                m_UUIDtoObj.Add(UID, aID);
            return;

        }

        if (string.IsNullOrEmpty(aID.m_UUID)) {

            // No ID yet, generate a new one.
            aID.m_UUID = System.Guid.NewGuid().ToString();
            aID.m_IDBackup = aID.m_UUID;
            m_UUIDtoObj.Add(aID.m_UUID, aID);
            m_ObjToUUID.Add(aID, aID.m_UUID);
            return;

        }

        UUID tmp;

        if (!m_UUIDtoObj.TryGetValue(aID.m_UUID, out tmp)) {

            // ID not known to the DB, so just register it
            m_UUIDtoObj.Add(aID.m_UUID, aID);
            m_ObjToUUID.Add(aID, aID.m_UUID);
            return;

        }

        if (tmp == aID) {

            // DB inconsistency
            m_ObjToUUID.Add(aID, aID.m_UUID);
            return;

        }

        if (tmp == null) {

            // object in DB got destroyed, replace with new
            m_UUIDtoObj[aID.m_UUID] = aID;
            m_ObjToUUID.Add(aID, aID.m_UUID);
            return;

        }

        // we got a duplicate, generate new ID
        aID.m_UUID = System.Guid.NewGuid().ToString();
        aID.m_IDBackup = aID.m_UUID;
        m_UUIDtoObj.Add(aID.m_UUID, aID);
        m_ObjToUUID.Add(aID, aID.m_UUID);

    }

    static void UnregisterUUID(UUID aID) {

        m_UUIDtoObj.Remove(aID.m_UUID);
        m_ObjToUUID.Remove(aID);

    }

    [SerializeField] private string m_UUID = null;
    private string m_IDBackup = null;

    public string ID { get { return m_UUID; } }

    public void OnAfterDeserialize() {

        if (m_UUID == null || m_UUID != m_IDBackup)
            RegisterUUID(this);

    }

    public void OnBeforeSerialize() {

        if (m_UUID == null || m_UUID != m_IDBackup)
            RegisterUUID(this);

    }

    void OnDestroy() {

        UnregisterUUID(this);
        m_UUID = null;

    }
}
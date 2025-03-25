using Unity.Netcode;
using UnityEngine;

public class SleepTimmy : NetworkBehaviour
{
    public NetworkVariable<bool> timmyActive = new NetworkVariable<bool>(true);
    public Animator animator;

    public override void OnNetworkSpawn()
    {
        timmyActive.OnValueChanged -= OnSetActiveSelf;
        timmyActive.OnValueChanged += OnSetActiveSelf;
        if (!IsServer) return;
        animator = GetComponent<Animator>();
        timmyActive.Value = true;
    }

    public override void OnNetworkDespawn()
    {
        timmyActive.OnValueChanged -= OnSetActiveSelf;
    }
        public void OnSetActiveSelf(bool oldValue, bool newValue)
    {
        gameObject.SetActive(newValue);
    }
}

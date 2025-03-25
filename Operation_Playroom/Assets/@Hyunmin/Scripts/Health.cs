using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class Health : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public bool isDead;

    public Action<Health> OnDie;

    int maxHealth;
    [SerializeField] Image hpBar;
    Character character;

    void Start()
    {
        character = GetComponent<Character>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        if (IsClient && IsOwner)
        {
            StartCoroutine(FindHpbar());
        }

        currentHealth.OnValueChanged -= OnHealthChanged;
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public void InitializeHealth() 
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        isDead = false;
    }

    public void TakeDamage(int damage, ulong clientId)
    {
        ModifyHealth(-damage, clientId);
        character.TakeDamage();

    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }

    public void RestoreHealth(int heal, ulong clientId)
    {
        ModifyHealth(heal, clientId);
    }

    void OnHealthChanged(int previousValue, int newValue)
    {
        if (IsClient && IsOwner)
        {
            if(!GetComponent<SoldierTest>())
            {
                UpdateHpbar();
            }
        }

        if (newValue == 0)
        {
            isDead = true;

            if (IsOwner)
            {
                OnDie?.Invoke(this);
            }

            if (GetComponent<PlayerController>() != null)
            {
                GetComponent<PlayerController>().isPlayable = false;
            }
        }
    }

    void ModifyHealth(int value, ulong clientId)
    {
        if (isDead) { return; }
        int newHealth = currentHealth.Value + value;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth.Value == 0)
        {
            isDead = true;

            if (GetComponent<SoldierTest>() == null)
            {
                //add death score
                PlayData deadPlayerData = GameManager.Instance.userPlayDatas[OwnerClientId];
                deadPlayerData.death++;
                GameManager.Instance.userPlayDatas[OwnerClientId] = deadPlayerData;

                //add kill score
                PlayData killPlayerData = GameManager.Instance.userPlayDatas[clientId];
                killPlayerData.kill++;
                GameManager.Instance.userPlayDatas[clientId] = killPlayerData;
            }

            character.Die();

            if (GetComponent<PlayerController>() != null)
            {
                GetComponent<PlayerController>().isPlayable = false;
            }

            OnDie?.Invoke(this);
        }
    }
    IEnumerator FindHpbar()
    {
        yield return new WaitUntil(() => GameObject.FindWithTag("HPBar"));

        hpBar = GameObject.FindWithTag("HPBar").GetComponent<Image>();
    }

    void UpdateHpbar()
    {
        if(hpBar != null)
        {
            hpBar.fillAmount = (float)currentHealth.Value / maxHealth;
        }
    }

    public void SetHp(int hp)
    {
        maxHealth = hp;

        if (!IsServer) return;

        currentHealth.Value = maxHealth;
    }
}

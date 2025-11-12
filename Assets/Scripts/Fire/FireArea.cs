using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Keeps a list of fire game objects and actives them, also keeps track of the state
/// </summary>
public class FireArea : MonoBehaviour
{
    [SerializeField] private string areaName;
    [SerializeField] private List<Fire> fires = new List<Fire>();
    public List<GameObject> notificationFire = new List<GameObject>();

    public float firePause = 2f;
    private bool active;
    private int fireCount;
    private Collider detectCollider;
    private Coroutine enableRoutine;

    private List<PlayerSetup> playersInside = new List<PlayerSetup>();
    // private bool stopIncreasing;

    public bool IsActive => active;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        active = false;

        // disable all fires
        foreach (Fire fire in fires)
        {
            fire.fireArea = this;
        }

        fireCount = 0;

        detectCollider = GetComponent<Collider>();

        if (notificationFire.Count > 0)
        {
            foreach (GameObject fire in notificationFire)
            {
                fire.SetActive(false);
            }

        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSetup playerSetup = other.GetComponent<PlayerSetup>();

            if (playerSetup != null && !playersInside.Contains(playerSetup))
            {
                playersInside.Add(playerSetup);

                if (active)
                    playerSetup.extinguisher.ActivateExtinguisher(true);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSetup playerSetup = other.GetComponent<PlayerSetup>();
            if (playerSetup != null && playersInside.Contains(playerSetup))
            {
                playersInside.Remove(playerSetup);
                playerSetup.extinguisher.ActivateExtinguisher(false);
            }
        }
    }

    public void PlayerTeleportOut(PlayerSetup playerSetup)
    {
        if (playerSetup != null && playersInside.Contains(playerSetup))
        {
            playersInside.Remove(playerSetup);
            playerSetup.extinguisher.ActivateExtinguisher(false);
        }
    }

    public void EnableFires()
    {
        if (active) return;

        if (notificationFire.Count > 0)
        {
            foreach (GameObject fire in notificationFire)
            {
                fire.SetActive(true);
            }

        }

        active = true;
        // stopIncreasing = false;

        if (enableRoutine != null)
        {
            StopCoroutine(enableRoutine);
        }

        enableRoutine = StartCoroutine(EnableRoutine());

        foreach (var player in playersInside)
        {
            if (player != null)
                player.extinguisher.ActivateExtinguisher(true);
        }
    }

    // gradually increase fires
    IEnumerator EnableRoutine()
    {
        fireCount = 0;

        foreach (Fire fire in fires)
        {
            // if (stopIncreasing) yield break;

            fire.StartFire();
            fireCount++;

            yield return new WaitForSeconds(firePause);
        }
    }

    // used by Fire to tell manager fire is put out
    public void DisableFire()
    {
        // reduce fire count
        fireCount--;
        // stopIncreasing = true;

        // if count is 0, fire is put out
        if (fireCount <= 0)
        {
            // notify fire manager area is safe
            active = false;

            if (notificationFire.Count > 0)
            {
                foreach (GameObject fire in notificationFire)
                {
                    fire.SetActive(false);
                }

            }

            foreach (PlayerSetup playerSetup in playersInside)
            {
                if (playerSetup != null && playerSetup.extinguisher != null)
                    playerSetup.extinguisher.ActivateExtinguisher(false);
            }

            if (enableRoutine != null)
            {
                StopCoroutine(enableRoutine);
                enableRoutine = null;
            }

            FireManager.Instance.StopFireArea(areaName);
        }
    }
}

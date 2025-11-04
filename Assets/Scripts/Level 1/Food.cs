using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Subsystems;

public class Food : InteractableObject
{
    // spawn food bites
    public int totalBites = 10;
    private int foodBiteCount;
    private ObjectPooler objectPooler;
    [SerializeField] private ParticleSystem particle;

    [SerializeField] private GameObject[] foodStates;
    [SerializeField] private int[] foodStateChange;
    [SerializeField] private GameObject bag;
    private int currIndex;

    public override void Start()
    {
        base.Start();
        objectPooler = ObjectPooler.Instance;
        foodBiteCount = 0;
        currIndex = 0;
        foodStates[currIndex].SetActive(true);

        StartCoroutine(WaitForScoreKeeper());

        particle.Stop();
    }

    IEnumerator WaitForScoreKeeper()
    {
        yield return new WaitUntil(() => ScoreKeeper.Instance != null);
        ScoreKeeper.Instance.AddScoring("Spaghetti completion", 2, true, false, totalBites);
    }

    public override void InteractWithHand(Transform wrist, HandMovement target)
    {
        if (foodBiteCount < totalBites && canPickup)
        {
            // spawn a food bite from the object pooler
            GameObject foodBiteObj = objectPooler.SpawnFromPool("FoodBite", transform.position, transform.rotation);

            PlayForOneSecond();

            FoodBite foodBite = foodBiteObj.GetComponent<FoodBite>();
            if (foodBite != null)
            {
                foodBite.SetBag(bag);
                foodBite.SetFoodBiteSpawner(this);
            }
            target.StopInteractingWithObject(this);
            target.InteractWithObject(foodBite);

            if (NovaLevel1Manager.Instance.ate)
                foodBiteCount++;

            // change animation state based on numbites
            ChangeFoodState(foodBiteCount);
            
            // Turn off outline when food bite is picked up (until another hover turns it back on)
            DisableOutline();
        }
        else
        {
            Debug.Log("No more food bites!");
            target.StopInteractingWithObject(this);
            canPickup = false;
        }
    }

    private void ChangeFoodState(int foodBiteCount)
    {
        if (foodStateChange.Contains(foodBiteCount))
        {
            foodStates[currIndex].SetActive(false);
            currIndex++;
            foodStates[currIndex].SetActive(true);
        }

        if (foodBiteCount == totalBites)
        {
            Level1TaskManager.CompleteTaskEatFood();
            canPickup = false;
        }
    }

    public void PlayForOneSecond()
    {
        StartCoroutine(PlayParticlesRoutine());
    }

    private IEnumerator PlayParticlesRoutine()
    {
        particle.Play();
        yield return new WaitForSeconds(1f);
        particle.Stop();
    }
}

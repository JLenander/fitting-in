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
                // Disable dropping foodbite on spawn until the hand has left the plate area
                // this prevents players from dropping the foodbite immediately
                foodBite.canDrop = false;
            }
            target.StopInteractingWithObject(this);
            target.ForceInteractionWithObject(foodBite);

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

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        // Logic to stop dropping of foodbite when hand is in plate.
        if (other != null && other.CompareTag("Hand"))
        {
            var hand = other.GetComponent<Hand>();
            if (hand != null)
            {
                handMovement = hand.GetHandMovement();
                FoodBite foodBite = handMovement.currObj as FoodBite;
                if (foodBite != null)
                {
                    foodBite.canDrop = false;
                }
            }
        }
    }
    
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // Logic to stop dropping of foodbite when hand is in plate.
        if (other != null && other.CompareTag("Hand"))
        {
            var hand = other.GetComponent<Hand>();
            if (hand != null)
            {
                handMovement = hand.GetHandMovement();
                FoodBite foodBite = handMovement.currObj as FoodBite;
                if (foodBite != null)
                {
                    foodBite.canDrop = true;
                }
            }
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

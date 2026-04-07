using System.Collections;
using UnityEngine;

public class NovaMouth : MonoBehaviour
{
    // the all consuming void that incinerates anything edible within it's path

    [SerializeField] private ParticleSystem spagParticle;

    [SerializeField] private ParticleSystem miscParticle;

    [SerializeField] private GameObject crumbs;

    private int counter;

    public void Start()
    {
        counter = 0;
        crumbs.SetActive(false);
        spagParticle.Stop();
        miscParticle.Stop();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            InteractableObject interactableObject = other.transform.GetComponent<InteractableObject>();
            if (interactableObject == null)
            {
                Debug.LogWarning("Food item that isn't an interactableObject encountered by mouth " + other.name);
                return;
            }
            // Verify food is being held by hand
            if (interactableObject.handMovement == null) return;
            
            // thanks for the food
            counter++;
            NovaLevel1Manager.Instance.ThankForFood(counter);

            if (counter > 1)
            {
                crumbs.SetActive(true);
            }

            if (other.gameObject.name.Contains("FoodBite"))
            {
                PlayParticlesRoutine(spagParticle);
            }
            else
            {
                PlayParticlesRoutine(miscParticle);
            }

            // stop the interaction
            HandMovement handMovement = interactableObject.handMovement;
            handMovement.handAnimator.SetTrigger("Neutral");
            handMovement.StopInteractingWithObject(interactableObject);
            handMovement.RemoveInteractableObject(interactableObject);

            Destroy(other.gameObject);
        }
    }

    private IEnumerator PlayParticlesRoutine(ParticleSystem particle)
    {
        particle.Play();
        yield return new WaitForSeconds(1f);
        particle.Stop();
    }
}

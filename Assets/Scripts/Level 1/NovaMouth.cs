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
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
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
            InteractableObject interactableObject = other.transform.GetComponent<InteractableObject>();
            interactableObject.handMovement.handAnimator.SetTrigger("Neutral");

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

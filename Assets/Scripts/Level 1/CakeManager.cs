using System.Collections.Generic;
using UnityEngine;

public class CakeManager : MonoBehaviour
{
    public Collider grappleCollider;
    // keeps track of the four cake slices
    public List<CakeSlice> cakeSlices = new List<CakeSlice>();

    public bool novaCake = true;
    
    [SerializeField] private TriggerSeat triggerSeat;

    public void GrabbedSlice(CakeSlice cakeSlice)
    {
        cakeSlices.Remove(cakeSlice);

        // Only activate Nova's steal food dialog if the player is inside the seat to prevent weird animation.
        if (novaCake && triggerSeat != null && triggerSeat.PlayerInsideSeat())
            NovaLevel1Manager.Instance.StealFood();

        if (cakeSlices.Count <= 0)
        {
            grappleCollider.enabled = false;
        }
    }

    public bool CanRemoveSlice()
    {
        if (cakeSlices.Count > 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public GameObject RemoveSlice()
    {
        if (cakeSlices.Count == 0)
            return null;

        CakeSlice slice = cakeSlices[0];
        cakeSlices.RemoveAt(0);

        return slice.gameObject;
    }
}

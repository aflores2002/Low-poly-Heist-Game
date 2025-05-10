using UnityEngine;

public class JewelrySpot : MonoBehaviour
{
    public bool isEmpty = false;
    public bool hasBeenInvestigated = false;

    public void MarkAsStolen()
    {
        isEmpty = true;
        hasBeenInvestigated = false; // reset when stolen
    }
}


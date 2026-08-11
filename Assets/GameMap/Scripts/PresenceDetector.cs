using UnityEngine;

public class PresenceDetector : MonoBehaviour
{
    public HouseBuilding house;
    [SerializeField, ReadOnly] private Collider currentPlacement;

    private bool inside = false;
    private bool inFront = false;
    private bool behind = false;

    public bool IsInside
    {
        get => inside;
    }

    public bool IsOutside
    {
        get => !inside && (behind || inFront);
    }

    public bool IsPresent
    {
        get => inside || behind || inFront;
    }

    public int CurrentPlaceIndex
    {
        get => house.GetIndexByCollider(currentPlacement);
    }

    public Collider CurrentPlacement
    {
        get => currentPlacement;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider == house.entrance || collider == house.exit)
            inside = true;
        else if (collider == house.back)
            behind = true;
        else if (collider == house.front)
            inFront = true;
    }

    void OnTriggerStay(Collider collider)
    {
        if (inside) {
            if (house.inside.Includes(collider))
                currentPlacement = collider;
        }
        else if (behind) currentPlacement = house.back;
        else if (inFront) currentPlacement = house.front;
    }

    void OnTriggerExit(Collider collider)
    {
        if ((collider == house.entrance || collider == house.exit)
            && (transform.position.x >= house.entrance.bounds.max.x
            || transform.position.x <= house.exit.bounds.min.x)) {
            inside = false;
        }
        else if (collider == house.back) behind = false;
        else if (collider == house.front) inFront = false;
        if (!inside && !behind && !inFront)
            currentPlacement = null;
    }
}

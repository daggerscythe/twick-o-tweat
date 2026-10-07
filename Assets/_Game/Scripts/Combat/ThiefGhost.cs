using UnityEngine;

// a purple ghost that steals 1 candy instead of hurting you
// lives on Ghost_Thief prefab

public class ThiefGhost : Ghost
{
    [Header("Thief")]
    [SerializeField] private Vector2 houseHalfSize = new Vector2(7f, 6f); // room is 14x12
    [SerializeField] private SpriteRenderer stolenIcon; // child that shows the candy it carries

    private PlayerBag bag;
    private CandyData stolen;
    private Vector3 fleeDirection;

    protected override void Awake()
    {
        base.Awake(); // run Ghosts Awake first
        if (stolenIcon != null) stolenIcon.enabled = false;
    }

    protected override void OnTouchPlayer(Vector3 away)
    {
        if (bag == null) bag = target.GetComponent<PlayerBag>();

        stolen = bag.StealRandom();
        if (stolen == null)
        {
            StartKnockback(away); // nothing to steal: bounce off and try again
            return;
        }

        if (stolenIcon != null)
        {
            stolenIcon.sprite = stolen.icon;
            stolenIcon.enabled = true;
        }

        fleeDirection = away.sqrMagnitude > 0.0001f ? away.normalized : -target.transform.forward;
        state = State.Fleeing;
    }

    protected override void Flee()
    {
        transform.position += fleeDirection * Speed * Time.deltaTime;

        if (!InsideHouse()) Despawn(); // got away with the candy
    }

    protected override void OnKilled()
    {
        if (stolen != null && InsideHouse()) bag.ReturnStolen(stolen);
    }

    private bool InsideHouse()
    {
        Vector3 p = transform.position;
        return Mathf.Abs(p.x) < houseHalfSize.x && Mathf.Abs(p.z) < houseHalfSize.y;
    }
}
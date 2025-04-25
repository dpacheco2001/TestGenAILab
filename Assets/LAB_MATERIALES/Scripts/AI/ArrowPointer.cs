using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform target;
    [SerializeField] float height = 2f;
    [SerializeField] float forwardDistance = 1.5f;

    void Update()
    {
        Vector3 basePos = player.position + Vector3.up * height + player.forward * forwardDistance;
        transform.position = basePos;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}

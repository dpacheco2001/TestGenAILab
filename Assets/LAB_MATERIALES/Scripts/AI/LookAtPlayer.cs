using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;

    void Reset()
    {
        if (player == null && Camera.main != null)
            player = Camera.main.transform;
    }

    void Update()
    {
        if (player == null) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0;  
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}

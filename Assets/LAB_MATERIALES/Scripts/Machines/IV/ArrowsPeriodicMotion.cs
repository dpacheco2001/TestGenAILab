using UnityEngine;

public class ArrowsPeriodicMotion : MonoBehaviour
{
    public Transform arrow1;
    public Transform arrow2;

    public enum MovementAxis { X, Y, Z }
    public MovementAxis movementAxis = MovementAxis.Z;

    public float amplitude = 0.5f;
    public float frequency = 2f;

    [Tooltip("Desfase de fase en segundos para desincronizar cada instancia.")]
    public float phaseOffset = 0f;

    [Tooltip("Si true, usa el eje local (arrow.forward, etc.). Si false, usa Vector3.forward, etc.")]
    public bool localSpace = true;

    private Vector3 arrow1StartPos;
    private Vector3 arrow2StartPos;

    void Start()
    {
        if (arrow1 != null) arrow1StartPos = arrow1.position;
        if (arrow2 != null) arrow2StartPos = arrow2.position;
    }

    void Update()
    {
        float offset = Mathf.Sin((Time.time + phaseOffset) * frequency) * amplitude;

        MoveArrow(arrow1, arrow1StartPos, offset);
        MoveArrow(arrow2, arrow2StartPos, offset);
    }

    void MoveArrow(Transform arrow, Vector3 startPos, float offset)
    {
        if (!arrow) return;

        Vector3 dir;
        switch (movementAxis) {
            case MovementAxis.X:
                dir = localSpace ? arrow.right : Vector3.right;
                break;
            case MovementAxis.Y:
                dir = localSpace ? arrow.up : Vector3.up;
                break;
            default: // Z
                dir = localSpace ? arrow.forward : Vector3.forward;
                break;
        }

        arrow.position = startPos + dir * offset;
    }
}

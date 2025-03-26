using UnityEngine;

public class BajarElectrodo : MonoBehaviour
{
    private Animator m_animator;
    private bool isMoving = false;

    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public void StartMovement()
    {
        if (!isMoving)
        {
            isMoving = true;
            m_animator.SetBool("bool_electrodo", true);
            Invoke("ResetAnimation", 5f); // Reset after 5 seconds
        }
    }

    private void ResetAnimation()
    {
        m_animator.SetBool("bool_electrodo", false);
        isMoving = false;
    }
}

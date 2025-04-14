using System.Collections;
using UnityEngine;

public class SidebarAnimationManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Sidebar Animator setup")]
    public Animator sidebarAnimator;
    public Animator tab;


    [Header("Trigger parameters")]

    public string sidebarhover = "hover";
    public string sidebarhover_b = "hover_b";
    public string sidebardeshover = "deshover";
    public string sidebardeshover_b = "deshover_b";
    public string sidebardeploy = "deploy";
    public string sidebardeploy_b = "deploy_b";
    

    //tab
    public string sidebardeployaction = "deployaction";
    public string sidebardeployaction_b = "deployaction_b";

    [Header("Simulate trigger")]
    public bool simulateHover = false;
    public bool simulateDeploy = false;

    [Header("Sidebar state")]
    public bool isHover = false;

    public bool deploying = false;
    public bool isDeployed = false;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDeployed && !isHover && simulateHover)
        {
            isHover = true;
            sidebarAnimator.SetTrigger(sidebarhover);
        }

        if(isHover && !isDeployed && !simulateHover)
        {
            isHover = false;
            sidebarAnimator.SetTrigger(sidebardeshover);
        }

        if(isHover && !isDeployed && simulateDeploy)
        {
            simulateDeploy = false;
            sidebarAnimator.SetTrigger(sidebardeploy);
            tab.SetTrigger(sidebardeployaction);
            StartCoroutine(WaitForAnimation(1.2f));
        }

        if(isDeployed && !isHover && simulateHover && !deploying)
        {
            isHover = true;
            sidebarAnimator.SetTrigger(sidebarhover_b);
        }

        if(isDeployed && isHover && !simulateHover && !deploying)
        {
            isHover = false;
            sidebarAnimator.SetTrigger(sidebardeshover_b);
        }
        if(isDeployed && isHover && simulateDeploy)
        {
            simulateDeploy = false;
            sidebarAnimator.SetTrigger(sidebardeploy_b);
            tab.SetTrigger(sidebardeployaction_b);
            StartCoroutine(WaitForAnimation(1.2f,1));
        }



    }

    IEnumerator WaitForAnimation(float duration, int b = 0)
    {
        deploying = true;
        isHover = false;
        simulateHover = false;
        yield return new WaitForSeconds(duration);

        Debug.Log("Terminó animación por tiempo.");

        isDeployed = (b == 0);
        
        deploying = false;
    }


}

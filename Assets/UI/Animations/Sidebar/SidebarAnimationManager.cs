using System.Collections;
using UnityEngine;

public class SidebarAnimationManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Sidebar Animator setup")]
    public Animator sidebarAnimator;

    [Header("Trigger parameters")]

    public string sidebarhover = "hover";

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



    }

    public void ToogleDeployment(){
        if(!isDeployed){
            Debug.LogError("Deploying sidebar");
            isDeployed = true;
            sidebarAnimator.SetTrigger("Pressed");
        }
        else{
            Debug.LogError("Dedeploying sidebar");
            isDeployed = false;
            sidebarAnimator.SetTrigger("Disabled");
        }
    }


}

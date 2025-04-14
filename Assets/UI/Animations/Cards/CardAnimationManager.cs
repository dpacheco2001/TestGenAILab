using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAnimationManager : MonoBehaviour
{
    [Header("Card Animators (3 cards)")]
    public Animator casoEstudioAnimator;
    public Animator ensayosAnimator;
    public Animator informesAnimator;

    [Header("Trigger Names")]
    public string hoverTrigger = "hover";
    public string deshoverTrigger = "deshover";


    [Header("Simulation")]

    public bool simulateHoverCasoEstudio;
    public bool simulateHoverEnsayos;
    public bool simulateHoverInformes;


    [Header("Card state")]
    public bool isHoverCasoEstudio = false;
    public bool isHoverEnsayos = false;
    public bool isHoverInformes = false;


    void Update()
    {
        if(simulateHoverCasoEstudio && !isHoverCasoEstudio)
        {
            isHoverCasoEstudio = true;
            casoEstudioAnimator.SetTrigger(hoverTrigger);
        }
        if(!simulateHoverCasoEstudio && isHoverCasoEstudio)
        {
            isHoverCasoEstudio = false;
            casoEstudioAnimator.SetTrigger(deshoverTrigger);
        }

        if(simulateHoverEnsayos && !isHoverEnsayos)
        {
            isHoverEnsayos = true;
            ensayosAnimator.SetTrigger(hoverTrigger);
        }
        if(!simulateHoverEnsayos && isHoverEnsayos)
        {
            isHoverEnsayos = false;
            ensayosAnimator.SetTrigger(deshoverTrigger);
        }

        if(simulateHoverInformes && !isHoverInformes)
        {
            isHoverInformes = true;
            informesAnimator.SetTrigger(hoverTrigger);
        }
        if(!simulateHoverInformes && isHoverInformes)
        {
            isHoverInformes = false;
            informesAnimator.SetTrigger(deshoverTrigger);
        }

    }

    public void menu_activated(){
        casoEstudioAnimator.SetTrigger("menuactive");
        ensayosAnimator.SetTrigger("menuactive");
        informesAnimator.SetTrigger("menuactive");
    }

    public void menu_deactivated(){
        casoEstudioAnimator.SetTrigger("menudeactivated");
        ensayosAnimator.SetTrigger("menudeactivated");
        informesAnimator.SetTrigger("menudeactivated");
    }
}

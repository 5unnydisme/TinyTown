using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static bool IsROVBrokenMenuActive { get; private set; }
    public static bool IsWarningMenuActive { get; private set; }
    public static bool IsBatteryWarningMenuActive { get; private set; }
    public GameObject ROV_BrokenMenu;
    public GameObject WarningMenu;
    public GameObject BatteryWarningMenu;

    private void OnEnable()
    {
        ROVBehaviour.OnROVDeath += EnableROVBrokenMenu;
        ROVBehaviour.OnSecondCollision += EnableWarningMenu; //Subscribe to second collision event
        ROVBehaviour.OnThirdCollision += EnableBatteryWarningMenu; //Subscribe to third collision event
        IsROVBrokenMenuActive = false;
        IsWarningMenuActive = false;
        IsBatteryWarningMenuActive = false;
    }

    private void OnDisable()
    {
        ROVBehaviour.OnROVDeath -= EnableROVBrokenMenu;
        ROVBehaviour.OnSecondCollision -= EnableWarningMenu; //Unsubscribe from second collision event
        ROVBehaviour.OnThirdCollision -= EnableBatteryWarningMenu; //Unsubscribe from third collision event
    }

    public void EnableROVBrokenMenu()
    {
        ROV_BrokenMenu.SetActive(true);
        IsROVBrokenMenuActive = true;
    }

    public void EnableWarningMenu()
    {
        WarningMenu.SetActive(true);
        IsWarningMenuActive = true;
    }

    public void DisableWarningMenu()
    {
        WarningMenu.SetActive(false);
        IsWarningMenuActive = false;
    }

    public void EnableBatteryWarningMenu()
    {
        BatteryWarningMenu.SetActive(true);
        IsBatteryWarningMenuActive = true;
    }

    public void DisableBatteryWarningMenu()
    {
        BatteryWarningMenu.SetActive(false);
        IsBatteryWarningMenuActive = false;
    }

    public void ROVInspectScene()
    {
        SceneManager.LoadScene(1);
        IsROVBrokenMenuActive = false;
    }
}

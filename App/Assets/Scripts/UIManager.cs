using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static bool IsROVBrokenMenuActive { get; private set; }
    public GameObject ROV_BrokenMenu;

    private void OnEnable()
    {
        CarBehaviour.OnCarDeath += EnableROVBrokenMenu;
        IsROVBrokenMenuActive = false;
    }

    private void OnDisable()
    {
        CarBehaviour.OnCarDeath -= EnableROVBrokenMenu;
    }

    public void EnableROVBrokenMenu()
    {
        ROV_BrokenMenu.SetActive(true);
        IsROVBrokenMenuActive = true;
    }

    public void ROVInspectScene()
    {
        SceneManager.LoadScene(1);
        IsROVBrokenMenuActive = false;
    }
}

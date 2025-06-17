using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelToClose;
    [SerializeField] private PlaceContent placeContent; // Reference to PlaceContent script

    // Start is called before the first frame update
    void Start()
    {
        // Ensure panel is visible when scene starts
        if (panelToClose != null)
        {
            panelToClose.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void CloseThisPanel()
    {
        if (panelToClose != null)
        {
            //checking to see if the button is pressed then panel is closed
            panelToClose.SetActive(false);
        }
        else
        {
            //If no button is pressed, fail safe to ensure panel visibility
            gameObject.SetActive(false);
        }

        // Enable AR placement functionality
        if (placeContent != null)
        {
            placeContent.EnablePlacement();
        }
    }
}

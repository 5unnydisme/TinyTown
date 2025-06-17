using UnityEngine;

public class BuildPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject buildPanel;  // Reference to your build panel UI
    private bool isPanelOpen = false;

    private void Start()
    {
        // Ensure panel is hidden at start
        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }

    public void ToggleBuildPanel()
    {
        if (buildPanel != null)
        {
            isPanelOpen = !isPanelOpen;
            buildPanel.SetActive(isPanelOpen);
        }
    }
}

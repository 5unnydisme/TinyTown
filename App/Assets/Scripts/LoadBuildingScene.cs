using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadBuildingScene : MonoBehaviour
{
    public void ChangetoBuildScene()
    {
        // Assuming the build scene is at index 2 in your build settings
        // You can change this index to match your scene's build index
        SceneManager.LoadScene(2);
    }
}

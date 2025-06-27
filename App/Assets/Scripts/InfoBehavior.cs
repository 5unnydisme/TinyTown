using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class InfoBehavior : MonoBehaviour
{
    const float SPEED = 6f;
    [SerializeField] Transform SectionInfo;
    Vector3 desiredScale = Vector3.zero;


    void Start()
    {
        if (SectionInfo == null)
        {
            Debug.LogWarning($"InfoBehavior on {gameObject.name}: SectionInfo is not assigned!");
        }
        else
        {
            Debug.Log($"InfoBehavior on {gameObject.name}: SectionInfo assigned to {SectionInfo.name}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if SectionInfo is assigned before using it
        if (SectionInfo != null)
        {
            SectionInfo.localScale = Vector3.Lerp(SectionInfo.localScale, desiredScale, Time.deltaTime * SPEED);
        }
    }

    public void OpenInfo()
    {
        desiredScale = Vector3.one; // Set the desired scale to 1
    }

    public void CloseInfo()
    {
        desiredScale = Vector3.zero; // Set the desired scale to 0
    }
}

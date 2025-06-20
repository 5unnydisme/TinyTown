using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class InfoBehavior : MonoBehaviour
{
    const float SPEED = 6f;
    [SerializeField] Transform SectionInfo;
    Vector3 desiredScale = Vector3.zero;


    // Update is called once per frame
    void Update()
    {
        SectionInfo.localScale = Vector3.Lerp(SectionInfo.localScale, desiredScale, Time.deltaTime * SPEED);

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

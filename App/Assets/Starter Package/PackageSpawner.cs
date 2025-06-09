/*
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PackageSpawner : MonoBehaviour
{
    public DrivingSurfaceManager DrivingSurfaceManager;
    public PackageBehaviour Package;
    public GameObject PackagePrefab;

    public int packagesDelivered;
    private UIManager uiManager;

    public Material[] packageMaterials;  // Drag different colored materials here in Inspector
    private List<int> usedMaterialIndices = new List<int>();

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        packagesDelivered = 0;
        Debug.Log("PackageSpawner initialized. Package count: " + packagesDelivered);
    }

    public static Vector3 RandomInTriangle(Vector3 v1, Vector3 v2)
    {
        float u = Random.Range(0.0f, 1.0f);
        float v = Random.Range(0.0f, 1.0f);
        if (v + u > 1)
        {
            v = 1 - v;
            u = 1 - u;
        }

        return (v1 * u) + (v2 * v);
    }

    public static Vector3 FindRandomLocation(ARPlane plane)
    {
        // Select random triangle in Mesh
        var mesh = plane.GetComponent<ARPlaneMeshVisualizer>().mesh;
        var triangles = mesh.triangles;
        var triangle = triangles[(int)Random.Range(0, triangles.Length - 1)] / 3 * 3;
        var vertices = mesh.vertices;
        var randomInTriangle = RandomInTriangle(vertices[triangle], vertices[triangle + 1]);
        var randomPoint = plane.transform.TransformPoint(randomInTriangle);

        return randomPoint;
    }

    private Material GetUnusedMaterial()
    {
        if (packageMaterials == null || packageMaterials.Length == 0)
            return null;

        // Reset used materials if all have been used
        if (usedMaterialIndices.Count >= packageMaterials.Length)
        {
            usedMaterialIndices.Clear();
            Debug.Log("Resetting material cycle");
        }

        // Find unused material
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, packageMaterials.Length);
        } while (usedMaterialIndices.Contains(randomIndex));

        usedMaterialIndices.Add(randomIndex);
        Debug.Log($"Using material {randomIndex}. Used materials: {usedMaterialIndices.Count}/{packageMaterials.Length}");
        return packageMaterials[randomIndex];
    }

    public void SpawnPackage(ARPlane plane)
    {
        var packageClone = GameObject.Instantiate(PackagePrefab);
        packageClone.transform.position = FindRandomLocation(plane);

        // Apply unused material to package
        var renderer = packageClone.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material unusedMaterial = GetUnusedMaterial();
            if (unusedMaterial != null)
            {
                renderer.material = unusedMaterial;
            }
        }

        Package = packageClone.GetComponent<PackageBehaviour>();
    }

    public void OnPackageDelivered()
    {
        packagesDelivered++;
        Debug.Log($"Package delivered! Total packages: {packagesDelivered}/4");
        
        if (packagesDelivered >= 4)
        {
            Debug.Log("4 packages reached - Activating broken menu");
            uiManager.EnableROVBrokenMenu();
            packagesDelivered = 0;
        }
    }

    private void Update()
    {
        if (UIManager.IsROVBrokenMenuActive)
            return;

        var lockedPlane = DrivingSurfaceManager.LockedPlane;
        if (lockedPlane != null)
        {
            if (Package == null)
            {
                SpawnPackage(lockedPlane);
                Debug.Log("New package spawned");
            }

            if (Package != null)  // Safety check
            {
                Vector3 newPosition = Package.gameObject.transform.position;
                newPosition.y = lockedPlane.center.y;
                Package.gameObject.transform.position = newPosition;
            }
        }
    }
}

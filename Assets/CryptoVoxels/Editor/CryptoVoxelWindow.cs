using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CryptoVoxelWindow : EditorWindow
{
    public int maxParcels = 10;
    public int maxDistance = 100;
    public Vector2 centerPosition = Vector2.zero;
    public string suburb = "The Center";
    
    public Material atlasMaterial;
    public Material glassMaterial;
    public Material imageMaterial;
    
    private const string URL = "https://www.cryptovoxels.com/grid/parcels";
    private const string PARCEL_URL = "https://www.cryptovoxels.com/api/parcels/";
    private Queue<ParcelDescription> parcelQueue = new Queue<ParcelDescription>();
    private int parcelCount = 0; 
    
    [MenuItem("Window/CryptoVoxel")]
    public static void Window()
    {
        CryptoVoxelWindow window = CreateInstance<CryptoVoxelWindow>();
        window.ShowUtility();
    }

    private void OnGUI()
    {
        GUILayout.Label("CryptoVoxel Importer", EditorStyles.boldLabel);

        maxParcels = EditorGUILayout.IntField("Max Parcels", maxParcels);
        maxDistance = EditorGUILayout.IntField("Max Distance", maxDistance);
        centerPosition = EditorGUILayout.Vector2Field("Center Position", centerPosition);
        suburb = EditorGUILayout.TextField("Suburb", suburb);
        atlasMaterial = (Material)EditorGUILayout.ObjectField("Atlas Material", atlasMaterial, typeof(Material), false);
        imageMaterial = (Material)EditorGUILayout.ObjectField("Image Material", imageMaterial, typeof(Material), false);
        glassMaterial = (Material)EditorGUILayout.ObjectField("Glass Material", glassMaterial, typeof(Material), false);
        
        if (GUILayout.Button("Import Voxels"))
        {
            InstantiateParcels();   
        }
    }

    private void InstantiateParcels()
    {
        WWW www = new WWW(URL);
        while(!www.isDone)
        {
            //Wait for URL to load, this hangs the editor, but is the easy solution.
        }
        
        if(www.error != null) 
        {
            Debug.LogError(www.error);
        } 
        else 
        {
            Response r = Response.CreateFromJSON(www.text);
            Debug.Log("Found " + r.parcels.Length + " parcels");

            parcelCount = r.parcels.Length <= maxParcels ? r.parcels.Length : maxParcels;
            if (maxParcels == 0)
            {
                parcelCount = r.parcels.Length;
            }
            for (int i = 0; i < parcelCount; i++)
            {
                if (i >= r.parcels.Length)
                {
                    break;
                }
                
                parcelQueue.Enqueue(r.parcels[i]);
                CreateParcel();
            }
        }
    }
    
    private void CreateParcel()
    {
        if (parcelQueue.Count <= 0) return;
        
        ParcelDescription p = parcelQueue.Dequeue();

        if (!string.IsNullOrEmpty(suburb))
        {
            string parcelSuburb = GetSuburb(p);
            if (parcelSuburb != suburb)
            {
                parcelCount++;
                return;
            }
        }

        Vector3 parcelPosition = new Vector3((p.x1 + p.x2) / 2f, -0.999f, (p.z1 + p.z2) / 2f);
        if (maxDistance != 0)
        {
            if (Vector3.Distance(new Vector3(centerPosition.x, 0, centerPosition.y), parcelPosition) > maxDistance)
            {
                parcelCount++;
                return;
            }
        }

        GameObject cube = new GameObject();
        cube.transform.position = parcelPosition;
        cube.name = "parcel-" + p.id;

        Parcel parcel = cube.AddComponent<Parcel>();
        parcel.SetDescription(p, atlasMaterial, glassMaterial, imageMaterial);
    }

    private string GetSuburb(ParcelDescription p)
    {
        WWW www = new WWW(PARCEL_URL + p.id + ".json");
        while(!www.isDone)
        {
            //Wait for URL to load, this hangs the editor, but is the easy solution.
        }
        
        if(www.error != null) 
        {
            Debug.LogError(www.error);
            return "Not_Found";
        }
        
        string parcelSuburb = www.text.Split(new []{"\"suburb\":"}, StringSplitOptions.None)[1].Split('\n')[0].Replace("\"", "").Split(',')[0];
        return parcelSuburb;
    } 
}

[System.Serializable]
public class Response
{
    public bool success;
    public ParcelDescription[] parcels;

    public static Response CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Response>(jsonString);
    }
}

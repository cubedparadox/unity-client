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
    private Queue<string> suburbQueue = new Queue<string>();
    private int parcelCount = 0;
    private List<GameObject> parcelObjects = new List<GameObject>();

    private bool _limitDistance;
    private bool _limitParcels;
    private bool _limitSuburb = true;
    private bool _overrideMaterials;
    private bool _overwriteExistingParcels = true;
    private bool _generateLightmapUVs;
    
    //[SerializeField]
    //private ParcelsContainer parcelCache;

    private ParcelCache _parcelCache;
    
    [MenuItem("Window/CryptoVoxel")]
    public static void Init()
    {
        CryptoVoxelWindow window = GetWindow<CryptoVoxelWindow>();
        window.titleContent = new GUIContent("CryptoVoxels");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("CryptoVoxel Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (_parcelCache == null || _parcelCache.parcelsContainer == null || _parcelCache.parcelsContainer.parcels == null || _parcelCache.parcelsContainer.parcels.Length == 0)
        {
            _parcelCache = Resources.Load<ParcelCache>("ParcelCache");
            if (_parcelCache == null)
            {
                _parcelCache = CreateInstance<ParcelCache>();
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");    
                }
                AssetDatabase.CreateAsset(_parcelCache, "Assets/Resources/ParcelCache.asset");
            }
            
            if (GUILayout.Button("Download Parcel Data"))
            {
                FetchParcelData();
            }
        }
        else
        {
            if (GUILayout.Button("Import Voxels"))
            {
                InstantiateParcels();
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Options");

        _limitParcels = EditorGUILayout.ToggleLeft("Limit by Parcel Count", _limitParcels);
        if (_limitParcels)
        {
            EditorGUI.indentLevel++;
            maxParcels = EditorGUILayout.IntField("Max Parcels", maxParcels);
            EditorGUI.indentLevel--;
        }
        
        _limitDistance = EditorGUILayout.ToggleLeft("Limit by Distance", _limitDistance);
        if (_limitDistance)
        {
            EditorGUI.indentLevel++;
            maxDistance = EditorGUILayout.IntField("Max Distance", maxDistance);
            centerPosition = EditorGUILayout.Vector2Field("Center Position", centerPosition);
            EditorGUI.indentLevel--;
        }

        _limitSuburb = EditorGUILayout.ToggleLeft("Limit by Suburb", _limitSuburb);
        if (_limitSuburb)
        {
            EditorGUI.indentLevel++;
            suburb = EditorGUILayout.TextField("Suburb", suburb);
            EditorGUI.indentLevel--;
        }
        
        _overrideMaterials = EditorGUILayout.ToggleLeft("Override Materials", _overrideMaterials);
        if (_overrideMaterials)
        {
            EditorGUI.indentLevel++;
            atlasMaterial = (Material)EditorGUILayout.ObjectField("Atlas Material", atlasMaterial, typeof(Material), false);
            imageMaterial = (Material)EditorGUILayout.ObjectField("Image Material", imageMaterial, typeof(Material), false);
            glassMaterial = (Material)EditorGUILayout.ObjectField("Glass Material", glassMaterial, typeof(Material), false);
            EditorGUI.indentLevel--;
        }

        //_generateLightmapUVs = EditorGUILayout.ToggleLeft("Generate Lightmap UVs", _generateLightmapUVs);
        if (GUILayout.Button("Generate Lightmap UVs"))
        {
            CreateLightmapUVs();
        }
        _overwriteExistingParcels = EditorGUILayout.ToggleLeft("Overwrite Existing Parcels", _overwriteExistingParcels);

        if (_parcelCache == null || _parcelCache.parcelsContainer == null || _parcelCache.parcelsContainer.parcels == null || _parcelCache.parcelsContainer.parcels.Length == 0) return;
        if (GUILayout.Button("Redownload Parcel Cache"))
        {
            FetchParcelData();
        }
    }

    private void FetchParcelData()
    {
        EditorUtility.DisplayProgressBar("Downloading Parcel Data", "Fetching parcels", .1f);
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
            
            _parcelCache.parcelsContainer = new ParcelsContainer();
            _parcelCache.parcelsContainer.parcels = r.parcels;
            _parcelCache.parcelsContainer.suburbs = new string[_parcelCache.parcelsContainer.parcels.Length];

            float i = 0;
            foreach (ParcelDescription parcel in _parcelCache.parcelsContainer.parcels)
            {
                string name = parcel.name;
                if (string.IsNullOrEmpty(name))
                {
                    name = parcel.id.ToString();
                }
                EditorUtility.DisplayProgressBar("Downloading Parcel Data", "Parcel " + name,  i/_parcelCache.parcelsContainer.parcels.Length);
                _parcelCache.parcelsContainer.suburbs[(int)i] = GetSuburb(parcel);
                i++;
            }
            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(_parcelCache);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    private void InstantiateParcels()
    {
        Debug.Log("Instantiating Parcels");
        if (!_overrideMaterials)
        {
            atlasMaterial = Resources.Load<Material>("Atlas");
            glassMaterial = Resources.Load<Material>("Glass");
            imageMaterial = Resources.Load<Material>("Image");

            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");    
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Resources");    
            }
            if (atlasMaterial == null)
            {
                atlasMaterial = new Material(Shader.Find("Particles/Standard Surface"));
                AssetDatabase.CreateAsset(atlasMaterial, "Assets/Materials/Resources/Atlas.mat");
            }
                
            if (imageMaterial == null)
            {
                imageMaterial = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(imageMaterial, "Assets/Materials/Resources/Image.mat");
            }
                
            if (glassMaterial == null)
            {
                glassMaterial = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(glassMaterial, "Assets/Materials/Resources/Glass.mat");
            }
        }

        parcelCount = _parcelCache.parcelsContainer.parcels.Length <= maxParcels ? _parcelCache.parcelsContainer.parcels.Length : maxParcels;
        if (maxParcels == 0 || !_limitParcels)
        {
            parcelCount = _parcelCache.parcelsContainer.parcels.Length;
        }
        
        for (int i = 0; i < parcelCount; i++)
        {
            if (i >= _parcelCache.parcelsContainer.parcels.Length)
            {
                break;
            }
                
            parcelQueue.Enqueue(_parcelCache.parcelsContainer.parcels[i]);
            suburbQueue.Enqueue(_parcelCache.parcelsContainer.suburbs[i]);
            CreateParcel();
        }
    }

    private void CreateParcel()
    {
        if (parcelQueue.Count <= 0) return;

        ParcelDescription p = parcelQueue.Dequeue();
        string parcelSuburb = suburbQueue.Dequeue();

        if (!string.IsNullOrEmpty(suburb) && _limitSuburb)
        {
            if (parcelSuburb != suburb)
            {
                parcelCount++;
                return;
            }
        }

        Vector3 parcelPosition = new Vector3((p.x1 + p.x2) / 2f, -0.999f, (p.z1 + p.z2) / 2f);
        if (maxDistance != 0 && _limitDistance)
        {
            if (Vector3.Distance(new Vector3(centerPosition.x, 0, centerPosition.y), parcelPosition) > maxDistance)
            {
                parcelCount++;
                return;
            }
        }

        if (_overwriteExistingParcels)
        {
            GameObject oldParcel = GameObject.Find("parcel-" + p.id);
            if (oldParcel != null)
            {
                DestroyImmediate(oldParcel);
            }
        }

        GameObject cube = new GameObject();
        cube.transform.position = parcelPosition;
        cube.name = "parcel-" + p.id;

        Parcel parcel = cube.AddComponent<Parcel>();
        parcel.SetDescription(p, atlasMaterial, glassMaterial, imageMaterial);

        //if (!_generateLightmapUVs) return;
        parcelObjects.Add(cube);
        //EditorApplication.delayCall += CreateLightmapUVs;
    }

    private void CreateLightmapUVs()
    {
        EditorUtility.DisplayProgressBar("Lightmap Unwrapping Parcels", "", 0);
        float j = 0;
        foreach (GameObject parcelObject in parcelObjects)
        {
            if (parcelObject == null)
            {
                j++;
                continue;
            }
            
            EditorUtility.DisplayProgressBar("Lightmap Unwrapping Parcels", parcelObject.name, j/parcelObjects.Count);
            List<MeshFilter> meshFilters = new List<MeshFilter>(parcelObject.GetComponentsInChildren<MeshFilter>());
            for (int i = meshFilters.Count - 1; i >= 0; i--)
            {
                if (meshFilters[i].sharedMesh == null)
                {
                    meshFilters.RemoveAt(i);
                    continue;
                }

                if (meshFilters[i].sharedMesh.uv2 != null && meshFilters[i].sharedMesh.uv2.Length > 0)
                {
                    meshFilters.RemoveAt(i);
                    continue;
                }

                Unwrapping.GenerateSecondaryUVSet(meshFilters[i].sharedMesh);
            }

            j++;
        }
        EditorUtility.ClearProgressBar();
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

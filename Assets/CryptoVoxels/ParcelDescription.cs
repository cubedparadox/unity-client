using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParcelsContainer
{
	public ParcelDescription[] parcels;
	public string[] suburbs;
}

[System.Serializable]
public class ParcelDescription
{
	public int id;
	public int x1;
	public int x2;
	public int y1;
	public int y2;
	public int z1;
	public int z2;
	public string name;
	public string voxels;
	public string tileset;
	public FeatureDescription[] features;
}

[System.Serializable]
public class FeatureDescription
{
	//"type": "sign",
	//"scale": [0.61875, 0.5, 0.5],
	//"text": "COMING SOON",
	//"position": [-8.75, 2, 5.5],
	//"rotation": [0, 1.5707963267948966, 0],
	//"uuid": "8f230c58-519c-40a5-a330-f015703b52b8",
	//"fontSize": 24

	public string uuid;
	public float[] position;
	public float[] scale;
	public float[] rotation;
	public string text;
	public string url;
	public int fontSize;
	public string type;
}
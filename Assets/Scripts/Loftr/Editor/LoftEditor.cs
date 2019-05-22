using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Loft))] 
public class LoftEditor : Editor 
{
	public override void OnInspectorGUI() 
	{
		Loft myTarget = (Loft) target;



		DrawDefaultInspector();
	}
}
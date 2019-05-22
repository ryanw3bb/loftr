using UnityEngine;

public static class GameObjectExtension
{
	public static T GetOrAddComponent<T> (this GameObject gameObject) where T : Component
	{
		return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
	}
		
	public static bool HasComponent<T> (this GameObject gameObject) where T : Component
	{
		return gameObject.GetComponent<T>() != null;
	}
}
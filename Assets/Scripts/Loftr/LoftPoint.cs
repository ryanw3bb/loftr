using UnityEngine;

public class LoftPoint 
{
	public int Id;
	public Vector3 P1;

	public float Distance { get; set; }
	public Vector3 Angle { get; set; }
	public Vector3 Cross { get; set; }

	public Vector3 Position 
	{ 
		get { return P1; } 
		set { P1 = value; }
	}

	public LoftPoint(int id, float distance, Vector3 position)
	{
		this.Id = id;
		this.Distance = distance;
		this.P1 = position;
	}

	public override string ToString()
	{
		return string.Format("[LoftPoint: Id={0}, Position={1}, Distance={2}, Normal={3}]", Id, Position, Distance, Cross);
	}
}
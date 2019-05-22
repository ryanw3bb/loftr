using UnityEngine;
using System;

public class LoftPointBezier : LoftPoint 
{
	public Vector3 P0;
	public Vector3 P2;

	public LoftPointBezier(int id, float distance, Vector3 p0, Vector3 p1, Vector3 p2) : base(id, distance, p1)
	{
		this.P0 = p0;
		this.P2 = p2;
	}

	public override string ToString()
	{
		return string.Format("[LoftPoint: Id={0}, Position={1}, Distance={2}, P0={3}, P2={4}]", Id, Position, Distance, P0, P2);
	}

	public string ToJson()
	{
		return "{\n\"p0\":\"" + P0.ToString("F2") + "\",\n\"p1\":\"" + P1.ToString("F2") + "\",\n\"p2\":\"" + P2.ToString("F2") + "\"\n}";
	}
}
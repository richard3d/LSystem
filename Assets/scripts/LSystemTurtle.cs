using UnityEngine;
using System.Collections;

[RequireComponent (typeof (LSystem))]
public class LSystemTurtle : MonoBehaviour {
	
	LSystem l;
	public Vector3 startPos;
	

	// Use this for initialization
	void Start () {
		l = GetComponent<LSystem>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

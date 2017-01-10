using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LSystemRule {
	public string a;
	public string b;
}

public class Branch {
	public float max_length = 1;
	public float length = 0;
	public Vector3 pos = Vector3.zero;
	public Vector3 dir = Vector3.up;
	public Vector3 orig_dir = Vector3.up;
	public Branch parent = null;
	public List<Branch> children = new List<Branch>();
	
	public void Draw() {
		GL.Begin(GL.LINES);			
		GL.Color(Color.red);
		GL.Vertex(pos);
		GL.Vertex(pos+dir*length);
		GL.End();
		for(int i = 0; i < children.Count; i++)
		{
			children[i].Draw();
		}
	}
	
	public void RecalcPos(float len) {
		length = len;
		if(parent != null) {
			pos = parent.pos + parent.dir * len;
		}
		
		for(int i = 0; i < children.Count; i++)
		{
			children[i].RecalcPos(len*0.95f);
		}
	}
	
	public void Grow() {
		if(length < max_length) {
			length += Time.deltaTime*4;
		}
		else{
			for(int i = 0; i < children.Count; i++)
			{
				children[i].Grow();
			}
		}
		
		
	}
}

public struct BranchTurtle {
	public Vector3 pos;
	public Quaternion rot;
	public Branch currBranch;
}

public class LSystem : MonoBehaviour {
	
	static public Material lineMaterial;
	public string sentence;
	public string axiom;
	public LSystemRule[] rules; 
	public bool turtle = true;
	public int num_iterations = 4;
	public int num_branches_to_draw = 1;
	public int debug_branch = 0;
	public float branch_length = 1;
	
	public float min_angle = 25;
	public float max_angle = 60;
	
	private Branch root = null;
	private List<Branch> tree_branches = new List<Branch>();
	
	// Use this for initialization
	void Start () {
		sentence = axiom;
		for(int i = 0; i < num_iterations; i++) {
			Generate();
		}
		BuildTree();
		num_branches_to_draw = tree_branches.Count;
	}
	
	// Update is called once per frame
	void Update () {
		//if (Input.GetMouseButtonDown(0)) {
			root.Grow();
		//}
	}
	
	void OnRenderObject () {
		if(turtle) {
			Turtle();
		} else {
			CreateLineMaterial();
			lineMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix (transform.localToWorldMatrix);
			root.Draw();
			GL.PopMatrix();
		}
	}
	
	static void CreateLineMaterial ()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find ("Hidden/Internal-Colored");
			lineMaterial = new Material (shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt ("_ZWrite", 0);
		}
	}
	
	//generatesa next gen of the L System based on the current sentence
	public void Generate() {
		string currentSentence = sentence;
		sentence = "";
		for(int i = 0; i < currentSentence.Length; i++) {
			string currChar = currentSentence[i].ToString();
			bool found = false;
			for(int j = 0; j < rules.Length; j++) {
				if(currChar == rules[j].a) {
					found = true;
					sentence += rules[j].b;
					break;
				}
			}
			if(!found) {
				sentence += currChar;
			}
		}
	}
	
	//renders the L System
	public void Turtle() {
		float b_length = branch_length;
		Matrix4x4 trs;
		CreateLineMaterial();
		lineMaterial.SetPass(0);
		
		GL.PushMatrix();
		GL.MultMatrix (transform.localToWorldMatrix);
		
		int num_branches_drawn = 0;
		
		for (int i = 0 ; i < sentence.Length; i++) {
			if(num_branches_drawn >= num_branches_to_draw)
				break;
			if(sentence[i] == 'F') {
				
				GL.Begin(GL.LINES);
				GL.Color(Color.red);
				GL.Vertex(Vector3.zero);
				GL.Vertex(Vector3.up * b_length);
				GL.End();
				//update the modelview matrix with our current info
				trs = Matrix4x4.TRS(Vector3.up* b_length, Quaternion.identity, Vector3.one);
				GL.modelview  = (GL.modelview * trs);
				num_branches_drawn++;
			} else if(sentence[i] == '+') {
				Quaternion rotation;
				rotation = Quaternion.Euler(0, 0, Random.Range(min_angle, max_angle));
				trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
				GL.modelview  = (GL.modelview * trs);
			} else if(sentence[i] == '-') {
				Quaternion rotation;
				rotation = Quaternion.Euler(0, 0, -Random.Range(min_angle, max_angle));
				trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
				GL.modelview  = (GL.modelview * trs);
			} else if(sentence[i] == '[') {
				GL.PushMatrix();
			} else if(sentence[i] == ']') {
				GL.PopMatrix();
			}
		}
		GL.PopMatrix();
	}
	

	public void BuildTree() {
		
		//initialize the root and add it to our list of branches
		root = new Branch();
		Branch currBranch = root;
		tree_branches.Add(root);
		
		//initialize the turtle that will help us create the branches
		BranchTurtle turtle = new BranchTurtle();
		turtle.rot = Quaternion.identity;
		turtle.pos = root.pos+root.dir;
		turtle.currBranch = root;
		
		Stack<BranchTurtle> branch_stack = new Stack<BranchTurtle>();
		float b_length = branch_length;
		
		for (int i = 0 ; i < sentence.Length; i++) {
			if(sentence[i] == 'F') {
				
				Branch next = new Branch();
				next.pos = turtle.pos;
				next.dir = turtle.rot * Vector3.up;
				next.parent = turtle.currBranch;
				next.parent.children.Add(next);
				tree_branches.Add(next);
				
				//keep track of the current branch and move the turtle
				turtle.currBranch = next;
				turtle.pos += turtle.rot * Vector3.up;
	
			} else if(sentence[i] == '+') {
				Quaternion rotation = Quaternion.Euler(0, 0, min_angle);
				turtle.rot =  turtle.rot * rotation;
			} else if(sentence[i] == '-') {
				Quaternion rotation = Quaternion.Euler(0, 0, -min_angle);
				turtle.rot =  turtle.rot * rotation;
			} else if(sentence[i] == '[') {
				branch_stack.Push(turtle);
			} else if(sentence[i] == ']') {
				turtle = branch_stack.Pop();
			}
		}
	}
	
	
}

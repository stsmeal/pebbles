using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graph.
/// </summary>
public class Graph {
	public List<Node> Nodes;
	public List<Path> Paths;

	/// <summary>
	/// Initializes a new instance of the <see cref="Graph"/> class.
	/// </summary>
	public Graph(){
		Nodes = new List<Node> ();
		Paths = new List<Path> ();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Graph"/> class.
	/// </summary>
	/// <param name="nodes">Nodes.</param>
	/// <param name="paths">Paths.</param>
	public Graph(List<Node> nodes, List<Path> paths){
		//Nodes = nodes;
		//Paths = paths;
	}

	/// <summary>
	/// Adds the node.
	/// </summary>
	/// <param name="n">N.</param>
	public void AddNode(Node n){
		n.Paths = new List<int> ();
		Nodes.Add (n);
	}


	/// <summary>
	/// Adds the path.
	/// </summary>
	/// <param name="p">P.</param>
	public void AddPath(Path p){
		if (p.VertexOne < Nodes.Count
		   && p.VertexTwo < Nodes.Count
		   && p.VertexOne == p.VertexTwo) {
			Nodes [p.VertexOne].Paths.Add (Paths.Count);
			Nodes [p.VertexTwo].Paths.Add (Paths.Count);
			p.SetPosition (Nodes [p.VertexOne], Nodes [p.VertexTwo]);
			Paths.Add (p);
		}
	}
}

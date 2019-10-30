using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graph manager.
/// </summary>
public class GraphManager : MonoBehaviour {
	[Header("Prefabs")]
	public Node NodePrefab;
	public Path PathPrefab;

	/// <summary>
	/// Gets or sets the current graph.
	/// </summary>
	/// <value>The current graph.</value>
	public Graph CurrentGraph{
		get {
			return graph;
		}
		set{
			graph = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="GraphManager"/> screen change.
	/// </summary>
	/// <value><c>true</c> if screen change; otherwise, <c>false</c>.</value>
	protected bool ScreenChange{
		get{ return screenChange; }
	}

	protected Graph graph = new Graph();
	private bool screenChange;
	private Vector2 OldScreen;


	/// <summary>
	/// Start this instance.
	/// </summary>
	protected virtual void Start(){
		OldScreen = Camera.main.pixelRect.size;
		this.screenChange = false;
	}


	/// <summary>
	/// Tick this instance.
	/// </summary>
	public virtual void Tick(){
		
		if (Camera.main.pixelRect.size != OldScreen) {
			screenChange = true;
			UpdatePaths ();
			OldScreen = Camera.main.pixelRect.size;
		} else {
			screenChange = false;
		}
	}

	/// <summary>
	/// Load the specified defaultNodes.
	/// </summary>
	/// <param name="defaultNodes">Default nodes.</param>
	public bool Load(List<Node> defaultNodes){
		List<Path> paths = new List<Path> ();
		List<Node> nodes = new List<Node> ();
		Dictionary<int, int> count = new Dictionary<int,int> ();

		for (int i = 0; i < defaultNodes.Count; i++) {
			foreach (int p in defaultNodes[i].Paths)
				if (count.ContainsKey (p))
					count [p]++;
				else
					count.Add (p, 1);
			
			Node n = Instantiate (defaultNodes[i], this.gameObject.transform);
			n.NodeId = i;
			n.gameObject.SetActive (true);

			nodes.Add (n);
		}

		bool validPaths = true;
		for (int i = 0; i < count.Count; i++) {
			if (!count.ContainsKey (i)) {
				Debug.Log ("Invalid Path, " + i + " does not exist");
				validPaths = false;
				break;
			} else if (count [i] != 2) {
				Debug.Log ("Invalid Path, " + i + " has " + count[i] + " nodes");
				validPaths = false;
				break;
			}
			var q = from n in nodes
			        where n.Paths.Contains (i)
			        select n.NodeId;

			Path tempPath = Instantiate (PathPrefab, this.gameObject.transform);
			var l = q.ToList ();
			tempPath.VertexOne = l [0];
			tempPath.VertexTwo = l [1];
			tempPath.SetPosition (nodes [l [0]], nodes [l [1]]);
			paths.Add (tempPath);
		}

		if (validPaths) {
			graph = new Graph ();
			graph.Nodes = nodes;
			graph.Paths = paths;
			return true;
		}

		return false;
	}


	/// <summary>
	/// Attacheds the nodes.
	/// </summary>
	/// <returns>The nodes.</returns>
	/// <param name="originNode">Origin node.</param>
	public List<Node> attachedNodes(Node originNode){
		List<Node> nodes = new List<Node> ();

		foreach (int i in originNode.Paths)
			nodes.Add(graph.Nodes[graph.Paths [i].End (originNode.NodeId)]);

		return nodes;
	}

	/*
	protected List<PebbleMove> FindPathToNode(Node CurrentNode, Node DestinationNode){
		Dictionary<int,int> MovesTo = new Dictionary<int, int>();
		List<PebbleMove> Moves = new List<PebbleMove> ();

		if (CurrentNode.NodeId != DestinationNode.NodeId) {
			return FindPathToNode (MovesTo, Moves, CurrentNode.NodeId, DestinationNode.NodeId);
		}

		return new List<PebbleMove> ();
	}

	private List<PebbleMove> FindPathToNode(ref Dictionary<Node,int> MovesTo, ref List<PebbleMove> moves, int cNodeId, int dNodeId){
		if (cNodeId != dNodeId) {

		}
	}
	*/


	/// <summary>
	/// Updates the paths.
	/// </summary>
	private void UpdatePaths(){
		foreach (Path p in graph.Paths) {
			p.SetPosition (graph.Nodes [p.VertexOne], graph.Nodes [p.VertexTwo]);
		}
	}
}

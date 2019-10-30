using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI2 {
	protected class Lnode
	{
		public int pebbles;
		public int id;
		public int[] paths;
	}

	protected class Lpath{
		public int v1;
		public int v2;
	}

	protected class Lmove{
		public int p1;
		public int p2;
	}

	protected Lnode[] nodes;
	protected Lpath[] paths;
	protected Dictionary<int,int> goalDistance;
	protected Dictionary<int,int[]> connectedNodes;
	protected int gId;

	private List<Path> Paths;


	public AI2(Graph graph){
		gId = 0;
		paths = new Lpath[graph.Paths.Count];

		setNodes (graph.Nodes);

		gId = graph.Nodes.Find (n => n.State == NodeState.Target).NodeId;

		for (int i = 0; i < graph.Paths.Count; i++) {
			paths[i] = new Lpath () {
				v1 = graph.Paths[i].VertexOne,
				v2 = graph.Paths[i].VertexTwo
			};
		}

		setConnectedNodes (nodes);

		goalDistance = getGoalDistances ();
	}

	public void DefenderMoveNoHeuristics(ref int originNode,ref int destinationNode){
		List<Lmove> moves = new List<Lmove> ();

		moves.Add (new Lmove () {
			p1 = originNode,
			p2 = destinationNode
		});

		nodes [originNode].pebbles -= 2;
		nodes [destinationNode].pebbles += 1;

		List<int> awms = new List<int> ();
		int aw = 0;

		List<Lmove> defMoves = defenderMoves (nodes, moves[0]);


		foreach (Lmove move in defMoves) {
			/*
			if (move.p2 == gId 
				|| (nodes[move.p2].pebbles > 0 && connectedNodes[move.p2].Contains(gId))) {
				awms.Add (1);
			} else {
				xMoves (move, moves, nodes, ref aw);
				awms.Add (aw);
				aw = 0;
			}*/
			xMoves (move, moves, nodes, ref aw);
			awms.Add (aw);
			aw = 0;
		}

		int index = 0;
		for (int i = 0; i < awms.Count; i++) {
			Debug.Log (awms [i]);
			Debug.Log (defMoves [i].p1 + "-> " + defMoves [i].p2);
			if (awms [i] < awms [index])
				index = i;
		}

		originNode = defMoves [index].p1;
		destinationNode = defMoves [index].p2;

		nodes [originNode].pebbles -= 2;
		nodes [destinationNode].pebbles += 1;
	}

	protected void xMoves(Lmove lastMove, List<Lmove> moves, Lnode[] _nodes, ref int aw){
		List<Lmove> cMoves = copyMoves (moves);
		cMoves.Add (lastMove);
		Lnode[] cNodes = copyNodes (_nodes);

		cNodes [lastMove.p1].pebbles -= 2;
		cNodes [lastMove.p2].pebbles += 1;

		List<Lmove> nMoves = (moves.Count % 2 == 0)? attackerMoves (cNodes): defenderMoves (cNodes,lastMove);

		foreach (Lmove move in nMoves) {
			if (move.p2 == gId
				|| (_nodes[move.p2].pebbles > 0 && connectedNodes[move.p2].Contains(gId))) {
				aw += 1;
			} else {
				xMoves (move, cMoves, cNodes, ref aw);
			}
		}
	}

	protected void print(Lnode[] ns){
		foreach (Lnode n in ns) {
			Debug.Log ("Node " + n.id + "\n" + "  pebbles: " + n.pebbles);
		}
	}

	protected List<Lmove> defenderMoves(Lnode[] _nodes, Lmove lastMove){
		List<Lmove> moves = new List<Lmove> ();
		Lnode[] cNodes;

		foreach (Lnode oNode in _nodes) {
			if (oNode.pebbles > 1) {
				cNodes = getConnectedNodes (oNode.id, _nodes);
				foreach (Lnode dNode in cNodes) {
					if (!(oNode.id == lastMove.p2 && dNode.id == lastMove.p1)) {
						moves.Add (new Lmove (){ p1 = oNode.id, p2 = dNode.id });
					}
				}
			}
		}


		return moves;
	}

	protected List<Lmove> attackerMoves(Lnode[] _nodes){
		List<Lmove> moves = new List<Lmove> ();
		Lnode[] cNodes;

		foreach (Lnode oNode in _nodes) {
			if (oNode.pebbles > 1) {
				cNodes = getConnectedNodes (oNode.id, _nodes);
				foreach (Lnode dNode in cNodes) {
					if (dNode.paths.Length > 1) {
						moves.Add (new Lmove (){ p1 = oNode.id, p2 = dNode.id });
					} else {
						if (nodes.Count ((Lnode node) => 
							node.pebbles > 1 && node != dNode && node != oNode) > 0 || dNode.id == gId) {
							moves.Add (new Lmove (){ p1 = oNode.id, p2 = dNode.id });
						}
					}
				}
			}
		}

		return moves;
	}

	protected Lnode[] copyNodes(Lnode[] _nodes){
		Lnode[] newNodes = new Lnode[_nodes.Length];

		for(int i = 0; i < _nodes.Length; i++) {
			newNodes[i] = new Lnode (){ 
				pebbles = _nodes[i].pebbles,
				id = _nodes[i].id,
				paths = _nodes[i].paths
			};
		}

		return newNodes;
	}

	protected List<Lmove> copyMoves (List<Lmove> moves){
		List<Lmove> newMoves = new List<Lmove> ();

		foreach (Lmove move in moves) {
			newMoves.Add (new Lmove (){ p1 = move.p1, p2 = move.p2 });
		}

		return newMoves;
	}

	public void setNodes(List<Node> Nodes){
		nodes = new Lnode[Nodes.Count];

		for (int i = 0; i < Nodes.Count; i++) {
			nodes[i] = new Lnode () {
				pebbles = Nodes[i].pebbles,
				id = Nodes[i].NodeId,
				paths = Nodes[i].Paths.ToArray()
			};
		}
	}

	protected Lnode[] getConnectedNodes(int id, Lnode[] cNodes){
		Lnode[] ns = new Lnode[connectedNodes [id].Length];

		for (int i = 0; i < connectedNodes [id].Length; i++) {
			ns [i] = nodes[connectedNodes [id] [i]];
		}

		return ns;
	}

	protected void setConnectedNodes(Lnode[] ns){
		connectedNodes = new Dictionary<int,int[]> ();

		foreach (Lnode node in ns) {
			int[] ids = new int[node.paths.Length];

			for(int i = 0; i < node.paths.Length; i++){
				ids[i] = connectedNode (node.paths[i], node.id);
			}

			connectedNodes.Add (node.id, ids);
		}
	}

	public Dictionary<int,int> getGoalDistances(){
		Dictionary<int,int> distances = new Dictionary<int, int> ();
		int cDistance = 1;
		int[] cn = connectedNodes[gId];

		distances.Add (gId, 0);

		foreach (int id in cn) {
			distances.Add (id, cDistance);
		}

		foreach (int id in cn) {
			getGoalDistances (ref distances, cDistance, id);
		}

		return distances;
	}

	private void getGoalDistances(ref Dictionary<int,int> distances, int cDistance, int id){
		int[] ids = connectedNodes[id];

		cDistance++;

		foreach (int i in ids) {
			if (!distances.ContainsKey (i)) {
				distances.Add (i, cDistance);
				getGoalDistances (ref distances, cDistance, i);
			} else if (distances [i] > cDistance) {
				distances[i] = cDistance;
				getGoalDistances (ref distances, cDistance, i);
			}
		}
	}

	private int connectedNode(int path, int node){
		return (paths [path].v1 == node) ? paths [path].v2 : paths [path].v1;
	}
}


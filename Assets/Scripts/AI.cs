using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI {

	protected struct LNode{
		public byte[] ConnectedNodes;
		public byte GoalDistance;
	}

	protected struct LMove{
		public byte o;
		public byte d;
	}

	protected byte Size;
	protected byte GId;
	protected LNode[] Nodes;
	protected byte[] Pebbles;

	byte[] FirstFailingNodes;
	byte[] SecondFailingNodes;
	byte[] FirstFailingParentNodes;
	byte[][] FirstFailingChildNodes;
	byte[] SecondFailingParentNodes;
	byte[][] SecondFailingChildNodes;

	public AI(Graph graph){
		Size = (byte)graph.Nodes.Count;
		Nodes = new LNode[Size];
		Pebbles = new byte[Size];
		LNode tempNode;
		int temp;

		for (int i = 0; i < Size; i++) {
			Pebbles [i] = (byte)graph.Nodes [i].Pebbles;

			tempNode = new LNode () {
				ConnectedNodes = new byte[graph.Nodes [i].Paths.Count],
				GoalDistance = 255
			};

			for (int p = 0; p < tempNode.ConnectedNodes.Length; p++) {
				temp = graph.Nodes [i].Paths [p];
				tempNode.ConnectedNodes [p] = (byte)((graph.Paths [temp].VertexOne == i) ?
					graph.Paths [temp].VertexTwo :
					graph.Paths [temp].VertexOne);
			}

			Nodes [i] = tempNode;

			if (graph.Nodes [i].State == NodeState.Target) {
				GId = (byte)i;
			}
		}

		SetGoalDistance ();
		SetFirstFailingNodes ();
		SetSecondFailingNodes ();
	}

	public void DefenderMove(ref int O, ref int D){
		List<double> awm = new List<double> ();
		bool attackerTurn = false;
		LMove lastMove = new LMove (){ o = (byte)O, d = (byte)D };
		ulong aw = 0;
		ulong tm = 0;

		Pebbles [O] -= 2;
		Pebbles [D]++;

		List<LMove> moves = GetDefenderMoves (Pebbles, lastMove);
		tm = (ulong)moves.Count;

		double d = 0d;
		foreach (LMove move in moves) {
			DefenderMove (Pebbles, move, attackerTurn, ref aw, ref tm);
			d = (double)aw /(double) tm;
			awm.Add (d);
			aw = 0;
			tm = (ulong)moves.Count;
		}

		if (moves.Count == 0) {
			foreach (byte n in Nodes[GId].ConnectedNodes) {
				if (Pebbles [n] > 0x01) {
					moves.Add (new LMove (){ o = n, d = GId });
					awm.Add (.5d);
				} else if (Pebbles [n] == 0x01) {
					foreach(byte v in Nodes[n].ConnectedNodes){
						if(Pebbles [v] > 0x01){
							moves.Add (new LMove (){ o = v, d = n });
							awm.Add (.25d);
						}
					}
				}
			}

			for (byte i = 0; i < Size; i++) {
				if (GId != i && Pebbles [i] > 0x01 && !Nodes [GId].ConnectedNodes.Contains (i)) {
					foreach (byte n in Nodes[i].ConnectedNodes) {
						if (!(Pebbles [n] == 0x01 && Nodes [n].ConnectedNodes.Contains (GId))) {
							if (!(lastMove.d == i && lastMove.o == n)) {
								moves.Add (new LMove (){ o = i, d = n });
								awm.Add (.1d);
							}
						}
					}
				}
			}
		}

		int min = 0;
		for (int i = 0; i < moves.Count; i++) {
			if (awm [i] < awm [min]) {
				min = i;
			}
		}

		O = (int)moves [min].o;
		D = (int)moves [min].d;

		Pebbles [O] -= 2;
		Pebbles [D]++;
	}

	private void DefenderMove(byte[] pebbles, LMove lastMove, bool attackerTurn, ref ulong aw, ref ulong tm){
		byte[] cPebbles = new byte[Size];
		System.Array.Copy (pebbles, 0, cPebbles, 0, Size);

		cPebbles [lastMove.o] -= 2;
		cPebbles [lastMove.d]++;

		attackerTurn = !attackerTurn;

		List<LMove> moves = attackerTurn ?
			GetAttackerMoves (cPebbles) :
			GetDefenderMoves (cPebbles, lastMove);

		//if () {
			foreach (LMove move in moves) {
				tm++;
			if (move.d == GId /*|| (attackerTurn && Nodes [move.d].ConnectedNodes.Contains (GId))*/) {
					aw++;
				} else {
				DefenderMove (cPebbles, move, attackerTurn, ref aw, ref tm);
				}
			}
		//}

	}

	protected List<LMove> GetDefenderMoves(byte[] pebbles, LMove lastMove){
		List<LMove> moves = new List<LMove> ();
		byte temp = 0;

		for (byte i = 0; i < Size; i++) {
			if (pebbles [i] > 0x01) {
				foreach (byte n in Nodes[i].ConnectedNodes) {
					if (!(i == lastMove.d && n == lastMove.o) && n != GId && 
						!(pebbles[n] > 0x00 && Nodes[n].ConnectedNodes.Contains(GId))) {
						moves.Add (new LMove (){ o = i, d = n });
					} 
				}
			}
		}

		//Remove first failing nodes
		for (int i = 0; i < FirstFailingParentNodes.Length; i++) {
			if (pebbles [FirstFailingParentNodes [i]] > 0 && pebbles [FirstFailingNodes [i]] > 0) {
				if (FirstFailingChildNodes [i].Count ((byte n) => pebbles [n] == 0) == 0) {
					moves.RemoveAll ((LMove move) => move.d == FirstFailingParentNodes [i]);
				}
			}
		}

		//remove second failing nodes
		for (int i = 0; i < Size; i++) {
			if (pebbles [i] > 0x01) {
				temp++;
			}
		}

		if (temp == 1) {
			for(int i = 0; i < SecondFailingParentNodes.Length; i++) {
				if (pebbles [SecondFailingParentNodes [i]] == 0x02 ||
					pebbles [SecondFailingParentNodes [i]] == 0x03) {
					if(pebbles [SecondFailingNodes[i]] == 0x01){
						if (SecondFailingChildNodes [i].Count ((byte n) => pebbles [n] == 0x01) > 0) {
							moves.RemoveAll ((LMove move) => move.d == SecondFailingNodes [i]);
							temp--;
							break;
						}
					}
				}
			}
		}

		if (temp == 1) {
			for (byte i = 0; i < Size; i++) {
				if (pebbles [i] == 0x02 || pebbles [i] == 0x03) {
					if (!SecondFailingParentNodes.Contains (i)) {
						for(int z = 0; z < SecondFailingParentNodes.Length; z++) {
							if ((pebbles [SecondFailingParentNodes[z]] == 0x02 || pebbles [SecondFailingParentNodes[z]] == 0x03)
								&& Nodes[SecondFailingParentNodes[z]].ConnectedNodes.Contains(i)) {
								if (pebbles [SecondFailingNodes [z]] == 0x01) {
									if (SecondFailingChildNodes [z].Count ((byte p) => pebbles [p] == 0) == 0) {
										moves.RemoveAll ((LMove move) => move.d == SecondFailingParentNodes [z]);
										break;
									}
								}
							}
						}
						break;
					}
				}
			}
		}

		if (temp == 2) {
			for (byte i = 0; i < Size; i++) {
				if (pebbles [i] == 0x02 || pebbles [i] == 0x03) {
					if (!SecondFailingParentNodes.Contains (i)) {
						for (int z = 0; z < SecondFailingParentNodes.Length; z++) {
							if (pebbles [SecondFailingParentNodes [z]] == 0x02 && Nodes [SecondFailingParentNodes [z]].ConnectedNodes.Contains (i)) {
								if (pebbles [SecondFailingNodes [z]] == 0x01) {
									if (SecondFailingChildNodes [z].Count ((byte p) => pebbles [p] == 0) == 0) {
										moves.RemoveAll ((LMove move) => move.d == SecondFailingParentNodes [z]);
										break;
									}
								}
							} else if(pebbles [SecondFailingParentNodes[z]] == 0x01 && Nodes [SecondFailingParentNodes[z]].ConnectedNodes.Count((byte n) => pebbles [n] > 0) > 2){
								moves.RemoveAll ((LMove move) => move.d == SecondFailingParentNodes [z]);
								break;
							}
						}
						break;
					}
				}
			}
		}

		return moves;
	}

	protected List<LMove> GetAttackerMoves(byte[] pebbles){
		List<LMove> moves = new List<LMove> ();

		byte count = 0;
		byte pebbleCount = 0;
		for (byte i = 0; i < Size; i++) {
			if (pebbles [i] > 0x01 && Nodes [i].ConnectedNodes.Length > 1) {
				count++;
			}
			if (pebbles [i] > 0x01) {
				pebbleCount++;
			}
		}

		//Check for winning move from second clause
		//of the beatable graph theorem
		if(SecondFailingNodes.Length > 0 
			&& pebbles.Count((byte p) => p > 1 && !SecondFailingNodes.Contains(p)) == 1){
			byte ln = (byte)pebbles.Where ((byte p) => p > 1 && !SecondFailingNodes.Contains (p)).First(); 

			foreach (byte fn in SecondFailingNodes) {
				if (pebbles [fn] > 1 && Nodes[fn].ConnectedNodes.Contains(ln)) {
					foreach (byte n in Nodes[fn].ConnectedNodes) {
						if (n != ln && pebbles [n] > 0x00) {
							moves.Clear ();
							moves.Add(new LMove(){ o = fn, d = n });
							return moves;
						}
					}
				}
			}
		}

		if (count != 1) {
			for (byte i = 0; i < Size; i++) {
				if (pebbles [i] > 0x01) {
					//Check for winning move from first clause
					//of the beatable graph theorem
					if (FirstFailingNodes.Length > 0) {
						foreach (byte n in Nodes[i].ConnectedNodes) {
							if (n == GId) {
								moves.Clear ();
								moves.Add (new LMove (){ o = i, d = n });
								return moves;
							}
							if (pebbles[n] > 0x00 
								&& FirstFailingNodes.Contains (n) 
								&& Nodes[n].ConnectedNodes.Count((byte an) => pebbles[an] == 0  && an != GId) == 0) {
								moves.Clear ();
								moves.Add (new LMove (){ o = i, d = n });
								return moves;
							}
							moves.Add (new LMove (){ o = i, d = n });
						}
					} else {
						foreach (byte n in Nodes[i].ConnectedNodes) {
							if (n == GId) {
								moves.Clear ();
								moves.Add (new LMove (){ o = i, d = n });
								return moves;
							}
							moves.Add (new LMove (){ o = i, d = n });
						}
					}
				} 
			}
		} else {
			for (byte i = 0; i < Size; i++) {
				if (pebbles [i] > 1) {
					if (pebbles [i] < 4) {
						foreach (byte n in Nodes[i].ConnectedNodes) {
							if (Nodes [n].ConnectedNodes.Length != 1 || n == GId) {
								if (n == GId) {
									moves.Clear ();
									moves.Add (new LMove (){ o = i, d = n });
									return moves;
								}
								moves.Add (new LMove (){ o = i, d = n });
							} else if (n == GId) {
								moves.Clear ();
								moves.Add (new LMove (){ o = i, d = n });
								return moves;
							}
						}
					} else {
						if (FirstFailingNodes.Length > 0) {
							foreach (byte n in Nodes[i].ConnectedNodes) {
								if (n == GId) {
									moves.Clear ();
									moves.Add (new LMove (){ o = i, d = n });
									return moves;
								}
								if (pebbles[n] > 0x00 
									&& FirstFailingNodes.Contains (n) 
									&& Nodes[n].ConnectedNodes.Count((byte an) => pebbles[an] == 0 && an != GId) == 0) {
									moves.Clear ();
									moves.Add (new LMove (){ o = i, d = n });
									return moves;
								}
								moves.Add (new LMove (){ o = i, d = n });
							}
						} else {
							foreach (byte n in Nodes[i].ConnectedNodes) {
								if (n == GId) {
									moves.Clear ();
									moves.Add (new LMove (){ o = i, d = n });
									return moves;
								}
								moves.Add (new LMove (){ o = i, d = n });
							}
						}
					}
				}
			}
		}

		return moves;
	}

	private void SetFirstFailingNodes(){
		List<byte> failingNodes = new List<byte> ();
		List<byte> failingParentNodes = new List<byte> ();
		List<List<byte>> failingChildNodes = new List<List<byte>> ();

		foreach (byte arn in Nodes[GId].ConnectedNodes) {
			if(Nodes[arn].ConnectedNodes.Count((byte n) => Nodes [n].GoalDistance > 1) < 2 
				&& Nodes[arn].ConnectedNodes.Length > 1){
				failingNodes.Add (arn);
				List<byte> cNodes = new List<byte> ();
				for (byte i = 0; i < Nodes [arn].ConnectedNodes.Length; i++) {
					if (Nodes [Nodes [arn].ConnectedNodes [i]].GoalDistance > 1) {
						failingParentNodes.Add (Nodes [arn].ConnectedNodes [i]);
					} else if (Nodes [arn].ConnectedNodes [i] != GId) {
						cNodes.Add (Nodes [arn].ConnectedNodes [i]);
					}
				}
				failingChildNodes.Add (cNodes);
			}
		}
			
		FirstFailingNodes = new byte[failingNodes.Count];
		FirstFailingNodes = failingNodes.ToArray ();

		FirstFailingParentNodes = new byte[failingParentNodes.Count];
		FirstFailingParentNodes = failingParentNodes.ToArray ();

		FirstFailingChildNodes = new byte[failingChildNodes.Count][];
		for (int i = 0; i < failingChildNodes.Count; i++) {
			FirstFailingChildNodes [i] = new byte[failingChildNodes [i].Count];
			FirstFailingChildNodes [i] = failingChildNodes [i].ToArray ();
		}
	}

	private void SetSecondFailingNodes(){
		List<byte> failingNodes = new List<byte> ();
		List<byte> failingParentNodes = new List<byte> ();
		List<List<byte>> failingChildNodes = new List<List<byte>> ();

		foreach (byte arn in Nodes[GId].ConnectedNodes) {
			foreach (byte n in Nodes[arn].ConnectedNodes) {
				if (n != GId && !failingNodes.Contains(n)
					&& Nodes [n].ConnectedNodes.Count ((byte ans) => Nodes [ans].GoalDistance > 1) == 1) {
					failingNodes.Add (n);
					List<byte> cNodes = new List<byte> ();
					for (byte i = 0; i < Nodes [n].ConnectedNodes.Length; i++) {
						if (Nodes [Nodes [n].ConnectedNodes [i]].GoalDistance > 1) {
							failingParentNodes.Add (Nodes [n].ConnectedNodes [i]);
						} else {
							cNodes.Add (Nodes [n].ConnectedNodes [i]);
						}
					}
					failingChildNodes.Add (cNodes);
				}
			}
		}

		SecondFailingNodes = new byte[failingNodes.Count];
		SecondFailingNodes = failingNodes.ToArray ();

		SecondFailingParentNodes = new byte[failingParentNodes.Count];
		SecondFailingParentNodes = failingParentNodes.ToArray ();

		SecondFailingChildNodes = new byte[failingChildNodes.Count][];
		for (int i = 0; i < failingChildNodes.Count; i++) {
			SecondFailingChildNodes [i] = new byte[failingChildNodes [i].Count];
			SecondFailingChildNodes [i] = failingChildNodes [i].ToArray ();
		}
	}

	private void SetGoalDistance(){
		byte cd = 1;

		Nodes [GId].GoalDistance = 0;

		foreach (byte n in Nodes[GId].ConnectedNodes) {
			Nodes [n].GoalDistance = cd;
		}

		foreach (byte n in Nodes[GId].ConnectedNodes) {
			SetGoalDistance (n, cd);
		}
	}

	private void SetGoalDistance(byte id, byte cd){
		cd++;

		foreach (byte n in Nodes[id].ConnectedNodes) {
			if (cd < Nodes [n].GoalDistance) {
				Nodes [n].GoalDistance = cd;
				SetGoalDistance (n, cd);
			}
		}
	}
}

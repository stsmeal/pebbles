using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialState : AState {

	[Header("Default Graph")]
	public GameObject DefaultGraph;

	[Header("UI")]
	public ConfirmPopup confirmPopup;
	public GameObject Prompt;
	public Text Message;
	public Text TurnText;
	public GameObject LeftArrow;
	public GameObject RightArrow;

	[Header("Other")]
	public GraphManager graphManager;
	public GameObject graphSpace;

	protected bool AttackerTurn{
		get{
			return attackerTurn;
		}
		set{
			TurnText.text = (value) ? "Attacker's Turn" : "Defender's Turn";
			attackerTurn = value;
		}
	}

	protected Graph graph;
	protected int step;
	protected bool attackerTurn;

	public override void Enter(AState from){
		Node[] ns = DefaultGraph.GetComponentsInChildren<Node> ();
		List<Node> nl = new List<Node> ();

		foreach (Node n in ns)
			nl.Add (n);

		if (graphManager.Load (nl)) {
			graph = graphManager.CurrentGraph;
			step = 0;
			this.gameObject.SetActive (true);
			LeftArrow.SetActive (false);
			RightArrow.SetActive (true);
			Prompt.SetActive (true);
			graphSpace.SetActive (true);
			attackerTurn = true;
			Message.text = "Welcome to Pebbles\nPress the right arrow to continue";
		}
	}

	public override void Exit(AState to){
		this.gameObject.SetActive (false);
		Helper.DestroyChildren (graphSpace);
	}

	public override void Tick(){

	}

	public override string GetName(){
		return "Tutorial";
	}

	public void Back(){
		confirmPopup.Open ("");
	}

	public void Exit(string s){
		GameManager.instance.SwitchState ("MainMenu");
	}

	public void NextStep(){
		GameManager.instance.playHighClick ();

		if (step == 0) {
			LeftArrow.SetActive (true);
		} 

		step++;

		DoStep ();
	}

	public void LastStep(){
		GameManager.instance.playClick ();

		step--;

		if (step == 0) {
			LeftArrow.SetActive (false);
		} 

		DoStep ();
	}

	private void DoStep(){
		switch (step) {
		case 0:
			Prompt.SetActive (true);
			Message.text = "Welcome to Pebbles!\nPress the right arrow to continue";
			break;
		case 1:
			Prompt.SetActive (true);
			Message.text = "Pebbles is a game based on graph pebbling.";
			break;
		case 2:
			Prompt.SetActive (true);
			Message.text = "The game consists of a connected graph of nodes which hold pebbles"
				+" that can be moved around.";
			break;
		case 3:
			Prompt.SetActive (true);
			Message.text = "It is a two-player game. One player is the attacker, and the other is the defender";
			break;
		case 4:
			Prompt.SetActive (true);
			Message.text = "The objective of the attacker is to move a pebble on the goal node.\n"
				+ "The defender must protect the goal node, or they will lose.\n"
				+ "The attacker moves first.";
			break;
		case 5:
			Prompt.SetActive (true);
			Message.text = "Click a node with at least two pebbles to select it.\n"
				+"Then, the possible nodes to move to will be highlighted.";
			break;
		case 6:
			Prompt.SetActive (true);
			Message.text = "Pressing the right arrow will show an attacker move.";

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [3].Pebbles = 14;
			graph.Nodes [0].Pebbles = 20;
			graph.Nodes [4].Pebbles = 1;

			graph.Nodes [5].State = NodeState.Target;
			break;
		case 7:
			Prompt.SetActive (false);

			AttackerTurn = true;

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [3].Pebbles = 14;
			graph.Nodes [0].Pebbles = 20;
			graph.Nodes [4].Pebbles = 1;

			graph.Nodes [5].State = NodeState.Target;
			graph.Nodes [3].State = NodeState.Active;
			graph.Nodes [6].State = NodeState.Hightlighted;
			graph.Nodes [4].State = NodeState.Hightlighted;
			break;
		case 8:
			Prompt.SetActive (false);

			AttackerTurn = true;

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [5].State = NodeState.Target;

			graph.Nodes [3].Pebbles = 12;
			graph.Nodes [0].Pebbles = 20;
			graph.Nodes [4].Pebbles = 2;

			AttackerTurn = false;
			break;
		case 9:
			Prompt.SetActive (true);
			Message.text = "Pressing the right arrow will show a defender move.\n"
				+"Notice, the defender cannot undo the previous move";
			break;
		case 10:
			Prompt.SetActive (false);

			AttackerTurn = false;

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [5].State = NodeState.Target;
			graph.Nodes [4].State = NodeState.Active;
			graph.Nodes [6].State = NodeState.Hightlighted;
			graph.Nodes [7].State = NodeState.Hightlighted;

			graph.Nodes [3].Pebbles = 12;
			graph.Nodes [0].Pebbles = 20;
			graph.Nodes [4].Pebbles = 2;

			AttackerTurn = false;
			break;
		case 11:
			Prompt.SetActive (false);

			AttackerTurn = true;

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [5].State = NodeState.Target;

			graph.Nodes [3].Pebbles = 12;
			graph.Nodes [0].Pebbles = 20;
			graph.Nodes [6].Pebbles = 1;
			break;
		case 12:
			Prompt.SetActive (true);
			Message.text = "Defender wins when there are no moves left.";
			break;
		case 13:
			Prompt.SetActive (true);
			Message.text = "Next, shows a move where the attacker wins.";
			break;
		case 14:
			Prompt.SetActive (false);

			AttackerTurn = true;

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [5].State = NodeState.Target;
			graph.Nodes [7].State = NodeState.Active;
			graph.Nodes [2].State = NodeState.Hightlighted;
			graph.Nodes [4].State = NodeState.Hightlighted;
			graph.Nodes [1].State = NodeState.Hightlighted;

			graph.Nodes [3].Pebbles = 2;
			graph.Nodes [0].Pebbles = 8;
			graph.Nodes [7].Pebbles = 3;
			break;
		case 15:
			Prompt.SetActive (false);

			AttackerTurn = true;

			foreach (Node node in graph.Nodes) {
				node.Pebbles = 0;
				node.State = NodeState.Inactive;
			}

			graph.Nodes [5].State = NodeState.Target;

			graph.Nodes [3].Pebbles = 2;
			graph.Nodes [0].Pebbles = 8;
			graph.Nodes [7].Pebbles = 1;
			break;
		case 16:
			Prompt.SetActive (true);
			Message.text = "Now you are ready to play!\n"
				+"Press the right arrow to continue to the main menu";
			break;
		case 17:
			GameManager.instance.SwitchState ("MainMenu");
			break;
		default:
			Debug.Log (step);
			step = 0;
			DoStep ();
			break;
		}
	}
}

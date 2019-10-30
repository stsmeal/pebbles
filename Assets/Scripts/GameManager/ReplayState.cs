using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ReplayState : AState {
	[Header("Containers")]
	public GameObject GraphSpace;
	public GameObject ReplayMenu;

	[Header("Menu Contents")]
	public GameObject MenuContent;

	[Header("UI")]
	public GameObject ReplayUI;
	public Text TurnText;
	public Text MovesText;
	public GameObject LeftArrow;
	public GameObject RightArrow;

	[Header("Prefabs")]
	public Node NodePrefab;
	public Path PathPrefab;
	public DeleteMenuButton DeleteMenuButtonPrefab;

	[Header("Other")]
	public GraphManager graphManager;
	public ConfirmPopup ConfirmDeletePopup;
	public NotificationPopup Note;

	protected bool AttackerTurn{
		get{return attackerTurn;}
		set{
			if (value) 
				TurnText.text = "Attacker's Turn";
			else
				TurnText.text = "Defender's Turn";

			attackerTurn = value;
		}
	}

	protected int CurrentMove{
		get{return currentMove;}
		set{
			MovesText.text = "Current Move: " + value;
			currentMove = value;
		}
	}

	private List<PebbleMove> moves;
	private int currentMove;
	private bool attackerTurn;
	private bool attackerWins;

	public override void Enter (AState from){
		gameObject.SetActive (true);

		ReplayMenu.SetActive (true);
		GraphSpace.SetActive (false);
		ReplayUI.SetActive (false);

		moves = new List<PebbleMove> ();
		currentMove = 0;
		AttackerTurn = true;

		ConfirmDeletePopup.ConfirmClick.AddListener (DeleteConfirm);

		LoadMenuContent ();
	}

	public override void Exit (AState to){
		moves = new List<PebbleMove> ();

		ReplayMenu.SetActive (false);
		GraphSpace.SetActive (false);
		ReplayUI.SetActive (false);

		ConfirmDeletePopup.ConfirmClick.RemoveListener (DeleteConfirm);

		Helper.DestroyChildren (GraphSpace);
		Helper.DestroyChildren (MenuContent);
	
		gameObject.SetActive (false);	
	}

	public override void Tick (){
		if (GraphSpace.activeInHierarchy) {
			graphManager.Tick ();
		}
	}

	public override string GetName (){
		return "Replay";
	}

	public void Back(){
		if (ReplayMenu.activeInHierarchy) {
			GameManager.instance.playClick ();
			GameManager.instance.SwitchState ("MainMenu");
		} else if (GraphSpace.activeInHierarchy) {
			GameManager.instance.playClick ();
			GraphSpace.SetActive (false);
			ReplayUI.SetActive (false);
			ReplayMenu.SetActive (true);
			moves = new List<PebbleMove> ();
			currentMove = 0;

			Helper.DestroyChildren (GraphSpace);
			LoadMenuContent ();
		}
	}

	public void LoadMenuContent(){
		float contentSize = DeleteMenuButtonPrefab.GetComponent<RectTransform> ().rect.height + 25f;

		foreach (string name in UserData.instance.ReplayIds.Keys) {
			DeleteMenuButton button = (DeleteMenuButton)Instantiate (DeleteMenuButtonPrefab, MenuContent.transform);
			button.Content.text = name;
			button.DeleteClick.AddListener (DeleteClick);
			button.ContentClick.AddListener (ContentClick);
		} 

		RectTransform smm = MenuContent.GetComponent<RectTransform> ();
		smm.sizeDelta = new Vector2 (smm.sizeDelta.x, 
			contentSize * (UserData.instance.ReplayIds.Count));
	}

	public void DeleteClick(string content){
		GameManager.instance.playClick ();
		ConfirmDeletePopup.Open (content);
	}

	public void DeleteConfirm(string content){
		UserData.instance.DeleteReplay (content);

		Helper.DestroyChildren (MenuContent);
		LoadMenuContent ();
	}


	public void ContentClick(string content){
		if (content != "" && UserData.instance.ReplayIds.ContainsKey(content)) {
			Node node;
			Path path;
			PebbleMove move;
			List<Node> nodes = new List<Node> ();
			List<Path> paths = new List<Path> ();
			int nodeCount, pathCount, moveCount;
			float xp, yp;
			string fileName = UserData.instance.ReplayIds [content];

			BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.OpenOrCreate));

			nodeCount = br.ReadInt32();
			for(int x = 0; x < nodeCount; x++){
				node = (Node)Instantiate (NodePrefab, GraphSpace.transform); 
				node.NodeId = br.ReadInt32 ();
				node.Pebbles = br.ReadInt32 ();
				node.State = (br.ReadBoolean()) ? NodeState.Target : NodeState.Inactive;

				xp = (float)br.ReadDouble();
				yp = (float)br.ReadDouble ();
				node.Size = new Vector2 (xp, yp);

				xp = (float)br.ReadDouble();
				yp = (float)br.ReadDouble ();
				node.Position = new Vector2 (xp, yp);

				pathCount = br.ReadInt32 ();
				for (int y = 0; y < pathCount; y++) {
					node.Paths.Add (br.ReadInt32 ());
				}

				nodes.Add (node);
			}

			pathCount = br.ReadInt32 ();
			for (int i = 0; i < pathCount; i++) {
				path = (Path)Instantiate (PathPrefab, GraphSpace.transform);
				path.VertexOne = br.ReadInt32 ();
				path.VertexTwo = br.ReadInt32 ();
				path.Width = (float)br.ReadDouble ();
				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);
			}

			moveCount = br.ReadInt32();
			for (int i = 0; i < moveCount; i++) {
				move.DestinationNode = nodes [br.ReadInt32()];
				move.OriginNode = nodes [br.ReadInt32()];
				moves.Add (move);
			}

			moveCount = br.ReadInt32();
			for(int i = 0; i < moveCount; i++){
				nodes[i].Pebbles = br.ReadInt32();
			}

			attackerWins = br.ReadBoolean();
			br.Close();

			Graph graph = new Graph();
			graph.Nodes = nodes;
			graph.Paths = paths;
			graphManager.CurrentGraph = graph;

			Note.Success("Replay Successfully Loaded");

			EnterReplay();
		} else {
			Note.Error ("Error when Loading");
		}
	}

	public void EnterReplay(){
		GameManager.instance.playClick ();
		Helper.DestroyChildren (MenuContent);
		GraphSpace.SetActive (true);
		ReplayUI.SetActive (true);
		ReplayMenu.SetActive (false);

		LeftArrow.SetActive (false);
		RightArrow.SetActive (true);
		AttackerTurn = true;
		CurrentMove = 0;
	}

	public void NextMove(){
		if (currentMove < moves.Count) {
			GameManager.instance.playClick ();
			PebbleMove move = moves [CurrentMove];
			move.OriginNode.Pebbles = move.OriginNode.Pebbles - 2;
			move.DestinationNode.Pebbles = move.DestinationNode.Pebbles + 1;

			CurrentMove = CurrentMove + 1;
			AttackerTurn = !AttackerTurn;

			if (CurrentMove == moves.Count) {
				TurnText.text = (attackerWins) ? "Attacker Wins!" : "Defender Wins!";
				RightArrow.SetActive (false);
			}

			LeftArrow.SetActive (true);
		}
	}

	public void LastMove(){
		if (CurrentMove > 0) {
			GameManager.instance.playClick ();
			CurrentMove = CurrentMove - 1;
			PebbleMove move = moves [CurrentMove];
			move.OriginNode.Pebbles = move.OriginNode.Pebbles + 2;
			move.DestinationNode.Pebbles = move.DestinationNode.Pebbles - 1;

			AttackerTurn = !AttackerTurn;

			if (CurrentMove == 0) {
				LeftArrow.SetActive (false);
			}

			RightArrow.SetActive (true);
		}
	}
}

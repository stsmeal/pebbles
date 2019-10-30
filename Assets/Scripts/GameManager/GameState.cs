using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pebble move.
/// </summary>
public struct PebbleMove{
	public Node OriginNode, DestinationNode;

	public PebbleMove(Node originNode, Node destinationNode){
		DestinationNode = destinationNode;
		OriginNode = originNode;
	}
}


/// <summary>
/// Game state.
/// </summary>
public class GameState : AState {
	protected enum State{
		Menu,
		SelectMap,
		Game,
		GameOver
	}

	[Header("Containers")]
	public GameObject SelectMap;
	public GameObject GameUI; 
	public GameObject Menu;
	public GameObject GraphSpace;
	public GameObject GameOver;
	public GameObject PausePopup;
	public SavePopup SaveGamePopup;
	public SavePopup SaveReplayPopup;

	[Header("Menu Contents")]
	public GameObject MainMenu;
	public GameObject SelectMapMenu;

	[Header("Default Graphs")]
	public GameObject[] DefaultGraphs; 

	[Header("UI")]
	public GameObject StartButton;
	public Text TurnText;
	public Text WinningMessage;

	[Header("Prefabs")]
	public Node NodePrefab;
	public Path PathPrefab;
	public Button MenuButtonPrefab;

	[Header("Other Data")]
	public GraphManager graphManager;
	public GameObject BackgroundFade;
	public GameObject PauseBackgroundFade;
	public NotificationPopup Note;
	public AI ai;

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="GameState"/> attacker turn.
	/// </summary>
	/// <value><c>true</c> if attacker turn; otherwise, <c>false</c>.</value>
	protected bool AttackerTurn {
		get{return attackerTurn;}
		set{
			if (value) 
				TurnText.text = "Attacker's Turn";
			else
				TurnText.text = "Defender's Turn";
			
			attackerTurn = value;
		}
	}
	protected State state;
	protected bool attackerTurn;
	protected bool gameRunning = false;
	protected LinkedList<PebbleMove> moves = new LinkedList<PebbleMove> ();
	protected Node lastNode;
	protected List<Node> highlightedNodes = new List<Node>();
	protected List<int> originalPebbleValues = new List<int>();

	private bool newGame;
	private bool attackerWins = false;

	/// <summary>
	/// Enter the specified from.
	/// </summary>
	/// <param name="from">From.</param>
	public override void Enter (AState from){
		originalPebbleValues = new List<int>();
		highlightedNodes = new List<Node>();
		moves = new LinkedList<PebbleMove> ();
		gameRunning = false;
		attackerTurn = true;
		
		if (UserData.instance.SavedGameIds.Count == 0) {
			state = State.SelectMap;

			gameObject.SetActive (true);

			GameUI.SetActive (false);
			SelectMap.SetActive (true);
			GraphSpace.SetActive (false);
			GameOver.SetActive (false);
			PausePopup.SetActive (false);
			PauseBackgroundFade.SetActive (false);
			Menu.SetActive (false);

			LoadSelectMapContent ();
		} else {
			state = State.Menu;

			gameObject.SetActive (true);

			GameUI.SetActive (false);
			SelectMap.SetActive (false);
			GraphSpace.SetActive (false);
			GameOver.SetActive (false);
			PausePopup.SetActive (false);
			PauseBackgroundFade.SetActive (false);
			Menu.SetActive (true);

			LoadMenuContent ();
		}
	}


	/// <summary>
	/// Exit the specified to.
	/// </summary>
	/// <param name="to">To.</param>
	public override void Exit (AState to){
		originalPebbleValues = new List<int>();
		highlightedNodes = new List<Node>();
		moves = new LinkedList<PebbleMove> ();
		gameRunning = false;
		attackerTurn = true;

		gameObject.SetActive (false);

		GameUI.SetActive (false);
		SelectMap.SetActive (false);
		GraphSpace.SetActive (false);
		GameOver.SetActive (false);
		PausePopup.SetActive (false);
		Menu.SetActive (false);


		Helper.DestroyChildren (SelectMapMenu);
		Helper.DestroyChildren (MainMenu);
		Helper.DestroyChildren (GraphSpace);
	}

	public override void Tick (){
		if (state == State.Game) {
			graphManager.Tick ();
		}

		if (gameRunning)
			GameLogic ();
	}


	/// <summary>
	/// Gets the name of the State.
	/// </summary>
	/// <returns>The name.</returns>
	public override string GetName (){
		return "Game";
	}


	/// <summary>
	/// Back this instance.
	/// </summary>
	public void Back(){
		GameManager.instance.playClick ();
		switch (state) {
		case State.Game:
			//TODO: Open Pause Popup
			if (gameRunning) {
				gameRunning = false;
				PausePopup.SetActive (true);
				PauseBackgroundFade.SetActive (true);
			} else {
				if (newGame) {
					state = State.SelectMap;
					SelectMap.SetActive (true);
					LoadSelectMapContent ();
				} else {
					state = State.Menu;
					Menu.SetActive (true);
					LoadMenuContent ();
				}
				GameUI.SetActive (false);
				GraphSpace.SetActive (false);
				StartButton.SetActive (false);
				TurnText.gameObject.SetActive (false);
				Helper.DestroyChildren (GraphSpace);
			}
			break;
		case State.Menu:
			GameManager.instance.SwitchState ("MainMenu");
			break;
		case State.SelectMap:
			if (UserData.instance.SavedGameIds.Count == 0) 
				GameManager.instance.SwitchState ("MainMenu");
			else {
				Helper.DestroyChildren (SelectMapMenu);
				Helper.DestroyChildren (MainMenu);
				LoadMenuContent ();
				SelectMap.SetActive (false);
				Menu.SetActive (true);
				state = State.Menu;
			}
			break;
		default:
			break;
		}
	}

	public void QuitPause(){
		GameManager.instance.playClick ();
		gameRunning = true;
		PausePopup.SetActive (false);
		PauseBackgroundFade.SetActive (false);
	}


	/// <summary>
	/// A New Game.
	/// </summary>
	public void NewGame(){
		state = State.SelectMap;

		gameObject.SetActive (true);

		GameUI.SetActive (false);
		SelectMap.SetActive (true);
		GraphSpace.SetActive (false);
		GameOver.SetActive (false);
		PausePopup.SetActive (false);
		Menu.SetActive (false);

		LoadSelectMapContent ();
	}


	/// <summary>
	/// Starts the game.
	/// </summary>
	public void StartGame(){
		GameManager.instance.playClick ();
		gameRunning = true;
		if(newGame)
			AttackerTurn = true;
		BackgroundFade.SetActive (false);
		StartButton.SetActive (false);
		TurnText.gameObject.SetActive (true);
	}


	/// <summary>
	/// Continues the game.
	/// </summary>
	public void ContinueGame(){
		LoadGame (Helper.GetButtonClickText ());
	}


	/// <summary>
	/// Loads the game.
	/// </summary>
	/// <param name="GameName">Game name.</param>
	public void LoadGame(string gameName){
		if (gameName != "" && UserData.instance.SavedGameIds.ContainsKey (gameName)) {
			Node node;
			List<Node> nodes = new List<Node> ();
			List<Path> paths = new List<Path> ();
			int nodeCount, pathCount, moveCount;
			float xp, yp;
			string fileName = UserData.instance.SavedGameIds [gameName];

			BinaryReader br = new BinaryReader (new FileStream (fileName, FileMode.OpenOrCreate));

			nodeCount = br.ReadInt32 ();
			for (int x = 0; x < nodeCount; x++) {
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
				Path path = (Path)Instantiate (PathPrefab, GraphSpace.transform);
				path.VertexOne = br.ReadInt32 ();
				path.VertexTwo = br.ReadInt32 ();
				path.Width = (float)br.ReadDouble ();
				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);
			}
			Graph graph = new Graph ();
			graph.Nodes = nodes;
			graph.Paths = paths;
			graphManager.CurrentGraph = graph;

			moveCount = br.ReadInt32 ();
			for (int i = 0; i < moveCount; i++) {
				PebbleMove move;
				move.DestinationNode = nodes [br.ReadInt32()];
				move.OriginNode = nodes [br.ReadInt32()];
				moves.AddLast (move);
			}

			AttackerTurn = br.ReadBoolean ();
			moveCount = br.ReadInt32 ();
			originalPebbleValues.Clear ();
			for (int i = 0; i < moveCount; i++) {
				originalPebbleValues.Add (br.ReadInt32 ());
			}

			br.Close ();


			EnterGame ();
			Note.Success ("Game Loaded Succesfully");
		}
	}


	/// <summary>
	/// Opens the save pop up.
	/// </summary>
	public void OpenSavePopUp(){
		SaveGamePopup.Open ();
	}


	/// <summary>
	/// Closes the save pop up.
	/// </summary>
	public void CloseSavePopUp(){
		GameManager.instance.playClick ();
		SaveGamePopup.Close ();
	}


	/// <summary>
	/// Saves the game.
	/// </summary>
	public void SaveGame(){
		SaveGame (SaveGamePopup.Save ());
	}


	/// <summary>
	/// Quit this instance.
	/// </summary>
	public void Quit(){
		GameManager.instance.playClick ();
		GameManager.instance.SwitchState ("MainMenu");
	}


	/// <summary>
	/// Restarts Game
	/// </summary>
	public void PlayAgain(){
		for (int i = 0; i < originalPebbleValues.Count; i++) {
			graphManager.CurrentGraph.Nodes [i].Pebbles = originalPebbleValues [i];
		}

		AttackerTurn = true;
		gameRunning = true;
		moves.Clear ();
		GameOver.SetActive (false);
		state = State.Game;
		QuitPause ();

		ai = new AI (graphManager.CurrentGraph);
	}

	public void OpenSaveReplayPopup(){
		GameManager.instance.playClick ();
		SaveReplayPopup.Open ();
	}

	public void SaveReplay(){
		SaveReplay (SaveReplayPopup.Save ());
	}

	protected void SaveReplay(string replayName){
		if (replayName != "") {
			if (!UserData.instance.ReplayIds.ContainsKey (replayName)) {
				string fileName = Application.persistentDataPath + "/Replay" 
					+ UserData.instance.SaveCount + ".rly";
				BinaryWriter bw = new BinaryWriter (new FileStream (fileName, FileMode.OpenOrCreate));

				bw.Write (graphManager.CurrentGraph.Nodes.Count);
				foreach (Node node in graphManager.CurrentGraph.Nodes) {
					bw.Write ((int)node.NodeId);
					bw.Write ((int)node.Pebbles);
					bw.Write ((bool)(node.State == NodeState.Target));
					bw.Write (((double)node.Size.x));
					bw.Write (((double)node.Size.y));
					bw.Write (((double)node.Position.x));
					bw.Write (((double)node.Position.y));
					bw.Write (node.Paths.Count);
					foreach (int path in node.Paths)
						bw.Write (path);
				}

				bw.Write (graphManager.CurrentGraph.Paths.Count);
				foreach (Path path in graphManager.CurrentGraph.Paths) {
					bw.Write (path.VertexOne);
					bw.Write (path.VertexTwo);
					bw.Write ((double)path.Width);
				}

				bw.Write (moves.Count);
				foreach (PebbleMove move in moves) {
					bw.Write (move.DestinationNode.NodeId);
					bw.Write (move.OriginNode.NodeId);
				}

				bw.Write (originalPebbleValues.Count);
				foreach (int count in originalPebbleValues) {
					bw.Write (count);
				}

				bw.Write (attackerWins);
				bw.Close ();

				UserData.instance.ReplayIds.Add (replayName, fileName);
				UserData.instance.Save ();

				Note.Success ("Replay Successfully Saved");
			} else {
				Note.Error ("Replay Already Exists");
			}

			SaveReplayPopup.Close ();
		}
	}


	protected void SaveGame(string gameName){
		if (gameName != "") {
			if (!UserData.instance.SavedGameIds.ContainsKey (gameName)) {
				string fileName = Application.persistentDataPath + "/Game" + UserData.instance.SaveCount + ".gam";
				BinaryWriter bw = new BinaryWriter (new FileStream (fileName, FileMode.OpenOrCreate));

				bw.Write (graphManager.CurrentGraph.Nodes.Count);
				foreach (Node node in graphManager.CurrentGraph.Nodes) {
					bw.Write ((int)node.NodeId);
					bw.Write ((int)node.Pebbles);
					bw.Write ((bool)(node.State == NodeState.Target));
					bw.Write (((double)node.Size.x));
					bw.Write (((double)node.Size.y));
					bw.Write (((double)node.Position.x));
					bw.Write (((double)node.Position.y));
					bw.Write (node.Paths.Count);
					foreach (int path in node.Paths)
						bw.Write (path);
				}

				bw.Write (graphManager.CurrentGraph.Paths.Count);
				foreach (Path path in graphManager.CurrentGraph.Paths) {
					bw.Write (path.VertexOne);
					bw.Write (path.VertexTwo);
					bw.Write ((double)path.Width);
				}

				bw.Write (moves.Count);
				foreach (PebbleMove move in moves) {
					bw.Write (move.DestinationNode.NodeId);
					bw.Write (move.OriginNode.NodeId);
				}

				bw.Write (attackerTurn);
				bw.Write (originalPebbleValues.Count);
				for(int i = 0; i < originalPebbleValues.Count; i++) {
					bw.Write (originalPebbleValues[i]);
				}
				bw.Close ();

				UserData.instance.SavedGameIds.Add (gameName, fileName);
				UserData.instance.Save ();

				Note.Success ("Game Succesfully Saved");
			} else {
				Note.Error ("Game Name Already Exists");
			}

			SaveGamePopup.Close ();
		}
	}


	/// <summary>
	/// Loads the graph.
	/// </summary>
	/// <param name="GraphName">Graph name.</param>
	protected void LoadGraph(string GraphName){
		if (GraphName != "" && UserData.instance.GraphIds.ContainsKey(GraphName)) {
			List<Node> nodes = new List<Node> ();
			List<Path> paths = new List<Path> ();
			string fileName = UserData.instance.GraphIds [GraphName];

			int nodeCount, pathCount;
			float xp, yp;

			BinaryReader br = new BinaryReader (new FileStream (fileName, FileMode.OpenOrCreate));

			nodeCount = br.ReadInt32 ();
			for (int x = 0; x < nodeCount; x++) {
				Node node = (Node)Instantiate (NodePrefab, GraphSpace.transform);
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
				Path path = (Path)Instantiate (PathPrefab, GraphSpace.transform);
				path.VertexOne = br.ReadInt32 ();
				path.VertexTwo = br.ReadInt32 ();
				path.Width = (float)br.ReadDouble ();
				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);
			}
			br.Close ();

			Graph graph = new Graph ();
			graph.Nodes = nodes;
			graph.Paths = paths;

			graphManager.CurrentGraph = graph;

			originalPebbleValues.Clear ();

			for (int i = 0; i < graphManager.CurrentGraph.Nodes.Count; i++)
				originalPebbleValues.Add (graphManager.CurrentGraph.Nodes [i].Pebbles);

			EnterGame ();
		}
	}


	/// <summary>
	/// Games the logic.
	/// </summary>
	protected void GameLogic(){
		/*
		if (Input.GetMouseButtonDown (0))
			Move (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));
		
		if (Input.touchCount > 0)
			if (Input.GetTouch (0).phase == TouchPhase.Began)
				Move (Input.GetTouch (0).position);
		*/

		if (GameManager.InputBegan) {
			Move (GameManager.InputPosition);
			if (!AttackerTurn) {
				int o, d;
				o = moves.Last.Value.OriginNode.NodeId;
				d = moves.Last.Value.DestinationNode.NodeId;
				ai.DefenderMove (ref o, ref d);
				graphManager.CurrentGraph.Nodes [o].Pebbles = graphManager.CurrentGraph.Nodes [o].Pebbles - 2;
				graphManager.CurrentGraph.Nodes [d].Pebbles = graphManager.CurrentGraph.Nodes [d].Pebbles + 1;
				moves.AddLast (new PebbleMove () {
					OriginNode = graphManager.CurrentGraph.Nodes[o],
					DestinationNode = graphManager.CurrentGraph.Nodes[d]
				});
				AttackerTurn = true;
			}
		}
	}


	/// <summary>
	/// Move the specified position.
	/// </summary>
	/// <param name="position">Position.</param>
	private void Move(Vector2 position){
		Vector3 wp = Camera.main.ScreenToWorldPoint (position);
		Vector2 tp = new Vector2 (wp.x, wp.y);

		Collider2D sprite = Physics2D.OverlapPoint (tp);

		if (sprite) {
			//let's skip the type check since there will be no case of non-nodes
			Node currentNode = sprite.GetComponent<Node> ();

			if (lastNode == null) {
				if (lastNode == null && currentNode.Pebbles > 1 && currentNode.State != NodeState.Target) {
					GameManager.instance.playClick ();

					lastNode = currentNode;
					lastNode.State = NodeState.Active;

					if (AttackerTurn) {
						highlightedNodes = graphManager.attachedNodes (lastNode);
						if (lastNode.Pebbles < 4) {
							if (graphManager.CurrentGraph.Nodes.Count ((Node n) => n.Pebbles > 1 
								&& (n.Paths.Count >  1 || !n.Paths.Exists((int p) => lastNode.Paths.Contains(p)))) < 2) {
								highlightedNodes.RemoveAll ((Node n) => n.Paths.Count <= 1 && n.State != NodeState.Target);
							}
						}
					} else {
						highlightedNodes = getDefenderMoves (lastNode);
					}

					foreach (Node node in highlightedNodes) {
						if (node.State != NodeState.Target)
							node.State = NodeState.Hightlighted;
					}
				}
			} else {
				if (lastNode != currentNode) {
					if (highlightedNodes.Contains (currentNode)) {
						GameManager.instance.playHighClick ();
						currentNode.Pebbles = currentNode.Pebbles + 1;
						lastNode.Pebbles = lastNode.Pebbles - 2;
						moves.AddLast (new PebbleMove (lastNode, currentNode));
						if (currentNode.State == NodeState.Target) {
							GameOver.SetActive (true);
							gameRunning = false;
							attackerWins = true;
							state = State.GameOver;
							WinningMessage.text = "Attacker Wins!";
						} else if (!graphManager.CurrentGraph.Nodes.Exists ((Node node) => node.Pebbles > 1)) {
							WinningMessage.text = "Defender Wins!";
							GameOver.SetActive (true);
							gameRunning = false;
							attackerWins = false;
							state = State.GameOver;
						} else
							AttackerTurn = !AttackerTurn;
					}
				} 

				foreach (Node node in highlightedNodes) {
					if (node.State != NodeState.Target)
						node.State = NodeState.Inactive;
				}
				highlightedNodes.Clear ();
				lastNode.State = NodeState.Inactive;
				lastNode = null;
			}

		} else {
			foreach (Node node in highlightedNodes) {
				if (node.State != NodeState.Target) {
					node.State = NodeState.Inactive;
				}
			}

			if(lastNode != null)
				lastNode.State = NodeState.Inactive;
			lastNode = null;
		}
	}


	/// <summary>
	/// Gets the defender moves.
	/// </summary>
	/// <returns>The defender moves.</returns>
	/// <param name="originNode">Origin node.</param>
	private List<Node> getDefenderMoves(Node originNode){
		List<Node> destNodes = graphManager.attachedNodes (originNode);

		if(moves.Count > 0){
			PebbleMove lastMove = moves.Last.Value;
			if (lastMove.DestinationNode.NodeId == originNode.NodeId)
				destNodes.RemoveAll ((Node n) => n.NodeId == lastMove.OriginNode.NodeId);
		}

		return destNodes;
	}


	/// <summary>
	/// Loads the content of the menu.
	/// </summary>
	private void LoadMenuContent(){
		float contentSize = MenuButtonPrefab.GetComponent<RectTransform> ().rect.height + 25f;

		Button newGameButton = (Button)Instantiate (MenuButtonPrefab, MainMenu.transform);
		newGameButton.GetComponentInChildren<Text> ().text = "New Game";
		newGameButton.onClick.AddListener (NewGame);


		//load saved games
		foreach(string name in UserData.instance.SavedGameIds.Keys) {
			Button button = (Button)Instantiate (MenuButtonPrefab, MainMenu.transform);
			var text = button.GetComponentInChildren<Text> ();
			text.text = name;
			button.onClick.AddListener (ContinueGame);
		}

		RectTransform smm = MainMenu.GetComponent<RectTransform> ();
		smm.sizeDelta = new Vector2 (smm.sizeDelta.x,
			contentSize * (UserData.instance.SavedGameIds.Count + 1));

		newGame = false;
	}


	/// <summary>
	/// Loads the content of the select map.
	/// </summary>
	private void LoadSelectMapContent(){
		float contentSize = MenuButtonPrefab.GetComponent<RectTransform> ().rect.height + 25f;
		//load default graphs
		for(int i = 0; i < DefaultGraphs.Length; i++) {
			Button button = (Button)Instantiate (MenuButtonPrefab, SelectMapMenu.transform);
			var text = button.GetComponentInChildren<Text> ();
			text.text = "Preset Graph " + (i + 1).ToString();
			button.onClick.AddListener(DefaultMapSelect);
		}

		//load user made graphs
		foreach(string name in UserData.instance.GraphIds.Keys) {
			Button button = (Button)Instantiate (MenuButtonPrefab, SelectMapMenu.transform);
			var text = button.GetComponentInChildren<Text> ();
			text.text = name;
			button.onClick.AddListener (UserMapSelected);
		}

		RectTransform smm = SelectMapMenu.GetComponent<RectTransform> ();
		smm.sizeDelta = new Vector2 (smm.sizeDelta.x,
			contentSize * (UserData.instance.GraphIds.Count + DefaultGraphs.Length));

		newGame = true;
	}


	/// <summary>
	/// Defaults the map select.
	/// </summary>
	private void DefaultMapSelect(){
		GameManager.instance.playClick ();
		int graphId;
		string s = Helper.GetButtonClickText();
		s = s.Remove (0, 13);
		if (int.TryParse (s, out graphId)) {
			Node[] ns = DefaultGraphs [graphId - 1].GetComponentsInChildren<Node> ();
			List<Node> nl = new List<Node> ();

			foreach (Node n in ns)
				nl.Add (n);
			
			if (graphManager.Load (nl)) {
				for (int i = 0; i < graphManager.CurrentGraph.Nodes.Count; i++)
					originalPebbleValues.Add (graphManager.CurrentGraph.Nodes [i].Pebbles);
				
				EnterGame ();
			}
		} 
	}

	/// <summary>
	/// Users the map selected.
	/// </summary>
	private void UserMapSelected(){
		GameManager.instance.playClick ();
		LoadGraph (Helper.GetButtonClickText());
	}


	/// <summary>
	/// Enters the game.
	/// </summary>
	private void EnterGame(){
		state = State.Game;
		
		SelectMap.SetActive (false);
		Menu.SetActive (false);
		Helper.DestroyChildren (MainMenu);
		Helper.DestroyChildren (SelectMapMenu);
		GameUI.SetActive (true);
		GraphSpace.SetActive (true);
		BackgroundFade.SetActive (true);
		StartButton.SetActive (true);
		TurnText.gameObject.SetActive (false);

		ai = new AI (graphManager.CurrentGraph);
	}
}

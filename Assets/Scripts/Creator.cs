using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Creator.
/// </summary>
public class Creator : GraphManager {
	protected enum State
	{
		AddNode,
		SelectGoal,
		AddPath,
		Delete,
		ScaleNode,
		EditPebbles,
		MassEdit,
		ToolSelect
	}

	[Header("UI")]
	public Text CurrentModeButtonText;
	public InputField PebbleCountFieldText;
	public GameObject ToolPanel;
	public GameObject BackgroundFade;
	public GameObject AddPebblesUI;
	public SavePopup SaveGraphPopUp;
	public ConfirmPopup ConfirmNewSavePopup;
	public NotificationPopup Note;

	[Header("Margins")]
	public BoxCollider2D TopMargin;
	public BoxCollider2D LeftMargin;
	public BoxCollider2D RightMargin;
	public BoxCollider2D BottomMargin;

	[Header("State Rect Transform")]
	public RectTransform rt;


	protected List<Node> createdNodes = new List<Node>();
	protected List<Path> createdPaths = new List<Path>();
	protected State state;

	private Node currentNode;
	private Node goalNode;
	private Path currentPath;
	private Vector2 initialPosition;
	private Vector2 defaultNodeSize = new Vector2 (75, 75);

	private CanvasScaler cs;

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected override void Start ()
	{
		base.Start ();

		state = State.ToolSelect;
		BackgroundFade.SetActive (true);
		ToolPanel.SetActive (true);

		TopMargin.size = new Vector2(0, TopMargin.GetComponent<RectTransform> ().rect.height * 3);
		TopMargin.offset = new Vector2( 0, TopMargin.size.y / 2 - TopMargin.GetComponent<RectTransform> ().rect.height);

		BottomMargin.size = new Vector2(0, BottomMargin.GetComponent<RectTransform> ().rect.height);
		BottomMargin.offset = new Vector2 (0, -1 * BottomMargin.size.y / 2);

		LeftMargin.size = new Vector2(LeftMargin.GetComponent<RectTransform> ().rect.width, 0);
		LeftMargin.offset = new Vector2 (-1 * LeftMargin.size.x / 2, 0);

		RightMargin.size = new Vector2(RightMargin.GetComponent<RectTransform> ().rect.width, 0);
		RightMargin.offset = new Vector2 (RightMargin.size.x / 2, 0);
	}


	/// <summary>
	/// Tick this instance.
	/// </summary>
	public override void Tick(){
		base.Tick ();
		if (state != State.ToolSelect) {
			UpdateGraph ();
			if (state == State.AddNode) {
				if (GameManager.InputBegan) {
					GameManager.instance.playClick ();
					Collider2D sprite = Physics2D.OverlapPoint (GameManager.InputWorldPosition);

					if (sprite) {
						currentNode = (Node)sprite.GetComponent ("Node");
						if (currentNode != null)
							currentNode.State = NodeState.Hightlighted;
					} else {
						currentNode = (Node)Instantiate (NodePrefab, this.gameObject.transform);
						currentNode.Size = defaultNodeSize;
						currentNode.Position = GameManager.InputWorldPosition;
						currentNode.NodeId = createdNodes.Count;
						currentNode.State = NodeState.Hightlighted;
						createdNodes.Add (currentNode);
					}					
				} else if (GameManager.InputDown && currentNode != null) {
					currentNode.Position = GameManager.InputWorldPosition;
				} else if (GameManager.InputEnd && currentNode != null) {
					if (goalNode != null && currentNode.NodeId == goalNode.NodeId) {
						currentNode.State = NodeState.Target;
					} else {
						currentNode.State = NodeState.Inactive;
					}
					GameManager.instance.playHighClick ();
					currentNode = null;
				}
			} else if (state == State.AddPath) {
				if (GameManager.InputBegan) {
					Node node;
					if (Helper.OverlapNode (GameManager.InputWorldPosition, out node)) {
						GameManager.instance.playClick ();
						Path path = Instantiate (PathPrefab, this.gameObject.transform);
						path.VertexOne = node.NodeId;
						path.VertexOnePosition = node.Position;
						path.VertexTwoPosition = GameManager.InputWorldPosition;
						currentPath = path;
						currentPath.VertexTwo = -1;
					}
				} else if (GameManager.InputDown && currentPath != null) {
					currentPath.VertexTwoPosition = GameManager.InputWorldPosition;

					Node node;
					if (Helper.OverlapNode (currentPath.VertexTwoPosition, out node)) {
						if (node.NodeId != currentPath.VertexOne) {
							currentPath.VertexTwo = node.NodeId;
							currentPath.VertexTwoPosition = node.Position;
						}
					} else {
						currentPath.VertexTwo = -1;
					}
				} else if (!GameManager.InputDown && currentPath != null) {
					if (!createdPaths.Exists ((Path path) => path != null && path.Compare (currentPath)) && currentPath.VertexTwo >= 0) {
						GameManager.instance.playHighClick ();
						createdNodes [currentPath.VertexOne].Paths.Add (createdPaths.Count);
						createdNodes [currentPath.VertexTwo].Paths.Add (createdPaths.Count);
						createdPaths.Add (currentPath);
					} else {
						Destroy (currentPath.gameObject);
					}
					currentPath = null;
				}
			} else if (state == State.Delete) {
				if (GameManager.InputBegan) {
					Collider[] cs = Physics.OverlapSphere (GameManager.InputWorldPosition, .1f);

					foreach (Collider c in cs) {
						Path path = c.GetComponent<Path> ();
						int pathId = createdPaths.IndexOf (path);
						createdNodes [path.VertexOne].Paths.Remove (pathId);
						createdNodes [path.VertexTwo].Paths.Remove (pathId);
						deletePath (path);
						GameManager.instance.playClick ();
					}

					Node node;
					if (Helper.OverlapNode (GameManager.InputWorldPosition, out node)) {
						deleteNode (node);
						GameManager.instance.playClick ();
					}
				}
			} else if (state == State.SelectGoal) {
				if (GameManager.InputBegan) {
					if (Helper.OverlapNode (GameManager.InputWorldPosition, out currentNode)) {
						if (goalNode != null)
							goalNode.State = NodeState.Inactive;
						goalNode = currentNode;
						currentNode = null;
						goalNode.State = NodeState.Target;
						GameManager.instance.playClick ();
					}
				}
			} else if (state == State.ScaleNode) {
				if (GameManager.InputBegan) {
					if (Helper.OverlapNode (GameManager.InputWorldPosition, out currentNode)) {
						currentNode.State = NodeState.Hightlighted;
						GameManager.instance.playClick ();
					}
				} else if (GameManager.InputDown && currentNode != null) {
					float distance = Vector2.Distance (currentNode.Position, 
						GameManager.InputWorldPosition) * 50;
					
					if (distance > 25f && distance < 150f) {
						defaultNodeSize = new Vector2 (distance, distance);
						currentNode.Size = defaultNodeSize; 
					}
				} else if (GameManager.InputEnd && currentNode != null) {
					currentNode.State = NodeState.Inactive;
					GameManager.instance.playHighClick ();
					currentNode = null;
				}
			}
			else if (state == State.MassEdit) {

			} else if (state == State.EditPebbles) {
				if (GameManager.InputBegan) {
					
					Node node; 
					if (Helper.OverlapNode (GameManager.InputWorldPosition, out node)) {
						if (node.State != NodeState.Target) {
							if (currentNode != null) {
								currentNode.State = NodeState.Inactive;
							}
							PebbleCountFieldText.text = "";
							currentNode = node;
							currentNode.State = NodeState.Hightlighted;
							GameManager.instance.playClick ();
						}
					}
				}
			}
		}
	}


	/// <summary>
	/// Open this instance.
	/// </summary>
	public void Open(){
		ConfirmNewSavePopup.ConfirmClick.AddListener (SaveConfirm);

		state = State.ToolSelect;
		BackgroundFade.SetActive (true);
		ToolPanel.SetActive (true);

		TopMargin.size = new Vector2(0, TopMargin.GetComponent<RectTransform> ().rect.height * 3);
		TopMargin.offset = new Vector2( 0, TopMargin.size.y / 2 - TopMargin.GetComponent<RectTransform> ().rect.height);

		BottomMargin.size = new Vector2(0, BottomMargin.GetComponent<RectTransform> ().rect.height);
		BottomMargin.offset = new Vector2 (0, -1 * BottomMargin.size.y / 2);

		LeftMargin.size = new Vector2(LeftMargin.GetComponent<RectTransform> ().rect.width, 0);
		LeftMargin.offset = new Vector2 (-1 * LeftMargin.size.x / 2, 0);

		RightMargin.size = new Vector2(RightMargin.GetComponent<RectTransform> ().rect.width, 0);
		RightMargin.offset = new Vector2 (RightMargin.size.x / 2, 0);
	}


	/// <summary>
	/// Close this instance.
	/// </summary>
	public void Close(){
		ConfirmNewSavePopup.ConfirmClick.RemoveListener (SaveConfirm);

		createdNodes = new List<Node> ();
		createdPaths = new List<Path> ();
		CurrentGraph = new Graph ();
		Helper.DestroyChildren (this.gameObject);
	}


	/// <summary>
	/// Opens the tool panel.
	/// </summary>
	public void OpenToolPanel(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (true);
		BackgroundFade.SetActive (true);
		AddPebblesUI.SetActive (false);

		if (currentNode != null) {
			currentNode.State = NodeState.Inactive;
			currentNode = null;
		}

		state = State.ToolSelect;
	}


	/// <summary>
	/// Enters the add node mode.
	/// </summary>
	public void EnterAddNodeMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (false);

		CurrentModeButtonText.text = "Add/Move Nodes";

		state = State.AddNode;
	}


	/// <summary>
	/// Enters the goal select mode.
	/// </summary>
	public void EnterGoalSelectMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (false);

		CurrentModeButtonText.text = "Select Goal";

		state = State.SelectGoal;
	}


	/// <summary>
	/// Enters the add path mode.
	/// </summary>
	public void EnterAddPathMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (false);

		CurrentModeButtonText.text = "Add Paths";

		state = State.AddPath;
	}


	/// <summary>
	/// Enters the delete mode.
	/// </summary>
	public void EnterDeleteMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (false);

		CurrentModeButtonText.text = "Delete Node/Paths";

		state = State.Delete;
	}


	/// <summary>
	/// Enters the scale node mode.
	/// </summary>
	public void EnterScaleNodeMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (false);

		CurrentModeButtonText.text = "Scale Node";

		state = State.ScaleNode;
	}


	/// <summary>
	/// Enters the mass node mode.
	/// </summary>
	public void EnterMassNodeMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (true);

		CurrentModeButtonText.text = "Mass Node Edit";

		state = State.MassEdit;
	}


	/// <summary>
	/// Enters the pebble edit mode.
	/// </summary>
	public void EnterPebbleEditMode(){
		GameManager.instance.playClick ();
		ToolPanel.SetActive (false);
		BackgroundFade.SetActive (false);
		AddPebblesUI.SetActive (true);

		CurrentModeButtonText.text = "Add/Remove Pebbles";

		state = State.EditPebbles;
	}


	/// <summary>
	/// Clears the graph.
	/// </summary>
	public void ClearGraph(){
		GameManager.instance.playClick ();
		goalNode = null;

		foreach (Node node in createdNodes) {
			if(node != null)
				Destroy (node.gameObject);
		}

		foreach (Path path in createdPaths) {
			if(path != null)
				Destroy (path.gameObject);
		}

		createdNodes = new List<Node> ();
		createdPaths = new List<Path> ();
	}


	/// <summary>
	/// Clears the pebbles.
	/// </summary>
	public void ClearPebbles(){
		GameManager.instance.playClick ();
		foreach (Node node in createdNodes) {
			if (node != null) {
				node.Pebbles = 0;
			}
		}
	}


	/// <summary>
	/// Adds the pebble.
	/// </summary>
	public void AddPebble(){
		if (currentNode != null) {
			GameManager.instance.playHighClick ();
			currentNode.Pebbles = currentNode.Pebbles + 1;
		}
	}


	/// <summary>
	/// Removes the pebble.
	/// </summary>
	public void RemovePebble(){
		if (currentNode != null && currentNode.Pebbles > 0) {
			GameManager.instance.playClick ();
			currentNode.Pebbles = currentNode.Pebbles - 1;
		}
	}


	/// <summary>
	/// Pebbles the count field change.
	/// </summary>
	/// <param name="input">Input.</param>
	public void PebbleCountFieldChange(string input){
		input = PebbleCountFieldText.text;
		if (currentNode != null) {
			int tempPebbles;
			if(int.TryParse(input, out tempPebbles)){
				if (tempPebbles >= 0) {
					currentNode.Pebbles = tempPebbles;
				} else {
					currentNode.Pebbles = 0;
				}
			}
		}
	}


	/// <summary>
	/// Opens the save pop up.
	/// </summary>
	public void OpenSavePopUp(){
		GameManager.instance.playClick ();
		SaveGraphPopUp.Open ();
	}


	/// <summary>
	/// Saves the graph.
	/// </summary>
	public void SaveGraph(){
		string graphName = SaveGraphPopUp.Save ();
		if (UserData.instance.GraphIds.ContainsKey (graphName)) {
			ConfirmNewSavePopup.ConfirmMessage.text = "File already exists, do you want overwrite saved data?";
			ConfirmNewSavePopup.Open (graphName);
		} else {
			SaveGraph(graphName);
		}
	}


	/// <summary>
	/// Loads the graph.
	/// </summary>
	/// <returns><c>true</c>, if graph was loaded, <c>false</c> otherwise.</returns>
	/// <param name="graphName">Graph name.</param>
	public bool LoadGraph(string graphName){
		if (graphName != "" && UserData.instance.GraphIds.ContainsKey (graphName)) {
			List<Node> nodes = new List<Node> ();
			List<Path> paths = new List<Path> ();
			string fileName = UserData.instance.GraphIds [graphName];

			int nodeCount, pathCount;
			float xp, yp;

			BinaryReader br = new BinaryReader (new FileStream (fileName, FileMode.OpenOrCreate));

			nodeCount = br.ReadInt32 ();
			for (int x = 0; x < nodeCount; x++) {
				Node node = (Node)Instantiate (NodePrefab, gameObject.transform);
				node.NodeId = br.ReadInt32 ();
				node.Pebbles = br.ReadInt32 ();
				node.State = (br.ReadBoolean ()) ? NodeState.Target : NodeState.Inactive;

				xp = (float)br.ReadDouble ();
				yp = (float)br.ReadDouble ();
				node.Size = new Vector2 (xp, yp);

				xp = (float)br.ReadDouble ();
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
				Path path = (Path)Instantiate (PathPrefab, gameObject.transform);
				path.VertexOne = br.ReadInt32 ();
				path.VertexTwo = br.ReadInt32 ();
				path.Width = (float)br.ReadDouble ();
				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);
			}
			br.Close ();

			createdNodes = nodes;
			createdPaths = paths;

			goalNode = nodes.Find ((Node n) => n.state == NodeState.Target);

			Note.Success ("Graph successfully loaded");
			return true;
		}

		return false;
	}


	/// <summary>
	/// Generates the cycle graph.
	/// </summary>
	/// <param name="size">Size.</param>
	public void GenerateCycleGraph(int size){
		List<Node> nodes = new List<Node> ();
		List<Path> paths = new List<Path> ();

		Node node;
		Path path;

		Vector2 worldSize = Camera.main.ScreenToWorldPoint(new Vector2(
			Camera.main.rect.width,
			Camera.main.rect.height));

		worldSize *= -2;

		float radius = worldSize.x / 2;
		float displacement;
		float nodeSize;

		radius = worldSize.x / 2;
		displacement = Mathf.Sin (Mathf.PI / size) * radius;
		displacement /= ((11f/16f) + Mathf.Sin (Mathf.PI / size) * (2f/3f));
		nodeSize = worldSize.x * displacement;

		nodeSize = (nodeSize < 100) ? nodeSize : 100;
		Debug.Log (nodeSize);

		for (int i = 0; i < size; i++) {
			node = (Node)Instantiate (NodePrefab, gameObject.transform);
			node.NodeId = i;
			node.Pebbles = 0;
			node.Size = new Vector2 (nodeSize, nodeSize);
			node.Position = new Vector2 (
				Mathf.Cos (Mathf.PI * 2 * i / size) * (radius - (2f/3f) * displacement),
				Mathf.Sin (Mathf.PI * 2 * i / size) * (radius - (2f/3f) * displacement) - 30f / worldSize.x);
			nodes.Add (node);
		}

		for (int i = 0; i < size - 1; i++) {
			path = (Path)Instantiate (PathPrefab, gameObject.transform);
			path.VertexOne = nodes [i].NodeId;
			nodes [i].Paths.Add (i);
			path.VertexTwo = nodes [i + 1].NodeId;
			nodes [i + 1].Paths.Add (i);

			path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
			paths.Add (path);
		}

		path = (Path)Instantiate (PathPrefab, gameObject.transform);
		path.VertexOne = 0;
		path.VertexTwo = nodes.Count - 1;
		path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
		nodes [0].Paths.Add (paths.Count);
		nodes [nodes.Count - 1].Paths.Add (paths.Count);

		paths.Add (path);	

		createdNodes = nodes;
		createdPaths = paths;
	}


	/// <summary>
	/// Generates the fan graph.
	/// </summary>
	/// <param name="leftColumn">Left column.</param>
	/// <param name="rightColumn">Right column.</param>
	public void GenerateFanGraph(int leftColumn, int rightColumn){
		cs = this.gameObject.GetComponentInParent<CanvasScaler> ();

		List<Node> nodes = new List<Node> ();
		List<Path> paths = new List<Path> ();

		Node node;
		Path path;

		Vector2 worldSize = Camera.main.ScreenToWorldPoint(new Vector2(
			Camera.main.rect.width,
			Camera.main.rect.height));

		worldSize *= -2;

		float margin = worldSize.y * TopMargin.GetComponent<RectTransform> ().rect.height / cs.referenceResolution.y; 
		float displacement;
		float nodeSize = (worldSize.y - 1.5f * margin) * worldSize.x;
		Vector2 offset;

		nodeSize /= (leftColumn > rightColumn)? (leftColumn - 1) * 1.5f + 1: (rightColumn - 1) * 1.5f + 1;

		if (nodeSize > 125) {
			nodeSize = 125;
		}

		displacement = (worldSize.y - 1.5f * margin) / ((leftColumn - 1) * 1.5f + 1);
		offset = new Vector2 ((nodeSize / worldSize.x - worldSize.x) / 2, worldSize.y / 2 - margin - displacement * .5f - 30f / worldSize.x);
		for (int i = 0; i < leftColumn; i++) {
			node = (Node)Instantiate (NodePrefab, gameObject.transform);
			node.NodeId = i;
			node.Pebbles = 0;
			node.Size = new Vector2 (nodeSize, nodeSize);
			node.Position = new Vector2 (0,displacement * i * -1.5f) + offset;
			nodes.Add (node);
		}

		displacement = (worldSize.y - 1.5f * margin) / ((rightColumn - 1) * 1.5f + 1);
		offset = new Vector2 ((worldSize.x - nodeSize / worldSize.x) / 2, worldSize.y / 2 - margin - displacement * .5f - 30f / worldSize.x);
		for (int i = 0; i < rightColumn; i++) {
			node = (Node)Instantiate (NodePrefab, gameObject.transform);
			node.NodeId =  i + leftColumn;
			node.Pebbles = 0;
			node.Size = new Vector2 (nodeSize, nodeSize);
			node.Position = new Vector2 (0, displacement * i * -1.5f) + offset;
			nodes.Add (node);
		}

		for (int l = 0; l < leftColumn; l++) {
			if (l < leftColumn - 1) {
				path = (Path)Instantiate (PathPrefab, gameObject.transform);
				path.VertexOne = nodes [l].NodeId;
				nodes [l].Paths.Add (paths.Count);
				path.VertexTwo = nodes [l + 1].NodeId;
				nodes [l + 1].Paths.Add (paths.Count);

				path.Width = .15f;

				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);

			}
			for (int r = 0; r < rightColumn; r++) {
				path = (Path)Instantiate (PathPrefab, gameObject.transform);
				path.VertexOne = nodes [l].NodeId;
				nodes [l].Paths.Add (paths.Count);
				path.VertexTwo = nodes [leftColumn + r].NodeId;
				nodes [leftColumn + r].Paths.Add (paths.Count);

				path.Width = .15f;

				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);
			}
		}

		createdNodes = nodes;
		createdPaths = paths;
	}


	/// <summary>
	/// Generates the power graph.
	/// </summary>
	/// <param name="size">Size.</param>
	/// <param name="power">Power.</param>
	public void GeneratePowerGraph(int size, int power){List<Node> nodes = new List<Node> ();
		List<Path> paths = new List<Path> ();

		Node node;
		Path path;

		Vector2 worldSize = Camera.main.ScreenToWorldPoint(new Vector2(
			Camera.main.rect.width,
			Camera.main.rect.height));

		worldSize *= -2;

		float radius = worldSize.x / 2;
		float displacement;
		float nodeSize;
		float pathWidth;

		radius = worldSize.x / 2;
		displacement = Mathf.Sin (Mathf.PI / size) * radius;
		displacement /= ((11f/16f) + Mathf.Sin (Mathf.PI / size) * (2f/3f));
		nodeSize = worldSize.x * displacement;

		nodeSize = (nodeSize < 100) ? nodeSize : 100;

		pathWidth = (size * power < 40) ? .25f - (size * power / 40) * .15f : .1f;  

		for (int i = 0; i < size; i++) {
			node = (Node)Instantiate (NodePrefab, gameObject.transform);
			node.NodeId = i;
			node.Pebbles = 0;
			node.Size = new Vector2 (nodeSize, nodeSize);
			node.Position = new Vector2 (
				Mathf.Cos (Mathf.PI * 2 * i / size) * (radius - (2f/3f) * displacement),
				Mathf.Sin (Mathf.PI * 2 * i / size) * (radius - (2f/3f) * displacement) - 30f / worldSize.x);
			nodes.Add (node);
		}

		for (int p = 1; p < power + 1; p++) {
			for (int i = 0; i < size - p; i++) {
				path = (Path)Instantiate (PathPrefab, gameObject.transform);
				path.VertexOne = nodes [i].NodeId;
				nodes [i].Paths.Add (paths.Count);
				path.VertexTwo = nodes [i + p].NodeId;
				nodes [i + p].Paths.Add (paths.Count);

				path.Width = pathWidth;
				path.SetPosition (nodes [path.VertexOne], nodes [path.VertexTwo]);
				paths.Add (path);
			}
		}

		createdNodes = nodes;
		createdPaths = paths;
	}


	/// <summary>
	/// Opens save confirm.
	/// </summary>
	/// <param name="graphName">Graph name.</param>
	public void SaveConfirm(string graphName){
		SaveGraph (graphName);
	}


	/// <summary>
	/// Updates the graph.
	/// </summary>
	protected void UpdateGraph(){
		TopMargin.size = new Vector2 (rt.rect.width, TopMargin.size.y);
		BottomMargin.size = new Vector2 (rt.rect.width, 125);
		RightMargin.size = new Vector2 (125, rt.rect.height * 1.25f);
		LeftMargin.size = new Vector2 (125, rt.rect.height * 1.25f);

		foreach (Path path in createdPaths) {
			if (path != null) {
				path.SetPosition (createdNodes [path.VertexOne], createdNodes [path.VertexTwo]);

				CapsuleCollider cc = path.GetComponent<CapsuleCollider> ();
				LineRenderer l = path.GetComponent<LineRenderer> ();

				cc.radius = l.startWidth / 2 / rt.localScale.x;
				cc.transform.position = path.VertexOnePosition + (path.VertexTwoPosition - path.VertexOnePosition) / 2;
				cc.transform.LookAt (path.VertexOnePosition);
				cc.height = (path.VertexTwoPosition - path.VertexOnePosition).magnitude / rt.localScale.x;
			}
		}
	}


	/// <summary>
	/// Deletes the node.
	/// </summary>
	/// <param name="node">Node.</param>
	protected void deleteNode(Node node){
		foreach (int p in node.Paths) {
			createdNodes [createdPaths [p].End (node.NodeId)].Paths.Remove (p);
			Destroy (createdPaths [p].gameObject);
		}

		Destroy (node.gameObject);
	}


	/// <summary>
	/// Deletes the path.
	/// </summary>
	/// <param name="path">Path.</param>
	protected void deletePath(Path path){
		int PathId = createdPaths.FindIndex ((Path p)=> p.Compare(path));
		createdNodes [path.VertexOne].Paths.Remove (PathId);
		createdNodes [path.VertexTwo].Paths.Remove (PathId);

		Destroy (path.gameObject);
	}


	/// <summary>
	/// Saves the graph.
	/// </summary>
	/// <param name="graphName">Graph name.</param>
	protected void SaveGraph(string graphName){
		if (graphName != "") {
			CreateGraph ();
			if (ValidateGraph ()) {
				graph.Nodes = createdNodes;
				graph.Paths = createdPaths;
				string fileName;
				if (UserData.instance.GraphIds.ContainsKey (graphName)) {
					fileName = UserData.instance.GraphIds [graphName];
				} else {
					fileName = Application.persistentDataPath + "/Graph" + UserData.instance.SaveCount + ".map";
				}
				BinaryWriter bw = new BinaryWriter (new FileStream (fileName, FileMode.OpenOrCreate));

				bw.Write (graph.Nodes.Count);
				foreach (Node node in graph.Nodes) {
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
						
				bw.Write (graph.Paths.Count);
				foreach (Path path in graph.Paths) {
					bw.Write (path.VertexOne);
					bw.Write (path.VertexTwo);
					bw.Write ((double)path.Width);
				}
				bw.Close ();

				UserData.instance.GraphIds.Add (graphName, fileName);
				UserData.instance.Save ();

				Note.Success ("Graph Saved");
			} 
		}

		SaveGraphPopUp.Close ();
	}


	/// <summary>
	/// Creates the graph.
	/// </summary>
	protected void CreateGraph(){
		List<Node> nodes = new List<Node> ();
		List<Path> paths = new List<Path> ();

		for (int i = 0; i < createdNodes.Count; i++) {
			if (createdNodes [i] != null) {
				foreach (int p in createdNodes[i].Paths) {
					if (createdPaths [p].VertexOne == i)
						createdPaths [p].VertexOne = nodes.Count;
					else
						createdPaths [p].VertexTwo = nodes.Count;
				}
				createdNodes [i].NodeId = nodes.Count;
				createdNodes [i].Paths = new List<int> ();

				nodes.Add (createdNodes [i]);
			}
		}
		for (int i = 0; i < createdPaths.Count; i++) {
			if(createdPaths[i] != null){
				nodes [createdPaths [i].VertexOne].Paths.Add (paths.Count);
				nodes [createdPaths [i].VertexTwo].Paths.Add (paths.Count);
				paths.Add (createdPaths [i]);
			}
		}


		createdPaths = paths;
		createdNodes = nodes;
	}


	/// <summary>
	/// Validates the graph.
	/// </summary>
	/// <returns><c>true</c>, if graph was validated, <c>false</c> otherwise.</returns>
	protected bool ValidateGraph(){
		bool validGraph = true;

		int pebbleCount = 0;

		foreach (Node node in createdNodes) {
			pebbleCount += node.Pebbles;
			if (node.Paths.Count == 0) {
				validGraph = false;
				Note.Error ("Invalid Graph, Isolated Node");
				break;
			}
		}

		if (pebbleCount == 0) {
			validGraph = false;
			Note.Error ("Invalid Graph, No Pebbles");
		}

		if (goalNode == null) {
			validGraph = false;
			Note.Error ("Invalid Graph, No Goal Node");
		}

		if (validGraph) {
			if (!GraphConnected (createdNodes, goalNode)) {
				validGraph = false;
				Note.Error ("Invalid Graph, Graph Must Be Connected");
			}
		}

		return validGraph;
	}


	/// <summary>
	/// Graphs the connected.
	/// </summary>
	/// <returns><c>true</c>, if connected was graphed, <c>false</c> otherwise.</returns>
	/// <param name="nodes">Nodes.</param>
	/// <param name="goalNode">Goal node.</param>
	protected bool GraphConnected(List<Node> nodes, Node goalNode){
		List<int> rNodes = new List<int>();
		List<int> cNodes = attachedNodes (goalNode.NodeId);

		foreach (Node node in nodes) {
			rNodes.Add (node.NodeId);
		}

		rNodes.Remove (goalNode.NodeId);

		foreach (int id in cNodes) {
			rNodes.Remove (id);
			graphConnected (ref rNodes, id);
		}

		foreach (int id in rNodes) {
			Debug.Log (id);
		}
		return rNodes.Count == 0;
	}


	/// <summary>
	/// Graphs the connected.
	/// </summary>
	/// <param name="rNodes">R nodes.</param>
	/// <param name="cNodeId">C node identifier.</param>
	private void graphConnected(ref List<int> rNodes, int cNodeId){
		List<int> nodeIds = attachedNodes (cNodeId);

		foreach (int id in nodeIds) {
			if (rNodes.Contains (id)) {
				rNodes.Remove (id);
				graphConnected (ref rNodes, id);
			}
		}
	}


	/// <summary>
	/// Attacheds the nodes.
	/// </summary>
	/// <returns>The nodes.</returns>
	/// <param name="nodeId">Node identifier.</param>
	private List<int> attachedNodes(int nodeId){
		List<int> nodes = new List<int> ();
		Node node = createdNodes.Find(n => n.NodeId == nodeId);

		if (node) {
			foreach (int i in node.Paths) {
				nodes.Add (createdPaths [i].End (nodeId));
			}
		}

		return nodes;
	}
}

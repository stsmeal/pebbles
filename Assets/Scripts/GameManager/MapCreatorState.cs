using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


/// <summary>
/// Map creator state.
/// </summary>
public class MapCreatorState : AState {
	protected enum State{
		Creator,
		ManageMenu,
		MainMenu,
		GenerateGraphMenu
	}


	[Header("Containers")]
	public GameObject GraphSpace;
	public GameObject MainMenu;
	public GameObject ManageMenu;
	public GameObject CreatorUI;
	public GameObject GenerateGraphMenu;

	[Header("Menu Contents")]
	public GameObject ManageMenuContent;

	[Header("UI")]
	public GameObject BackButton;

	[Header("Prefabs")]
	public EditDeleteRow MenuButtonPrefab;

	[Header("Generate Graph UI")]
	public GameObject GenerateCycleGraphPopup;
	public InputField CycleGraphSizeField;
	public GameObject GenerateFanGraphPopup;
	public InputField LeftColumnFanGraphSizeField;
	public InputField RightColumnFanGraphSizeField;
	public GameObject GeneratePowerGraphPoup;
	public InputField PowerGraphSizeField;
	public InputField PowerGraphPowerField;
	public GameObject BackgroundFade;

	[Header("Other Data")]
	public Creator creator;
	public ConfirmPopup ConfirmDeletePopup;
	public NotificationPopup Note;

	protected State state;
	protected State previousState;


	/// <summary>
	/// Enter the specified from.
	/// </summary>
	/// <param name="from">From.</param>
	public override void Enter (AState from){
		state = State.MainMenu;
		previousState = State.MainMenu;
		this.gameObject.SetActive (true);

		GraphSpace.SetActive (false);
		MainMenu.SetActive (true);
		ManageMenu.SetActive (false);
		CreatorUI.SetActive (false);
		GenerateGraphMenu.SetActive (false);
		BackButton.SetActive (true);

		ConfirmDeletePopup.ConfirmClick.AddListener (DeleteConfirm);
	}


	/// <summary>
	/// Exit the specified to.
	/// </summary>
	/// <param name="to">To.</param>
	public override void Exit (AState to){
		this.gameObject.SetActive (false);

		GraphSpace.SetActive (false);
		MainMenu.SetActive (false);
		ManageMenu.SetActive (false);
		CreatorUI.SetActive (false);
		GenerateGraphMenu.SetActive (false);
		BackButton.SetActive (false);

		Helper.DestroyChildren (ManageMenuContent);
		Helper.DestroyChildren (GraphSpace);

		ConfirmDeletePopup.ConfirmClick.RemoveListener (DeleteConfirm);
	}


	/// <summary>
	/// Tick this instance.
	/// </summary>
	public override void Tick (){
		if(state == State.Creator)
			creator.Tick ();
	}


	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <returns>The name.</returns>
	public override string GetName (){
		return "MapCreator";
	}


	/// <summary>
	/// Opens the creator.
	/// </summary>
	public void OpenCreator(){
		creator.Open ();

		if (state == State.ManageMenu) {
			previousState = State.ManageMenu;
			ManageMenu.SetActive (false);
			Helper.DestroyChildren (ManageMenuContent);
		} else if (state == State.MainMenu) {
			GameManager.instance.playClick ();
			previousState = State.MainMenu;
			MainMenu.SetActive (false);
		} else if (state == State.GenerateGraphMenu) {
			previousState = State.GenerateGraphMenu;
			GenerateGraphMenu.SetActive (false);
		}

		state = State.Creator;
		GraphSpace.SetActive (true);
		CreatorUI.SetActive (true);
		BackButton.SetActive (false);
	}


	/// <summary>
	/// Closes the creator.
	/// </summary>
	public void CloseCreator(){
		GameManager.instance.playClick ();
		creator.Close ();

		if (previousState == State.ManageMenu) {
			state = State.ManageMenu;
			ManageMenu.SetActive (true);
			LoadManageMenuContents ();
		} else if (previousState == State.MainMenu) {
			state = State.MainMenu;
			MainMenu.SetActive (true);
		} else if (previousState == State.GenerateGraphMenu) {
			state = State.GenerateGraphMenu;
			GenerateGraphMenu.SetActive (true);
		}

		GraphSpace.SetActive (false);
		CreatorUI.SetActive (false);
		BackButton.SetActive (true);
	}

	public void OpenGenerateGraphMenu(){
		GameManager.instance.playClick ();
		state = State.GenerateGraphMenu;
		MainMenu.SetActive (false);
		GenerateGraphMenu.SetActive (true);
	}


	/// <summary>
	/// Opens the manage menu.
	/// </summary>
	public void OpenManageMenu(){
		GameManager.instance.playClick ();
		if (UserData.instance.GraphIds.Count > 0) {
			state = State.ManageMenu;
			MainMenu.SetActive (false);
			ManageMenu.SetActive (true);
			LoadManageMenuContents ();
		} else {
			Note.Error ("There are no saved graphs");
		}
	}


	/// <summary>
	/// Back this instance.
	/// </summary>
	public void Back(){
		GameManager.instance.playClick ();
		switch (state) {
		case State.Creator:
			break;
		case State.MainMenu:
			GameManager.instance.SwitchState ("MainMenu");
			break;
		case State.ManageMenu:
			Helper.DestroyChildren (ManageMenuContent);
			ManageMenu.SetActive (false);
			MainMenu.SetActive (true);
			state = State.MainMenu;
			break;
		case State.GenerateGraphMenu: 
			GenerateGraphMenu.SetActive (false);
			MainMenu.SetActive (true);
			state = State.MainMenu;
			break;
		}
	}

	public void OpenGenerateCycleGraphPopup(){
		GameManager.instance.playClick ();
		GenerateCycleGraphPopup.SetActive (true);
		BackgroundFade.SetActive (true);
		CycleGraphSizeField.text = "";
	}

	public void OpenGenerateFanGraphPopup(){
		GameManager.instance.playClick ();
		GenerateFanGraphPopup.SetActive (true);
		BackgroundFade.SetActive (true);
		LeftColumnFanGraphSizeField.text = "";
		RightColumnFanGraphSizeField.text = "";
	}

	public void OpenGeneratePowerGraphPopup(){
		GameManager.instance.playClick ();
		GeneratePowerGraphPoup.SetActive (true);
		BackgroundFade.SetActive (true);
		PowerGraphSizeField.text = "";
		PowerGraphPowerField.text = "";
	}

	public void CloseGenerateCycleGraphPopup(){
		GameManager.instance.playClick ();
		GenerateCycleGraphPopup.SetActive (false);
		BackgroundFade.SetActive (false);
	}


	public void CloseGenerateFanGraphPopup(){
		GameManager.instance.playClick ();
		GenerateFanGraphPopup.SetActive (false);
		BackgroundFade.SetActive (false);
	}


	public void CloseGeneratePowerGraphPopup(){
		GameManager.instance.playClick ();
		GeneratePowerGraphPoup.SetActive (false);
		BackgroundFade.SetActive (false);
	}

	public void GenerateCycleGraph(){
		if (CycleGraphSizeField.text != "") {
			int size = int.Parse (CycleGraphSizeField.text);
			if (size > 2) {
				if (size <= 25) {
					creator.GenerateCycleGraph (size);
					CloseGenerateCycleGraphPopup ();
					OpenCreator ();
				} else {
					Note.Error ("Graph must have 25 or less nodes");
				}
			} else {
				Note.Error ("Graph must have more than 2 nodes");
			}
		} else {
			Note.Error ("Enter Graph Size");
		}
	}


	public void GenerateFanGraph(){
		if (LeftColumnFanGraphSizeField.text != "") {
			if (RightColumnFanGraphSizeField.text != "") {
				int leftSize = int.Parse (LeftColumnFanGraphSizeField.text);
				int rightSize = int.Parse (RightColumnFanGraphSizeField.text);

				if (leftSize >= 2) {
					if (leftSize <= 12) {
						if (rightSize >= 2) {
							if (rightSize <= 12) {
								creator.GenerateFanGraph (leftSize, rightSize);
								CloseGenerateFanGraphPopup ();
								OpenCreator ();
							} else {
								Note.Error ("Right column must have 12 or less nodes");
							}
						} else {
							Note.Error ("Right column must have at least two nodes");
						}
					} else {
						Note.Error ("Left column must have 12 or less nodes");
					}
				} else {
					Note.Error ("Left column must have at least two nodes");
				}
			} else {
				Note.Error ("Enter size of right column");
			}
		} else {
			Note.Error ("Enter size of left column");
		}
	}

	public void GeneratePowerGraph(){
		if (PowerGraphSizeField.text != "") {
			if (PowerGraphPowerField.text != "") {
				int size = int.Parse(PowerGraphSizeField.text);
				int power = int.Parse (PowerGraphPowerField.text);

				if (size > 2) {
					if (size <= 25) {
						if (power > 0) {
							if (size > power) {
								creator.GeneratePowerGraph (size, power);
								CloseGeneratePowerGraphPopup ();
								OpenCreator ();
							} else {
								Note.Error ("The power of graph cannot exceed size");
							}
						} else {
							Note.Error ("Power must be greater than zero");
						}
					} else {
						Note.Error ("Graph must have 25 or less nodes");
					}
				} else {
					Note.Error ("Graph must have more than 2 nodes");
				}
			} else {
				Note.Error ("Enter Power of Graph");
			}
		} else {
			Note.Error ("Enter Size of Graph");
		}
	}

	/// <summary>
	/// Loads the manage menu contents.
	/// </summary>
	private void LoadManageMenuContents(){
		float contentSize = MenuButtonPrefab.GetComponent<RectTransform> ().rect.height + 25f;

		foreach(string name in UserData.instance.GraphIds.Keys) {
			EditDeleteRow button = (EditDeleteRow)Instantiate (MenuButtonPrefab, ManageMenuContent.transform);
			button.Content.text = name;
			button.EditClick.AddListener (EditSelectedMap);
			button.DeleteClick.AddListener (DeleteSelectedMap);
		}

		RectTransform smm = ManageMenuContent.GetComponent<RectTransform> ();
		smm.sizeDelta = new Vector2 (smm.sizeDelta.x,
			contentSize * (UserData.instance.GraphIds.Count));
	}


	/// <summary>
	/// Edits the selected map.
	/// </summary>
	/// <param name="text">Text.</param>
	private void EditSelectedMap(string text){
		GameManager.instance.playClick ();
		if (creator.LoadGraph (text)) {
			OpenCreator ();
		} else {
			Note.Error ("Graph failed to load");
		}
	}

	/// <summary>
	/// Deletes the selected map.
	/// </summary>
	/// <param name="text">Text.</param>
	private void DeleteSelectedMap(string text){
		GameManager.instance.playClick ();
		ConfirmDeletePopup.ConfirmMessage.text = "Are you sure you want to delete";
		ConfirmDeletePopup.Open (text);
	}

	private void DeleteConfirm(string content){
		GameManager.instance.playClick ();
		UserData.instance.DeleteGraph (content);
		if (state == State.ManageMenu) {

			Helper.DestroyChildren (ManageMenuContent);
			LoadManageMenuContents ();
		}
	}
}

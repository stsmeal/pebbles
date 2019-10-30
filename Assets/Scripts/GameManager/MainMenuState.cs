using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Main menu state.
/// </summary>
public class MainMenuState : AState {

	[Header("Popups")]
	public GameObject SettingsPopup;
	public GameObject ConfirmPopup;

	[Header("Background Fades")]
	public GameObject SettingsBackgroundFade;
	public GameObject ConfirmBackgroundFade;

	[Header("Other")]
	public Text ConfirmMessage;
	public Toggle SoundToggle;
	public NotificationPopup Note;

	private int DeleteSelector;


	/// <summary>
	/// Enter the specified from.
	/// </summary>
	/// <param name="from">From.</param>
	public override void Enter (AState from){
		gameObject.SetActive (true);
		SettingsPopup.SetActive (false);
		SettingsBackgroundFade.SetActive (false);
		ConfirmPopup.SetActive (false);
		ConfirmBackgroundFade.SetActive (false);

		DeleteSelector = 0;
	}


	/// <summary>
	/// Exit the specified to.
	/// </summary>
	/// <param name="to">To.</param>
	public override void Exit (AState to){
		gameObject.SetActive (false);
	}


	/// <summary>
	/// Tick this instance.
	/// </summary>
	public override void Tick (){

	}


	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <returns>The name.</returns>
	public override string GetName (){
		return "MainMenu";
	}


	/// <summary>
	/// Starts the click.
	/// </summary>
	public void StartClick(){
		GameManager.instance.playClick ();
		GameManager.instance.SwitchState ("Game");
	}


	/// <summary>
	/// Creators the click.
	/// </summary>
	public void CreatorClick(){
		GameManager.instance.playClick ();
		GameManager.instance.SwitchState ("MapCreator");
	}


	/// <summary>
	/// Replaies the click.
	/// </summary>
	public void ReplayClick(){
		if (UserData.instance.ReplayIds.Count > 0) {
			GameManager.instance.playClick ();
			GameManager.instance.SwitchState ("Replay");
		} else {
			Note.Error ("There are no saved replays");
		}
	}


	/// <summary>
	/// Settingses the click.
	/// </summary>
	public void SettingsClick(){
		GameManager.instance.playClick ();
		SoundToggle.isOn = UserData.instance.SoundOn;
		SettingsPopup.SetActive (true);
		SettingsBackgroundFade.SetActive (true);
	}

	public void ToggleSound(bool value){
		UserData.instance.SoundOn = SoundToggle.isOn;
		UserData.instance.Save ();
	}


	/// <summary>
	/// Closes the settings popup.
	/// </summary>
	public void CloseSettingsPopup(){
		GameManager.instance.playClick ();
		SettingsPopup.SetActive (false);
		SettingsBackgroundFade.SetActive (false);
	}


	/// <summary>
	/// Opens the confirm popup.
	/// </summary>
	public void OpenConfirmPopup(){
		GameManager.instance.playClick ();
		ConfirmPopup.SetActive (true);
		ConfirmBackgroundFade.SetActive (true);
	}


	/// <summary>
	/// Closes the confirm popup.
	/// </summary>
	public void CloseConfirmPopup(){
		GameManager.instance.playClick ();
		ConfirmPopup.SetActive (false);
		ConfirmBackgroundFade.SetActive (false);
	}


	/// <summary>
	/// Deletes the games click.
	/// </summary>
	public void DeleteGamesClick(){
		DeleteSelector = 1;
		ConfirmMessage.text = "Are you sure you want to delete saved games?";
		OpenConfirmPopup ();
	}


	/// <summary>
	/// Deletes the graphs click.
	/// </summary>
	public void DeleteGraphsClick(){
		DeleteSelector = 2;
		ConfirmMessage.text = "Are you sure you want to delete created graphs?";
		OpenConfirmPopup ();
	}


	/// <summary>
	/// Deletes the replays click.
	/// </summary>
	public void DeleteReplaysClick(){
		DeleteSelector = 3;
		ConfirmMessage.text = "Are you sure you want to delete saved replays?";
		OpenConfirmPopup ();
	}


	/// <summary>
	/// Deletes the click.
	/// </summary>
	public void DeleteClick(){
		if (DeleteSelector == 1) {
			UserData.instance.DeleteSavedGames ();
		} else if (DeleteSelector == 2) {
			UserData.instance.DeleteGraphs ();
		} else if (DeleteSelector == 3) {
			UserData.instance.DeleteReplays ();
		}

		CloseConfirmPopup ();
	}

	/// <summary>
	/// Enters Tutorial State
	/// </summary>
	public void TutorialClick(){
		GameManager.instance.playClick ();
		GameManager.instance.SwitchState ("Tutorial");
	}
}

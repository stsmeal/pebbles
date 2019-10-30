using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Save popup.
/// </summary>
public class SavePopup : MonoBehaviour {
	public GameObject BackgroundFade;
	public NotificationPopup note;
	public InputField FileNameInput;


	/// <summary>
	/// Open this instance.
	/// </summary>
	public void Open(){
		FileNameInput.text = "";
		this.gameObject.SetActive (true);
		BackgroundFade.SetActive (true);
	}

	/// <summary>
	/// Save this instance.
	/// </summary>
	public string Save(){
		if (FileNameInput.text == "") {
			note.Error ("Enter FileName");
			return "";
		}

		return FileNameInput.text;
	}

	/// <summary>
	/// Close this instance.
	/// </summary>
	public void Close(){
		GameManager.instance.playClick ();
		this.gameObject.SetActive (false);
		BackgroundFade.SetActive (false);
	}
}

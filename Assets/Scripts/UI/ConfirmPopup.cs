using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour {
	public GameObject Popup;
	public GameObject BackgroundFade;
	public ClickEvent ConfirmClick;
	public Text ConfirmMessage;

	private string content = "";

	public void Open(string content){
		this.gameObject.SetActive (true);
		this.content = content;
		Popup.SetActive (true);
		BackgroundFade.SetActive (true);

		if (ConfirmClick == null)
			ConfirmClick = new ClickEvent ();
	}

	public void Close(){
		GameManager.instance.playClick ();
		gameObject.SetActive (false);
	}

	public void OnConfirmClick(){
		ConfirmClick.Invoke (content);
		Close ();
	}
}

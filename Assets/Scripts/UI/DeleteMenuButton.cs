using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteMenuButton : MonoBehaviour {
	public Text Content;
	public ClickEvent ContentClick;
	public ClickEvent DeleteClick;

	// Use this for initialization
	void Start () {
		if (ContentClick == null) {
			ContentClick = new ClickEvent ();
		}
		
		if (DeleteClick == null) {
			DeleteClick = new ClickEvent ();
		}
	}

	public void OnContentClick(){
		if (Content != null) {
			ContentClick.Invoke (Content.text.ToString ());
		}
	}

	public void OnDeleteClick(){
		if (Content != null) {
			DeleteClick.Invoke (Content.text.ToString ());
		}
	}
}

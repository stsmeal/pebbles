using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ClickEvent : UnityEvent<string>{
}

public class EditDeleteRow : MonoBehaviour {
	public Text Content;
	public ClickEvent EditClick;
	public ClickEvent DeleteClick;

	// Use this for initialization
	void Start () {
		if (EditClick == null)
			EditClick = new ClickEvent ();
		
		if (DeleteClick == null)
			DeleteClick = new ClickEvent ();
	}
	
	public void OnEditClick(){
		if(Content != null)
			EditClick.Invoke (Content.text.ToString ());
	}

	public void OnDeleteClick(){
		if(Content != null)
			DeleteClick.Invoke (Content.text.ToString ());
	}
}

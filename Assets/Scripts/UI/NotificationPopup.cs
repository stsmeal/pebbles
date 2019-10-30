using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Notification popup.
/// </summary>
public class NotificationPopup : MonoBehaviour {
	private float timeStart;
	private float Duration;
	private bool isOpened = false;

	/// <summary>
	/// Success the specified message.
	/// </summary>
	/// <param name="message">Message.</param>
	public void Success(string message){
		this.gameObject.SetActive (true);
		Text text = GetComponentInChildren<Text> ();
		text.color = Color.black;
		text.text = message;
		timeStart = Time.time;
		isOpened = true;
		Duration = 2f;
	}


	/// <summary>
	/// Success the specified message and duration.
	/// </summary>
	/// <param name="message">Message.</param>
	/// <param name="duration">Duration.</param>
	public void Success(string message, float duration){
		Success (message);
		Duration = duration; 
	}


	/// <summary>
	/// Error the specified message.
	/// </summary>
	/// <param name="message">Message.</param>
	public void Error(string message){
		this.gameObject.SetActive (true);
		Text text = GetComponentInChildren<Text> ();
		text.color = Color.red;
		text.text = message;
		timeStart = Time.time;
		isOpened = true;
		Duration = 2f;
	}


	/// <summary>
	/// Error the specified message and duration.
	/// </summary>
	/// <param name="message">Message.</param>
	/// <param name="duration">Duration.</param>
	public void Error(string message, float duration){
		Error (message);
		Duration = duration;
	}
	
	// Update is called once per frame
	void Update () {
		if (isOpened) {
			if (Time.time - timeStart > Duration) {
				isOpened = false;
				this.gameObject.SetActive (false);
			}
		}
	}
}

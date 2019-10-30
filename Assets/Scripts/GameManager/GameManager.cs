using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Game manager.
/// </summary>
public class GameManager : MonoBehaviour {
	static public GameManager instance{ get { return s_Instance; } }
	static protected GameManager s_Instance;

	public AState[] States;
	public AudioClip ClickSound;
	public AudioClip HighClickSound;

	/// <summary>
	/// Gets the input position.
	/// </summary>
	/// <value>The input position.</value>
	public static Vector2 InputPosition { 
		get { 
			if (Input.touchCount > 0)
				return Input.GetTouch (0).position;
			else if (Input.touchCount == 0 && touchOld)
				return touchPosition;
			else
				return Input.mousePosition;
		} 
	}

	/// <summary>
	/// Gets the input world position.
	/// </summary>
	/// <value>The input world position.</value>
	public static Vector2 InputWorldPosition { get { return (Vector2)Camera.main.ScreenToWorldPoint(InputPosition);} }

	/// <summary>
	/// Gets the input position secondary.
	/// </summary>
	/// <value>The input position secondary.</value>
	public static Vector2 InputPositionSecondary { get { return (Input.touchCount > 1) ? Input.GetTouch (1).position : Vector2.zero;}}

	/// <summary>
	/// Gets the input world position secondary.
	/// </summary>
	/// <value>The input world position secondary.</value>
	public static Vector2 InputWorldPositionSecondary { get { return (Vector2)Camera.main.ScreenToWorldPoint(InputPositionSecondary); } }

	/// <summary>
	/// Gets a value indicating whether this <see cref="GameManager"/> input began.
	/// </summary>
	/// <value><c>true</c> if input began; otherwise, <c>false</c>.</value>
	public static bool InputBegan{ 
		get 
		{ 
			return (Input.touchCount > 0 && !touchOld) || Input.GetMouseButtonDown (0);
		} 
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="GameManager"/> input end.
	/// </summary>
	/// <value><c>true</c> if input end; otherwise, <c>false</c>.</value>
	public static bool InputEnd{ 
		get 
		{ 
			return (Input.touchCount == 0 && touchOld) || Input.GetMouseButtonUp (0); 
		} 
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="GameManager"/> input down.
	/// </summary>
	/// <value><c>true</c> if input down; otherwise, <c>false</c>.</value>
	public static bool InputDown{ 
		get { 
			return (Input.touchCount > 0 || Input.GetMouseButton (0));
		} 
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="GameManager"/> input multiple.
	/// </summary>
	/// <value><c>true</c> if input multiple; otherwise, <c>false</c>.</value>
	public static bool InputMultiple{ get { return Input.touchCount > 1; } }

	protected Dictionary<string, AState> m_States;
	protected AState c_State = null;

	private static Vector2 touchPosition;
	private static bool touchOld;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start(){
		UserData.Create ();
		ColorPack.Create ();

		s_Instance = this;

		touchOld = false;
		touchPosition = new Vector2(0,0);

		if (States.Length == 0)
			return;

		m_States = new Dictionary<string, AState> ();
		foreach (var state in States)
			m_States.Add (state.GetName (), state);
		
		c_State = States [0];

		if (UserData.instance.NewUser) {
			SwitchState ("Tutorial");
		} 
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	protected void Update(){
		if(c_State != null)
			c_State.Tick ();
		
		if (Input.touchCount > 0) {
			touchPosition = Input.GetTouch (0).position;
			touchOld = true;
		} else {
			touchOld = false;
		}
	}


	/// <summary>
	/// Switchs the state.
	/// </summary>
	/// <param name="newState">New state.</param>
	public void SwitchState(string newState){
		AState state = FindState (newState);

		if (state == null)
			return;

		c_State.Exit (state);
		state.Enter (c_State);
		c_State = state;
	}

	public void playClick(){
		if (UserData.instance.SoundOn) {
			AudioSource source = GetComponent<AudioSource> ();
			source.PlayOneShot (ClickSound);
		}
	}

	public void playHighClick(){
		if (UserData.instance.SoundOn) {
			AudioSource source = GetComponent<AudioSource> ();
			source.PlayOneShot (HighClickSound);
		}
	}


	/// <summary>
	/// Finds the state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="stateName">State name.</param>
	protected AState FindState(string stateName){
		AState state;
		if (!m_States.TryGetValue (stateName, out state))
			return null;

		return state;
	}

}


/// <summary>
/// A state.
/// </summary>
public abstract class AState: MonoBehaviour{
	[HideInInspector]
	public GameManager manager;

	public abstract void Enter (AState from);
	public abstract void Exit (AState to);
	public abstract void Tick ();

	public abstract string GetName();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// Helper Class contains useful helper functions
/// </summary>
public class Helper {
	/// <summary>
	/// Gets the button click text.
	/// </summary>
	/// <returns>The button click text.</returns>
	static public string GetButtonClickText(){
		return EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text> ().text;
	}

	/// <summary>
	/// Destroies the children.
	/// </summary>
	/// <param name="gameObject">Game object.</param>
	static public void DestroyChildren(GameObject gameObject){
		var children = gameObject.GetComponentsInChildren<Transform> ();
		foreach (var c in children) {
			if(c.name != gameObject.name)
				MonoBehaviour.Destroy (c.gameObject.gameObject);
		}
	}


	/// <summary>
	/// Overlaps the node.
	/// </summary>
	/// <returns>The node.</returns>
	/// <param name="position">Position.</param>
	static public Node OverlapNode(Vector2 position){
		Collider2D c = Physics2D.OverlapPoint (position);
		if (c)
			return (Node)c.GetComponent ("Node");
		else
			return null;
	}


	/// <summary>
	/// Overlaps the node.
	/// </summary>
	/// <returns><c>true</c>, if node was overlaped, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	/// <param name="node">Node.</param>
	static public bool OverlapNode(Vector2 position, out Node node){
		Collider2D c = Physics2D.OverlapPoint (position);

		if (c)
			node = (Node)c.GetComponent ("Node");
		else
			node = null;

		return (node != null);
	}
}

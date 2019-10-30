using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Node state.
/// </summary>
public enum NodeState{
	Active,
	Hightlighted,
	Target,
	Inactive,
	Disabled
}


/// <summary>
/// Node.
/// </summary>
public class Node : MonoBehaviour {
	public int NodeId{ get { return nodeId; } set { nodeId = value; } }

	/// <summary>
	/// Gets or sets the size.
	/// </summary>
	/// <value>The size.</value>
	public Vector2 Size {
		get{
			var tr = GetComponent<RectTransform> ();
			return new Vector2 (
				tr.localScale.x * tr.rect.width,
				tr.localScale.y * tr.rect.height);
		}
		set{
			var tr = GetComponent<RectTransform> ();
			tr.localScale = new Vector3 (
				value.x / tr.rect.width,
				value.y / tr.rect.height,
				1f);
		}
	}

	/// <summary>
	/// Gets or sets the position.
	/// </summary>
	/// <value>The position.</value>
	public Vector2 Position{
		get{
			var sp = GetComponent<SpriteRenderer> ();
			return new Vector2(
				sp.transform.position.x, 
				sp.transform.position.y);
		}
		set{
			var sp = GetComponent<SpriteRenderer> ();
			sp.transform.position = new Vector3 (
				value.x,
				value.y,
				0);
		}
	}

	/// <summary>
	/// Gets or sets the pebbles.
	/// </summary>
	/// <value>The pebbles.</value>
	public int Pebbles {
		get{ return (State == NodeState.Target)? 0: pebbles; }
		set {
			var t = GetComponent<Text> ();
			if (State != NodeState.Target) {
				pebbles = value;
				t.text = pebbles.ToString();
			} else {
				t.text = "Goal";
			}
		}
	}

	/// <summary>
	/// Gets or sets the state.
	/// </summary>
	/// <value>The state.</value>
	public NodeState State {
		get { return state; }
		set {
			var sp = GetComponent<SpriteRenderer> ();

			switch (value) {
			case NodeState.Active:
				sp.color = ColorPack.Colors.Active;
				break;
			case NodeState.Hightlighted:
				sp.color = ColorPack.Colors.Highlighted;
				break;
			case NodeState.Target:
				sp.color = ColorPack.Colors.Target;
				break;
			case NodeState.Inactive:
				sp.color = ColorPack.Colors.Inactive;
				break;
			case NodeState.Disabled:
				sp.color = ColorPack.Colors.Disabled;
				break;
			}

			state = value;
			Pebbles = pebbles;
		}
	}

	public NodeState state;
	public int pebbles;
	public List<int> Paths;
	private int nodeId;

	// Use this for initialization
	void Start () {
		if (State != NodeState.Target)
			State = NodeState.Inactive;
		else
			State = NodeState.Target;

		Pebbles = pebbles;
	}
}

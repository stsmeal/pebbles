using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Path.
/// </summary>
public class Path : MonoBehaviour {
	public int VertexOne;
	public int VertexTwo;

	/// <summary>
	/// Gets or sets the color of the line.
	/// </summary>
	/// <value>The color of the line.</value>
	public Color LineColor{
		get{
			var l = GetComponent<LineRenderer> ();
			return l.endColor;
		}
		set{
			var l = GetComponent<LineRenderer> ();
			l.endColor = value;
			l.startColor = value;
		}
	}


	/// <summary>
	/// Gets or sets the width.
	/// </summary>
	/// <value>The width.</value>
	public float Width{
		get{
			var l = GetComponent<LineRenderer> ();
			return l.endWidth;
		}
		set{
			var l = GetComponent<LineRenderer> ();
			l.endWidth = value;
			l.startWidth = value;
		}
	}


	/// <summary>
	/// Gets or sets the vertex one position.
	/// </summary>
	/// <value>The vertex one position.</value>
	public Vector2 VertexOnePosition{
		get{
			return (Vector2)GetComponent<LineRenderer> ().GetPosition (0);
		}
		set{
			GetComponent<LineRenderer> ().SetPosition (0, value);
		}
	}


	/// <summary>
	/// Gets or sets the vertex two position.
	/// </summary>
	/// <value>The vertex two position.</value>
	public Vector2 VertexTwoPosition{
		get{
			return (Vector2)GetComponent<LineRenderer> ().GetPosition (1);
		}
		set{
			GetComponent<LineRenderer> ().SetPosition (1, value);
		}
	}


	/// <summary>
	/// Compare the specified node1 and node2.
	/// </summary>
	/// <param name="node1">Node1.</param>
	/// <param name="node2">Node2.</param>
	public bool Compare(int node1, int node2){
		if (node1 == VertexOne && node2 == VertexTwo)
			return true;
		else if (node1 == VertexTwo && node2 == VertexOne)
			return true;
		else
			return false;
	}

	/// <summary>
	/// Compare the specified otherPath.
	/// </summary>
	/// <param name="otherPath">Other path.</param>
	public bool Compare(Path otherPath){
		if (otherPath.VertexOne == VertexOne && otherPath.VertexTwo == VertexTwo)
			return true;
		else if (otherPath.VertexOne == VertexTwo && otherPath.VertexTwo == VertexOne)
			return true;
		else
			return false;
	}

	/// <summary>
	/// End the specified node. Returns -1, if node is not contained in path
	/// </summary>
	/// <param name="node">Node.</param>
	public int End(int node){
		if (node == VertexOne)
			return VertexTwo;
		else if (node == VertexTwo)
			return VertexOne;
		else
			return -1;
	}

	/// <summary>
	/// Sets the position.
	/// </summary>
	/// <param name="n1">N1.</param>
	/// <param name="n2">N2.</param>
	public void SetPosition(Node n1, Node n2){
		var l = GetComponent<LineRenderer> ();
		l.SetPositions (new Vector3[]{ n1.Position, n2.Position });
	}
}

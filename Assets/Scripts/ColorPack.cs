using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPack {
	static public ColorPack Colors { get { return m_Colors; } }
	static protected ColorPack m_Colors;

	public Color Active;
	public Color Target;
	public Color Inactive;
	public Color Highlighted;
	public Color Disabled;


	static public void Create(){
		if (m_Colors == null)
			m_Colors = new ColorPack ();
	}

	public ColorPack(){
		SetToDefaults ();
	}

	public void SetToDefaults(){
		Active = Color.yellow;
		Target = Color.gray;
		Inactive = Color.white;
		Highlighted = new Color(1f, .85f, .01f, .95f);
		Disabled = Color.red;
	}
}

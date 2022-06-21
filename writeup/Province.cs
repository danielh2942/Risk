using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Province.cs
 *  Author: Daniel Hannon (19484286)
 *  Version: 1
 */

public class Province : MonoBehaviour {
	public string ProvinceName;
	public string Color = "neutral";
	public int TroopCount = 0;
	public GameObject Flag;
	public GameObject TroopField;
	public GameObject[] neighbors;
	private LineRenderer highlight = null;


	void start() {
		TroopCount = 0;
		Color = "neutral";
	}

	void Update() {

		if(TroopField!= null) {
			TroopField.GetComponent<TextMesh>().text = TroopCount.ToString();
		}
	}

   public void Select() {
		//The select method creates a LineRenderer and follows the path set by the PolygonCollider used to outline the border of the country
		//As a means of letting the user know what territory they currently have selected, I realized this would be a better option than
		//Some UI prompt that has something like *Currently Selected: Region Name* as region names are not on the map
		highlight = gameObject.AddComponent<LineRenderer>();
		if (highlight) {
			Vector2[] path = gameObject.GetComponent<PolygonCollider2D>().points;
			Color black = new Color(0, 0, 0, 1);
			highlight.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
			highlight.startColor = black;
			highlight.endColor = black;
			highlight.startWidth = 0.03f;
			for (int i = 0; i < path.Length; i++) {
				path[i] = gameObject.transform.TransformPoint(path[i]);
			}
			highlight.positionCount = path.Length + 1;
			for (int i = 0; i < path.Length; i++) {
				Vector3 finalLine = path[i];
				finalLine.z = 30;
				highlight.SetPosition(i, finalLine);

				if (i == (path.Length - 1)) {
					finalLine = path[0];
					finalLine.z = 30;
					highlight.SetPosition(path.Length, finalLine);
				}
			}
		}
	}

	public void Deselect() {
		//This destroys the LineRenderer (I was worried it'd delete the points for the PolygonCollider but luckily it does not)
		if (highlight) {
			Destroy(gameObject.GetComponent<LineRenderer>());
			highlight = null;
		}
	}
}

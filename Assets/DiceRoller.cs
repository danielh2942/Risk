using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *	DiceRoller.cs
 *	Author: Daniel Hannon (19484286)
 *	Version: 1
 */

public class DiceRoller : MonoBehaviour {
	// Start is called before the first frame update
	public Sprite[] AttackDiceFaces;
	public Sprite[] DefenseDiceFaces;
	public int dice_type;
	public int current_value = 0;

	public int Roll() {
		current_value = Random.Range(1, 7);
		if (dice_type == 0) {
			//Attack Dice
			gameObject.GetComponent<Image>().sprite = AttackDiceFaces[current_value];
		}
		else {
			//Defense Dice
			gameObject.GetComponent<Image>().sprite = DefenseDiceFaces[current_value];
		}
		return current_value;
	}

	public void SetInactive() {
		gameObject.GetComponent<Image>().sprite = AttackDiceFaces[0];
	}
}

﻿using UnityEngine;
using System.Collections;

public class Mouse_Controls : MonoBehaviour {
	// This script will handle the Mouse Controls including:
		// Friendly Unit SELECTION **
		// Target SELECTION that overrides the selected unit's current target **
		// Menu interaction
		// Placing Units on the battle map

	GameObject selectedUnit;
	GameObject selectedTarget;

	void Start () {
	
	}
	
	void Update () {
		SelectUnit ();
		if (selectedUnit != null) {
			Captain cpn = selectedUnit.GetComponent<Captain> ();
			SelectTargetWithMouse(cpn);
		}
	}


	void SelectUnit (){
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), -Vector2.up);  
		if (hit.collider != null) {
			if (hit.collider.CompareTag ("Captain")) {
				if (Input.GetMouseButtonDown(0)){
					print ("You clicked on Captain.");
					selectedUnit = hit.collider.gameObject; // this stores the selected unit as a GameObject
				}
			}
		}
	} 


	void SelectTargetWithMouse(Captain selectCaptain){
		// this will check if the Mouse is over a Monster, if it IS  when user RIGHTCLICKS it returns target as GameObject 
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), -Vector2.up);  
		if (hit.collider != null) {
			if (hit.collider.CompareTag ("Enemy")) {
				if (Input.GetMouseButtonDown (1)) {
					selectedTarget = hit.collider.gameObject;
					selectCaptain.PlayerTargetAssign(selectedTarget);
					print ("You clicked a Monster");
				}
			}
		} 

	}

	// TODO: A way to activate the Selection Box on the current captain, then deactivate when deselected
	void ActivateSelectionBox(GameObject selected){
		SpriteRenderer sr = selected.GetComponentInChildren<SpriteRenderer> ();
		sr.enabled = true;
	}


}

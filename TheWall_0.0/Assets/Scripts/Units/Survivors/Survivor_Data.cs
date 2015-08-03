﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class Survivor_Data {
	// Survivor vars:
	public Sprite mySprite;

	//Mood: (-10 traitorous - 0 OK - 10 Loyalist)
	public float myMood = 0; // 0 = Neutral (OK)

	public string myName;
	// CLASS or Type of Survivor (Each Class carries unique bonuses and needs)
	public enum SurvivorClass{
		soldier,
		farmer,
		scientist,
		mechanic
	}
	public SurvivorClass myClass;

	public Survivor_Data(string name, Sprite sprite, float mood, SurvivorClass sClass){
		myName = name;
		mySprite = sprite;
		myMood = mood;
		myClass = sClass;
	}


}

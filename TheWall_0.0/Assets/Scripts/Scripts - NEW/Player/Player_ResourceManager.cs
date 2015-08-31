﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Player_ResourceManager : MonoBehaviour {

	public int ore;
	public int food;
	public int credits;
	public int water;

	public int maxCitizenCount = 1;// the maximum citizens will increase with each level, bringing in new characters

	public int totalFoodCost; // this is the cost of food every turn/cycle. It gets added & subtracted by the resource grid whenever it 
													// Swaps tiles.

	public float dayTime; // How long a DAY is. Leaving this public for now but it can be set by the level the player is on
	bool feeding;
	bool starveMode;
	int turnsStarving = 0;

	Building_UIHandler buildingUI;

	public ResourceGrid resourceGrid;

	List <GameObject> buildingsStarved = new List<GameObject>();

	GameObject lastBuildingPicked, currStarvedBuilding;

	// Keep track of all Farms built and how much they produce per day
	public int farmCount{ get; private set; }
	public int foodProducedPerDay { get; private set; }

	// Keep track of all Storage buildings keeping ore and water, each storage adds itself when instantiated
	public List<Storage> storageBuildings = new List<Storage> ();

	// Keep track of all Water plants/pumps
	public int waterPumpCount{ get; private set; }
	public int waterProducedPerDay{ get; private set; }
	public int totalWaterCost;

	// Keep track of all Extractors gathering Ore
	public int extractorCount{ get; private set; }
	public int oreExtractedPerDay { get; private set; }
	
	/// <summary>
	/// Calculates the food production per day.
	/// Each farm takes X time to create Y food. A day takes T time.
	/// Production Rate = T divided by X ( how many times food is produced in a day).
	/// Total food produced per day = Y * Production Rate
	/// </summary>
	/// <param name="foodProduced">Food produced.</param>
	/// <param name="rateOfProd">Rate of prod.</param>
	/// <param name="trueIfSubtracting">Set to true if this Farm is being destroyed.</param>
	public void CalculateFoodProduction(int foodProduced, float rateOfProd, int waterNeeded, bool trueIfSubtracting){
	
		float productionRate = dayTime / rateOfProd;
//		Debug.Log ("Production Rate: " + productionRate);

		int perDay = Mathf.RoundToInt (foodProduced * productionRate);
//		Debug.Log ("PerDay: " + perDay);

		if (!trueIfSubtracting) {
			farmCount++;
			foodProducedPerDay = foodProducedPerDay + perDay;
			// add this farm's water needed stat to keep track of how much water we need
			totalWaterCost = totalWaterCost + waterNeeded;
		} else {
			farmCount--;
			foodProducedPerDay = foodProducedPerDay - perDay;

			totalWaterCost = totalWaterCost - waterNeeded;
		}
		Debug.Log ("Food Produced Per Day = " + foodProducedPerDay + " from " + farmCount + " Farms.");
	}

	public void CalculateWaterProduction(int waterPumped, float rateOfPump, bool trueIfSubtracting){
		float productionRate = dayTime / rateOfPump;
		
		int perDay = Mathf.RoundToInt (waterPumped * productionRate);
		
		if (!trueIfSubtracting) {
			waterPumpCount++;
			waterProducedPerDay = waterProducedPerDay + perDay;
		} else {
			waterPumpCount--;
			waterProducedPerDay = waterProducedPerDay - perDay;
		}
	}

	public void CalculateOreProduction(int oreExtracted, float rateOfExtract, bool trueIfSubtracting){
		float productionRate = dayTime / rateOfExtract;
		
		int perDay = Mathf.RoundToInt (oreExtracted * productionRate);
		
		if (!trueIfSubtracting) {
			extractorCount++;
			oreExtractedPerDay = oreExtractedPerDay + perDay;
		} else {
			extractorCount--;
			oreExtractedPerDay = oreExtractedPerDay - perDay;
		}
	}

	void Start(){
		buildingUI = GetComponentInChildren<Building_UIHandler> ();
	
		feeding = true;
	}
	
	void Update(){
	
		if (totalFoodCost > 0) {
			if (feeding)
				StartCoroutine(WaitToFeed());
		}
	}
	IEnumerator WaitToFeed(){
		feeding = false;
		yield return new WaitForSeconds (dayTime); // feeding ONCE per day
		FeedPopulation ();
	}

	void FeedPopulation(){
		if (food >= totalFoodCost) {

			if (buildingsStarved.Count > 0)
				UnStarveBuildings();

			int calc = food - totalFoodCost;
			Debug.Log ("Feeding population!");
			if (calc > 0){
				food = calc;
				feeding = true;
			}else{
				food = calc;
				feeding = true;
				buildingUI.CreateIndicator("NO FOOD LEFT!");
			}
		} else {
			Debug.Log("Not enough food!");
			feeding = true;

			// WAIT three cycles of the CoRoutine in starvation before turning off a building
				// Give 3 different WARNINGS to the Player informing them of the lack of food
			turnsStarving++;
			if (turnsStarving == 1){
				buildingUI.CreateIndicator("Population is HUNGRY!");
				GetBuildingToStarve();
			}else if (turnsStarving == 2){
				buildingUI.CreateIndicator("Population NEEDS FOOD!");
			}else if (turnsStarving == 3){
				buildingUI.CreateIndicator("Population is STARVING!");
				turnsStarving = 0;		// reset turns Starving ( this causes the manager to wait ANOTHER 3 turns before turning off anymore buildings)
				GetBuildingToStarve();
			}
		}

	}

	void GetBuildingToStarve(){

		foreach (GameObject tile in resourceGrid.spawnedTiles) {
			// check if another building had already been picked
			if (lastBuildingPicked != null){
				if (tile != null){
					if (tile.CompareTag("Building")){
						if (tile != lastBuildingPicked){
							// turn off the building
							currStarvedBuilding = tile;
							StarveBuildingControl(currStarvedBuilding, true);
							break;
						}
					}
				}
			}else{
				if (tile != null){
					if (tile.CompareTag("Building")){
						// turn off the building
						currStarvedBuilding = tile;
						StarveBuildingControl(currStarvedBuilding, true);
						break;
					}
				}
			}
		}
	}

	void UnStarveBuildings(){
		for (int i = 0; i < buildingsStarved.Count; i++) {
			// call starve control with starving as false
			StarveBuildingControl(buildingsStarved[i], false);
			// then remove that object from the list
			buildingsStarved.RemoveAt(i);
		}
	}


	/// <summary>
	/// Starves the building by using the object's name in a switch.
	/// It will make the starvedMode bool false in each component, depending on building type,
	/// unless starving is set to false or building name is not found.
	/// </summary>
	/// <param name="building">Building.</param>
	/// <param name="starving">If set to <c>true</c> turns starving mode true.</param>
	void StarveBuildingControl (GameObject building, bool starving){
		// first starve the building by finding what kind of building it is by name
		string buildingName = building.name;
		// add the current building to the list
		buildingsStarved.Add (building);
		// store it as the last building picked
		lastBuildingPicked = building;

		switch (buildingName) {						//** WARNING! ALL ATTACKING BUILDINGS HAVE THE COMPONENT IN CHILDREN!!!
		case "Extractor":	// extractor
			if (starving){
				building.GetComponent<Extractor>().starvedMode = true;
				buildingUI.CreateIndicator("An " + buildingName + " stopped working.");
			}else{
				building.GetComponent<Extractor>().starvedMode = false;
				buildingUI.CreateIndicator(buildingName + " back online!");
			}
			break;
		case "Machine Gun": // machine gun
			if (starving){
			building.GetComponentInChildren<Tower_TargettingHandler>().starvedMode = true;
			buildingUI.CreateIndicator("A " + buildingName + " stopped working.");
			}else{
				building.GetComponentInChildren<Tower_TargettingHandler>().starvedMode = false;
				buildingUI.CreateIndicator(buildingName + " back online!");
			}
			break;
		case "Cannons": // cannons
			if (starving){
			building.GetComponentInChildren<Tower_AoETargettingHandler>().starvedMode = true;
			buildingUI.CreateIndicator(buildingName + " stopped working.");
			}else{
				building.GetComponentInChildren<Tower_AoETargettingHandler>().starvedMode = false;
				buildingUI.CreateIndicator(buildingName + " back online!");
			}
			break;
		case "Harpooner's Hall": // harpooners hall
			if (starving){
			building.GetComponentInChildren<Barracks_SpawnHandler>().starvedMode = true;
			buildingUI.CreateIndicator("A " + buildingName + " stopped working.");
			}else{
				building.GetComponentInChildren<Barracks_SpawnHandler>().starvedMode = false;
				buildingUI.CreateIndicator(buildingName + " back online!");
			}
			break;
		case "Seaweed Farm": // seaweed farm
			if (starving){
			building.GetComponent<FoodProduction_Manager>().starvedMode = true;
			buildingUI.CreateIndicator("A " + buildingName + " stopped working.");
			}else{
				building.GetComponent<FoodProduction_Manager>().starvedMode = true;
				buildingUI.CreateIndicator(buildingName + " back online!");
			}
			break;
		default:
			Debug.Log("couldn't starve " + buildingName + " building!");
			break;
		}
	}




	/// <summary>
	/// Add or subtract a resource. This is changing the total ammount that is then seen in the UI.
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="quantity">Quantity.</param>
	public void ChangeResource (string id, int quantity){
		Debug.Log ("Changing " + id + " by " + quantity);

		switch (id) {
		case "Ore":
			ore = ore + quantity;

			break;
		case "Food":

			food = food + quantity;

			break;
		case "Credits":

			credits = credits + quantity;

			break;
		case "Water":
			water = water + quantity;

			break;
		default:
			print ("Cant find that resource type!");
			break;
		}
	}

	public bool CheckStorageForResource(string id, int ammnt){
		if (storageBuildings.Count > 0) {
			for (int i =0; i < storageBuildings.Count; i++) {
				if (id == "Ore") {
					if (storageBuildings [i].oreStored >= ammnt)
						return true;
				} else if (id == "Water") {
					if (storageBuildings [i].waterStored >= ammnt)
						return true;
				} else {
					// no other resource but ore and water in storage right now
					return false;
				}
			}

			// if none of the storages have returned true then just return false
			return false;

		} else {

			return false;

		}

	}

	/// <summary>
	/// Charges an ammount of WATER or ORE from the first
	/// storage that contains more than or equal the charge ammount.
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="charge">Charge.</param>
	public void ChargeFromStorage(int charge, string id){
		if (storageBuildings.Count > 0) {
			for (int x=0; x < storageBuildings.Count; x++){
				if (id == "Ore"){
					if (storageBuildings[x].oreStored >= charge){
						storageBuildings[x].ChargeResource(charge, "Ore");
						break;
					}
				}else if (id == "Water"){
					if (storageBuildings[x].waterStored >= charge){
						storageBuildings[x].ChargeResource(charge, "Water");
						break;
					}
				}
			}
		}
	}

	public void ChargeOreorWater(string id, int ammnt){
		// this assumes that I've already checked there is more than ammnt in the total Ore / Water

		// First, check if there are any storage buildings
		if (storageBuildings.Count > 0) {
			// Since there ARE storage buildings let's try charging this ammnt from one of them

			if (CheckStorageForResource(id, -ammnt)){ // here we check if there's a storage with enough of that resource

				ChargeFromStorage(ammnt, id); // this TAKES the ammnt from a storage

			}
		
		} else {
			// if there are NO storage buildings we just charge from the total Ore / Water
			ChangeResource(id, ammnt);

		}
	}


	/// <summary>
	/// Removes the storage building from the list.
	/// </summary>
	/// <param name="storage">Storage.</param>
	public void RemoveStorageBuilding(Storage storage){
		storageBuildings.Remove (storage);
	}

}

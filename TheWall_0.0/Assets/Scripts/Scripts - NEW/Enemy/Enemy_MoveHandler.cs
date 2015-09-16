﻿using UnityEngine;
using System.Collections.Generic;

public class Enemy_MoveHandler : MonoBehaviour {

	[System.Serializable]
	public class MovementStats {
		public float startMoveSpeed;

		private float _moveSpeed;

		public float curMoveSpeed{ get { return _moveSpeed; } set { _moveSpeed = Mathf.Clamp(value, 0, startMoveSpeed); } }

		public void InitMoveStats(){
			curMoveSpeed = startMoveSpeed;
		}
	}

	public MovementStats mStats = new MovementStats();

	public ResourceGrid resourceGrid;
	
	public int posX, targetPosX;
	public int posY, targetPosY;
	
	// This stores this unit's path
	public List<Node>currentPath = null;
	
	private Vector3 velocity = Vector3.zero;

	public bool isAttacking = false; // this turns true when this object enters a player unit's collider OR a blocking tile

	public Enemy_AttackHandler enemyAttkHandler;

//	public float movementSpeed = 1.0F;

	public bool moving;

	public int spwnPtIndex;
	public SpawnPoint_Handler spwnPtHandler;

	public float formationOffset;

	public float stoppingDistance;
	CircleCollider2D collider;
	public Vector3 destination;


	public bool movingBackToPath = false, movingToFormation = false;
	Vector2 formationPos;

	// THE BUDDY SYSTEM: each individual spawned enemy will know the Move Handler of the enemy before them (given to them by the SpawnHandler)
	public Enemy_MoveHandler myBuddy;

	public Animator anim;

	private Vector2 _capitalPosition;

	public bool isKamikaze;

	public bool unitInitialized { get; private set;} 

	public enum State { IDLING, MOVING, MOVING_BACK, AVOIDING, ATTACKING };

	private State _state = State.IDLING;

	[HideInInspector]
	public State state { get { return _state; } set { _state = value; }}

	private Vector2 lastKnownNode, disperseDirection = Vector2.zero;

	[SerializeField]
	private bool isAvoider; // if unit is an Avoider, they won't attack tiles, just go around them
	
	void Start () 
	{

		// Initialize Movement Speed stat
		mStats.InitMoveStats ();

		// Get the Animator
		if (anim == null) {
			anim = GetComponentInChildren<Animator>();
		}

		// Store initial position for Grid as an int
		posX = (int)transform.position.x;
		posY = (int)transform.position.y;

		// Store the Attack Handler to interact with its state
		enemyAttkHandler = GetComponent<Enemy_AttackHandler> ();

		// Get First Path, pass in True argument only if this is a Kamikaze unit
		if (!isKamikaze) {

			GetFirstPath (false);

		} else {

			GetFirstPath(true);
		}

		// Store the Capital position from the resource grid
		if (resourceGrid != null)
			_capitalPosition = new Vector2 (resourceGrid.capitalSpawnX, resourceGrid.capitalSpawnY);

		// This unit has been initialized (meaning it's already been spawned once)
		// In order to know which units already spawned from pool and need to reset stats
		unitInitialized = true;
	}


	// Called by scripts spawning this unit to make sure it gets the right path
	public void InitPath()
	{
		if (!isKamikaze) {
			GetFirstPath (false);
		} else {
			GetFirstPath(true);
		}
	}


	void GetFirstPath(bool isKamikaze)
	{
		// Just in case this unit had a path already (it's a recycled unit) make it null
		if (currentPath != null)
			currentPath.Clear ();

		// Get the path from Spawn Point Handler
		if (spwnPtHandler != null) {

			// Regular Units path:
			if (!isKamikaze){

				// Initialize Current Path
				currentPath = new List<Node>();

				// Loop through each Node of the corresponding path and Add Node to Current Path
				for (int x = 0; x < spwnPtHandler.path[spwnPtIndex].Count; x++){
					currentPath.Add(spwnPtHandler.path[spwnPtIndex][x]);

					// When the Loop reaches the last Node store the value as a Vector3 destination
					if (x == spwnPtHandler.path[spwnPtIndex].Count - 1){

						destination = new Vector3( currentPath[x].x, currentPath[x].y, 0.0f);
					}
				}

				// Now that all Nodes in Current Path have been set, start moving
				moving = true;
				_state = State.MOVING;

			}else{// Kamikaze Path:

				// Init Current Path
				currentPath = new List<Node>();

				// Loop through each Node in Kamikaze path and add Nodes to Current Path
				for (int x = 0; x < spwnPtHandler.kamikazePath[spwnPtIndex].Count; x++){
					currentPath.Add(spwnPtHandler.kamikazePath[spwnPtIndex][x]);

					// As above, last Node is stored in Vector3 destination
					if (x == spwnPtHandler.kamikazePath[spwnPtIndex].Count - 1){
						destination = new Vector3( currentPath[x].x, currentPath[x].y, 0.0f);
					}
				}

				// All Nodes in Current Path are set, start moving
//				moving = true;
				// Change state:
				_state = State.MOVING;
			}
		
		}
	}

	Vector2 Disperse(Vector2 obstaclePos)
	{
		Vector2 disperseDirection = Vector2.zero;

		if (obstaclePos.y == transform.position.y) {
		
			// Move to tile above or below it
			int decision = Random.Range(0, 3);

			if (decision == 1){
				// move above
				disperseDirection = new Vector2 (obstaclePos.x, obstaclePos.y + 1);
			}else{
				// move below
				disperseDirection = new Vector2 (obstaclePos.x, obstaclePos.y - 1);
			}
		} else if (obstaclePos.x == transform.position.x) {
			// Move to tile above or below it
			int decision2 = Random.Range(0, 3);

			if (decision2 == 1){
				// move left
				disperseDirection = new Vector2 (obstaclePos.x + 1, obstaclePos.y);
			}else{
				// move right
				disperseDirection = new Vector2 (obstaclePos.x - 1, obstaclePos.y);
			}
		}

		return disperseDirection;
	}

//	Vector2 Avoid(Vector2 lastNode)
//	{
//		Vector2 avoidDirection = Vector2.zero;
//		
//		if (lastNode.y == transform.position.y) {
//
//			// is it left or right 
//			if (lastNode.x < transform.position.x){
//				// im moving left
//			}else{
//				// im moving right
//			}
//			avoidDirection = new Vector2 (obstaclePos.x, obstaclePos.y - 1);
//		
//		} else if (lastNode.x == transform.position.x) {
//			// Move to tile above or below it
//			int decision2 = Random.Range(0, 3);
//			
//			if (decision2 == 1){
//				// move left
//				avoidDirection = new Vector2 (obstaclePos.x + 1, obstaclePos.y);
//			}else{
//				// move right
//				avoidDirection = new Vector2 (obstaclePos.x - 1, obstaclePos.y);
//			}
//		}
//		
//		return avoidDirection;
//	}

	// Move Back to Path after dispersing
	void MoveBackToPath(Vector2 _nodePos)
	{
		// Alter the speed slightly as they move back to keep them from bunching up
		float randomSpeed = Random.Range (0.1f, mStats.startMoveSpeed);
		mStats.curMoveSpeed = randomSpeed;

		if (Vector2.Distance (transform.position, _nodePos) == 0) {
			GetPath ();
		} else {
			
			transform.position = Vector2.MoveTowards(transform.position, _nodePos ,mStats.curMoveSpeed * Time.deltaTime);
		}
	}

	// To find path after moving away from it
	public void GetPath()
	{
		posX = (int)transform.position.x;
		posY = (int)transform.position.y;

		int myPosIndexInPath = 0;

		if (spwnPtHandler != null) {

			currentPath = new List<Node>();
			for (int x = 0; x < spwnPtHandler.path[spwnPtIndex].Count; x++){
				// find my position on the path
				if(spwnPtHandler.path[spwnPtIndex][x].x == posX && spwnPtHandler.path[spwnPtIndex][x].y == posY){
					Debug.Log("Found my node at pathtoCapital" + x);
					myPosIndexInPath = x;
					break;
				}
			}
			if (myPosIndexInPath > 0){ // At this point Index should never be 0 if we found our position in the path
				for (int i = myPosIndexInPath; i < spwnPtHandler.path[spwnPtIndex].Count; i++){
					currentPath.Add(spwnPtHandler.path[spwnPtIndex][i]);
				}

				// Change speed back to original speed
				mStats.curMoveSpeed = mStats.startMoveSpeed;

				// Start moving on path
				_state = State.MOVING;



			}else{
				_state = State.IDLING;
				Debug.Log("Couldn't find my node :(");
			}


		}
	}
	


	void Update () {

		DrawLine ();
	
		MyStateMachine (_state);
	

		// NOTE: Turning ON the buddy system forces the entire wave to stop when the "leader" is blocked by a building. This makes
		// it look quite static and robotic. The other alternative is NOT activating it, which causes all the units to move up to
		// the building. This makes them all pile together and when they destroy the building they move in a jumbled mess.

		// NOTE: Disperse works! Now they pile together when they go back to their path though
		// TODO: Consider altering their speed slightly while they are moving back to path, that should cause them to NOT look like a mess

//		if (myBuddy != null) {
//			// NOTE: Buddy System is only called by units who were not the first to spawn in their Wave
//			// Buddy System links units of the same wave to allow them to attack a Tile or Unit in unison
//			BuddySystem();
//		}
	}

	void MyStateMachine(State _curState)
	{
		switch (_curState) {
		case State.IDLING:
			// just spawned, not moving & not attacking
			break;
		case State.MOVING:
			// Move
			ActualMove();
			break;
		case State.ATTACKING:
			// Attack
			// While they are attacking, units disperse around the tile under attack
			if (disperseDirection != Vector2.zero)
				transform.position = Vector2.MoveTowards(transform.position, disperseDirection, mStats.curMoveSpeed * Time.deltaTime);

			break;
		case State.MOVING_BACK:
			// Move Back to Path, once found this will call Moving state again
			MoveBackToPath(lastKnownNode);
			break;

		case State.AVOIDING:
			// Disperse, then once on the disperse tile Move Back to Path
			if (disperseDirection != Vector2.zero){
				if (Vector2.Distance(transform.position, disperseDirection) > 0){
					transform.position = Vector2.MoveTowards(transform.position, disperseDirection, mStats.curMoveSpeed * Time.deltaTime);
				}else{
					_state = State.MOVING_BACK;
				}
			}
			break;
		default:
			state = State.IDLING;
			break;
		}

	}

	// DEBUG NOTE: Using this to draw line to show path. Only works in Preview mode
	void DrawLine()
	{
		// Debug Draw line for visual reference
		if (currentPath != null) {
			int currNode = 0;
			while (currNode < currentPath.Count -1) {
				Vector3 start = resourceGrid.TileCoordToWorldCoord (currentPath [currNode].x, currentPath [currNode].y);
				Vector3 end = resourceGrid.TileCoordToWorldCoord (currentPath [currNode + 1].x, currentPath [currNode + 1].y);
				;
				Debug.DrawLine (start, end, Color.blue);
				currNode++;
			}
		} 
	}

	// Physically moves the unit through world space
	void ActualMove()
	{
		// Movement:

		// Have we moved close enough to the target tile that we can move to next tile in current path?
		if (Vector2.Distance (transform.position, resourceGrid.TileCoordToWorldCoord (posX, posY)) < (0.1f)) {
			MoveToNextTile ();
		}
		transform.position = Vector2.MoveTowards(transform.position, 
		                                         resourceGrid.TileCoordToWorldCoord (posX, posY), 
		                                         mStats.curMoveSpeed * Time.deltaTime);
		// ANIMATION CONTROLS:
		if (posX > transform.position.x){
			anim.SetTrigger ("movingRight");
			anim.ResetTrigger("movingLeft");
		}else if (posX < transform.position.x){
			anim.SetTrigger ("movingLeft");
			anim.ResetTrigger("movingRight");
		}
		
	}

	// Move through Path:
	public void MoveToNextTile(){
		if (currentPath == null) {
			return;
		}
		// Remove the old first node from the path
		currentPath.RemoveAt (0);
		
		// Check if the next tile is a UNWAKABLE tile OR if it is clear path
		if (resourceGrid.UnitCanEnterTile (currentPath [1].x, currentPath [1].y) == false) {

			// Since Path is blocked set the state to Idling until this unit knows if it must attack
			_state = State.IDLING;

			if (CheckForTileAttack (currentPath [1].x, currentPath [1].y)) {

				// Start its attack on the tile:

				// If it's the Destination tile, not just any tile, then it needs to do a special attack
				if (currentPath[1].x == destination.x && currentPath[1].y == destination.y)
				{
					// we are at the destination! Do special!
					enemyAttkHandler.SpecialAttack(currentPath[1].x, currentPath[1].y);
				}
				else
				{
					// Check if this unit is NOT an Avoider 
					if (!isAvoider){

						// the Target Tile is not our destination, so do Normal Attack
						targetPosX = currentPath [1].x;
						targetPosY = currentPath [1].y;
						enemyAttkHandler.targetTilePosX = currentPath [1].x;
						enemyAttkHandler.targetTilePosY = currentPath [1].y;
						enemyAttkHandler.resourceGrid = resourceGrid;

						// Change attack handler state to Attacking Tile
						enemyAttkHandler.state = Enemy_AttackHandler.State.ATTACK_TILE;

						// Change my state to Attack to stop movement
						_state = State.ATTACKING;

						// Record the Node location that contains the obstacle
						lastKnownNode = new Vector2(currentPath[1].x, currentPath[1].y);

						// Record the direction Units must use to disperse
						disperseDirection = Disperse(lastKnownNode);
					}else{
						// This IS an Avoider, instead of attacking they move around the tile

						// Record the Node location that contains the obstacle
						lastKnownNode = new Vector2(currentPath[1].x, currentPath[1].y);
						
						// Record the direction Units must use to disperse
						disperseDirection = Disperse(lastKnownNode);

						// Change state to Avoiding
						_state = State.AVOIDING;

					}

				}
			
			} 

		} else {
			if (_state == State.ATTACKING){ // at this point if this is true it means this unit is engaging a Player Unit
				currentPath = null;
				return;
			}

			// this check if for KAMIKAZE UNITS ONLY
			if (isKamikaze){
				// if the next tile on the Path is the destination
				if (currentPath[1].x == destination.x && currentPath[1].y == destination.y){
					// reached the destination, do Special Attack!
					enemyAttkHandler.SpecialAttack(currentPath[1].x, currentPath[1].y);
				}
			}
		} 	
		// Move to the next Node position in path
		posX = currentPath [1].x;
		posY = currentPath [1].y;

		// We are on the tile that is our DESTINATION, 
		// CLEAR PATH
		if (currentPath.Count == 1) {
			currentPath = null;
		}
	}

	bool CheckForTileAttack(int x, int y){
		if (resourceGrid.tiles [x, y].tileType != TileData.Types.empty && resourceGrid.tiles [x, y].tileType != TileData.Types.rock) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Checks if MY BUDDY has:
	/// -Reached the destination OR
	/// - is Attacking
	/// </summary>
	void BuddySystem(){
//		if (!destinationReached) {
//			if (myBuddy.destinationReached == true){
//				destinationReached = true;
////				moving = false;
////				Debug.Log ("stopping.");
//				//TODO: Instead of stopping here, when they reach their destination, I want them to surround the building
//			}
//		}
		if (_state != State.ATTACKING) {
			if (myBuddy.state == State.ATTACKING){
				if (CheckForTileAttack(myBuddy.targetPosX, myBuddy.targetPosY)){

					// We found a tile to attack, change state to attacking to stop movement
					_state = State.ATTACKING;

//					isAttacking = true;
//					moving = false;

					targetPosX = myBuddy.targetPosX;
					targetPosY = myBuddy.targetPosY;
					enemyAttkHandler.targetTilePosX = myBuddy.targetPosX;
					enemyAttkHandler.targetTilePosY = myBuddy.targetPosY;
					enemyAttkHandler.resourceGrid = resourceGrid;

					// Change attack handler state to Attacking Tile
					enemyAttkHandler.state = Enemy_AttackHandler.State.ATTACK_TILE;
					Debug.Log ("Also attacking tile!");
				}
			}
		}
	}
}

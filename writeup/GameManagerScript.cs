using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 *	GameManagerScript.cs
 *	Author: Daniel Hannon (19484286)
 *	Version: 1
 */
public class GameManagerScript : MonoBehaviour {
	// Game Control Variables
	public Camera mainCamera;
	public bool InDebugMode;
	private bool attackMenuVisible = false;
	private bool reinforceMenuVisible = false;
	private bool mainMenuVisible = true;
	public Text statusBar;
	public Button EndTurnButton;

	//Selectors
	public GameObject selected;
	public GameObject selected2;

	//Menus
	public GameObject atkmenu;
	public GameObject renMenu;
	public GameObject statusMenu;
	public GameObject mainMenu;

	//Main Menu Buttons
	public Text MenuText;
	public Button twoPlayerButton;
	public Button threePlayerButton;
	public Button fourPlayerButton;
	public Button fivePlayerButton;
	public Button sixPlayerButton;
	public Button quitGame;

	//Attack Menu Specific Variables
	public Text attacker;
	public Text defender;
	public Text attackerTextCount;
	public Text defenderTextCount;
	public GameObject[] AttackerDie;
	public GameObject[] DefenderDie;
	public Button AttackButton;
	public Button DoOrDieButton;
	public Button CancelButton;

	//Reinforce Menu Specific Variables
	public Text ReinforceFrom;
	public Text ReinforceTo;
	public Text ReinforceFromCount;
	public Text ReinforceToCount;
	private int FromTerritoryTemp = 0;
	private int ToTerritoryTemp = 0;
	public Button reinforceCancel;
	public Button reinforceConfirm;
	public Button reinforceAdd1;
	public Button reinforceDec1;
	public Button reinforceAdd5;
	public Button reinforceDec5;
	public Button reinforceAddAB1;
	public Button reinforceDecAB1;

	//Turn Specific stuff
	enum Stages { NOT_STARTED, GAME_SETUP, REPLENISH, ATTACK, REINFORCE };
	Stages currStage = Stages.NOT_STARTED;
	int currentPlayer = 0;
	int numberOfActivePlayers = 2;
	int numberOfTroopsToPlace = 0;
	public List<string> playerColors;
	List<Province>[] TeamTerritories = new List<Province>[6];
	public bool reinforcementPerformedThisTurn = false;


	// Essential Game Variables
	// I initalize everything at runtime so I don't need to constantly allocate/deallocate memory
	public List<Province> ClosedList = new List<Province>();
	public List<Province> OpenList = new List<Province>();
	public List<string> TeamColors = new List<string>() { "neutral", "red", "blue", "green", "yellow", "pink", "brown"};
	public List<Province> AllProvinces = new List<Province>();
	public List<Province> Europe = new List<Province>();
	public List<Province> Africa = new List<Province>();
	public List<Province> Asia = new List<Province>();
	public List<Province> Oceania = new List<Province>();
	public List<Province> SouthAmerica = new List<Province>();
	public List<Province> NorthAmerica = new List<Province>();

	void Start() {
		mainCamera.aspect = 23f / 9f;
		//Attack Menu Setup
		atkmenu.SetActive(false);
		AttackButton.GetComponent<Button>().onClick.AddListener(PerformAttack);
		CancelButton.GetComponent<Button>().onClick.AddListener(CancelButtonListener);
		DoOrDieButton.GetComponent<Button>().onClick.AddListener(DoOrDieListener);

		//Reinforce Menu Setup
		renMenu.SetActive(false);
		reinforceCancel.GetComponent<Button>().onClick.AddListener(CancelReinforce);
		reinforceAdd1.GetComponent<Button>().onClick.AddListener(ReinforceIncrement1);
		reinforceAdd5.GetComponent<Button>().onClick.AddListener(ReinforceIncrement5);
		reinforceAddAB1.GetComponent<Button>().onClick.AddListener(ReinforceIncrementAB1);
		reinforceDec1.GetComponent<Button>().onClick.AddListener(ReinforceDecrement1);
		reinforceDec5.GetComponent<Button>().onClick.AddListener(ReinforceDecrement5);
		reinforceDecAB1.GetComponent<Button>().onClick.AddListener(ReinforceDecrementAB1);
		reinforceConfirm.GetComponent<Button>().onClick.AddListener(ReinforceConfirm);

		EndTurnButton.GetComponent<Button>().onClick.AddListener(NewTurn);
		statusMenu.SetActive(false);

		twoPlayerButton.GetComponent<Button>().onClick.AddListener(TwoPlayer);
		threePlayerButton.GetComponent<Button>().onClick.AddListener(ThreePlayers);
		fourPlayerButton.GetComponent<Button>().onClick.AddListener(FourPlayers);
		fivePlayerButton.GetComponent<Button>().onClick.AddListener(FivePlayers);
		sixPlayerButton.GetComponent<Button>().onClick.AddListener(SixPlayers);
		quitGame.GetComponent<Button>().onClick.AddListener(QuitGame);

		InDebugMode = false;
	}

	// Attack Mechanism Listeners

	void AttackSetup() {
		if (CheckAdjacency(selected.GetComponent<Province>(), selected2.GetComponent<Province>()) && !selected.GetComponent<Province>().Color.Equals(selected2.GetComponent<Province>().Color)) {
			attackMenuVisible = !attackMenuVisible;
			atkmenu.SetActive(attackMenuVisible);
			attacker.text = selected.GetComponent<Province>().ProvinceName;
			defender.text = selected2.GetComponent<Province>().ProvinceName;
			attackerTextCount.text = selected.GetComponent<Province>().TroopCount.ToString();
			defenderTextCount.text = selected2.GetComponent<Province>().TroopCount.ToString();
		}
	}

	void CancelButtonListener() {
		for(int i = 0; i < 3; i++) {
			AttackerDie[i].GetComponent<DiceRoller>().SetInactive();
		}

		for(int j = 0; j < 2; j++) {
			DefenderDie[j].GetComponent<DiceRoller>().SetInactive();

		}
		attackMenuVisible = false;
		atkmenu.SetActive(false);
	}

	void DoOrDieListener() {
		//Calls Perform Attack until either you conquer the territory, or run out of troops, whatever comes first.
		while(attackMenuVisible) {
			PerformAttack();
		}
	}

	void PerformAttack() {
		//Basic Attack Code lol
		int AttackerDieCount = 0;
		int DefenderDieCount = 0;
		List<int> AttackerDiceRolls = new List<int>();
		List<int> DefenderDiceRolls = new List<int>();
		//Get Number of Attacker Die
		switch(selected.GetComponent<Province>().TroopCount) {
			case 1:
				CancelButtonListener();
				break;
			case 2:
				//One Die
				AttackerDieCount = 1;
				break;
			case 3:
				//Two Die
				AttackerDieCount = 2;
				break;
			default:
				//Three Die
				AttackerDieCount = 3;
				break;
		}

		//Get Number of Defender Die
		switch(selected2.GetComponent<Province>().TroopCount) {
			case 0:
				string temp = selected2.GetComponent<Province>().Color;
				selected2.GetComponent<Province>().Color = selected.GetComponent<Province>().Color;
				selected2.GetComponent<Province>().TroopCount = 1;
				selected.GetComponent<Province>().TroopCount--;
				selected2.GetComponent<Province>().Flag.GetComponent<Animator>().SetInteger("team", TeamColors.IndexOf(selected2.GetComponent<Province>().Color));
				TeamTerritories[playerColors.IndexOf(selected.GetComponent<Province>().Color)].Add(selected2.GetComponent<Province>());
				TeamTerritories[playerColors.IndexOf(temp)].Remove(selected2.GetComponent<Province>());
				CancelButtonListener();
				if(selected.GetComponent<Province>().TroopCount > 1) {
					ReinforceSetup();
				}
				break;
			case 1:
				DefenderDieCount = 1;
				break;
			default:
				DefenderDieCount = 2;
				break;
		}

		for(int i = AttackerDieCount; i < 3; i++) {
			AttackerDie[i].GetComponent<DiceRoller>().SetInactive();
		}

		for (int i = DefenderDieCount; i < 2; i++) {
			DefenderDie[i].GetComponent<DiceRoller>().SetInactive();
		}

		for(int i = 0; i < AttackerDieCount; i++) {
			AttackerDiceRolls.Add(AttackerDie[i].GetComponent<DiceRoller>().Roll());
		}

		for(int i = 0; i < DefenderDieCount; i++) {
			DefenderDiceRolls.Add(DefenderDie[i].GetComponent<DiceRoller>().Roll());
		}

		AttackerDiceRolls.Sort();
		AttackerDiceRolls.Reverse();
		DefenderDiceRolls.Sort();
		DefenderDiceRolls.Reverse();

		while(!(AttackerDiceRolls.Count == 0) && !(DefenderDiceRolls.Count == 0)) {
			if(DefenderDiceRolls[0] >= AttackerDiceRolls[0]) {
				selected.GetComponent<Province>().TroopCount--;
				attackerTextCount.text = selected.GetComponent<Province>().TroopCount.ToString();
			} else {
				selected2.GetComponent<Province>().TroopCount--;
				defenderTextCount.text = selected2.GetComponent<Province>().TroopCount.ToString();
			}
			AttackerDiceRolls.RemoveAt(0);
			DefenderDiceRolls.RemoveAt(0);
		}
	}

	// Troop Transfer/ Reinforce Mechanism

	public void ReinforceSetup() {
		if (CheckForPath(selected.GetComponent<Province>(), selected2.GetComponent<Province>()) && selected2.GetComponent<Province>().Color.Equals(selected.GetComponent<Province>().Color)) {
			reinforcementPerformedThisTurn = true;
			reinforceMenuVisible = true;
			renMenu.SetActive(true);
			ReinforceFrom.text = selected.GetComponent<Province>().ProvinceName;
			ReinforceTo.text = selected2.GetComponent<Province>().ProvinceName;
			FromTerritoryTemp = selected.GetComponent<Province>().TroopCount - 1;
			ToTerritoryTemp = 0;
			ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
			ReinforceToCount.text = selected2.GetComponent<Province>().TroopCount.ToString();
		}
	}

	public void ReinforceIncrement1() {
		if (FromTerritoryTemp >= 1) {
			FromTerritoryTemp -= 1;
			ToTerritoryTemp++;
			ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
			ReinforceToCount.text = (selected2.GetComponent<Province>().TroopCount + ToTerritoryTemp).ToString();
		}
	}

	public void ReinforceDecrement1() {
		if (ToTerritoryTemp >= 1) {
			ToTerritoryTemp -= 1;
			FromTerritoryTemp++;
			ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
			ReinforceToCount.text = (selected2.GetComponent<Province>().TroopCount + ToTerritoryTemp).ToString();
		}
	}

	public void ReinforceIncrement5() {
		if (FromTerritoryTemp >= 1) {
			if (FromTerritoryTemp >= 5) {
				FromTerritoryTemp -= 5;
				ToTerritoryTemp += 5;

			} else {
				ToTerritoryTemp += FromTerritoryTemp;
				FromTerritoryTemp = 0;
			}
			ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
			ReinforceToCount.text = (selected2.GetComponent<Province>().TroopCount + ToTerritoryTemp).ToString();
		}
	}

	public void ReinforceDecrement5() {
		if (ToTerritoryTemp >= 1) {
			if (ToTerritoryTemp >= 5) {
				ToTerritoryTemp -= 5;
				FromTerritoryTemp += 5;

			}
			else {
				FromTerritoryTemp += ToTerritoryTemp;
				ToTerritoryTemp = 0;
			}
			ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
			ReinforceToCount.text = (selected2.GetComponent<Province>().TroopCount + ToTerritoryTemp).ToString();
		}
	}

	public void ReinforceIncrementAB1() {
		ToTerritoryTemp += FromTerritoryTemp;
		FromTerritoryTemp = 0;
		ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
		ReinforceToCount.text = (selected2.GetComponent<Province>().TroopCount + ToTerritoryTemp).ToString();
	}

	public void ReinforceDecrementAB1() {
		FromTerritoryTemp += ToTerritoryTemp;
		ToTerritoryTemp = 0;
		ReinforceFromCount.text = (selected.GetComponent<Province>().TroopCount - ToTerritoryTemp).ToString();
		ReinforceToCount.text = (selected2.GetComponent<Province>().TroopCount + ToTerritoryTemp).ToString();
	}

	public void CancelReinforce() {
		//Close without making change
		reinforceMenuVisible = false;
		renMenu.SetActive(false);
	}

	public void ReinforceConfirm() {
		selected.GetComponent<Province>().TroopCount -= ToTerritoryTemp;
		selected2.GetComponent<Province>().TroopCount += ToTerritoryTemp;

		FromTerritoryTemp = 0;
		ToTerritoryTemp = 0;
		reinforceMenuVisible = false;
		renMenu.SetActive(false);
	}

	//Main Menu
	public void TwoPlayer() {
		numberOfActivePlayers = 2;
		mainMenu.SetActive(false);
		mainMenuVisible = false;
		statusMenu.SetActive(true);
		GameSetup();
	}

	public void ThreePlayers() {
		numberOfActivePlayers = 3;
		mainMenu.SetActive(false);
		mainMenuVisible = false;
		statusMenu.SetActive(true);
		GameSetup();
	}

	public void FourPlayers() {
		numberOfActivePlayers = 4;
		mainMenu.SetActive(false);
		mainMenuVisible = false;
		statusMenu.SetActive(true);
		GameSetup();
	}

	public void FivePlayers() {
		numberOfActivePlayers = 5;
		mainMenu.SetActive(false);
		mainMenuVisible = false;
		statusMenu.SetActive(true);
		GameSetup();
	}

	public void SixPlayers() {
		numberOfActivePlayers = 6;
		mainMenu.SetActive(false);
		mainMenuVisible = false;
		statusMenu.SetActive(true);
		GameSetup();
	}

	public void QuitGame() {
		Application.Quit();
	}

	//General Game Mechanics

	public GameObject GetTerritoryClick() {
		Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
		if(hit) {
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Provinces")) {
				return hit.collider.gameObject;
			}
		}
		return null;
	}

	public bool CheckAdjacency(Province p1, Province p2) {
		//this is for attacking
		for(int i = 0; i < p1.neighbors.Length; i++) {
			if (p1.neighbors[i].GetComponent<Province>().Equals(p2)) {
				return true;
			}
		}
		return false;
	}

	public bool CheckForPath(Province p1, Province p2) {
		OpenList.Add(p1);
		return CheckForPath(p2);
	}

	public bool CheckForPath(Province p2) {
		//this is for reinforcement
		Province temp;
		Province temp2;
		while(OpenList.Count != 0) {
			temp = OpenList[0];
			OpenList.Remove(temp);
			ClosedList.Add(temp);
			for (int i = 0; i < temp.neighbors.Length; i++) {
				temp2 = temp.neighbors[i].GetComponent<Province>();
				if(temp2.Color.Equals(temp.Color)) {
					if (temp2.Equals(p2)) {
						OpenList.Clear();
						ClosedList.Clear();
						return true;
					} else if (ClosedList.Contains(temp2) != true) {
						OpenList.Add(temp2);
					}
				}
			}
		}
		OpenList.Clear();
		ClosedList.Clear();
		return false;
	}

	public void DebugEval() {
		if (Input.GetKeyDown(KeyCode.E)) {
			GameSetup();
		}


		if (Input.GetMouseButtonDown(0) && !attackMenuVisible && !reinforceMenuVisible) {
			GameObject temp = GetTerritoryClick();
			if (temp != null) {
				if (selected != null) {
					if (temp != selected) {
						selected.GetComponent<Province>().Deselect();
					}
				}
				if (temp != selected) {
					temp.GetComponent<Province>().Select();
				}
				selected = temp;
				selected.GetComponent<Province>().TroopCount += 1;
			}
		}

		if (Input.GetMouseButtonDown(1) && !attackMenuVisible) {
			selected2 = GetTerritoryClick();
		}

		if (selected != null) {
			/*
			 * Neutral  0
			 * Red      1
			 * Blue     2
			 * Green    3
			 * Yellow   4
			 * Pink     5
			 * Brown    6
			 */
			//Flag Color Handling 
			GameObject temp = selected.GetComponent<Province>().Flag;
			if (temp != null) {
				if (Input.GetKeyDown(KeyCode.Alpha1)) {
					selected.GetComponent<Province>().Color = "red";
					temp.GetComponent<Animator>().SetInteger("team", 1);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha2)) {
					selected.GetComponent<Province>().Color = "blue";
					temp.GetComponent<Animator>().SetInteger("team", 2);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha3)) {
					selected.GetComponent<Province>().Color = "green";
					temp.GetComponent<Animator>().SetInteger("team", 3);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha4)) {
					selected.GetComponent<Province>().Color = "yellow";
					temp.GetComponent<Animator>().SetInteger("team", 4);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha5)) {
					selected.GetComponent<Province>().Color = "pink";
					temp.GetComponent<Animator>().SetInteger("team", 5);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha6)) {
					selected.GetComponent<Province>().Color = "brown";
					temp.GetComponent<Animator>().SetInteger("team", 6);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha0)) {
					selected.GetComponent<Province>().Color = "neutral";
					temp.GetComponent<Animator>().SetInteger("team", 0);
				}
			}

			if (selected2 != null) {
				//Adjacency Checker
				if (Input.GetKeyDown(KeyCode.A)) {
					if (CheckAdjacency(selected.GetComponent<Province>(), selected2.GetComponent<Province>())) {
						Debug.Log(selected.GetComponent<Province>().ProvinceName + " and " + selected2.GetComponent<Province>().ProvinceName + " Are Adjacent!");
					}
					else {
						Debug.Log(selected.GetComponent<Province>().ProvinceName + " and " + selected2.GetComponent<Province>().ProvinceName + " Are Not Adjacent!");
					}
				}

				if (Input.GetKeyDown(KeyCode.D)) {
					//Attack test
					AttackSetup();
				}

				//Check for Path (for troop reinforcements)
				if (Input.GetKeyDown(KeyCode.S)) {
					OpenList.Add(selected.GetComponent<Province>());
					if (CheckForPath(selected2.GetComponent<Province>())) {
						Debug.Log("A Path From " + selected.GetComponent<Province>() + " to " + selected2.GetComponent<Province>() + " Exists!");
					}
					else {
						Debug.Log("A Path Does Not Exist!");
					}
				}

				if (Input.GetKeyDown(KeyCode.R)) {
					ReinforceSetup();
				}
			}
		}
	}

	public void GameSetup() {
		List<Province> provincesCopy = new List<Province>(AllProvinces);
		for(int i = 0; i < 6; i++) {
			TeamTerritories[i] = new List<Province>();
		}
		int numOfTeams = numberOfActivePlayers;
		if(numOfTeams == 2) {
			numOfTeams = 3;
		}
		while(provincesCopy.Count != 0) {
			int temp = Random.Range(0, provincesCopy.Count);
			provincesCopy[temp].Color = playerColors[currentPlayer];
			provincesCopy[temp].Flag.GetComponent<Animator>().SetInteger("team", TeamColors.IndexOf(playerColors[currentPlayer]));
			TeamTerritories[currentPlayer].Add(provincesCopy[temp]);
			provincesCopy[temp].TroopCount = 1;
			provincesCopy.RemoveAt(temp);
			if(currentPlayer == (numOfTeams - 1)) {
				currentPlayer = 0;
			} else {
				currentPlayer++;
			}
		}

		switch(numberOfActivePlayers) {
			case 2:
				numberOfTroopsToPlace = 26;
				break;
			case 3:
				numberOfTroopsToPlace = 21;
				break;
			case 4:
				numberOfTroopsToPlace = 19;
				break;
			case 5:
				numberOfTroopsToPlace = 16;
				break;
			case 6:
				numberOfTroopsToPlace = 13;
				break;
		}
		currStage = Stages.GAME_SETUP;
		currentPlayer = 0;
	}

	public void NewTurn() {
		if(selected) {
			selected.GetComponent<Province>().Deselect();
		}
		if(TeamTerritories[currentPlayer].Count == 42) {
			MenuText.text = playerColors[currentPlayer].ToUpper() + " Won the Game!";
			mainMenuVisible = true;
			mainMenu.SetActive(true);
			statusMenu.SetActive(false);
		}
		else if (!attackMenuVisible && !reinforceMenuVisible && (currStage > Stages.REPLENISH)) {
			if (currentPlayer == (numberOfActivePlayers - 1)) {
				currentPlayer = 0;
			}
			else {
				currentPlayer++;

			}
			while(TeamTerritories[currentPlayer].Count == 0) {
				if(currentPlayer == (numberOfActivePlayers - 1)) {
					currentPlayer = 0;
				} else {
					currentPlayer++;
				}
			}
			numberOfTroopsToPlace = calculateReinforcements();
			currStage = Stages.REPLENISH;
		}
	}

	public bool FindInList(List<Province> subList, List<Province> superList) {
		foreach (Province province in subList) {
			if(!superList.Contains(province)) {
				return false;
			}
		}
		return true;
	}

	int calculateReinforcements() {
		int territoryOwnershipCount = Mathf.Max(TeamTerritories[currentPlayer].Count / 3,3);
		int continentBonus = 0;
		//Check Continental Bonuses
		if(FindInList(Europe, TeamTerritories[currentPlayer])) {
			continentBonus += 5;
		}
		if(FindInList(Asia, TeamTerritories[currentPlayer])) {
			continentBonus += 7;
		}
		if(FindInList(Africa, TeamTerritories[currentPlayer])) {
			continentBonus += 3;
		}
		if(FindInList(Oceania, TeamTerritories[currentPlayer])) {
			continentBonus += 2;
		}
		if(FindInList(SouthAmerica,TeamTerritories[currentPlayer])) {
			continentBonus += 2;
		}
		if(FindInList(NorthAmerica,TeamTerritories[currentPlayer])) {
			continentBonus += 5;
		}
		return territoryOwnershipCount + continentBonus;
	}


	// Update is called once per frame
	void Update() {
		/* Basically I want to keep debug mode (in case of emergency) so it's activated/deactivated using ` (The same key to open the command menu in most modern games) */
		if(Input.GetKeyDown(KeyCode.BackQuote)) {
			//Toggle Debug mode
			InDebugMode = !InDebugMode;
			Debug.Log("Debug Mode Toggled!");
		}

		if (InDebugMode) {
			DebugEval();
		} else if (!mainMenuVisible) {
			switch (currStage) {
				case Stages.NOT_STARTED:
					GameSetup();
					break;
				case Stages.GAME_SETUP:
					statusBar.text = playerColors[currentPlayer].ToUpper() + ": Place a Troop!";
					if(Input.GetMouseButtonDown(0) && !attackMenuVisible && !reinforceMenuVisible) {
						selected = GetTerritoryClick();
						if (selected && selected.GetComponent<Province>().Color.Equals(playerColors[currentPlayer])) {
							selected.GetComponent<Province>().TroopCount++;
							if(currentPlayer == (numberOfActivePlayers - 1)) {
								currentPlayer = 0;
								numberOfTroopsToPlace--;
								if (numberOfTroopsToPlace == 0) {
									currStage = Stages.REPLENISH;
									numberOfTroopsToPlace = calculateReinforcements();
								}
							} else {
								currentPlayer++;
							}
						}
					}
					break;
				case Stages.REPLENISH:
					statusBar.text = playerColors[currentPlayer].ToUpper() + ": Place " + numberOfTroopsToPlace.ToString() + " Troops!";
					if(Input.GetMouseButtonDown(0) && !attackMenuVisible && !reinforceMenuVisible) {
						selected = GetTerritoryClick();
						if (selected && selected.GetComponent<Province>().Color.Equals(playerColors[currentPlayer])) {
							selected.GetComponent<Province>().TroopCount++;
							numberOfTroopsToPlace--;
							if(numberOfTroopsToPlace == 0) {
								currStage = Stages.ATTACK;
								selected = null;
							}
						}
					}
					break;
				case Stages.ATTACK:
					if(!selected) {
						statusBar.text = playerColors[currentPlayer].ToUpper() + ": Click a Territory to attack from";
					} else {
						statusBar.text = playerColors[currentPlayer].ToUpper() + ": Select a region to attack\nRight Click To Reinforce";
					}
					if(Input.GetMouseButtonDown(0) && !attackMenuVisible && !reinforceMenuVisible) {
						GameObject temp = GetTerritoryClick();
						if(temp) {
							if (temp.GetComponent<Province>().Color.Equals(playerColors[currentPlayer])) {
								if(selected) {
									selected.GetComponent<Province>().Deselect();
								}
								selected = temp;
								selected.GetComponent<Province>().Select();
							} else {
								selected2 = temp;
								AttackSetup();
							}
						}
					}
					if(Input.GetMouseButtonDown(1)) {
						currStage = Stages.REINFORCE;
						reinforcementPerformedThisTurn = false;
						if (selected) {
							selected.GetComponent<Province>().Deselect();
						}
						selected = null;
						selected2 = null;
					}
					break;
				case Stages.REINFORCE:
					if (!selected) {
						statusBar.text = playerColors[currentPlayer].ToUpper() + ": Select a Region to Move Troops From";
					} else {
						statusBar.text = playerColors[currentPlayer].ToUpper() + ": Select a Region to Move Troops To\n(Right Click To change departing region)";
					}
					if(Input.GetMouseButtonDown(0) && !attackMenuVisible && !reinforceMenuVisible) {
						GameObject temp = GetTerritoryClick();
						if(temp) {
							if(temp.GetComponent<Province>().Color.Equals(playerColors[currentPlayer])) {
								if(!selected) {
									temp.GetComponent<Province>().Select();
									selected = temp;
								} else {
									selected2 = temp;
									ReinforceSetup();
								}
							} 
						}
					}
					if(Input.GetMouseButtonDown(1) && !attackMenuVisible && !reinforceMenuVisible) {
						GameObject temp = GetTerritoryClick();
						if(temp) {
							if(temp.GetComponent<Province>().Color.Equals(playerColors[currentPlayer])) {
								if(selected) {
									selected.GetComponent<Province>().Deselect();
								}
								selected = temp;
							}
						}
					}
					if(!reinforceMenuVisible && reinforcementPerformedThisTurn) {
						NewTurn();
					}
					break;
			}
			if(Input.GetKeyDown(KeyCode.Escape)) {
				mainMenu.SetActive(true);
				mainMenuVisible = true;
				statusMenu.SetActive(true);
			}
		}
	}
}

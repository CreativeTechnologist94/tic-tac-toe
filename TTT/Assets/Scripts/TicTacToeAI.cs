using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
	int _aiLevel;

	TicTacToeState[,] boardState; 

	[SerializeField]
	private bool _isPlayerTurn;

	[SerializeField]
	private int _gridSize = 3;  
	
	private TicTacToeState playerState = TicTacToeState.cross;
	private TicTacToeState aiState = TicTacToeState.circle;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	ClickTrigger[,] _triggers;

	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];
		onGameStarted.Invoke();
    
		// Initialze Grid
		boardState = new TicTacToeState[_gridSize,_gridSize];
		for(int i = 0; i < 3; i++)
        {
			for(int j = 0; j < 3; j++)
            {
				boardState[j,i] = TicTacToeState.none;
            }
        }
	}

	public void PlayerSelects(int coordX, int coordY)
	{
		if (!_isPlayerTurn) return ;										// Dont allow player to play when its ai mode
		if (boardState[coordX, coordY] != TicTacToeState.none) return;		// Dont allow to select a filled tile
		boardState[coordX, coordY] = playerState;							// Fill the grid location with playerState
	
		SetVisual(coordX, coordY, playerState);
	}

	public void AiSelects(int coordX, int coordY)
	{									
		if (boardState[coordX, coordY] != TicTacToeState.none) return;     // Dont allow to select a filled tile
		boardState[coordX, coordY] = aiState;                              // Fill the grid location with aiState
		
		SetVisual(coordX, coordY, aiState);
	}

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
        
		if (CheckGameEnd()) return;

		// Switch turn
		_isPlayerTurn = !_isPlayerTurn;										

		if (!_isPlayerTurn)
			Invoke("AI",1.0f);
	}

	private bool CheckGameEnd()
    {
		// check player win
		if(CheckAllCombinations(playerState))
        {
			GameOver(2);
			return true;
        }
		// check AI win
		else if (CheckAllCombinations(aiState))
		{
			GameOver(1);
			return true;
		}
		// check for a tie
		else if (NumberOfEmptyTiles() == 0)
        {
			GameOver(-1);
			return true;
		}
		return false;
	}

	private void GameOver(int winner)
    {
		onPlayerWin.Invoke(winner);
    }

	private int NumberOfEmptyTiles()
    {
		int counter = 0;
		for (int j = 0; j < 3; j++)
		{
			for (int i = 0; i < 3; i++)
			{
				if (boardState[i, j] == TicTacToeState.none) counter++;
			}
		}
		return counter;
	}

	private void AI()
    {
		int x = -1;
		int y = -1;

		// Check if player has 2 points in a row
		AICheckPlayerRowTile(out x, out y);

		// Check if player has 2 points in a column
		if (x == -1 || y == -1) AICheckPlayerColumnTile(out x, out y);

		// Otherwise play a random move on an empty tile
		if (x == -1 || y == -1)
        {
			do
			{
				x = UnityEngine.Random.Range(0, 3);
				y = UnityEngine.Random.Range(0, 3);
			} while (boardState[x, y] != TicTacToeState.none);
		}

		AiSelects(x, y);
	}

	private bool AICheckPlayerRowTile(out int x , out int y)
    {
		for(int j = 0; j < 3; j++)
        {
			int counter = 0;
			int xIndex	= -1;
			for(int i = 0; i < 3; i++)
            {
				if (boardState[i, j] == playerState) counter++;
				else if (boardState[i, j] == TicTacToeState.none) xIndex = i;
            }
			if(counter == 2 && xIndex != -1)
            {
				x = xIndex;
				y = j;
				return true;
            }
        }
		x = -1;
		y = -1;
		return false;
    }

	private bool AICheckPlayerColumnTile(out int x, out int y)
	{
		for (int i = 0; i < 3; i++)
		{
			int counter = 0;
			int yIndex = -1;
			for (int j = 0; j < 3; j++)
			{
				if (boardState[i, j] == playerState) counter++;
				else if (boardState[i, j] == TicTacToeState.none) yIndex = j;
			}
			if (counter == 2 && yIndex != -1)
			{
				y = yIndex;
				x = i;
				return true;
			}
		}
		x = -1;
		y = -1;
		return false;
	}

	private bool CheckAllCombinations(TicTacToeState state)
    {
		bool findCombination = false;
		if(boardState[0,0] ==state && boardState[1, 0] == state && boardState[2, 0] == state)
        {
			findCombination = true;
		}
		else if (boardState[0, 1] == state && boardState[1, 1] == state && boardState[2, 1] == state)
		{
			findCombination = true;
		}
		else if (boardState[0, 2] == state && boardState[1, 2] == state && boardState[2, 2] == state)
		{
			findCombination = true;
		}
		else if (boardState[0, 0] == state && boardState[0, 1] == state && boardState[0, 2] == state)
		{
			findCombination = true;
		}
		else if (boardState[1, 0] == state && boardState[1, 1] == state && boardState[1, 2] == state)
		{
			findCombination = true;
		}
		else if (boardState[2, 0] == state && boardState[2, 1] == state && boardState[2, 2] == state)
		{
			findCombination = true;
		}
		else if (boardState[0, 0] == state && boardState[1, 1] == state && boardState[2, 2] == state)
		{
			findCombination = true;
		}
		else if (boardState[2, 0] == state && boardState[1, 1] == state && boardState[0, 2] == state)
		{
			findCombination = true;
		}
		return findCombination;
    }
}

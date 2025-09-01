using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimonGameManager : MonoBehaviour
{
    private VisualElement simon_gameBoard;
    private Button simon_startButton;
    private List<Button> simon_gameTiles;

    public void SetGameElements(VisualElement gameBoard, Button startButton, List<Button> tiles)
    {
        simon_gameBoard = gameBoard;
        simon_startButton = startButton;
        simon_startButton?.RegisterCallback<ClickEvent>(StartGame);
        simon_gameTiles = tiles;
        foreach (var tile in simon_gameTiles)
        {
            tile.RegisterCallback<ClickEvent>(TileClicked);
        }
    }

    private void StartGame(ClickEvent evt)
    {
        
    }

    private void TileClicked(ClickEvent evt)
    {
        
    }
}

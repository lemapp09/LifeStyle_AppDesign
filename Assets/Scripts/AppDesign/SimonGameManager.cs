using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimonGameManager : MonoBehaviour
{
    private VisualElement simon_gameBoard;
    private Button simon_startButton;
    private List<Button> simon_gameTiles;
    private Label simon_playerTurnLabel;
    [SerializeField] private AudioClip simon_audio;

    [Header("SFX Properties")]
    [Tooltip("The duration in seconds that the SFX tone will play for.")]
    [SerializeField] private float _sfxPlayDuration = 0.5f;

    private enum GameMode
    {
        None,
        Menu,
        Listening,
        Playing
    }
    private GameMode _gameMode = GameMode.None;
    private List<int> _levelTiles;
    private int _currentIndex;

    public void SetGameElements(VisualElement gameBoard, Button startButton, List<Button> tiles, Label playerTurnLabel)
    {
        simon_gameBoard = gameBoard;
        simon_startButton = startButton;
        simon_startButton?.RegisterCallback<ClickEvent>(StartGame);
        simon_gameTiles = tiles;
        foreach (var tile in simon_gameTiles)
        {
            tile.RegisterCallback<ClickEvent>(TileClicked);
        }
        simon_playerTurnLabel = playerTurnLabel;
    }

    private void StartGame(ClickEvent evt)
    {
        AudioManager.Instance.DuckBackgroundVolume();
        _gameMode = GameMode.Menu;
        //Hide the Play Button
        simon_startButton.visible = false;
        simon_playerTurnLabel.visible = false;
        
        // Clear out the old level data, start with 3 tones
        _levelTiles = new List<int>();
        _levelTiles = new()
        {
            Random.Range(0, 9),
            Random.Range(0, 9),
            Random.Range(0, 9)
        };
        
        // Play the game light sequence;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // Set the approriate game mode
        _gameMode = GameMode.Listening;
        
        // wait two seconds to start
        yield return new WaitForSeconds(2.0f);
        
        // Light each of the tiles in sequnce
        foreach (int index in _levelTiles)
        {
            yield return FlashTile(index);
        }
        
        // Set teh Game Mode to Playing to allow the user input
        _currentIndex = 0;
        _gameMode = GameMode.Playing;
        simon_playerTurnLabel.text = "Your Turn";
        simon_playerTurnLabel.visible = true;
    }

    private IEnumerator MenuTileAnimation()
    {
        while (_gameMode == GameMode.Menu)
        {
            yield return FlashTile(Random.Range(0, 9));
        }
    }

    private IEnumerator FlashTile(int range)
    {
        // Play tone assigned to this tile
        AudioManager.Instance.PlaySFX(range);
        
        // Exapand border around the tile for 0.5f seconds
        yield return ExpandTileBorder(simon_gameTiles[range]);
        yield return new WaitForSeconds(_sfxPlayDuration);
    }

    private void TileClicked(ClickEvent evt)
    {
        simon_playerTurnLabel.visible = false;
        if (_gameMode != GameMode.Playing) return;
        // Get the last digital of the tile button's name
        var clickedButton = evt.target as Button;
        string name = clickedButton.name;
        int lastDigit = 0;
        char lastChar = name[name.Length - 1]; // Gets last character

        if (char.IsDigit(lastChar))
        {
            lastDigit = int.Parse(lastChar.ToString());
        }
        else
        {
            Debug.LogWarning("Last character is not a digit!");
        }
        
        //if it was the correct tile
        if (lastDigit - 1 == _levelTiles[_currentIndex ])
        {
            // Play the SFX 
            AudioManager.Instance.PlaySFX(lastDigit - 1);

            // Exapand border around the tile for 0.5f seconds
            StartCoroutine(ExpandTileBorder(clickedButton));
            
            _currentIndex++;
            // If we've reached the end, add another light and play sequence.
            if (_currentIndex == _levelTiles.Count)
            {
                _levelTiles.Add(Random.Range(0, 9));
                StartCoroutine(PlaySequence());
            }
            
        }
        else // End the game
        {
            simon_playerTurnLabel.visible = true;
            simon_playerTurnLabel.text = $"You got to level {_levelTiles.Count - 2}";
            _gameMode = GameMode.Menu;
            simon_startButton.visible = true;
            AudioManager.Instance.RestoreBackgroundVolume();
        }
    }

    private IEnumerator ExpandTileBorder(Button clickedButton)
    {
        clickedButton.AddToClassList("simon_gameTile_active");
        yield return new WaitForSeconds(_sfxPlayDuration);
        clickedButton.RemoveFromClassList("simon_gameTile_active");
    }

    private void OnDisable()
    {
        simon_startButton?.UnregisterCallback<ClickEvent>(StartGame);
        foreach (var tile in simon_gameTiles)
        {
            tile.UnregisterCallback<ClickEvent>(TileClicked);
        }
        simon_startButton.visible = true;
    }
}
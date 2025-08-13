using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AppDesign
{
    public class TicTacToeController : MonoBehaviour
    {
        private List<Button> _cells;
        private Label _scoreLabel;
        private int _playerScore;
        private int _computerScore;
        private bool _isPlayerTurn = true;
        private bool _gameOver;

        public void Initialize(VisualElement root)
        {
            _cells = root.Query<Button>(className: "tic-tac-toe-cell").ToList();
            _scoreLabel = root.Q<Label>("ScoreLabel");

            foreach (var cell in _cells)
            {
                cell.RegisterCallback<ClickEvent>(OnCellClicked);
            }

            var resetButton = root.Q<Button>("ResetButton");
            resetButton.RegisterCallback<ClickEvent>(ResetGame);

            ResetGame(null);
        }

        private void OnCellClicked(ClickEvent evt)
        {
            if (_gameOver || !_isPlayerTurn) return;

            var button = evt.target as Button;
            if (string.IsNullOrEmpty(button.text))
            {
                button.text = "X";
                _isPlayerTurn = false;

                if (CheckForWin("X"))
                {
                    EndGame(true);
                }
                else if (IsDraw())
                {
                    EndGame(null);
                }
                else
                {
                    ComputerTurn();
                }
            }
        }

        private void ComputerTurn()
        {
            int bestScore = int.MinValue;
            int bestMove = -1;

            for (int i = 0; i < _cells.Count; i++)
            {
                if (string.IsNullOrEmpty(_cells[i].text))
                {
                    _cells[i].text = "O";
                    int score = Minimax(false);
                    _cells[i].text = "";
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = i;
                    }
                }
            }

            _cells[bestMove].text = "O";
            if (CheckForWin("O"))
            {
                EndGame(false);
            }
            else if (IsDraw())
            {
                EndGame(null);
            }
            else
            {
                _isPlayerTurn = true;
            }
        }

        private int Minimax(bool isMaximizing)
        {
            if (CheckForWin("O")) return 1;
            if (CheckForWin("X")) return -1;
            if (IsDraw()) return 0;

            int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

            for (int i = 0; i < _cells.Count; i++)
            {
                if (string.IsNullOrEmpty(_cells[i].text))
                {
                    _cells[i].text = isMaximizing ? "O" : "X";
                    int score = Minimax(!isMaximizing);
                    _cells[i].text = "";
                    bestScore = isMaximizing ? Mathf.Max(score, bestScore) : Mathf.Min(score, bestScore);
                }
            }
            return bestScore;
        }

        private bool CheckForWin(string player)
        {
            // Check rows
            for (int i = 0; i < 9; i += 3)
            {
                if (_cells[i].text == player && _cells[i + 1].text == player && _cells[i + 2].text == player)
                    return true;
            }

            // Check columns
            for (int i = 0; i < 3; i++)
            {
                if (_cells[i].text == player && _cells[i + 3].text == player && _cells[i + 6].text == player)
                    return true;
            }

            // Check diagonals
            if (_cells[0].text == player && _cells[4].text == player && _cells[8].text == player)
                return true;
            if (_cells[2].text == player && _cells[4].text == player && _cells[6].text == player)
                return true;

            return false;
        }

        private bool IsDraw()
        {
            foreach (var cell in _cells)
            {
                if (string.IsNullOrEmpty(cell.text))
                    return false;
            }
            return true;
        }

        private void EndGame(bool? playerWon)
        {
            _gameOver = true;
            if (playerWon.HasValue)
            {
                if (playerWon.Value)
                {
                    _playerScore++;
                }
                else
                {
                    _computerScore++;
                }
            }
            UpdateScore();
        }

        private void ResetGame(ClickEvent evt)
        {
            foreach (var cell in _cells)
            {
                cell.text = "";
            }
            _gameOver = false;
            _isPlayerTurn = true;
            UpdateScore();
        }

        private void UpdateScore()
        {
            _scoreLabel.text = $"Player: {_playerScore} - Computer: {_computerScore}";
        }
    }
}


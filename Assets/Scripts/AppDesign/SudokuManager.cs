using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class SudokuManager : MonoBehaviour
    {
        private SudokuCellData[] _sudokuCellData = new SudokuCellData[82];
        private List<Label> _sudokuErrors;
        private int _currentErrors = 0;
        private static int _cluesToKeep = 0;
        private int _blankCells;
        private bool _gameBegun = false;
        private SudokuDifficulty _currentDiffLevel = SudokuDifficulty.Easy;
        private int _currentNumberSelected = 0;
        private Button _currentButton;
        private int[,] _board = new int[9, 9];
        private VisualElement _sudokuGameWon;
        private Color _blankCellColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        // Entry point to generate a filled Sudoku _board
        public int[,] GenerateBoard()
        {
            FillBoard(0, 0);
            return _board;
        }

        // Recursive backtracking function
        private bool FillBoard(int row, int col)
        {
            if (row == 9) // Done filling all rows
                return true;

            // Move to next row if column exceeds
            if (col == 9)
                return FillBoard(row + 1, 0);

            // Generate a random sequence of numbers 1-9 to try at this cell
            int[] numbers = GetShuffledNumbers();

            foreach (int num in numbers)
            {
                if (IsValid(row, col, num))
                {
                    _board[row, col] = num;

                    if (FillBoard(row, col + 1))
                        return true;

                    // Backtrack
                    _board[row, col] = 0;
                }
            }

            // No valid number found, trigger backtracking
            return false;
        }

        // Check if placing num in _board[row, col] is valid
        private bool IsValid(int row, int col, int num)
        {
            // Check row
            for (int c = 0; c < 9; c++)
                if (_board[row, c] == num)
                    return false;

            // Check column
            for (int r = 0; r < 9; r++)
                if (_board[r, col] == num)
                    return false;

            // Check block
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (_board[r, c] == num)
                    return false;

            return true;
        }

        // Return an array of numbers 1-9 shuffled randomly each time
        private int[] GetShuffledNumbers()
        {
            int[] numbers = new int[9];
            for (int i = 0; i < 9; i++)
                numbers[i] = i + 1;

            // Fisher-Yates shuffle using UnityEngine.Random.Range
            for (int i = 9 - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }

            return numbers;
        }

        private void DisplayBoard()
        {
            var mask = GeneratePuzzleMask(_currentDiffLevel);
            int cellNumberSetter = 1;
            for (int row = 0; row < 9; row++) // Set each cell's background to invisible
            {
                for (int col = 0; col < 9; col++)
                {
                    _sudokuCellData[cellNumberSetter].UILabel.style.backgroundColor =
                        new StyleColor(new Color(0f, 0f, 0f, 0f));
                    cellNumberSetter++;
                }
            }

            for (int cellNumber = 1; cellNumber < 82; cellNumber++)
            {
                int row = 3 * (((cellNumber - 1) / 9) / 3) + (((cellNumber - 1) % 9) / 3);
                int col = 3 * (((cellNumber - 1) / 9) % 3) + (((cellNumber - 1) % 9) % 3);
                _sudokuCellData[cellNumber].SolvedValue = _board[row, col];
                if (mask[row, col] == 1)
                {
                    _sudokuCellData[cellNumber].DisplayValue = _board[row, col];
                    _sudokuCellData[cellNumber].UILabel.text = _board[row, col].ToString();
                    _sudokuCellData[cellNumber].UILabel.parent.focusable = false;
                }
                else
                {
                    int currentCellNumber = cellNumber;
                    _sudokuCellData[cellNumber].DisplayValue = 0;
                    _sudokuCellData[cellNumber].UILabel.text = "";
                    _sudokuCellData[cellNumber].UILabel.style.backgroundColor =
                        new StyleColor(_blankCellColor);
                    _sudokuCellData[cellNumber].UILabel.clicked += () =>
                        CellClicked(_sudokuCellData[currentCellNumber].UILabel as Button);
                    _sudokuCellData[cellNumber].UILabel.RegisterCallback<PointerEnterEvent>(CellEntered);
                    _sudokuCellData[cellNumber].UILabel.RegisterCallback<PointerLeaveEvent>(CellExited);
                }
            }
        }

        public enum SudokuDifficulty
        {
            Easy,
            Moderate,
            Hard,
            Expert
        }

        public static int[,] GeneratePuzzleMask(SudokuDifficulty difficulty)
        {
            _cluesToKeep = difficulty switch
            {
                SudokuDifficulty.Easy => UnityEngine.Random.Range(56, 63),
                SudokuDifficulty.Moderate => UnityEngine.Random.Range(51, 55),
                SudokuDifficulty.Hard => UnityEngine.Random.Range(41, 50),
                SudokuDifficulty.Expert => UnityEngine.Random.Range(31, 41),
                _ => 36
            };

            int[,] mask = new int[9, 9];
            List<(int row, int col)> cells = new List<(int, int)>();

            // Collect all cell positions
            for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cells.Add((r, c));

            // Shuffle cells randomly
            for (int i = 0; i < cells.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, cells.Count);
                (cells[i], cells[j]) = (cells[j], cells[i]);
            }

            // Keep "cluesToKeep" number of cells visible
            for (int i = 0; i < _cluesToKeep; i++)
            {
                var (r, c) = cells[i];
                mask[r, c] = 1; // clue visible
            }

            // The rest remain 0 (blank)
            return mask;
        }

        public void SetSudokuCellData(SudokuCellData[] sudokuCellData)
        {
            _sudokuCellData = sudokuCellData;
            GenerateBoard();
            DisplayBoard();
            _blankCells = 81 - _cluesToKeep;
        }

        public void LevelSelected(ChangeEvent<ToggleButtonGroupState> evt)
        {
            if (evt.newValue.length > 0 && !_gameBegun)  // can not change difficulty level once the game has begun
            {
                if (evt.newValue[0])
                {
                    _currentDiffLevel = SudokuDifficulty.Easy;
                }

                if (evt.newValue[1])
                {
                    _currentDiffLevel = SudokuDifficulty.Moderate;
                }

                if (evt.newValue[2])
                {
                    _currentDiffLevel = SudokuDifficulty.Hard;
                }

                if (evt.newValue[3])
                {
                    _currentDiffLevel = SudokuDifficulty.Expert;
                }

                GenerateBoard(); // reset the board for a new game based on new level
                DisplayBoard();
                _blankCells = 81 - _cluesToKeep;
            }
        }

        public void NumberSelected(PointerUpEvent evt)
        {
            if (evt.target is Button clickedButton)
            {
                _currentNumberSelected = int.Parse(clickedButton.text);

                if (_currentButton != null)
                {
                    _currentButton.text = _currentNumberSelected.ToString();

                    string lastTwo = _currentButton.name.Substring(_currentButton.name.Length - 2, 2);
                    int cellNumber = int.Parse(lastTwo);

                    if (_currentNumberSelected != _sudokuCellData[cellNumber].SolvedValue)
                    {
                        _currentButton.style.color = new StyleColor(Color.red);
                        if (_currentErrors < 3)
                        {
                            _sudokuErrors[_currentErrors].visible = true;
                            _currentErrors++;
                        }
                    }
                    else
                    {
                        _currentButton.style.color = new StyleColor(new Color(0.08f, 0.08f, 0.08f, 1.0f));
                        _currentButton.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
                        _sudokuCellData[cellNumber].UILabel.parent.focusable = false;
                        _sudokuCellData[cellNumber].UILabel.clickable.activators.Clear();
                        _sudokuCellData[cellNumber].UILabel.UnregisterCallback<PointerEnterEvent>(CellEntered);
                        _sudokuCellData[cellNumber].UILabel.UnregisterCallback<PointerLeaveEvent>(CellExited);
                        
                        // Set all border widths to 0 pixels
                        _currentButton.style.borderTopWidth = 0f;
                        _currentButton.style.borderRightWidth = 0f;
                        _currentButton.style.borderBottomWidth = 0f;
                        _currentButton.style.borderLeftWidth = 0f;

                        // Set all border colors to clear
                        _currentButton.style.borderTopColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                        _currentButton.style.borderRightColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                        _currentButton.style.borderBottomColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                        _currentButton.style.borderLeftColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                        
                        _blankCells--;
                        if(_blankCells < 1)
                        {
                            _sudokuGameWon.visible = true;
                            StartCoroutine(RemoveGameWon());
                            Debug.Log("Game Won!");
                        }
                    }
                }
            }
        }

        private IEnumerator RemoveGameWon()
        {
            yield return new WaitForSeconds(5.0f);
            _sudokuGameWon.visible = false;
        }

        private void CellClicked(Button clickedButton)
        {
            if (!_gameBegun)  // Once a cell is selected, the game has begun
            {
                _gameBegun = true;
            }
            
            if (_currentButton != null) // reset the previous cell
            {
                // Reset previous button cell
                // Set all border widths to 0 pixels
                _currentButton.style.borderTopWidth = 0f;
                _currentButton.style.borderRightWidth = 0f;
                _currentButton.style.borderBottomWidth = 0f;
                _currentButton.style.borderLeftWidth = 0f;

                // Set all border colors to clear
                _currentButton.style.borderTopColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                _currentButton.style.borderRightColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                _currentButton.style.borderBottomColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
                _currentButton.style.borderLeftColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0f));
            }

            _currentButton = clickedButton;
            // Set all border widths to 2 pixels
            _currentButton.style.borderTopWidth = 5f;
            _currentButton.style.borderRightWidth = 5f;
            _currentButton.style.borderBottomWidth = 5f;
            _currentButton.style.borderLeftWidth = 5f;

            // Set all border colors to red
            _currentButton.style.borderTopColor = new StyleColor(Color.red);
            _currentButton.style.borderRightColor = new StyleColor(Color.red);
            _currentButton.style.borderBottomColor = new StyleColor(Color.red);
            _currentButton.style.borderLeftColor = new StyleColor(Color.red);
        }

        private void CellEntered(PointerEnterEvent evt)
        {
            if (evt.target is Button clickedButton)
            {
                clickedButton.style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.7f));
            }
        }

        private void CellExited(PointerLeaveEvent evt)
        {
            if (evt.target is Button clickedButton)
            {
                clickedButton.style.backgroundColor = new StyleColor(_blankCellColor);
            }
        }

        public void SetSudokuErrors(List<Label> sudokuErrors)
        {
            _sudokuErrors = sudokuErrors;
            foreach (Label sudokuError in sudokuErrors)
            {
                sudokuError.visible = false; //SetEnabled(false);
            }
        }

        public void SetSudokuGameWon(VisualElement sudokuGameWon)
        {
            _sudokuGameWon = sudokuGameWon;
        }
    }
}
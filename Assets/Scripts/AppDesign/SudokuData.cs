using UnityEngine.UIElements;

namespace AppDesign
{
    public class SudokuCellData
    {
        public int ID; // unique id number for each cell (1-81)
        public int Row; // Group row (0-2)
        public int Column; // Group column (0-2)
        public int Cell; // Cell within the group (0-8)
        public Button UILabel;
        public int SolvedValue; // 1-9
        public int DisplayValue; // 0-9

        public SudokuCellData(int id, int row, int column, int cell, Button uiLabel, int solvedValue, int displayValue)
        {
            ID = id;
            Row = row;
            Column = column;
            Cell = cell;
            UILabel = uiLabel;
            SolvedValue = solvedValue;
            DisplayValue = displayValue;
        }
    }
}
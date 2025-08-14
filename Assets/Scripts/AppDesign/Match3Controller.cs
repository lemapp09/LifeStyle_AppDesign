using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppDesign
{
    public class Match3Piece
    {
        public VisualElement element;
        public int type;
        public int x, y;
    }

    public class Match3Controller : MonoBehaviour
    {
        private int _gridSize;
        private VisualElement _gridContainer;
        private Label _scoreLabel;
        private int _score;
        private List<Texture2D> _pieceTextures = new List<Texture2D>();
        private Match3Piece[,] _grid;
        private Match3Piece _draggedPiece;
        private WiggleEffect _wiggleEffect;

        public void Initialize(VisualElement root, int gridSize, WiggleEffect wiggleEffect)
        {
            _gridSize = gridSize;
            _gridContainer = root.Q<VisualElement>("Match3Grid");
            _scoreLabel = root.Q<Label>("Match3ScoreLabel");
            _grid = new Match3Piece[_gridSize, _gridSize];
            _wiggleEffect = wiggleEffect;

            StartCoroutine(LoadPieceTexturesAndCreateGrid());
        }

        private IEnumerator LoadPieceTexturesAndCreateGrid()
        {
            yield return StartCoroutine(LoadPieceTextures());
            CreateGrid();
            UpdateScoreDisplay();
        }

        private IEnumerator LoadPieceTextures()
        {
            _pieceTextures.Clear();
            var random = new System.Random();
            var textureNumbers = Enumerable.Range(1, 78).OrderBy(x => random.Next()).Take(_gridSize).ToList();

            foreach (var num in textureNumbers)
            {
                var textureRequest = Resources.LoadAsync<Texture2D>($"Match3/Element{num:D2}");
                yield return textureRequest;

                if (textureRequest.asset != null)
                {
                    _pieceTextures.Add(textureRequest.asset as Texture2D);
                }
            }
        }

        private void CreateGrid()
        {
            _gridContainer.Clear();

            for (int y = 0; y < _gridSize; y++)
            {
                var row = new VisualElement();
                row.AddToClassList("match3-row");
                _gridContainer.Add(row);

                for (int x = 0; x < _gridSize; x++)
                {
                    var cell = new VisualElement();
                    cell.AddToClassList("match3-cell");
                    row.Add(cell);
                    CreatePiece(x, y, cell);
                }
            }
        }

        private void CreatePiece(int x, int y, VisualElement cell)
        {
            if (_pieceTextures.Count == 0) return;

            cell.Clear();

            var piece = new Match3Piece { x = x, y = y };
            piece.type = Random.Range(0, _pieceTextures.Count);

            piece.element = new VisualElement();
            piece.element.AddToClassList($"match3-piece{_gridSize:D2}");
            piece.element.style.backgroundImage = _pieceTextures[piece.type];

            piece.element.RegisterCallback<PointerDownEvent>(evt => OnPiecePointerDown(evt, piece));

            _grid[x, y] = piece;
            cell.Add(piece.element);
        }

        private void OnPiecePointerDown(PointerDownEvent evt, Match3Piece piece)
        {
            _draggedPiece = piece;
            _draggedPiece.element.CapturePointer(evt.pointerId);
            _wiggleEffect.OnHoverEnter(new PointerEnterEvent { target = _draggedPiece.element });
            _draggedPiece.element.RegisterCallback<PointerMoveEvent>(OnPiecePointerMove);
            _draggedPiece.element.RegisterCallback<PointerUpEvent>(OnPiecePointerUp);
        }

        private void OnPiecePointerMove(PointerMoveEvent evt)
        {
            if (_draggedPiece == null) return;
            
            var targetPiece = FindPieceAt(evt.position);
            if (targetPiece != null && targetPiece != _draggedPiece)
            {
                SwapPieces(_draggedPiece, targetPiece);
            }
        }

        private void OnPiecePointerUp(PointerUpEvent evt)
        {
            if (_draggedPiece == null) return;
            
            _wiggleEffect.OnHoverLeave(new PointerLeaveEvent { target = _draggedPiece.element });
            _draggedPiece.element.UnregisterCallback<PointerMoveEvent>(OnPiecePointerMove);
            _draggedPiece.element.UnregisterCallback<PointerUpEvent>(OnPiecePointerUp);
            _draggedPiece.element.ReleasePointer(evt.pointerId);
            _draggedPiece = null;
        }

        private void SwapPieces(Match3Piece piece1, Match3Piece piece2)
        {
            var tempType = piece1.type;
            piece1.type = piece2.type;
            piece2.type = tempType;

            if (piece1.type >= 0 && piece1.type < _pieceTextures.Count)
            {
                piece1.element.style.backgroundImage = _pieceTextures[piece1.type];
            }
            if (piece2.type >= 0 && piece2.type < _pieceTextures.Count)
            {
                piece2.element.style.backgroundImage = _pieceTextures[piece2.type];
            }

            if (!CheckForMatches())
            {
                // Swap back if no matches
                tempType = piece1.type;
                piece1.type = piece2.type;
                piece2.type = tempType;

                if (piece1.type >= 0 && piece1.type < _pieceTextures.Count)
                {
                    piece1.element.style.backgroundImage = _pieceTextures[piece1.type];
                }
                if (piece2.type >= 0 && piece2.type < _pieceTextures.Count)
                {
                    piece2.element.style.backgroundImage = _pieceTextures[piece2.type];
                }
            }
        }

        private bool CheckForMatches()
        {
            var matches = new List<Match3Piece>();

            // Check rows
            for (int y = 0; y < _gridSize; y++)
            {
                for (int x = 0; x < _gridSize - 2; x++)
                {
                    if (_grid[x, y].type == _grid[x + 1, y].type && _grid[x + 1, y].type == _grid[x + 2, y].type)
                    {
                        matches.Add(_grid[x, y]);
                        matches.Add(_grid[x + 1, y]);
                        matches.Add(_grid[x + 2, y]);
                    }
                }
            }

            // Check columns
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize - 2; y++)
                {
                    if (_grid[x, y].type == _grid[x, y + 1].type && _grid[x, y + 1].type == _grid[x, y + 2].type)
                    {
                        matches.Add(_grid[x, y]);
                        matches.Add(_grid[x, y + 1]);
                        matches.Add(_grid[x, y + 2]);
                    }
                }
            }

            if (matches.Count > 0)
            {
                foreach (var piece in matches.Distinct())
                {
                    ClearPiece(piece);
                }
                StartCoroutine(FillAndCheck());
                return true;
            }

            return false;
        }

        private IEnumerator FillAndCheck()
        {
            yield return new WaitForSeconds(0.5f);
            FillGrid();
            if (CheckForMatches())
            {
                StartCoroutine(FillAndCheck());
            }
        }
        
        private void FillGrid()
        {
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_grid[x, y].type == -1)
                    {
                        for (int y2 = y + 1; y2 < _gridSize; y2++)
                        {
                            if (_grid[x, y2].type != -1)
                            {
                                _grid[x, y].type = _grid[x, y2].type;
                                _grid[x, y2].type = -1;
                                _grid[x, y].element.style.backgroundImage = _pieceTextures[_grid[x, y].type];
                                _grid[x, y2].element.style.backgroundImage = null;
                                break;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < _gridSize; y++)
            {
                for (int x = 0; x < _gridSize; x++)
                {
                    if (_grid[x, y].type == -1)
                    {
                        CreatePiece(x, y, _grid[x,y].element.parent);
                    }
                }
            }
        }
        
        private void ClearPiece(Match3Piece piece)
        {
            if (piece.type == -1) return;

            piece.type = -1;
            piece.element.style.backgroundImage = null;
            _score++;
            UpdateScoreDisplay();
        }
        
        private Match3Piece FindPieceAt(Vector2 position)
        {
            foreach (var piece in _grid)
            {
                if (piece.element.worldBound.Contains(position))
                {
                    return piece;
                }
            }
            return null;
        }

        private void UpdateScoreDisplay()
        {
            _scoreLabel.text = $"Score: {_score}";
        }
    }
}
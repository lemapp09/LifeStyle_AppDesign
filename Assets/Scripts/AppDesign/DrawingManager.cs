using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class DrawingManager : MonoBehaviour
    {
        private VisualElement _drawingPad;
        private DropdownField _drawBrushSelector;
        private DropdownField _drawColorSelector;
        private Button _drawClearButton;

        private Texture2D _canvasTexture;
        private bool _isDrawing = false;
        private Vector2 _lastPosition;

        private Color _currentColor = Color.black;
        private int _brushSize = 8;

        public void DrawingStart()
        {
            // Load blank canvas and clone into writable texture
            Texture2D sourceTex = Resources.Load<Texture2D>("blankCanvas");
            if (sourceTex == null)
            {
                Debug.LogError("blankCanvas.png not found in Resources!");
                return;
            }

            _canvasTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);

            // Copy pixels safely (requires Read/Write Enabled in import settings)
            _canvasTexture.SetPixels(sourceTex.GetPixels());
            _canvasTexture.Apply();

            UpdatePadBackground();

            // Hook up events
            _drawingPad.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _drawingPad.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _drawingPad.RegisterCallback<PointerUpEvent>(OnPointerUp);

            // Setup brush selector
            _drawBrushSelector.label = "";
            _drawBrushSelector.choices = new System.Collections.Generic.List<string> { "4", "8", "12", "20" };
            _drawBrushSelector.value = "8";
            _drawBrushSelector.RegisterValueChangedCallback(evt =>
            {
                int.TryParse(evt.newValue, out _brushSize);
            });

            // Setup color selector
            _drawColorSelector.label = "";
            _drawColorSelector.choices = new System.Collections.Generic.List<string> { "Black", "Red", "Green", "Blue", "Yellow", "Purple", "White", "Orange" };
            _drawColorSelector.value = "Black";
            _drawColorSelector.RegisterValueChangedCallback(evt =>
            {
                _currentColor = ParseColor(evt.newValue);
            });

            // Setup clear button
            _drawClearButton.clicked += DrawClearCanvas;
            DrawClearCanvas();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDrawing = true;
            _lastPosition = evt.localPosition;
            DrawAt(evt.localPosition);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_isDrawing)
            {
                _lastPosition = evt.localPosition;
                DrawAt(evt.localPosition);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            _isDrawing = false;
        }

        private void DrawAt(Vector2 localPos)
        {
            int texX = Mathf.RoundToInt(localPos.x * _canvasTexture.width / _drawingPad.resolvedStyle.width);
            int texY = Mathf.RoundToInt((_drawingPad.resolvedStyle.height - localPos.y) * _canvasTexture.height / _drawingPad.resolvedStyle.height);

            for (int x = -_brushSize; x <= _brushSize; x++)
            {
                for (int y = -_brushSize; y <= _brushSize; y++)
                {
                    if (x * x + y * y <= _brushSize * _brushSize)
                    {
                        int px = texX + x;
                        int py = texY + y;
                        if (px >= 0 && px < _canvasTexture.width && py >= 0 && py < _canvasTexture.height)
                        {
                            _canvasTexture.SetPixel(px, py, _currentColor);
                        }
                    }
                }
            }

            _canvasTexture.Apply();
            UpdatePadBackground();
        }

        private void DrawClearCanvas()
        {
            Texture2D sourceTex = Resources.Load<Texture2D>("blankCanvas");
            if (sourceTex == null)
            {
                Debug.LogError("blankCanvas.png not found in Resources!");
                return;
            }

            _canvasTexture.SetPixels(sourceTex.GetPixels());
            _canvasTexture.Apply();
            UpdatePadBackground();
        }

        private void UpdatePadBackground()
        {
            _drawingPad.style.backgroundImage = new StyleBackground(_canvasTexture);
        }

        private Color ParseColor(string colorName)
        {
            switch (colorName)
            {
                case "Red": return Color.red;
                case "Green": return Color.green;
                case "Blue": return Color.blue;
                case "Yellow": return Color.yellow;
                case "Purple": return Color.purple;
                case "Orange": return Color.orange;
                case "White": return Color.white;
                default: return Color.black;
            }
        }

        // References are injected from AppManager
        public void SetDrawingElements(
            VisualElement drawingPad,
            DropdownField drawBrushSelector,
            DropdownField drawColorSelector,
            Button drawClearButton)
        {
            _drawingPad = drawingPad;
            _drawBrushSelector = drawBrushSelector;
            _drawColorSelector = drawColorSelector;
            _drawClearButton = drawClearButton;
        }
    }
}

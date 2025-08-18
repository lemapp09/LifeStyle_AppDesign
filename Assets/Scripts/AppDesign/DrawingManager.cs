using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class DrawingManager : MonoBehaviour
    {
        private VisualElement _drawingPad;
        private bool _isDrawing = false;
        private Vector2 _lastPosition;

        public void DrawingStart()
        {
            // Register event callbacks
            _drawingPad.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _drawingPad.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _drawingPad.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }
        

        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDrawing = true;
            _lastPosition = evt.localPosition;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_isDrawing)
            {
                // You would draw a line from _lastPosition to evt.localPosition
                // This requires a custom C# `VisualElement` or a more complex approach.
                // A simpler approach is to use a series of smaller VisualElements.
                DrawCircle(evt.localPosition);
                _lastPosition = evt.localPosition;
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            _isDrawing = false;
        }

        // A simple method to "draw" by adding small VisualElements as dots
        private void DrawCircle(Vector2 position)
        {
            var dot = new VisualElement();
            dot.style.position = Position.Absolute;
            dot.style.width = 5;
            dot.style.height = 5;
            dot.style.left = position.x - 2.5f;
            dot.style.top = position.y - 2.5f;
            dot.style.backgroundColor = new StyleColor(Color.black);
            dot.style.borderTopLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            dot.style.borderTopRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            dot.style.borderBottomLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            dot.style.borderBottomRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            _drawingPad.Add(dot);
        }

        public void SetDrawingPad(VisualElement drawingPad)
        {
            _drawingPad = drawingPad;
        }
    }
}
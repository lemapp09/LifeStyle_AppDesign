using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace AppDesign
{
    public class WiggleEffect : MonoBehaviour
    {
        private Dictionary<VisualElement, Coroutine> _wiggleCoroutines = new Dictionary<VisualElement, Coroutine>();

        public void OnHoverEnter(PointerEnterEvent evt)
        {
            var elem = evt.target as VisualElement;
            if (elem != null)
            {
                if (_wiggleCoroutines.ContainsKey(elem))
                {
                    StopCoroutine(_wiggleCoroutines[elem]);
                }

                _wiggleCoroutines[elem] = StartCoroutine(WiggleElement(elem));
            }
        }

        public void OnHoverLeave(PointerLeaveEvent evt)
        {
            var elem = evt.target as VisualElement;
            if (elem != null)
            {
                if (_wiggleCoroutines.ContainsKey(elem))
                {
                    StopCoroutine(_wiggleCoroutines[elem]);
                    _wiggleCoroutines.Remove(elem);
                }

                elem.style.rotate = new Rotate(Quaternion.identity);
            }
        }

        private IEnumerator WiggleElement(VisualElement elem)
        {
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime * 10f; // speed
                float angle = Mathf.Sin(timer) * 5f; // amplitude
                elem.style.rotate = new Rotate(angle, new Vector3(0, 0, angle));
                yield return null;
            }
        }
    }
}
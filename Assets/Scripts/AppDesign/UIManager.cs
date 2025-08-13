using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AppDesign
{
    public class UIManager : MonoBehaviour
    {
        private List<VisualElement> _otherScreens = new List<VisualElement>();
        private VisualElement _mainScreen;

        public void Initialize(VisualElement root)
        {
            _mainScreen = root.Q<VisualElement>("MainScreen");
            root.Query<VisualElement>(className: "ScreenTemplate").ForEach(screenElem =>
            {
                _otherScreens.Add(screenElem);
                screenElem.style.display = DisplayStyle.None;
            });

            if (_mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.Flex;
            }
        }

        public void ShowScreen(string screenName)
        {
            foreach (var screen in _otherScreens)
            {
                screen.style.display = DisplayStyle.None;
            }
            if (_mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.None;
            }

            var selectedScreen = _otherScreens.Find(screen => screen.name == screenName);
            if (selectedScreen != null)
            {
                selectedScreen.style.display = DisplayStyle.Flex;
            }
            else if (screenName == "MainScreen" && _mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.Flex;
            }
        }
    }
}


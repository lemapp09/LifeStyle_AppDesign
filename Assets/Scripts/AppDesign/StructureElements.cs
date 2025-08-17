using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{

    public class StructureElements : MonoBehaviour
    {
        public void FindScreens(VisualElement root, VisualElement _mainScreen, List<VisualElement> _otherScreens )
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
            else
            {
                Debug.LogError("MainScreen not found. Ensure it has the name 'MainScreen'.");
            }

            if (_otherScreens.Count != 12)
            {
                Debug.LogWarning($"Expected 12 non-main screens, found {_otherScreens.Count}.");
            }
        }

        public void FindAppElements(VisualElement root, List<VisualElement> _appElements, WiggleEffect _wiggleEffect)
        {
            _appElements.Clear();
            root.Query<VisualElement>(className: "appElement").ForEach(appElem =>
            {
                _appElements.Add(appElem);
                appElem.RegisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                appElem.RegisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
            });
        }

        public void AssignScreensToAppElements(List<VisualElement> _appElements, List<VisualElement> _otherScreens, AppManager _appManager )
        {
            for (int i = 0; i < _appElements.Count && i < _otherScreens.Count; i++)
            {
                int screenIndex = i;
                var appElem = _appElements[i];
                appElem.RegisterCallback<ClickEvent>(evt => _appManager.ShowScreen(_otherScreens[screenIndex].name));
            }
        }

        public void SetupBackButtons(List<VisualElement> _backButtons, List<VisualElement> _otherScreens, AppManager _appManager, WiggleEffect _wiggleEffect)
        {
            _backButtons.Clear();
            foreach (var screen in _otherScreens)
            {
                var backButton = screen.Q<Label>(className: "back-button");
                if (backButton != null)
                {
                    _backButtons.Add(backButton);
                    var currentScreen = screen;
                    backButton.RegisterCallback<PointerUpEvent>(evt => { _appManager.ShowScreen("MainScreen"); });
                    backButton.RegisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                    backButton.RegisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
                }
            }
        }

        public void SetupNavigationDropdowns(VisualElement root, List<DropdownField> _navigationDropdowns , List<VisualElement> _otherScreens,AppManager _appManager)
        {
            _navigationDropdowns.Clear();
            root.Query<DropdownField>(className: "navigation-dropdown").ForEach(dropdown =>
            {
                _navigationDropdowns.Add(dropdown);
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    int selectedIndex = dropdown.choices.IndexOf(evt.newValue);
                    if (selectedIndex >= 0 && selectedIndex < _otherScreens.Count)
                    {
                        _appManager.ShowScreen(_otherScreens[selectedIndex].name);
                    }
                });
            });
        }
    }
}
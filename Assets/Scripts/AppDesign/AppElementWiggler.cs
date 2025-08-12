using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class AppElementWiggler : MonoBehaviour
{
    private List<VisualElement> appElements = new List<VisualElement>();
    private List<VisualElement> otherScreens = new List<VisualElement>();
    private VisualElement mainScreen;

    private Dictionary<VisualElement, bool> hoverStates = new Dictionary<VisualElement, bool>();
    private Dictionary<VisualElement, float> wiggleTimers = new Dictionary<VisualElement, float>();

    private UIDocument uiDocument;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No UIDocument component found on this GameObject.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // 1. Find all ScreenTemplate elements
        List<VisualElement> allScreens = new List<VisualElement>();
        root.Query<VisualElement>(className: "ScreenTemplate").ForEach(screenElem =>
        {
            allScreens.Add(screenElem);

            // Check if this is the MainScreen
            if (screenElem.name == "MainScreen" || screenElem.ClassListContains("MainScreen"))
            {
                mainScreen = screenElem;
            }
            else
            {
                otherScreens.Add(screenElem);
            }

            // Hide all initially
            screenElem.style.display = DisplayStyle.None;
        });

        if (mainScreen == null)
        {
            Debug.LogError("MainScreen not found. Ensure it has name='MainScreen' or class='MainScreen'.");
        }
        else
        {
            // Show MainScreen by default
            mainScreen.style.display = DisplayStyle.Flex;
        }

        if (otherScreens.Count != 12)
        {
            Debug.LogWarning($"Expected 12 non-main screens, found {otherScreens.Count}.");
        }

        // 2. Find all AppElements
        appElements.Clear();
        root.Query<VisualElement>(className: "appElement").ForEach(appElem =>
        {
            appElements.Add(appElem);
            hoverStates[appElem] = false;
            wiggleTimers[appElem] = 0f;

            // Register hover callbacks
            appElem.RegisterCallback<PointerEnterEvent>(OnHoverEnter);
            appElem.RegisterCallback<PointerLeaveEvent>(OnHoverLeave);
        });

        // 3. Assign screens to appElements
        for (int i = 0; i < appElements.Count && i < otherScreens.Count; i++)
        {
            int screenIndex = i; // local copy for closure
            var appElem = appElements[i];
            appElem.RegisterCallback<ClickEvent>(evt => ShowScreen(screenIndex));
        }
        
        // 4. Assign Back Buttons on each of the 12 Screens
        foreach (var screen in otherScreens)
        {
            // Query only Label elements with class 'back-button'
            var backButton = screen.Q<Label>(className: "back-button");
            if (backButton != null)
            {
                var currentScreen = screen; // capture for closure

                // Register a pointer click event
                backButton.RegisterCallback<PointerUpEvent>(evt =>
                {
                    currentScreen.style.display = DisplayStyle.None;
                    if (mainScreen != null)
                        mainScreen.style.display = DisplayStyle.Flex;
                });

                // Optional: Change appearance on hover to look like it's clickable
                backButton.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    backButton.AddToClassList("hover");
                });
                backButton.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    backButton.RemoveFromClassList("hover");
                });
            }
        }

    }

    void Update()
    {
        foreach (var elem in appElements)
        {
            if (hoverStates[elem])
            {
                wiggleTimers[elem] += Time.deltaTime * 20f; // speed
                float angle = Mathf.Sin(wiggleTimers[elem]) * 5f; // amplitude
                elem.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void OnHoverEnter(PointerEnterEvent evt)
    {
        var elem = evt.target as VisualElement;
        if (elem != null)
        {
            hoverStates[elem] = true;
            wiggleTimers[elem] = 0f;
        }
    }

    private void OnHoverLeave(PointerLeaveEvent evt)
    {
        var elem = evt.target as VisualElement;
        if (elem != null)
        {
            hoverStates[elem] = false;
            elem.transform.rotation = Quaternion.identity;
        }
    }

    private void ShowScreen(int index)
    {
        // Hide all screens first
        foreach (var screen in otherScreens)
            screen.style.display = DisplayStyle.None;

        if (mainScreen != null)
            mainScreen.style.display = DisplayStyle.None;

        // Show selected screen
        if (index >= 0 && index < otherScreens.Count)
            otherScreens[index].style.display = DisplayStyle.Flex;
    }

    void OnDisable()
    {
        foreach (var appElem in appElements)
        {
            appElem.UnregisterCallback<PointerEnterEvent>(OnHoverEnter);
            appElem.UnregisterCallback<PointerLeaveEvent>(OnHoverLeave);
            // Click events are anonymous lambdas, so they can't be unregistered without storing delegates.
        }
    }
}

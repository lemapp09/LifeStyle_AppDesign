using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace AppDesign
{
    public class AppManager : MonoBehaviour
    {
        private List<VisualElement> _appElements = new List<VisualElement>();
        private List<VisualElement> _otherScreens = new List<VisualElement>();
        private VisualElement _mainScreen;

        private List<DropdownField> _navigationDropdowns = new List<DropdownField>();
        private List<VisualElement> _backButtons = new List<VisualElement>();

        private WiggleEffect _wiggleEffect;

        private UIDocument _uiDocument;
        private NewsManager _newsManager;
        private VisualElement _newsContainer;
        private bool _newsLoaded;
        private TicTacToeController _ticTacToeController;
        private TVMazeManager _tvMazeManager;

        void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("No UIDocument component found on this GameObject.");
                return;
            }

            var root = _uiDocument.rootVisualElement;

            _wiggleEffect = GetComponent<WiggleEffect>();
            if (_wiggleEffect == null)
            {
                _wiggleEffect = gameObject.AddComponent<WiggleEffect>();
            }

            _newsManager = GetComponent<NewsManager>();
            if (_newsManager == null)
            {
                _newsManager = gameObject.AddComponent<NewsManager>();
            }

            _ticTacToeController = GetComponent<TicTacToeController>();
            if (_ticTacToeController == null)
            {
                _ticTacToeController = gameObject.AddComponent<TicTacToeController>();
            }

            _tvMazeManager = GetComponent<TVMazeManager>();
            if (_tvMazeManager == null)
            {
                _tvMazeManager = gameObject.AddComponent<TVMazeManager>();
            }

            _newsContainer = root.Q<VisualElement>("NewsContainer");


            FindScreens(root);
            FindAppElements(root);
            AssignScreensToAppElements();
            SetupBackButtons();
            SetupNavigationDropdowns(root);
        }

        private void FindScreens(VisualElement root)
        {
            List<VisualElement> allScreens = new List<VisualElement>();
            _mainScreen = root.Q<VisualElement>("MainScreen");
            root.Query<VisualElement>(className: "ScreenTemplate").ForEach(screenElem =>
            {
                _otherScreens.Add(screenElem);

                screenElem.style.display = DisplayStyle.None;
            });

            if (_mainScreen == null)
            {
                Debug.LogError("MainScreen not found. Ensure it has name='MainScreen' or class='MainScreen'.");
            }
            else
            {
                _mainScreen.style.display = DisplayStyle.Flex;
            }

            if (_otherScreens.Count != 12)
            {
                Debug.LogWarning($"Expected 12 non-main screens, found {_otherScreens.Count}.");
            }
        }

        private void FindAppElements(VisualElement root)
        {
            _appElements.Clear();
            root.Query<VisualElement>(className: "appElement").ForEach(appElem =>
            {
                _appElements.Add(appElem);
                appElem.RegisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                appElem.RegisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
            });
        }

        private void AssignScreensToAppElements()
        {
            for (int i = 0; i < _appElements.Count && i < _otherScreens.Count; i++)
            {
                int screenIndex = i;
                var appElem = _appElements[i];
                appElem.RegisterCallback<ClickEvent>(evt => ShowScreen(_otherScreens[screenIndex].name));
            }
        }

        private void SetupBackButtons()
        {
            _backButtons.Clear();
            foreach (var screen in _otherScreens)
            {
                var backButton = screen.Q<Label>(className: "back-button");
                if (backButton != null)
                {
                    _backButtons.Add(backButton);
                    var currentScreen = screen;
                    backButton.RegisterCallback<PointerUpEvent>(evt =>
                    {
                        currentScreen.style.display = DisplayStyle.None;
                        if (_mainScreen != null)
                            _mainScreen.style.display = DisplayStyle.Flex;
                    });
                    backButton.RegisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                    backButton.RegisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
                }
            }
        }

        private void SetupNavigationDropdowns(VisualElement root)
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
                        ShowScreen(_otherScreens[selectedIndex].name);
                    }
                });
            });
        }

        private void PopulateNews(NewsArticle[] articles)
        {
            if (articles == null)
            {
                return;
            }

            foreach (var article in articles)
            {
                var articleElement = new VisualElement();
                articleElement.AddToClassList("news-article");

                var title = new Label(article.title);
                title.AddToClassList("news-title");

                var description = new Label(article.description);
                description.AddToClassList("news-description");

                articleElement.Add(title);
                articleElement.Add(description);

                if (!string.IsNullOrEmpty(article.url))
                {
                    var linkIcon = new VisualElement();
                    linkIcon.AddToClassList("news-link-icon");
                    linkIcon.tooltip = "Read full article";
                    linkIcon.RegisterCallback<ClickEvent>(evt => Application.OpenURL(article.url));
                    articleElement.Add(linkIcon);
                }

                _newsContainer.Add(articleElement);
            }
        }

        private void DisplayShows(List<Show> shows)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            if (shows == null) return;

            foreach (var show in shows)
            {
                var showElement = new VisualElement();
                showElement.AddToClassList("show-element");

                var showImage = new VisualElement();
                showImage.AddToClassList("show-image");
                if (show.image != null && !string.IsNullOrEmpty(show.image.medium))
                {
                    StartCoroutine(LoadImage(show.image.medium, showImage));
                }
                showElement.Add(showImage);

                var showName = new Label(show.name);
                showName.AddToClassList("show-name");
                showElement.Add(showName);

                showElement.RegisterCallback<ClickEvent>(evt => ShowShowDetails(show));

                showList.Add(showElement);
            }
        }

        private void ShowShowDetails(Show show)
        {
            // Placeholder for future implementation
        }

        private IEnumerator LoadImage(string url, VisualElement element)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    element.style.backgroundImage = new StyleBackground(DownloadHandlerTexture.GetContent(webRequest));
                }
            }
        }


        private void ShowScreen(string screenName)
        {
            var allScreens = new List<VisualElement>(_otherScreens);
            if (_mainScreen != null)
            {
                allScreens.Add(_mainScreen);
            }

            foreach (var screen in allScreens)
            {
                screen.style.display = DisplayStyle.None;
            }

            // Show selected screen
            var selectedScreen = _otherScreens.Find(screen => screen.name == screenName);
            if (selectedScreen != null)
            {
                selectedScreen.style.display = DisplayStyle.Flex;

            if (selectedScreen.name == "Screen04" && !_newsLoaded)
            {
                _newsLoaded = true;
                if (_newsContainer != null)
                {
                    StartCoroutine(_newsManager.GetNews(PopulateNews));
                }
            }
            if (selectedScreen.name == "Screen04" && !_newsLoaded)
            {
                _newsLoaded = true;
                if (_newsContainer != null)
                {
                    StartCoroutine(_newsManager.GetNews(PopulateNews));
                }
            }
            else if (selectedScreen.name == "Screen05")
            {
                _ticTacToeController.Initialize(selectedScreen);
            }

            if (selectedScreen.name == "Screen02")
            {
                var searchField = selectedScreen.Q<TextField>("ShowSearchField");
                searchField.RegisterCallback<ChangeEvent<string>>(evt => StartCoroutine(_tvMazeManager.SearchShows(evt.newValue, DisplayShows)));
            }
            }
            else if (screenName == "MainScreen" && _mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.Flex;
            }
        }

        void OnDisable()
        {
            foreach (var appElem in _appElements)
            {
                appElem.UnregisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                appElem.UnregisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
            }

            foreach (var backButton in _backButtons)
            {
                backButton.UnregisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                backButton.UnregisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
            }
        }
    }
}
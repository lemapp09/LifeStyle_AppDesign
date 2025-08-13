using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

namespace AppDesign
{
    public class AppManager : MonoBehaviour
    {
        // UI Elements
        private List<VisualElement> _appElements = new List<VisualElement>();
        private List<VisualElement> _otherScreens = new List<VisualElement>();
        private VisualElement _mainScreen;
        private List<DropdownField> _navigationDropdowns = new List<DropdownField>();
        private List<VisualElement> _backButtons = new List<VisualElement>();

        // Component & Manager References
        private UIDocument _uiDocument;
        private WiggleEffect _wiggleEffect;
        private NewsManager _newsManager;
        private TicTacToeController _ticTacToeController;
        private TVMazeManager _tvMazeManager;

        // State & Data Containers
        private VisualElement _newsContainer;
        private bool _newsLoaded;

        void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("No UIDocument component found on this GameObject.");
                return;
            }

            var root = _uiDocument.rootVisualElement;

            // Initialize components
            _wiggleEffect = GetComponent<WiggleEffect>() ?? gameObject.AddComponent<WiggleEffect>();
            _newsManager = GetComponent<NewsManager>() ?? gameObject.AddComponent<NewsManager>();
            _ticTacToeController = GetComponent<TicTacToeController>() ?? gameObject.AddComponent<TicTacToeController>();
            _tvMazeManager = GetComponent<TVMazeManager>() ?? gameObject.AddComponent<TVMazeManager>();

            // Find UI containers
            _newsContainer = root.Q<VisualElement>("NewsContainer");

            // Setup UI
            FindScreens(root);
            FindAppElements(root);
            AssignScreensToAppElements();
            SetupBackButtons();
            SetupNavigationDropdowns(root);
        }

        private void FindScreens(VisualElement root)
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
                        ShowScreen("MainScreen");
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

        private void ShowScreen(string screenName)
        {
            // Hide all screens first
            foreach (var screen in _otherScreens)
            {
                screen.style.display = DisplayStyle.None;
            }
            if (_mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.None;
            }

            // Show selected screen
            var selectedScreen = _otherScreens.Find(screen => screen.name == screenName);
            if (selectedScreen != null)
            {
                selectedScreen.style.display = DisplayStyle.Flex;

                // Screen-specific logic
                if (selectedScreen.name == "Screen02")
                {
                    var searchField = selectedScreen.Q<TextField>("ShowSearchField");
                    searchField.RegisterCallback<ChangeEvent<string>>(evt => StartCoroutine(_tvMazeManager.SearchShows(evt.newValue, DisplayShows)));
                }
                else if (selectedScreen.name == "Screen04" && !_newsLoaded)
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
            }
            else if (screenName == "MainScreen" && _mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.Flex;
            }
        }

        private void PopulateNews(NewsArticle[] articles)
        {
            if (articles == null) return;
            _newsContainer.Clear();
            foreach (var article in articles)
            {
                var articleElement = new VisualElement();
                articleElement.AddToClassList("news-article");

                var title = new Label(article.title);
                title.AddToClassList("news-title");

                var description = new Label(StripHtml(article.description));
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
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("show-details-container");

            var showSummary = new Label();
            showSummary.text = StripHtml(show.summary);
            showSummary.AddToClassList("show-summary");
            detailsContainer.Add(showSummary);

            var episodesHeader = new Label("Episodes");
            episodesHeader.AddToClassList("details-header");
            detailsContainer.Add(episodesHeader);

            var episodesContainer = new ScrollView();
            episodesContainer.AddToClassList("episodes-container");
            detailsContainer.Add(episodesContainer);

            StartCoroutine(_tvMazeManager.GetEpisodes(show.id, episodes => DisplayEpisodes(episodes, episodesContainer)));

            var castHeader = new Label("Cast");
            castHeader.AddToClassList("details-header");
            detailsContainer.Add(castHeader);

            var castContainer = new ScrollView();
            castContainer.AddToClassList("cast-container");
            detailsContainer.Add(castContainer);

            StartCoroutine(_tvMazeManager.GetCast(show.id, cast => DisplayCast(cast, castContainer)));

            showList.Add(detailsContainer);
        }

        private void DisplayEpisodes(List<Episode> episodes, VisualElement container)
        {
            if (episodes == null) return;
            container.Clear();
            foreach (var episode in episodes)
            {
                var episodeElement = new VisualElement();
                episodeElement.AddToClassList("episode-element");
                var episodeName = new Label($"S{episode.season:00}E{episode.number:00}: {episode.name}");
                episodeName.AddToClassList("episode-name");
                episodeElement.Add(episodeName);
                episodeElement.RegisterCallback<ClickEvent>(evt => ShowEpisodeDetails(episode));
                container.Add(episodeElement);
            }
        }

        private void DisplayCast(List<Cast> cast, VisualElement container)
        {
            if (cast == null) return;
            container.Clear();
            foreach (var member in cast)
            {
                var castElement = new VisualElement();
                castElement.AddToClassList("cast-element");

                var personImage = new VisualElement();
                personImage.AddToClassList("person-image");
                if (member.person.image != null && !string.IsNullOrEmpty(member.person.image.medium))
                {
                    StartCoroutine(LoadImage(member.person.image.medium, personImage));
                }
                castElement.Add(personImage);

                var personName = new Label(member.person.name);
                personName.AddToClassList("person-name");
                castElement.Add(personName);

                var characterName = new Label($"as {member.character.name}");
                characterName.AddToClassList("character-name");
                castElement.Add(characterName);

                castElement.RegisterCallback<ClickEvent>(evt => ShowPersonDetails(member.person));

                container.Add(castElement);
            }
        }

        private void ShowEpisodeDetails(Episode episode)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("show-details-container");

            var episodeName = new Label(episode.name);
            episodeName.AddToClassList("details-header");
            detailsContainer.Add(episodeName);

            var episodeSummary = new Label(StripHtml(episode.summary));
            episodeSummary.AddToClassList("show-summary");
            detailsContainer.Add(episodeSummary);

            showList.Add(detailsContainer);
        }

        private void ShowPersonDetails(Person person)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("show-details-container");

            var personImage = new VisualElement();
            personImage.AddToClassList("person-image-large");
            if (person.image != null && !string.IsNullOrEmpty(person.image.original))
            {
                StartCoroutine(LoadImage(person.image.original, personImage));
            }
            detailsContainer.Add(personImage);

            var personName = new Label(person.name);
            personName.AddToClassList("details-header");
            detailsContainer.Add(personName);

            showList.Add(detailsContainer);
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, "<.*?>", string.Empty);
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
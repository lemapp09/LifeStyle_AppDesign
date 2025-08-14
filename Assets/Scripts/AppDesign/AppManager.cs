using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using TMPro;

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
        private WeatherManager _weatherManager;
        private NewsManager _newsManager;
        private TicTacToeController _ticTacToeController;
        private TVMazeManager _tvMazeManager;
        private Match3Controller _match3Controller;

        // State & Data Containers
        private TextField _weatherSearch;
        private Label _weatherSubmitButton;
        private VisualElement _weatherContainer;
        private bool _weatherLoaded;
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
            _weatherManager = GetComponent<WeatherManager>() ?? gameObject.AddComponent<WeatherManager>();
            if (_weatherManager != null)
            {
                _weatherManager.OnWeatherRetrieved += PopulateWeather;
            }

            _newsManager = GetComponent<NewsManager>() ?? gameObject.AddComponent<NewsManager>();
            _ticTacToeController =
                GetComponent<TicTacToeController>() ?? gameObject.AddComponent<TicTacToeController>();
            _tvMazeManager = GetComponent<TVMazeManager>() ?? gameObject.AddComponent<TVMazeManager>();
            _match3Controller = GetComponent<Match3Controller>() ?? gameObject.AddComponent<Match3Controller>();

            // Find UI containers
            _weatherSearch = root.Q<TextField>("WeatherSearchField");
            _weatherSubmitButton = root.Q<Label>("WeatherSubmitButton");
            if (_weatherSubmitButton != null)
            {
                _weatherSubmitButton.RegisterCallback<PointerUpEvent>(evt =>
                {
                    StartCoroutine(_weatherManager.GetWeather(_weatherSearch.value));
                });
            }

            _weatherContainer = root.Q<VisualElement>("WeatherContainer");
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
                    backButton.RegisterCallback<PointerUpEvent>(evt => { ShowScreen("MainScreen"); });
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
                if (selectedScreen.name == "Screen01" && !_weatherLoaded) // Weather Screen
                {
                    string cityName = "New York City";
                    _weatherLoaded = true;
                    if (_weatherLoaded)
                    {
                        cityName = _weatherSearch.value;
                    }

                    if (_weatherContainer != null)
                    {
                        StartCoroutine(_weatherManager.GetWeather(cityName));
                    }
                }
                else if (selectedScreen.name == "Screen02") // TV Show Search
                {
                    var searchField = selectedScreen.Q<TextField>("ShowSearchField");
                    searchField.RegisterCallback<ChangeEvent<string>>(evt =>
                        StartCoroutine(_tvMazeManager.SearchShows(evt.newValue, DisplayShows)));
                }
                else if (selectedScreen.name == "Screen03") // Sports
                {
                }
                else if (selectedScreen.name == "Screen04" && !_newsLoaded) // News
                {
                    _newsLoaded = true;
                    if (_newsContainer != null)
                    {
                        StartCoroutine(_newsManager.GetNews(PopulateNews));
                    }
                }
                else if (selectedScreen.name == "Screen05") // Tic-Tac-Toe
                {
                    _ticTacToeController.Initialize(selectedScreen);
                }
                else if (selectedScreen.name == "Screen06") // Match-3
                {
                    var gridSizeSelector = selectedScreen.Q<VisualElement>("GridSizeSelector");
                    var buttons = gridSizeSelector.Query<Button>(className: "grid-size-button").ToList();
                    buttons[0].RegisterCallback<ClickEvent>(evt =>
                        _match3Controller.Initialize(selectedScreen, 9, _wiggleEffect));
                    buttons[1].RegisterCallback<ClickEvent>(evt =>
                        _match3Controller.Initialize(selectedScreen, 10, _wiggleEffect));
                    buttons[2].RegisterCallback<ClickEvent>(evt =>
                        _match3Controller.Initialize(selectedScreen, 11, _wiggleEffect));
                    buttons[3].RegisterCallback<ClickEvent>(evt =>
                        _match3Controller.Initialize(selectedScreen, 12, _wiggleEffect));

                    // Default to 9x9
                    _match3Controller.Initialize(selectedScreen, 9, _wiggleEffect);
                }
            }
            else if (screenName == "MainScreen" && _mainScreen != null)
            {
                _mainScreen.style.display = DisplayStyle.Flex;
            }
        }

        public void PopulateWeather(WeatherForecast.WeatherRoot weatherData)
        {
            _weatherContainer.Clear();
            
            _weatherContainer.Add(CreateWeatherDisplay(weatherData));
        }

        public VisualElement CreateWeatherDisplay(WeatherForecast.WeatherRoot weatherData)
        {
            // The main container for all weather information
            var mainContainer = new VisualElement();
            mainContainer.style.flexDirection = FlexDirection.Row; // Horizontal layout

            // The left-side container for the large temperature display and "FEELS LIKE" text
            var temperatureContainer = new VisualElement();
            temperatureContainer.style.flexGrow = 1; // Allows it to take up available space
            temperatureContainer.style.alignItems = Align.Center;
            
            // Weather Icon
            var weatherIconContainer = new VisualElement();
            var icon = Resources.Load<Texture2D>("WeatherIcons/Clear");
            weatherIconContainer.style.backgroundImage = new StyleBackground(icon);
            weatherIconContainer.AddToClassList("weather-icon");
            temperatureContainer.Add(weatherIconContainer);

            // The large temperature value
            var temperatureLabel = new Label(weatherData.current.temp_f.ToString() + "°");
            temperatureLabel.style.fontSize = 200;
            //temperatureLabel.style.unityFontWeight = FontWeight.Bold;
            temperatureContainer.Add(temperatureLabel);

            // The "FEELS LIKE" text
            var feelsLikeLabel = new Label("FEELS LIKE: " + weatherData.current.feelslike_f.ToString() + "°");
            feelsLikeLabel.style.fontSize = 48;
            temperatureContainer.Add(feelsLikeLabel);

            mainContainer.Add(temperatureContainer);

            // The right-side container for the detailed conditions
            var detailsContainer = new VisualElement();
            detailsContainer.style.flexGrow = 1;

            // Function to create a detail line (e.g., "WIND: " + weatherData.current.wind_dir + "weatherData.current.wind_mph.ToString() " +  + " MPH")
            void AddDetailLine(string labelText)
            {
                var detailLabel = new Label(labelText);
                detailLabel.style.fontSize = 48;
                detailsContainer.Add(detailLabel);
            }

            AddDetailLine(
                "WIND: " + weatherData.current.wind_dir + " "+ weatherData.current.wind_mph.ToString()  + " MPH");
            AddDetailLine("PRESSURE: " + weatherData.current.pressure_in.ToString() + "\"");
            AddDetailLine("DEWPOINT: " + weatherData.current.dewpoint_f.ToString() + "°");
            AddDetailLine("HUMIDITY: " + weatherData.current.humidity.ToString() + "%");

            mainContainer.Add(detailsContainer);

            // The lower row container for weatherAPI's logo
            var logoContainer = new VisualElement();
            logoContainer.style.flexGrow = 1; // Allows it to take up available space
            logoContainer.style.alignItems = Align.Center;
            
            // Weather API logo
            var logo = Resources.Load<Texture2D>("WeatherIcons/weatherapi_logo");
            logoContainer.style.backgroundImage = new StyleBackground(logo);
            logoContainer.RegisterCallback<ClickEvent>(evt => Application.OpenURL("https://www.weatherapi.com/"));
            logoContainer.AddToClassList("weather-logo");
            
            var displayContainer = new VisualElement();
            displayContainer.style.flexGrow = 1;
            displayContainer.style.alignItems = Align.FlexEnd;
            displayContainer.Add(mainContainer);
            displayContainer.Add(logoContainer);

            return displayContainer;
        }

        private void PopulateNews(NewsArticle[] articles)
        {
            _newsLoaded = false;
            if (articles == null) return;
            int newsCount = 0;
            _newsContainer.Clear();
            foreach (var article in articles)
            {
                var articleElement = new VisualElement();
                switch (newsCount % 3)
                {
                    case 0:
                        articleElement.AddToClassList("news-article01");
                        break;
                    case 1:
                        articleElement.AddToClassList("news-article02");
                        break;
                    case 2:
                        articleElement.AddToClassList("news-article03");
                        break;
                    default:
                        // This should never be hit, but included as good practice
                        articleElement.AddToClassList("news-article01");
                        break;
                }

                newsCount++;

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

            StartCoroutine(
                _tvMazeManager.GetEpisodes(show.id, episodes => DisplayEpisodes(episodes, episodesContainer)));

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


            if (_weatherManager != null)
            {
                _weatherManager.OnWeatherRetrieved -= PopulateWeather;
            }
        }
    }
}
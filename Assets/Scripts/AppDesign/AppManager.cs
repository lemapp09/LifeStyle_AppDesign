using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Linq;

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
        private StructureElements _structureElements;
        private WiggleEffect _wiggleEffect;
        private WeatherManager _weatherManager;
        private WeatherUIManager _weatherUIManager;
        private NewsManager _newsManager;
        private NewsUIManager _newsUIManager;
        private SportsManager _sportsManager;
        private SportsUIManager _sportsUIManager;
        private TicTacToeController _ticTacToeController;
        private TVMazeManager _tvMazeManager;
        private TVMazeUIManager _tvMazeUIManager;
        private Match3Controller _match3Controller;

        // State & Data Containers
        private TextField _weatherSearch;
        private Label _weatherSubmitButton;
        private VisualElement _weatherContainer;
        private bool _weatherLoaded;
        private VisualElement _newsContainer;
        private VisualElement _sportsContainer;
        
        // Sudoku
        private SudokuCellData[] _sudokuCellData = new SudokuCellData[82];
        private SudokuManager _sudokuManager;
        private List<Button> _sudokuNumberSelectors;
        private List<Label> _sudokuErrors;
        private ToggleButtonGroup _sudokuToggleButtonGroup;

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
            _weatherUIManager = GetComponent<WeatherUIManager>() ?? gameObject.AddComponent<WeatherUIManager>();
            if (_weatherManager != null)
            {
                _weatherManager.OnWeatherRetrieved += _weatherUIManager.PopulateWeather;
            }

            _structureElements = GetComponent<StructureElements>() ?? gameObject.AddComponent<StructureElements>();
            _newsManager = GetComponent<NewsManager>() ?? gameObject.AddComponent<NewsManager>();
            _newsUIManager = GetComponent<NewsUIManager>() ?? gameObject.AddComponent<NewsUIManager>();
            _sportsManager = GetComponent<SportsManager>() ?? gameObject.AddComponent<SportsManager>();
            _sportsUIManager = GetComponent<SportsUIManager>() ?? gameObject.AddComponent<SportsUIManager>();
            _ticTacToeController =
                GetComponent<TicTacToeController>() ?? gameObject.AddComponent<TicTacToeController>();
            _tvMazeManager = GetComponent<TVMazeManager>() ?? gameObject.AddComponent<TVMazeManager>();
            _tvMazeUIManager = GetComponent<TVMazeUIManager>() ?? gameObject.AddComponent<TVMazeUIManager>();
            _tvMazeUIManager.SetUIDocument(_uiDocument);
            _tvMazeUIManager.SetTvMazeManager(_tvMazeManager);
            _match3Controller = GetComponent<Match3Controller>() ?? gameObject.AddComponent<Match3Controller>();
            _sudokuManager = GetComponent<SudokuManager>() ?? gameObject.AddComponent<SudokuManager>();

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
            _weatherUIManager.SetWeatherContainer(_weatherContainer);
            _newsContainer = root.Q<VisualElement>("NewsContainer");
            _newsUIManager.SetNewsContainer(_newsContainer);
            _sportsContainer = root.Q<VisualElement>("sports-scrollview");
            
            //Sudoku Set-up
            for (int i = 0; i < _sudokuCellData.Length; i++)
            {
                _sudokuCellData[i] = new SudokuCellData(0, 0, 0, 0, null, 0, 0);
            }
            var cells = root.Query<Button>(className: "sudoku-cell").ToList();
            foreach (var cell in cells)
            {
                string lastTwo = cell.name.Substring(cell.name.Length - 2, 2);
                int value = int.Parse(lastTwo);
                _sudokuCellData[value].ID = value;
                _sudokuCellData[value].UILabel = cell;
                int block = (value/ 9) + 1;
                
                // Row calculation (1-based)
                int row = ((block - 1) / 3) + 1;
                _sudokuCellData[value].Row = row;

                // Column calculation (1-based)
                int column = ((block - 1) % 3) + 1;
                _sudokuCellData[value].Column = column;
                
                // Cell calculation
                int cellNumber = (value % 9) + 1;
                _sudokuCellData[value].Cell = cellNumber;
            }
            _sudokuNumberSelectors = root.Query<Button>( className:"sudoku-number-selector").ToList();
            foreach (var selector in _sudokuNumberSelectors)
            {
                selector.RegisterCallback<PointerUpEvent>(_sudokuManager.NumberSelected);
            }
            _sudokuToggleButtonGroup = root.Q<ToggleButtonGroup>("sudoku-level-selector");
            _sudokuErrors = root.Query<Label>(className:"sudoku-error").ToList();
            _sudokuManager.SetSudokuErrors(_sudokuErrors);
            _sudokuToggleButtonGroup.RegisterValueChangedCallback(_sudokuManager.LevelSelected);

            _sudokuManager.SetSudokuCellData(_sudokuCellData);

            // Setup UI
            _structureElements.FindScreens(root, _mainScreen, _otherScreens);
            _structureElements.FindAppElements(root, _appElements, _wiggleEffect);
            _structureElements.AssignScreensToAppElements( _appElements,_otherScreens, this);
            SetUpSportsContainers();
            _structureElements.SetupBackButtons(_backButtons, _otherScreens, this, _wiggleEffect);
            _structureElements.SetupNavigationDropdowns(root, _navigationDropdowns ,_otherScreens, this);
        }

        private void SetUpSportsContainers()
        {
            // 
        }

        public void ShowScreen(string screenName)
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
                        StartCoroutine(_tvMazeManager.SearchShows(evt.newValue, _tvMazeUIManager.DisplayShows)));
                }
                else if (selectedScreen.name == "Screen03") // Sports
                {
                    if (_sportsContainer != null)
                    {
                        StartCoroutine(_sportsManager.GetSports(_sportsUIManager.PopulateSports));
                    }
                }
                else if (selectedScreen.name == "Screen04" ) // News
                {
                    if (_newsContainer != null)
                    {
                        StartCoroutine(_newsManager.GetNews(_newsUIManager.PopulateNews));
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
                _weatherManager.OnWeatherRetrieved -= _weatherUIManager.PopulateWeather;
            }
        }
    }
}
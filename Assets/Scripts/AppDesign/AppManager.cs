using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AppDesign
{
    public class AppManager : MonoBehaviour
    {
        // UI Elements
        private List<VisualElement> _appElements = new List<VisualElement>();
        private List<VisualElement> _otherScreens = new List<VisualElement>();
        private VisualElement _mainScreen ;
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
        private TriviaManager _triviaManager;
        private QuoteManager _quoteManager;
        private MoneyManager _moneyManager;
        private FunFactsManager _funFactManager;
        private DrawingManager _drawingManager;
        private ResponseGameManager _responseGameManager;
        private SimonGameManager _simonGameManager;

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
        private VisualElement _sudokuGameWon;
        private ScrollView _triviaScrollView;
        
        // Quote
        private Label _quoteDropCap, _quoteRestOfText, _quoteAuthor;
        
        //Money
        private ScrollView _moneyScrollview;
        private Label _moneyLastUpdated;
        
        //Funfacts
        private Label _funfactsText;
        private Label _funfactsSource;
        
        // Drawing Pad
        private VisualElement _drawingPad;
        private DropdownField _drawBrushSelector;
        private DropdownField _drawColorSelector;
        private Button _drawClearButton;
        
        // Response Game
        private VisualElement response_gameArea;
        private VisualElement response_gameDot;
        private Label response_slowTimeLabel, response_fastestTimeLabel, response_averageTimeLabel;
        private Button response_startButton;
        
        // Simon Game
        private VisualElement simon_gameBoard;
        private Button simon_startButton;
        private List<Button> simon_gameTiles;
        private Label simon_playerTurnLabel;

        void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (!_uiDocument)
            {
                Debug.LogError("No UIDocument component found on this GameObject.");
                return;
            }

            var root = _uiDocument.rootVisualElement;
            _mainScreen = new VisualElement();
            _otherScreens = new List<VisualElement>();

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
            _triviaManager = GetComponent<TriviaManager>() ?? gameObject.AddComponent<TriviaManager>();
            _sudokuManager = GetComponent<SudokuManager>() ?? gameObject.AddComponent<SudokuManager>();
            _quoteManager = GetComponent<QuoteManager>() ?? gameObject.AddComponent<QuoteManager>();
            _moneyManager = GetComponent<MoneyManager>() ?? gameObject.AddComponent<MoneyManager>();
            _funFactManager = GetComponent<FunFactsManager>() ?? gameObject.AddComponent<FunFactsManager>();
            _drawingManager = GetComponent<DrawingManager>() ?? gameObject.AddComponent<DrawingManager>();
            _responseGameManager = GetComponent<ResponseGameManager>() ?? gameObject.AddComponent<ResponseGameManager>();
            _simonGameManager = GetComponent<SimonGameManager>() ?? gameObject.AddComponent<SimonGameManager>();

            // Find UI containers
            #region Weather
            _weatherSearch = root.Q<TextField>("WeatherSearchField");
            _weatherSubmitButton = root.Q<Label>("WeatherSubmitButton");
            _weatherSubmitButton?.RegisterCallback<PointerUpEvent>(evt =>
            {
                StartCoroutine(_weatherManager.GetWeather(_weatherSearch.value));
            });

            _weatherContainer = root.Q<VisualElement>("WeatherContainer");
            _weatherUIManager.SetWeatherContainer(_weatherContainer);
            #endregion Weather
            
            #region News
            _newsContainer = root.Q<VisualElement>("NewsContainer");
            _newsUIManager.SetNewsContainer(_newsContainer);
            #endregion News
            
            #region Sports
            _sportsContainer = root.Q<VisualElement>("sports-scrollview");
            _sportsUIManager.SetSportsContainer(_sportsContainer);
            #endregion Sports

            #region Sudoku
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
            _sudokuGameWon = root.Q<VisualElement>(className:"sudoku-game-won");
            _sudokuGameWon.visible = false;
            _sudokuManager.SetSudokuGameWon(_sudokuGameWon);

            _sudokuManager.SetSudokuCellData(_sudokuCellData);
            #endregion Sudoku

            #region Trivia
            _triviaScrollView = root.Q<ScrollView>("trivia-scrollview");
            _triviaManager.SetTriviaScrollview(_triviaScrollView);
            #endregion
            
            #region Quote
            _quoteDropCap = root.Q<Label>(className:"quote-drop-cap");
            _quoteRestOfText = root.Q<Label>(className:"quote-restOfText");
            _quoteAuthor = root.Q<Label>(className:"quote-author");
            _quoteManager.SetUIElements(_quoteDropCap, _quoteRestOfText, _quoteAuthor);
            _quoteManager.QuoteStart();
            #endregion
            
            #region Money
            _moneyScrollview = root.Q<ScrollView>("money-scrollview");
            _moneyLastUpdated = root.Q<Label>("money-update-time");
            _moneyManager.SetMoneyScrollview(_moneyScrollview, _moneyLastUpdated);
            #endregion
            
            #region FunFacts
            _funfactsText = root.Q<Label>(className: "funfacts-text");
            _funfactsSource = root.Q<Label>(className: "funfacts-source");
            _funFactManager.SetLabels(_funfactsText,  _funfactsSource);
            #endregion
            
            #region Drawing Pad
            _drawingPad = root.Q<VisualElement>(className: "drawing-pad");
            _drawBrushSelector = root.Q<DropdownField>(className: "draw-brush-selector");
            _drawColorSelector = root.Q<DropdownField>(className: "draw-color-selector");
            _drawClearButton = root.Q<Button>(className: "draw-clear-button");
            _drawingManager.SetDrawingElements(_drawingPad,_drawBrushSelector, _drawColorSelector, _drawClearButton );
            #endregion Drawing Pad

            #region Response Game
            response_gameArea = root.Q<VisualElement>("response_gameSpace");
            response_gameDot = root.Q<Button>("response_gameDot");
            response_slowTimeLabel = root.Q<Label>("response_slowestScore");
            response_fastestTimeLabel = root.Q<Label>("response_fastestScore");
            response_averageTimeLabel = root.Q<Label>("response_averageScore");
            response_startButton = root.Q<Button>("response_startButton");
            _responseGameManager.SetResponseGameElements(response_gameArea, response_gameDot, response_slowTimeLabel, response_fastestTimeLabel, response_averageTimeLabel, response_startButton);
            #endregion 
            
            #region Simon Game
            simon_gameBoard = root.Q<VisualElement>("simon_gameBoard");
            simon_startButton = root.Q<Button>("simon_startButton");
            simon_gameTiles = root.Query<Button>(className: "simon_gameTile").ToList();
            simon_playerTurnLabel = root.Q<Label>("simon_playerTurnLabel");
            _simonGameManager.SetGameElements(simon_gameBoard, simon_startButton, simon_gameTiles, simon_playerTurnLabel);
            #endregion
            
            // Setup UI
            _structureElements.FindScreens(root, _mainScreen, _otherScreens);
            _structureElements.FindAppElements(root, _appElements, _wiggleEffect);
            _structureElements.AssignScreensToAppElements( _appElements,_otherScreens, this);
            
            _structureElements.SetupBackButtons(_backButtons, _otherScreens, this, _wiggleEffect);
            _structureElements.SetupNavigationDropdowns(root, _navigationDropdowns ,_otherScreens, this);
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
                else if (selectedScreen.name == "Screen08") // Trivia
                {
                    _triviaManager.TriviaStart();
                }
                else if (selectedScreen.name == "Screen11") // Trivia
                {
                    _funFactManager.FunFactsStart();
                }
                else if (selectedScreen.name == "Screen12") // Drawing Pad
                {
                    _drawingManager.DrawingStart();
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
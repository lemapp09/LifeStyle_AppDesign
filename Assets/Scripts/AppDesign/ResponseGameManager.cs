using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class ResponseGameManager : MonoBehaviour
{
    private VisualElement response_gameArea;
    private VisualElement response_gameDot;
    private Label response_slowTimeLabel, response_fastestTimeLabel, response_averageTimeLabel;
    private Button response_startButton;
    
    private List<float> _clickTimes = new List<float>();
    private float _roundStartTime;
    private int _currentRound = 0, _totalRounds = 5;
    private System.Random _rng = new System.Random();
    private bool _isPlaying;

    public void SetResponseGameElements(VisualElement gameArea, VisualElement circle, Label slowTimeLabel, 
        Label fastTimeLabel, Label averageTimeLabel, Button startButton)
    {
        response_gameArea = gameArea;
        response_gameDot = circle;
        response_gameDot.visible = false;
        response_slowTimeLabel = slowTimeLabel;
        response_fastestTimeLabel = fastTimeLabel;
        response_averageTimeLabel = averageTimeLabel;
        response_startButton = startButton;
        response_startButton?.RegisterCallback<ClickEvent>(StartNewGame);
        response_gameDot?.RegisterCallback<ClickEvent>(evt => OnCircleClicked());
    }

    private void StartNewGame(ClickEvent evt)
    {
        _isPlaying = true;
        response_gameDot.visible = true;
        _currentRound = 0;
        _totalRounds = 5;
        _clickTimes.Clear();
        response_slowTimeLabel.text = "";
        response_fastestTimeLabel.text = "";
        response_averageTimeLabel.text = "";

        _roundStartTime = Time.realtimeSinceStartup;
        response_slowTimeLabel.text = "Time: 0.00s";
        
        StartNextRound();
    }

    private void StartNextRound()
    {
        // Random position (keep within bounds)
        float maxX = response_gameArea.resolvedStyle.width - 120;
        float maxY = response_gameArea.resolvedStyle.height - 120;
        float posX = (float)_rng.NextDouble() * maxX;
        float posY = (float)_rng.NextDouble() * maxY;
        response_gameDot.style.left = posX;
        response_gameDot.style.top = posY;
    }

    private void OnCircleClicked()
    {
        float clickTime = Time.realtimeSinceStartup - _roundStartTime;
        _clickTimes.Add(clickTime);

        _currentRound++;
        if (_currentRound < _totalRounds)
        {
            StartNextRound();
        }
        else
        {
            _isPlaying = false;
            ShowResults();
        }
    }

    private void Update()
    {
        if (response_gameDot != null && _isPlaying)
        {
            float elapsed = Time.realtimeSinceStartup - _roundStartTime;
            response_slowTimeLabel.text = $"Time: {elapsed:F2}s";
        }
    }

    private void ShowResults()
    {
        float min = _clickTimes.Min();
        float max = _clickTimes.Max();
        float avg = _clickTimes.Average();
        response_fastestTimeLabel.text = $"Fastest: {min:F2}s";
        response_averageTimeLabel.text = $"Average: {avg:F2}s";
        response_slowTimeLabel.text = $"Slowest: {max:F2}s";
        // Remove or hide the response_gameDot if needed
        response_gameDot.visible = false;
    }

    private void OnDisable()
    {
        response_startButton.UnregisterCallback<ClickEvent>(StartNewGame);
    }
}

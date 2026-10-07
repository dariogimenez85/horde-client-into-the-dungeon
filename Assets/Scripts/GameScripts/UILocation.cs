using System;
using UnityEngine;

public class UILocation : MonoBehaviour
{
    float screenWidth;

    [SerializeField] private RectTransform rectScore;
    [SerializeField] private RectTransform rectTime;
    [SerializeField] private RectTransform rectLives;

    private Vector2 rectScoreDefaultPosition;
    private Vector2 rectTimeDefaultPosition;
    private Vector2 rectLivesDefaultPosition;

    private Vector2 lastSize;
    private RectTransform rt;

    private void OnEnable()
    {
        GameBridge.OnOrientationChange += UIPositions;
    }

    private void OnDisable()
    {
        GameBridge.OnOrientationChange -= UIPositions;
    }

    private void UIPositions(string orientation = "landscape")
    {
        screenWidth = GameBridge.instance.canvasSize.x;

        if (orientation == "portrait")
        {
            print("pantalla angosta");
            rectScore.anchoredPosition = new Vector2(-207, -150);
            rectTime.anchoredPosition = new Vector2(207, -150);
        }
        else
        {
            print("pantalla ancha");
            rectScore.anchoredPosition = rectScoreDefaultPosition;
            rectTime.anchoredPosition = rectTimeDefaultPosition;
            rectLives.anchoredPosition = rectLivesDefaultPosition;
        }
    }


    private void Awake()
    {
        rectScoreDefaultPosition = new Vector2(320, 0);
        rectTimeDefaultPosition = new Vector2(-320, 0);
        rectLivesDefaultPosition = Vector2.zero;
    }
}
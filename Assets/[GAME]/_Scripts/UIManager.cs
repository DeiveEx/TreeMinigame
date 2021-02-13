using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text feedbackText;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        //Add a handler for when a round is finished
        gameManager.roundFinished += (sender, e) =>
        {
            ShowFeedBackText("Round Finished");
        };
    }

    private void Start()
    {
        feedbackText.text = string.Empty;
    }

    public void RemovePiece()
    {
        gameManager.RemovePiece();
    }

    public void GenerateNewTree()
    {
        gameManager.GenerateNewTree();
    }

    public void StartNewRound()
    {
        gameManager.StartNewRound();
    }

    private void ShowFeedBackText(string message)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        StopAllCoroutines();
        StartCoroutine(DisableFeedbackTextRoutine());
    }

    private IEnumerator DisableFeedbackTextRoutine()
    {
        yield return new WaitForSeconds(1);
        feedbackText.gameObject.SetActive(false);
    }
}

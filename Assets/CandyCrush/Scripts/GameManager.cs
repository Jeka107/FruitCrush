using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private GameObject victoryLabel;
    [SerializeField] private GameObject loseLabel;
    [SerializeField] private GameObject border;

    public int goal; //the amount of point for win.
    public int moves; //the numer of turn you can take.
    public int points; //current points.

    public bool isGameEnded = false;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI movesText;

    private void Awake()
    {
        Instance = this;

        pointsText.text = points.ToString() + "/" + goal.ToString();
        movesText.text = moves.ToString();
    }
    public void Initialize(int _moves,int _goal)
    {
        moves = _moves;
        goal = _goal;
    }
    public void ProcessTurn(int _pointsToGain,bool _substractMoves)
    {
        points += _pointsToGain;

        if (_substractMoves)
            moves--;

        if(points>=goal)//won the game.
        {
            isGameEnded = true;

            //display victory Label
            backgroundPanel.SetActive(true);
            victoryLabel.SetActive(true);
            border.SetActive(false);
            ShapeBoard.instance.gameObject.SetActive(false);
            return;
        }
        if(moves==0)//lose the game.
        {
            isGameEnded = true;

            //display lose Label
            backgroundPanel.SetActive(true);
            loseLabel.SetActive(true);
            border.SetActive(false);
            ShapeBoard.instance.gameObject.SetActive(false);
            return;
        }
        pointsText.text = points.ToString() + "/" + goal.ToString();
        movesText.text = moves.ToString();
    }
    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }
    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
}

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Scrambler
{
    public class GameUiController : MonoBehaviour
    {
        #region FIELDS INSPECTOR
        [Header("HUD")]
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private TextMeshProUGUI _hudScore;

        [Header("WIN")]
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private TextMeshProUGUI _winBestScore;
        [SerializeField] private TextMeshProUGUI _winCurrentScore;
        [SerializeField] private ParticleSystem[] _confettis;
        #endregion

        #region UNITY CALLBACKS
        private void Start()
        {
            GameManager.OnScoreChange += ScoreChangeHandler;
            GameManager.OnGameStateChange += GameStateChangeHandler;
        }

        private void OnDestroy()
        {
            GameManager.OnScoreChange -= ScoreChangeHandler;
            GameManager.OnGameStateChange -= GameStateChangeHandler;
        }
        #endregion

        #region METHODS PRIVATE
        private void ScoreChangeHandler(int score)
        {
            DOVirtual.Int(Int32.Parse(_hudScore.text), score, 1f, (v) => {
                _hudScore.text = $"{v}";
                _winCurrentScore.text = $"LEVEL SCORE:\r\n{v}";
            }).SetEase(Ease.Linear);
        }

        private void GameStateChangeHandler(GameState gameState)
        {
            if (gameState == GameState.Win)
            {
                _hudPanel.GetComponentsInChildren<Button>().ToList().ForEach(i => i.interactable = false);
                ShowWinPanel();
            }
        }

        private void ShowWinPanel()
        {
            var bestScore = PlayerPrefs.GetInt("best-score");
            _winBestScore.text = $"BEST SCORE:\r\n{bestScore}";

            _winPanel.SetActive(true);

            StartCoroutine(LaunchFireworks());
        }
        #endregion

        #region METHODS PUBLIC
        public void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ResetLastLetter()
        {
            GameManager.ResetLastLetter();
        }

        public void BombWord()
        {
            GameManager.DestroyLastRow();
        }

        public void MixLetters()
        {
            GameManager.MixBones();
        }

        public void CheckWord()
        {

            GameManager.CheckWord();
        }
        #endregion

        #region COROUTINES
        IEnumerator LaunchFireworks()
        {
            foreach (var confetti in _confettis)
            {
                yield return new WaitForSeconds(0.5f);
                confetti.Play();
            }
        }
        #endregion
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class HudManagerUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] private Button pauseButton;
    [SerializeField] private WindowUI pauseWindowUI;

    [SerializeField] private Image timerFilledImage;

    private void Awake() {
        GameManager.OnTick += GameManager_OnTick;
        Player.OnAnyMatchStateBefore += Player_OnAnyMatchStateBefore;
    }

    private void OnDestroy() {
        GameManager.OnTick -= GameManager_OnTick;
        Player.OnAnyMatchStateBefore -= Player_OnAnyMatchStateBefore;
    }

    private void Start() {
        UpdateTimerText();
        UpdateTimerImage(1f);
        UpdateScoreText();

        pauseButton.onClick.AddListener(() => {
            GameManager.Pause();
            pauseWindowUI.TransitionShow();
        });
    }

    private void GameManager_OnTick() {
        UpdateTimerText();
        UpdateTimerImage(GameManager.Instance.GetNormalziedPassedTick());
    }

    private void Player_OnAnyMatchStateBefore(object sender, Player.OnAnyMatchStateChangeEventArgs args) {
        UpdateScoreText();
    }

    private void UpdateTimerText() {
        int tick = GameManager.Instance.GetTick();
        int sec = TimeSegment.GetSecond(tick);
        int min = TimeSegment.GetMinute(tick);
        timerText.text = $"{min:00}:{sec:00}";
    }

    private void UpdateTimerImage(float value) {
        timerFilledImage.fillAmount = value;
    }

    private void UpdateScoreText() {
        scoreText.text = Player.Instance.GetCorrectGuess().ToString();
    }
}

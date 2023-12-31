using GameManagement;
using ResourceItem;
using Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class LoadSceneMainWindowUI : WindowUI {
    [Header("Card Count UI")]
    [SerializeField] private CardCountUI cardCountUITemplate;
    [SerializeField] private Transform cardCountContainer;

    [Header("Card Type UI")]
    [SerializeField] private List<CardTypeUI> cardTypeList;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button difficultyButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;


    private int difficultyIndex = 1;
    private Dictionary<int, LevelDataSO> id2LevelSO;
    private HashSet<CardCountUI> cardUISet;
    private List<LevelDataSO> sortedLevelSOList;

    private CardCountUI selectedCardUI;

    private void Start() {
        id2LevelSO = DatabaseUtility.Build<LevelDataSO>("LevelDataSO/");
        cardUISet = new();

        AddBehaviour(startButton, difficultyButton, closeButton, musicSlider, soundSlider);

        SetupCards();
        SetupButtons();
        SetupSliders();

        this.ImmediateShow();
    }


    public void SelectCardCountUI(CardCountUI selectedCardUI) {
        foreach (CardCountUI cardUI in cardUISet) {
            cardUI.HideOutline();
        }

        selectedCardUI.ShowOutline();
        this.selectedCardUI = selectedCardUI;
    }


    public void SelectCardTypeUI(CardTypeUI cardTypeUI) {
        if (cardTypeUI.GetActive() && !IsOnlyActiveOne(cardTypeUI)) {
            cardTypeUI.Hide();
        } else {
            cardTypeUI.Show();
        }
    }

    private bool IsOnlyActiveOne(CardTypeUI selectedCardTypeUI) {
        int activeCount = 0;
        CardTypeUI lastActiveOne = null;

        foreach (CardTypeUI cardUI in cardTypeList) {
            if (cardUI.GetActive()) {
                lastActiveOne = cardUI;
                activeCount++;
            }
        }

        if (activeCount > 1) {
            return false;
        }

        return selectedCardTypeUI == lastActiveOne;
    }

    private IEnumerable<LevelDataSO> GetLevelSOEnumerable() => sortedLevelSOList;

    private void SetupCards() {
        cardCountUITemplate.gameObject.SetActive(false);

        foreach (Transform t in cardCountContainer) {
            if (t.gameObject == cardCountUITemplate.gameObject) { continue; }
            Destroy(t.gameObject);
        }

        sortedLevelSOList = id2LevelSO.Values.OrderBy(l => l.count).ToList();

        foreach (LevelDataSO levelSO in GetLevelSOEnumerable()) {
            CardCountUI cardUI = Instantiate(cardCountUITemplate, cardCountContainer);
            cardUI.transform.ResetLocalTransform();
            cardUI.Initialize(this, levelSO);
            cardUI.gameObject.SetActive(true);
            cardUISet.Add(cardUI);
        }

        foreach (CardTypeUI cardUI in cardTypeList) {
            cardUI.Initialize(this);
        }

        SelectCardCountUI(cardUISet.First());
        SelectCardTypeUI(cardTypeList.First());
    }

    private void SetupButtons() {
        startButton.onClick.AddListener(() => {


            PlayerPrefs.SetInt(SaveID.CardConfigID, selectedCardUI.GetLevelSO().Id);

            foreach (var card in cardTypeList) {
                PlayerPrefs.SetInt(card.GetSaveID(), card.GetActive() ? 1 : 0);
            }

            PlayerPrefs.Save();

            if (LoadSceneManager.Instance.IsFreeToLoad()) {
                DisableUIElements();
                LoadSceneManager.Instance.LoadScene("MainScene");
            }
        });

        closeButton.onClick.AddListener(() => {
            if (!Application.isEditor) {
                DisableUIElements();
            }
            Application.Quit();
        });

        TextMeshProUGUI diffBtnText = difficultyButton.GetComponentInChildren<TextMeshProUGUI>();
        difficultyIndex = PlayerPrefs.GetInt(SaveID.GameDifficulty, difficultyIndex);
        diffBtnText.text = ((GameMode) difficultyIndex).ToString();

        difficultyButton.onClick.AddListener(() => {
            int enumLength = Enum.GetValues(typeof(GameMode)).Length;
            difficultyIndex = (difficultyIndex + 1) % enumLength;
            PlayerPrefs.SetInt(SaveID.GameDifficulty, difficultyIndex);
            diffBtnText.text = ((GameMode) difficultyIndex).ToString();
        });
    }

    private void SetupSliders() {
        musicSlider.value = PlayerPrefs.GetFloat(SaveID.Music, 1f);
        soundSlider.value = PlayerPrefs.GetFloat(SaveID.Sound, 1f);

        int musicHash = SoundManager.NameToHash("Music");
        int soundHash = SoundManager.NameToHash("Player");

        SoundManager.Instance.SetMixerVolume(musicHash, musicSlider.value);
        SoundManager.Instance.SetMixerVolume(soundHash, soundSlider.value);

        musicSlider.onValueChanged.AddListener((value) => {
            PlayerPrefs.SetFloat(SaveID.Music, value);
            SoundManager.Instance.SetMixerVolume(musicHash, value);
            PlayerPrefs.Save();
        });

        soundSlider.onValueChanged.AddListener((value) => {
            PlayerPrefs.SetFloat(SaveID.Sound, value);
            SoundManager.Instance.SetMixerVolume(soundHash, value);
            PlayerPrefs.Save();
        });
    }
}

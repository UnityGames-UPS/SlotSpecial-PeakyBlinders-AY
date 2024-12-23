using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using System.Text;
public class UIManager : MonoBehaviour
{

    [Header("AutoSpin Popup")]
    [SerializeField] private Button AutoSpinButton;
    [SerializeField] private Button AutoSpinPopUpClose;
    [SerializeField] private TMP_Text autoSpinCost;
    [SerializeField] private GameObject autoSpinPopupObject;

    [Header("Bets Popup")]
    [SerializeField] private Button betButton;
    [SerializeField] private Button[] betValueButtons;
    [SerializeField] private TMP_Text[] betTexts;
    [SerializeField] private GameObject betPopupObject;
    [SerializeField] private Button BetPopupClose;

    [Header("Free Spin Popup")]
    [SerializeField] private GameObject FreeSPinPopUpObject;
    [SerializeField] private TMP_Text FreeSpinCount;
    [SerializeField] private GameObject freeSpinBg;
    [SerializeField] private Image[] purpleBar;

    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;

    [Header("Paytable Popup")]
    [SerializeField] private Button paytable_Button;
    [SerializeField] private GameObject payTablePopup_Object;
    [SerializeField] private Button paytableExit_Button;


    [Header("Paytable Texts")]
    [SerializeField] private TMP_Text[] SymbolsText;
    [SerializeField] private TMP_Text[] BonusSymbolsText;
    [SerializeField] private TMP_Text Wild_Text;
    [SerializeField] private TMP_Text[] mini_grand_text;



    [Header("Settings Popup")]
    [SerializeField] private GameObject SettingsPopup_Object;
    [SerializeField] internal Button Settings_Button;
    [SerializeField] private Button SettingsExit_Button;
    [SerializeField] private Button SoundToggle_button;
    [SerializeField] private Button MusicToggle_button;
    [SerializeField] private Sprite empty;
    [SerializeField] private Sprite button;
    private bool isMusic = true;
    private bool isSound = true;

    [Header("all Win Popup")]
    [SerializeField] private GameObject specialWinObject;
    [SerializeField] private Image specialWinTitle;
    [SerializeField] private Sprite[] winTitleSprites;
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;
    [SerializeField] private Button cancelWinButton;


    [Header("low balance popup")]
    [SerializeField] private GameObject LowBalancePopup_Object;
    [SerializeField] private Button Close_Button;


    [Header("disconnection popup")]
    [SerializeField] private GameObject DisconnectPopup_Object;
    [SerializeField] private Button CloseDisconnect_Button;

    [Header("Quit Popup")]
    [SerializeField] private GameObject QuitPopupObject;
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private Button no_Button;
    [SerializeField] private Button yes_Button;

    [Header("Splash Screen")]
    [SerializeField] private GameObject spalsh_screen;
    [SerializeField] private Image progressbar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("AnotherDevice Popup")]
    [SerializeField] private Button CloseAD_Button;
    [SerializeField] private GameObject ADPopup_Object;

    [Header("free spin info")]

    [SerializeField] private TMP_Text freeSpinInfo;
    [SerializeField] private TMP_Text freeSpinWinnings;

    [SerializeField] private GameObject freeSpinPanel;
    [SerializeField] private GameObject gameButtonPanel;
    [SerializeField] private Transform freeSpinText;

    [SerializeField]
    private Button m_AwakeGameButton;

    private bool isExit = false;

    [Header("player texts")]
    [SerializeField] private TMP_Text playerCurrentWinning;
    [SerializeField] private TMP_Text playerBalance;

    private GameObject currentPopup;

    internal Action<bool, string> ToggleAudio;
    internal Action<string> playButtonAudio;

    internal Action OnExit;

    Tweener normalWinTween;
    private void Awake()
    {
        //if (spalsh_screen) spalsh_screen.SetActive(true);
        //StartCoroutine(LoadingRoutine());
        SimulateClickByDefault();
    }

    private void SimulateClickByDefault()
    {
        Debug.Log("Awaken The Game...");
        m_AwakeGameButton.onClick.AddListener(() => { Debug.Log("Called The Game..."); });
        m_AwakeGameButton.onClick.Invoke();
    }

    private void Start()
    {
        // Set up each button with the appropriate action
        SetButton(yes_Button, CallOnExitFunction);
        SetButton(no_Button, () => { if (!isExit) { ClosePopup(); } });
        SetButton(GameExit_Button, () => OpenPopup(QuitPopupObject));
        SetButton(paytable_Button, () => OpenPopup(payTablePopup_Object));
        SetButton(paytableExit_Button, () => ClosePopup());
        SetButton(Settings_Button, () => OpenPopup(SettingsPopup_Object));
        SetButton(SettingsExit_Button, () => ClosePopup());

        SetButton(MusicToggle_button, ToggleMusic);
        SetButton(SoundToggle_button, ToggleSound);

        SetButton(CloseDisconnect_Button, CallOnExitFunction);
        SetButton(Close_Button, ClosePopup);
        SetButton(QuitSplash_button, () => OpenPopup(QuitPopupObject));
        SetButton(AutoSpinButton, () => OpenPopup(autoSpinPopupObject));
        SetButton(betButton, () => OpenPopup(betPopupObject));
        SetButton(AutoSpinPopUpClose, () => ClosePopup());
        SetButton(BetPopupClose, () => ClosePopup());
        // Initialize other settings
        isMusic = false;
        isSound = false;
        ToggleMusic();
        ToggleSound();
    }

    private void SetButton(Button button, Action action)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            playButtonAudio?.Invoke("default");
            action?.Invoke();

        });
    }



    // private IEnumerator LoadingRoutine()
    // {
    //     StartCoroutine(LoadingTextAnimate());
    //     float fillAmount = 0.7f;
    //     progressbar.DOFillAmount(fillAmount, 2f).SetEase(Ease.Linear);
    //     yield return new WaitForSecondsRealtime(2f);
    //     yield return new WaitUntil(() => !socketManager.isLoading);
    //     progressbar.DOFillAmount(1, 1f).SetEase(Ease.Linear);
    //     yield return new WaitForSecondsRealtime(1f);
    //     if (spalsh_screen) spalsh_screen.SetActive(false);
    //     StopCoroutine(LoadingTextAnimate());
    // }



    internal void UpdatePlayerInfo(PlayerData playerData)
    {
        playerCurrentWinning.text = playerData.currentWining.ToString();
        playerBalance.text = playerData.Balance.ToString("f4");

    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (loadingText) loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.5f);
            if (loadingText) loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.5f);
            if (loadingText) loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.5f);
        }
    }


    internal void UpdateAutoSpinCost(double cost)
    {
        autoSpinCost.text = cost.ToString();
    }
    internal void LowBalPopup()
    {

        OpenPopup(LowBalancePopup_Object);
    }

    internal void ToggleFreeSpinPanel(bool toggle)
    {
        freeSpinPanel.SetActive(toggle);
        gameButtonPanel.SetActive(!toggle);

    }




    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    internal void PopulateSymbolsPayout(UIData uIData)
    {
        string text;
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            text = "";
            for (int j = 0; j < uIData.symbols[i].Multiplier.Count; j++)
            {
                text += $"{5 - j}x - {uIData.symbols[i].Multiplier[j][0]}x \n";
            }
            SymbolsText[i].text = text;

        }

        // Wild_Text.text = uIData.symbols[10].description.ToString();

        for (int i = 0; i < uIData.specialBonusSymbolMulipliers.Count; i++)
        {
            text = "";
            if (uIData.specialBonusSymbolMulipliers[i].name == "Mini Multiplier")
            {
                mini_grand_text[0].text = uIData.specialBonusSymbolMulipliers[i].value.ToString();
            }
            else if (uIData.specialBonusSymbolMulipliers[i].name == "Major Multiplier")
            {
                mini_grand_text[1].text = uIData.specialBonusSymbolMulipliers[i].value.ToString();
            }
            else if (uIData.specialBonusSymbolMulipliers[i].name == "Mega Multiplier")
            {
                mini_grand_text[2].text = uIData.specialBonusSymbolMulipliers[i].value.ToString();
            }
            else if (uIData.specialBonusSymbolMulipliers[i].name == "Grand Multiplier")
            {
                mini_grand_text[3].text = uIData.specialBonusSymbolMulipliers[i].value.ToString();
            }


        }

    }

    internal void PopulateBets(List<double> bets, Action<int> onclick)
    {

        for (int i = 0; i < betValueButtons.Length; i++)
        {
            if (i < bets.Count)
            {
                int index = i;
                betTexts[index].text = bets[index].ToString();
                betValueButtons[index].onClick.AddListener(() =>
                {
                    onclick(index);
                    ClosePopup();
                });
            }
            else
            {
                betValueButtons[i].gameObject.SetActive(false);
            }
        }

    }

    private void CallOnExitFunction()
    {
        isExit = true;
        OnExit?.Invoke();
        // audioController.PlayButtonAudio();
        // socketManager.CloseSocket();
    }

    private void OpenPopup(GameObject Popup)
    {
        if (currentPopup != null && !DisconnectPopup_Object.activeSelf)
        {
            ClosePopup();
        }
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        currentPopup = Popup;
        // paytableList[CurrentIndex].SetActive(true);
    }

    internal void ClosePopup()
    {
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (currentPopup != null)
            {
                currentPopup.SetActive(false);
                if (MainPopup_Object) MainPopup_Object.SetActive(false);

                currentPopup = null;
            }

        }
    }





    internal void FreeSpinPopup(int amount, GameObject Bg)
    {
        FreeSpinCount.text = amount.ToString();
        OpenPopup(FreeSPinPopUpObject);
        if (Bg)
            Bg.SetActive(true);
    }

    internal void CloseFreeSpinPopup(GameObject Bg)
    {
        ClosePopup();
        FreeSpinCount.text = "0";
        if (Bg && Bg.activeSelf)
            Bg.SetActive(false);
    }
    internal void EnableWinPopUp(int value)
    {

        OpenPopup(WinPopup_Object);
        if (value > 0)
            specialWinObject.SetActive(true);

        switch (value)
        {
            case 1:
                specialWinTitle.sprite = winTitleSprites[0];
                break;
            case 2:
                specialWinTitle.sprite = winTitleSprites[1];
                break;
            case 3:
                specialWinTitle.sprite = winTitleSprites[2];
                break;
        }
    }

    internal void DeductBalanceAnim(double finalAmount, double initAmount)
    {

        DOTween.To(() => initAmount, (val) => initAmount = val, finalAmount, 0.8f).OnUpdate(() =>
        {
            playerBalance.text = initAmount.ToString("f4");

        }).OnComplete(() =>
        {

            playerBalance.text = finalAmount.ToString();
        });
    }

    internal IEnumerator WinTextAnim(double amount)
    {
        double initAmount = 0;
        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 2f).OnUpdate(() =>
        {
            Win_Text.text = initAmount.ToString("f3");

        }).OnComplete(() =>
        {
            Win_Text.text = amount.ToString();
        });

        yield return new WaitForSeconds(3f);
        ClosePopup();
        if (specialWinObject.activeSelf)
            specialWinObject.SetActive(false);
            
        yield return null;



    }


    internal void NormalWinAnimation(){
            normalWinTween=playerCurrentWinning.transform.DOScale(1.2f,1f).SetLoops(-1,LoopType.Yoyo);
    }
    internal void StopNormalWinAnimation(){
            normalWinTween.Kill();
            playerCurrentWinning.transform.localScale= Vector2.one;
    }
    internal void DisconnectionPopup()
    {
        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }
    }

    internal void UpdateFreeSpinInfo(int freespinCount = -1, double winnings = -1)
    {
        if (freespinCount >= 0)
            freeSpinInfo.text = freespinCount.ToString();
        if (winnings >= 0)
            freeSpinWinnings.text = winnings.ToString();
    }

    private void ToggleMusic()
    {
        isMusic = !isMusic;
        if (isMusic)
        {
            MusicToggle_button.image.sprite = button;

            ToggleAudio?.Invoke(false, "bg");
        }
        else
        {
            MusicToggle_button.image.sprite = empty;
            ToggleAudio?.Invoke(true, "bg");
        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            SoundToggle_button.image.sprite = button;
            ToggleAudio?.Invoke(false, "button");
            ToggleAudio?.Invoke(false, "wl");
        }
        else
        {
            SoundToggle_button.image.sprite = empty;
            ToggleAudio?.Invoke(true, "button");
            ToggleAudio?.Invoke(true, "wl");
        }
    }

}

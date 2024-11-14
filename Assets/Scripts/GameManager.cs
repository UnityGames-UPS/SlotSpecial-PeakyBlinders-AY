using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
public class GameManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private SlotController slotManager;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private SocketController socketController;
    [SerializeField] private AudioController audioController;
    [SerializeField] private PaylineController PayLineCOntroller;

    [Header("For spins")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button Maxbet_button;
    [SerializeField] private Button BetMinus_Button;
    [SerializeField] private Button BetPlus_Button;
    [SerializeField] private Button ToatlBetMinus_Button;
    [SerializeField] private Button TotalBetPlus_Button;
    [SerializeField] private TMP_Text betPerLine_text;
    [SerializeField] private TMP_Text totalBet_text;
    [SerializeField] private bool isSpinning;
    [SerializeField] private TMP_Text gameStateText;

    [Header("For auto spins")]
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button AutoSpinPopup_Button;
    [SerializeField] private TMP_Text AUtoSpinCountText;
    [SerializeField] TMP_Dropdown autoSpinDropDown;
    [SerializeField] TMP_Dropdown betPerLineDropDown;
    [SerializeField] private bool isAutoSpin;
    [SerializeField] private int autoSpinCounter;
    [SerializeField] private TMP_Text autoSpinText;
    List<int> autoOptions = new List<int>() { 15, 20, 25, 30, 40, 100 };


    [Header("For Gamble")]
    [SerializeField] private Button Double_Button;
    [SerializeField] private Button Head_option;
    [SerializeField] private Button Tail_Option;
    [SerializeField] private Button Collect_Option;
    [SerializeField] private Button allGambleButton;
    [SerializeField] private Button halfGambleButton;
    [SerializeField] private GameObject gambleObject;
    [SerializeField] private Transform coinBlast;
    [SerializeField] private ImageAnimation coinAnim;
    [SerializeField] private string gambleOption;
    [SerializeField] private int gambleChance;
    private double bank;

    [SerializeField] private Sprite headImage;
    [SerializeField] private Sprite tailImage;

    [SerializeField] private bool isFreeSpin;
    [SerializeField] private bool freeSpinStarted;
    [SerializeField] private double currentBalance;
    [SerializeField] private double currentTotalBet;
    [SerializeField] private int betCounter = 0;

    private Coroutine autoSpinRoutine;
    private Coroutine freeSpinRoutine;
    private Coroutine iterativeRoutine;
    [SerializeField] private int wildPosition;
    [SerializeField] private int maxIterationWinShow;
    [SerializeField] private int winIterationCount;

    [SerializeField] private int freeSpinCount;

    [SerializeField] private List<ImageAnimation> VHcomboList;

    [SerializeField] private bool turboMode;
    [SerializeField] private Button Turbo_button;

    private bool initiated;

    void Start()
    {
        SetButton(SlotStart_Button, ExecuteSpin, true);
        SetButton(AutoSpin_Button, () =>
        {
            ExecuteAutoSpin();
            uIManager.ClosePopup();
        }, true);
        SetButton(AutoSpinStop_Button, () => StartCoroutine(StopAutoSpinCoroutine()));
        SetButton(BetPlus_Button, () => OnBetChange(true));
        SetButton(BetMinus_Button, () => OnBetChange(false));
        SetButton(ToatlBetMinus_Button, () => OnBetChange(false));
        SetButton(TotalBetPlus_Button, () => OnBetChange(true));
        SetButton(Maxbet_button, MaxBet);
        SetButton(Double_Button, OnInitGamble);
        SetButton(Head_option, () => StartCoroutine(OnSelectGamble("HEAD")));
        SetButton(Tail_Option, () => StartCoroutine(OnSelectGamble("TAIL")));
        SetButton(Collect_Option, () => StartCoroutine(OnGambleCollect()));

        SetButton(allGambleButton, () => changeGambleType(true));
        SetButton(halfGambleButton, () => changeGambleType(false));

        // autoSpinDropDown.onValueChanged.AddListener((int index) =>
        // {
        //     autoSpinCounter = index;
        //     CalculateCost();
        // });

        // betPerLineDropDown.onValueChanged.AddListener((int index) =>
        // {
        //     betCounter = index;
        //     betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();
        //     CalculateCost();
        // });

        SetButton(Turbo_button, ToggleTurbo);

        slotManager.shuffleInitialMatrix();
        // socketController.OnInit = InitGame;
        uIManager.ToggleAudio = audioController.ToggleMute;
        uIManager.playButtonAudio = audioController.PlayButtonAudio;
        uIManager.OnExit = () => socketController.CloseSocket();
        socketController.ShowDisconnectionPopup = uIManager.DisconnectionPopup;

        // socketController.OpenSocket();
    }


    private void SetButton(Button button, Action action, bool slotButton = false)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // playButtonAudio?.Invoke();
            if (slotButton)
                audioController.PlayButtonAudio("spin");
            else
                audioController.PlayButtonAudio();
            action?.Invoke();

        });
    }
    void InitGame()
    {
        if (!initiated)
        {
            initiated = true;
            betCounter = 0;
            currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;
            currentBalance=socketController.socketModel.playerData.Balance;
            if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
            if (betPerLine_text) betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();
            PayLineCOntroller.paylines = socketController.socketModel.initGameData.lineData;
            uIManager.UpdatePlayerInfo(socketController.socketModel.playerData);
            uIManager.PopulateSymbolsPayout(socketController.socketModel.uIData);
            PopulateAutoSpinDropDown();
            PopulateBetPerlineDropDown();
            CalculateCost();
            Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
        }
        else
        {
            uIManager.PopulateSymbolsPayout(socketController.socketModel.uIData);
            PopulateAutoSpinDropDown();
            PopulateBetPerlineDropDown();
        }


    }


    void ExecuteSpin() => StartCoroutine(SpinRoutine());


    void ExecuteAutoSpin()
    {
        if (!isSpinning && autoOptions[autoSpinCounter] > 0)
        {

            isAutoSpin = true;
            autoSpinText.text = autoOptions[autoSpinCounter].ToString();
            autoSpinText.transform.parent.gameObject.SetActive(true);
            // AutoSpin_Button.gameObject.SetActive(false);

            AutoSpinStop_Button.gameObject.SetActive(true);
            autoSpinRoutine = StartCoroutine(AutoSpinRoutine());
        }

    }

    IEnumerator FreeSpinRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (freeSpinCount > 0)
        {
            freeSpinCount--;
            yield return SpinRoutine();
            yield return new WaitForSeconds(1);
        }
        StopFreeSpin();
        isAutoSpin = false;
        isSpinning = false;
        isFreeSpin = false;
        VHcomboList.Clear();
        ToggleButtonGrp(true);
        yield return null;
    }
    IEnumerator AutoSpinRoutine()
    {
        int noOfSPin = autoOptions[autoSpinCounter];
        while (noOfSPin > 0 && isAutoSpin)
        {
            noOfSPin--;
            autoSpinText.text = noOfSPin.ToString();

            yield return SpinRoutine();
            if (!turboMode)
                yield return new WaitForSeconds(1);
            else
                yield return new WaitForSeconds(0.5f);

        }
        autoSpinText.transform.parent.gameObject.SetActive(false);
        autoSpinText.text = "0";
        isSpinning = false;
        StartCoroutine(StopAutoSpinCoroutine());
        yield return null;
    }

    private IEnumerator StopAutoSpinCoroutine(bool hard = false)
    {
        Debug.Log("stop autospin called");
        isAutoSpin = false;
        AutoSpin_Button.gameObject.SetActive(true);
        AutoSpinStop_Button.gameObject.SetActive(false);
        if (!hard)
            yield return new WaitUntil(() => !isSpinning);

        if (autoSpinRoutine != null)
        {
            StopCoroutine(autoSpinRoutine);
            autoSpinRoutine = null;
            autoSpinText.transform.parent.gameObject.SetActive(false);
            autoSpinText.text = "0";
        }
        AutoSpinPopup_Button.gameObject.SetActive(true);
        if (!hard)
            ToggleButtonGrp(true);
        autoSpinText.text = "0";
        yield return null;

    }
    IEnumerator SpinRoutine()
    {
        bool start = OnSpinStart();
        if (!start)
        {

            isSpinning = false;
            if (isAutoSpin)
            {
                StartCoroutine(StopAutoSpinCoroutine());
            }

            ToggleButtonGrp(true);
            yield break;
        }

        if (!isFreeSpin)
            uIManager.DeductBalanceAnim(socketController.socketModel.playerData.Balance - currentTotalBet, socketController.socketModel.playerData.Balance);

        yield return OnSpin();
        yield return OnSpinEnd();

        if (socketController.socketModel.resultGameData.isFreeSpin)
        {
            if (freeSpinRoutine != null)
                StopCoroutine(freeSpinRoutine);
            if (autoSpinRoutine != null)
                StartCoroutine(StopAutoSpinCoroutine(true));
            isFreeSpin = true;
            isAutoSpin = false;
            yield return InitiateFreeSpin(socketController.socketModel.resultGameData.vampHuman);
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            yield break;
        }
        // slotManager.DisableGlow();
        if (!isAutoSpin && !isFreeSpin)
        {
            isSpinning = false;
            ToggleButtonGrp(true);
        }
        if (socketController.socketModel.playerData.currentWining > 0 && !isFreeSpin)
        {
            Double_Button.interactable = true;
        }

    }
    bool OnSpinStart()
    {

        isSpinning = true;
        winIterationCount = 0;
        if (iterativeRoutine != null)
            StopCoroutine(iterativeRoutine);
        slotManager.disableIconsPanel.SetActive(false);
        if (currentBalance < currentTotalBet && !isFreeSpin)
        {
            uIManager.LowBalPopup();
            return false;
        }
        Double_Button.interactable = false;
        ToggleButtonGrp(false);
        uIManager.ClosePopup();
        return true;


    }

    IEnumerator OnSpin()
    {
        // var spinData = new { data = new { currentBet = betCounter, currentLines = 30, spins = 1 }, id = "SPIN" };
        // socketController.SendData("message", spinData);
        yield return slotManager.StartSpin();
        // slotManager.StopIconAnimation();
        if (audioController) audioController.PlaySpinAudio();
        // yield return new WaitUntil(() => socketController.isResultdone);
        // slotManager.PopulateSLotMatrix(socketController.socketModel.resultGameData.ResultReel);
        // currentBalance = socketController.socketModel.playerData.Balance;

        yield return slotManager.StopSpin();
        if (audioController) audioController.StopSpinAudio();


    }
    IEnumerator OnSpinEnd()
    {

        if (isFreeSpin)
        {

        }

        // if (socketController.socketModel.resultGameData.symbolsToEmit.Count > 0){


        // }



        if (socketController.socketModel.playerData.currentWining > 0)
        {
            CheckWinPopups(socketController.socketModel.playerData.currentWining);
            // yield return new WaitForSeconds(1f);
            yield return uIManager.WinTextAnim(socketController.socketModel.playerData.currentWining);
            yield return new WaitForSeconds(0.4f);
        }
        else
        {

        }
        uIManager.UpdatePlayerInfo(socketController.socketModel.playerData);

        audioController.StopWLAaudio();

    }



    IEnumerator InitiateFreeSpin(List<string> VHPos)
    {

        freeSpinCount = socketController.socketModel.resultGameData.count;
        slotManager.disableIconsPanel.SetActive(false);
        slotManager.IconShakeAnim(VHPos);
        yield return new WaitForSeconds(1f);
        slotManager.StartIconBlastAnimation(VHPos, true);
        yield return new WaitForSeconds(0.15f);
        slotManager.FreeSpinVHAnim(VHPos, ref VHcomboList);
        yield return new WaitForSeconds(1f);
        uIManager.FreeSpinPopup(freeSpinCount);
        yield return new WaitForSeconds(1.5f);
        uIManager.CloseFreeSpinPopup();
    }

    void StopFreeSpin()
    {

        for (int i = 0; i < VHcomboList.Count; i++)
        {
            VHcomboList[i].StopAnimation();
            VHcomboList[i].gameObject.SetActive(false);
        }
    }
    void OnInitGamble()
    {
        if (isAutoSpin)
        {
            StartCoroutine(StopAutoSpinCoroutine());
        }
        if (iterativeRoutine != null)
            StopCoroutine(iterativeRoutine);

        object gambleInitData = new { data = new { }, id = "GAMBLEINIT" };
        socketController.SendData("message", gambleInitData);
        changeGambleType(true);
        bank = socketController.socketModel.playerData.currentWining;
        uIManager.UpdategambleInfo(bank);
        gambleObject.SetActive(true);
        gambleChance = 3;

    }
    IEnumerator OnSelectGamble(string type)
    {
        ToggleGambleBtnGrp(false);
        coinAnim.StartAnimation();
        audioController.PlaySpinAudio("gamble");
        object gambleResData = new { data = new { selected = type, gambleOption = gambleOption }, id = "GAMBLERESULT" };
        socketController.SendData("message", gambleResData);
        yield return new WaitUntil(() => socketController.isResultdone);
        if (turboMode)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        if (socketController.socketModel.gambleData.coin == "HEAD")
        {
            coinAnim.textureArray.RemoveAt(0);
            coinAnim.textureArray.Insert(0, headImage);
        }
        else if (socketController.socketModel.gambleData.coin == "TAIL")
        {
            coinAnim.textureArray.RemoveAt(0);
            coinAnim.textureArray.Insert(0, tailImage);

        }
        coinAnim.StopAnimation();
        audioController.StopSpinAudio();
        if (socketController.socketModel.gambleData.playerWon)
            audioController.PlayWLAudio("gamble");
        yield return new WaitForSeconds(0.5f);



        bank = socketController.socketModel.gambleData.currentWinning;

        if (gambleOption == "HALF")
        {
            uIManager.UpdategambleInfo(bank, true);
            if (!socketController.socketModel.gambleData.playerWon && socketController.socketModel.gambleData.currentWinning > 0)
            {
                gambleChance--;
                if (gambleChance <= 0)
                {
                    yield return new WaitForSeconds(1);
                    yield return OnGambleCollect();
                    yield break;

                }

            }
        }
        else
            uIManager.UpdategambleInfo(bank);

        if (bank <= 0)
        {
            yield return new WaitForSeconds(1);
            yield return OnGambleCollect();
            yield break;
        }
        coinAnim.StopAnimation();
        ToggleGambleBtnGrp(true);
        audioController.StopWLAaudio();


    }

    IEnumerator OnGambleCollect()
    {
        ToggleGambleBtnGrp(false);
        object gambleResData = new { data = new { }, id = "GAMBLECOLLECT" };
        socketController.SendData("message", gambleResData);
        yield return new WaitUntil(() => socketController.isResultdone);
        currentBalance = socketController.socketModel.gambleData.balance;
        // PlayerData playerData = new PlayerData();
        socketController.socketModel.playerData.currentWining = socketController.socketModel.gambleData.currentWinning;
        socketController.socketModel.playerData.Balance = socketController.socketModel.gambleData.balance;
        // Debug.Log("balance "+JsonConvert.SerializeObject(socketController.socketModel.gambleData));
        // Debug.Log("player "+JsonConvert.SerializeObject(socketController.socketModel));
        gambleChance = 0;
        Double_Button.interactable = false;
        uIManager.UpdatePlayerInfo(socketController.socketModel.playerData);
        gambleObject.SetActive(false);
        ToggleGambleBtnGrp(true);


    }

    internal void changeGambleType(bool full)
    {

        if (full)
        {
            halfGambleButton.transform.GetChild(0).gameObject.SetActive(false);
            allGambleButton.transform.GetChild(0).gameObject.SetActive(true);
            gambleOption = "ALL";

            uIManager.UpdategambleInfo(bank);

        }
        else
        {
            halfGambleButton.transform.GetChild(0).gameObject.SetActive(true);
            allGambleButton.transform.GetChild(0).gameObject.SetActive(false);
            gambleOption = "HALF";
            uIManager.UpdategambleInfo(bank, true);

        }

    }

    void ToggleGambleBtnGrp(bool toggle)
    {
        Head_option.interactable = toggle;
        Tail_Option.interactable = toggle;
        Collect_Option.interactable = toggle;
        allGambleButton.interactable = toggle;
        halfGambleButton.interactable = toggle;

    }
    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (Maxbet_button) Maxbet_button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        if (AutoSpinPopup_Button) AutoSpinPopup_Button.interactable = toggle;
        if (ToatlBetMinus_Button) ToatlBetMinus_Button.interactable = toggle;
        if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = toggle;
        uIManager.Settings_Button.interactable = toggle;
    }

    private void OnBetChange(bool inc)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (inc)
        {
            if (betCounter < socketController.socketModel.initGameData.Bets.Count - 1)
            {
                betCounter++;
            }
        }
        else
        {
            if (betCounter > 0)
            {
                betCounter--;
            }
        }

        if (betPerLine_text) betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;
        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();

        betCounter = socketController.socketModel.initGameData.Bets.Count - 1;
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;

        totalBet_text.text = currentTotalBet.ToString();
        betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();

        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }


    private void PopulateAutoSpinDropDown()
    {
        autoSpinDropDown.ClearOptions();
        List<string> autoOptionsString = new List<string>();

        for (int i = 0; i < autoOptions.Count; i++)
        {
            autoOptionsString.Add(autoOptions[i].ToString());
        }
        autoSpinDropDown.AddOptions(autoOptionsString);
        autoSpinDropDown.value = 0;
        autoSpinDropDown.RefreshShownValue();
    }

    private void PopulateBetPerlineDropDown()
    {

        betPerLineDropDown.ClearOptions();

        List<string> betOptionsString = new List<string>();

        for (int i = 0; i < socketController.socketModel.initGameData.Bets.Count; i++)
        {
            betOptionsString.Add(socketController.socketModel.initGameData.Bets[i].ToString());
        }

        betPerLineDropDown.AddOptions(betOptionsString);

        betPerLineDropDown.value = 0;
        betPerLineDropDown.RefreshShownValue();

    }



    private double CalculateCost()
    {
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;
        uIManager.UpdateAutoSpinCost(currentTotalBet * autoOptions[autoSpinCounter]);

        return 0;
    }


    void CheckWinPopups(double amount)
    {
        if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
        {
            uIManager.EnableWinPopUp(1);
        }
        else if (amount >= currentTotalBet * 15 && amount < currentTotalBet * 20)
        {
            uIManager.EnableWinPopUp(2);
        }
        else if (amount >= currentTotalBet * 20)
        {
            uIManager.EnableWinPopUp(3);
        }
        else
        {
            uIManager.EnableWinPopUp(0);
        }
    }

    void ToggleTurbo()
    {

        turboMode = !turboMode;
        if (turboMode)
        {
            Turbo_button.transform.GetChild(0).gameObject.SetActive(true);
            Turbo_button.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            Turbo_button.transform.GetChild(0).gameObject.SetActive(false);
            Turbo_button.transform.GetChild(1).gameObject.SetActive(true);
        }

    }
}

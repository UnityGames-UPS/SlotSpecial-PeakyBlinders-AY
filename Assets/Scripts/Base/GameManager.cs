using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
public class GameManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private SlotController slotManager;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private SocketController socketController;
    [SerializeField] private ThunderFreeSpinController thunderFP;
    [SerializeField] private PollyFreeSpinController pollyFP;
    [SerializeField] private ArthurFreeSpinController arthurFP;
    [SerializeField] private TommyFPController tommyFP;
    [SerializeField] private AudioController audioController;
    [SerializeField] private PaylineController payLineController;

    [Header("For spins")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button Bet_Button;
    [SerializeField] private TMP_Text totalBet_text;
    [SerializeField] private bool isSpinning;
    [SerializeField] private Transform paylineSymbolAnimPanel;
    [SerializeField] private Button StopSpinButton;
    [SerializeField] private Button TurboButton;
    [SerializeField] private bool ImmediateStop;
    [SerializeField] private bool turboMode;
    [SerializeField] private Sprite turboActive;
    [SerializeField] private Sprite turboInActive;

    [Header("For auto spins")]
    [SerializeField] private GameObject originalReel;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button[] AutoSpinsButtons;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button AutoSpinPopup_Button;
    [SerializeField] private Button autoSpinUp;
    [SerializeField] private Button autoSpinDown;
    [SerializeField] private bool isAutoSpin;
    [SerializeField] private int autoSpinCounter;
    [SerializeField] private TMP_Text autoSpinText;
    List<int> autoOptions = new List<int>() { 15, 20, 25, 30, 40, 100 };



    [Header("For FreeSpins")]
    [SerializeField] private bool specialSpin;

    [SerializeField] private double currentBalance;
    [SerializeField] private double currentTotalBet;
    [SerializeField] private int betCounter = 0;

    [SerializeField] private Button freeSpinStartButton;

    [SerializeField] private Button CancelWinAnim;

    private Coroutine autoSpinRoutine;
    private Coroutine freeSpinRoutine;
    private Coroutine symbolAnim;

    private Coroutine winAnim;

    private Coroutine spinRoutine;
    [SerializeField] private int winIterationCount;

    [SerializeField] private int freeSpinCount;

    [SerializeField] private bool isFreeSpin;


    private bool initiated;
    bool thunderFreeSpins;

    [SerializeField] int autoSpinLeft;

    [SerializeField] bool autoSpinShouldContinue;
    void Start()
    {
        SetButton(SlotStart_Button, ExecuteSpin, true);

        CancelWinAnim.onClick.AddListener(() => StopWinAnimImmediate());
        TurboButton.onClick.AddListener(ToggleTurboMode);

        for (int i = 0; i < AutoSpinsButtons.Length; i++)
        {
            int capturedIndex = i; // Capture the current value of 'i'
            AutoSpinsButtons[capturedIndex].onClick.AddListener(() =>
            {
                ExecuteAutoSpin(autoOptions[capturedIndex]);
                uIManager.ClosePopup();
                audioController.PlayButtonAudio("spin");
            });
        }
        SetButton(AutoSpinStop_Button, () =>
        {
            autoSpinShouldContinue = false;
            autoSpinLeft = 0;
            StartCoroutine(StopAutoSpinCoroutine());

        });
        // SetButton(ToatlBetMinus_Button, () => OnBetChange(false));
        // SetButton(freeSpinStartButton, () => freeSpinRoutine = StartCoroutine(FreeSpinRoutine()));



        slotManager.shuffleInitialMatrix();
        socketController.OnInit = InitGame;
        uIManager.ToggleAudio = audioController.ToggleMute;
        uIManager.playButtonAudio = audioController.PlayButtonAudio;
        uIManager.OnExit = () => socketController.CloseSocket();
        socketController.ShowDisconnectionPopup = uIManager.DisconnectionPopup;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                arthurFP.slotMatrix[i].slotImages[j].id = slotManager.slotMatrix[i].slotImages[j].id;
                arthurFP.slotMatrix[i].slotImages[j].iconImage.sprite = slotManager.slotMatrix[i].slotImages[j].iconImage.sprite;
            }
        }

        socketController.OpenSocket();


        // tommyFP.spriteRef.AddRange(slotManager.iconImages);
        tommyFP.SpinRoutine = SpinRoutine;
        tommyFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        tommyFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        tommyFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        tommyFP.thunderFP = thunderFP;

        thunderFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        thunderFP.SpinRoutine = SpinRoutine;
        thunderFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        thunderFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        thunderFP.imageRef.AddRange(slotManager.iconImages);

        arthurFP.iconref.AddRange(slotManager.iconImages);
        arthurFP.populateOriginalMatrix = slotManager.PopulateSLotMatrix;
        arthurFP.SpinRoutine = SpinRoutine;
        arthurFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        arthurFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        arthurFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        arthurFP.thunderFP = thunderFP;

        pollyFP.SpinRoutine = SpinRoutine;
        pollyFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        pollyFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        pollyFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        pollyFP.thunderFP = thunderFP;

        StopSpinButton.onClick.AddListener(() => StartCoroutine(StopSpin()));
    }


    private void SetButton(Button button, Action action, bool slotButton = false)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (slotButton)
                audioController.PlayButtonAudio("spin");
            else
                audioController.PlayButtonAudio();

            action?.Invoke();

        });
    }

    void ToggleTurboMode(){
        turboMode = !turboMode;
        if(turboMode)
        TurboButton.image.sprite=turboActive;
        else
        TurboButton.image.sprite=turboInActive;


    }
    void InitGame()
    {
        if (!initiated)
        {
            initiated = true;
            betCounter = 0;
            currentTotalBet = SocketModel.initGameData.Bets[betCounter];
            currentBalance = SocketModel.playerData.Balance;
            payLineController.paylines.AddRange(SocketModel.initGameData.lineData);
            if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
            uIManager.UpdatePlayerInfo(SocketModel.playerData);
            uIManager.PopulateSymbolsPayout(SocketModel.uIData);
            uIManager.PopulateBets(SocketModel.initGameData.Bets, OnBetChange);
            Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
        }
        else
        {
            uIManager.PopulateSymbolsPayout(SocketModel.uIData);
        }


    }


    void ExecuteSpin() => StartCoroutine(SpinRoutine());

    void ExecuteAutoSpin(int noOfSPin)
    {
        Debug.Log(noOfSPin);
        if (!isSpinning && noOfSPin > 0)
        {
        if(StopSpinButton.gameObject.activeSelf)
        StopSpinButton.gameObject.SetActive(false);
            // autoSpinCounter = index;
            isAutoSpin = true;
            autoSpinText.text = noOfSPin.ToString();
            autoSpinText.transform.parent.gameObject.SetActive(true);
            // AutoSpin_Button.gameObject.SetActive(false);

            AutoSpinStop_Button.gameObject.SetActive(true);
            // int noOfSPin = autoOptions[index];
            if (autoSpinRoutine != null)
                StopCoroutine(autoSpinRoutine);

            autoSpinRoutine = StartCoroutine(AutoSpinRoutine(noOfSPin));
        }

    }

    IEnumerator FreeSpinRoutine(bool initiate = true)
    {
        ImmediateStop = false;
        uIManager.ToggleFreeSpinPanel(true);
                if(StopSpinButton.gameObject.activeSelf)
        StopSpinButton.gameObject.SetActive(false);
        // uIManager.CloseFreeSpinPopup();
        isFreeSpin = true;
        for (int i = 0; i < 5; i++)
        {
            slotManager.RespectMask(i);
        }

        yield return CheckNStartFP(
            arthur: SocketModel.resultGameData.isArthurBonus,
            tommy: SocketModel.resultGameData.isTomBonus,
            polly: SocketModel.resultGameData.isPollyBonus,
            thunder: SocketModel.resultGameData.isThunderSpin,
            initiate: initiate
        );

        yield return new WaitForSeconds(1f);
        slotManager.ResetAllSymbols();
        paylineSymbolAnimPanel.gameObject.SetActive(false);
        payLineController.ResetLines();
        audioController.playBgAudio("FP");
        uIManager.ToggleFreeSpinPanel(false);
        ToggleButtonGrp(true);
        isSpinning = false;
        isFreeSpin = false;
        if (autoSpinLeft > 0 && autoSpinShouldContinue)
        {
            ExecuteAutoSpin(autoSpinLeft);
        }

        yield return null;
    }
    IEnumerator AutoSpinRoutine(int noOfSPin)
    {
        while (noOfSPin > 0 && isAutoSpin)
        {
            noOfSPin--;
            autoSpinLeft = noOfSPin;
            autoSpinText.text = noOfSPin.ToString();

            yield return SpinRoutine();

            if (TurboButton)
                yield return new WaitForSeconds(1f);
            else
                yield return new WaitForSeconds(2f);


        }
        autoSpinText.transform.parent.gameObject.SetActive(false);
        autoSpinText.text = "0";
        isSpinning = false;
        StartCoroutine(StopAutoSpinCoroutine());
        yield return null;
    }

    private IEnumerator StopAutoSpinCoroutine(bool hard = false)
    {
        isAutoSpin = false;
        // AutoSpin_Button.gameObject.SetActive(true);
        AutoSpinStop_Button.gameObject.SetActive(false);
        autoSpinText.transform.parent.gameObject.SetActive(false);
        autoSpinText.text = "0";
        if (!hard)
            yield return new WaitUntil(() => !isSpinning);

        if (autoSpinRoutine != null)
        {
            StopCoroutine(autoSpinRoutine);
            autoSpinRoutine = null;

        }
        AutoSpinPopup_Button.gameObject.SetActive(true);
        if (!hard)
            ToggleButtonGrp(true);
        autoSpinText.text = "0";
        yield return null;

    }

    IEnumerator StopSpin()
    {

        if (isAutoSpin || isFreeSpin || thunderFreeSpins || ImmediateStop)
            yield break;

        ImmediateStop = true;
        StopSpinButton.interactable = false;
        yield return new WaitUntil(() => !isSpinning);
        ImmediateStop = false;
        // StopSpinButton.gameObject.SetActive(false);
        StopSpinButton.interactable = true;


    }

    IEnumerator SpinRoutine(Action OnSpinAnimStart = null, Action OnSpinAnimStop = null, bool playBeforeStart = false, bool playBeforeEnd = false, float delay = 0, float delay1 = 0)
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
            uIManager.DeductBalanceAnim(SocketModel.playerData.Balance - currentTotalBet, SocketModel.playerData.Balance);

        yield return OnSpin(OnSpinAnimStart, OnSpinAnimStop, playBeforeStart, playBeforeEnd, delay, delay1);
        yield return OnSpinEnd();

        if (SocketModel.resultGameData.freeSpinCount > 0 && !isFreeSpin)
        {
            if (autoSpinRoutine != null)
            {
                yield return StopAutoSpinCoroutine(true);
                if (autoSpinLeft > 0)
                    autoSpinShouldContinue = true;
            }
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = SocketModel.resultGameData.freeSpinCount;
            uIManager.UpdateFreeSpinInfo(freeSpinCount);
            // uIManager.FreeSpinPopup(freeSpinCount, true);
            if (turboMode)
                yield return new WaitForSeconds(0.5f);
            else
                yield return new WaitForSeconds(1f);

            paylineSymbolAnimPanel.gameObject.SetActive(false);
            // uIManager.CloseFreeSpinPopup();
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            audioController.playBgAudio("FP");

            yield break;
        }
        else if (SocketModel.resultGameData.thunderSpinCount > 0 && !thunderFreeSpins && !isFreeSpin)
        {
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = SocketModel.resultGameData.thunderSpinCount;
            uIManager.UpdateFreeSpinInfo(freeSpinCount);
            if (autoSpinRoutine != null)
            {
                yield return StopAutoSpinCoroutine(true);
                if (autoSpinLeft > 0)
                    autoSpinShouldContinue = true;

            }
            // uIManager.FreeSpinPopup(freeSpinCount, true);
            if (turboMode)
                yield return new WaitForSeconds(0.5f);
            else
                yield return new WaitForSeconds(1f);
            paylineSymbolAnimPanel.gameObject.SetActive(false);
            // uIManager.CloseFreeSpinPopup();
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            audioController.playBgAudio("FP");
            yield break;


        }

        if (!isAutoSpin && !isFreeSpin)
        {
            // symbolAnim = StartCoroutine(PayLineSymbolRoutine());
            isSpinning = false;
            ToggleButtonGrp(true);
        }

    }
    bool OnSpinStart()
    {

        // audioController.PlayButtonAudio("spin");
        isSpinning = true;
        winIterationCount = 0;
        paylineSymbolAnimPanel.gameObject.SetActive(false);
        if (symbolAnim != null)
            StopCoroutine(symbolAnim);

        if (currentBalance < currentTotalBet && !isFreeSpin)
        {
            uIManager.LowBalPopup();
            return false;
        }

        StopAllWinAnimation();
        // paylineSymbolAnimPanel.gameObject.SetActive(false);

        ToggleButtonGrp(false);
        uIManager.ClosePopup();
        return true;


    }

    internal IEnumerator OnSpin(Action OnSpinStart, Action OnSpinStop, bool playBeforeStart, bool playBeforeEnd, float delay1, float delay2)
    {
        if(!isAutoSpin && !isFreeSpin)
        StopSpinButton.gameObject.SetActive(true);
        var spinData = new { data = new { currentBet = betCounter, currentLines = 1, spins = 1 }, id = "SPIN" };
        socketController.SendData("message", spinData);

        if (playBeforeStart)
        {
            OnSpinStart?.Invoke();
            if (delay1 > 0)
                yield return new WaitForSeconds(delay1);

        }

        yield return slotManager.StartSpin(turboMode, ImmediateStop);

        if (!playBeforeStart)
        {
            OnSpinStart?.Invoke();
            if (delay1 > 0)
                yield return new WaitForSeconds(delay1);
        }

        yield return new WaitUntil(() => SocketController.isResultdone);

        if (!turboMode)
            yield return new WaitForSeconds(0.45f);
        else
            yield return new WaitForSeconds(0.35f);

        // slotManager.StopIconAnimation();
        slotManager.PopulateSLotMatrix(SocketModel.resultGameData.ResultReel, SocketModel.resultGameData.frozenIndices);
        currentBalance = SocketModel.playerData.Balance;

        if (playBeforeEnd)
        {
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);
        }

        yield return slotManager.StopSpin(ignore: !thunderFreeSpins,
        playStopSound: audioController.PlaySpinStopAudio,
        isFreeSpin: isFreeSpin,
        immediateStop: ImmediateStop, turboMode: turboMode);

        if (!playBeforeEnd)
        {
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);

        }

        if(StopSpinButton.gameObject.activeSelf)
        StopSpinButton.gameObject.SetActive(false);


    }
    IEnumerator OnSpinEnd()
    {
        audioController.StopSpinAudio();
        SingleLoopAnimation();
        audioController.StopWLAaudio();
        uIManager.UpdatePlayerInfo(SocketModel.playerData);

        if (SocketModel.playerData.currentWining > 0)
        {

            CheckWinPopups(SocketModel.playerData.currentWining);
            uIManager.NormalWinAnimation();
            yield return new WaitForSeconds(0.5f);
            audioController.StopWLAaudio();

        }
        if (isFreeSpin)
            uIManager.UpdateFreeSpinInfo(winnings: SocketModel.playerData.currentWining);

        slotManager.StopIconAnimation();
        if (thunderFreeSpins)
            yield break;

        if (isAutoSpin || isFreeSpin)
        {
            symbolAnim = StartCoroutine(PayLineSymbolRoutine(true));
            yield break;
        }

        if(SocketModel.resultGameData.linesToEmit.Count==1){
            SingleLoopAnimation();
            yield break;
        }

        if (SocketModel.resultGameData.linesToEmit.Count > 1)
        {
            symbolAnim = StartCoroutine(PayLineSymbolRoutine(false));
        }
        yield return null;
    }

    private void SingleLoopAnimation()
    {
        if (SocketModel.resultGameData.symbolsToEmit.Count > 0)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartIconAnimation(Helper.RemoveDuplicates(SocketModel.resultGameData.symbolsToEmit), paylineSymbolAnimPanel);
        }


        if (SocketModel.resultGameData.linesToEmit.Count > 0)
        {
            for (int i = 0; i < SocketModel.resultGameData.linesToEmit.Count; i++)
            {
                payLineController.GeneratePayline(SocketModel.resultGameData.linesToEmit[i]);
            }
        }
    }

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (AutoSpinPopup_Button) AutoSpinPopup_Button.interactable = toggle;
        if (Bet_Button) Bet_Button.interactable = toggle;
        uIManager.Settings_Button.interactable = toggle;
        if (TurboButton) TurboButton.interactable = toggle;
    }

    private void OnBetChange(int index)
    {
        if (audioController) audioController.PlayButtonAudio();

        Debug.Log(index);
        betCounter = index;
        currentTotalBet = SocketModel.initGameData.Bets[betCounter];
        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }




    void CheckWinPopups(double amount)
    {
        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();

        if (amount >= currentTotalBet * 5 && amount < currentTotalBet * 7.5)
        {
            uIManager.EnableWinPopUp(1);
            winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
            audioController.PlayWLAudio();

        }
        else if (amount >= currentTotalBet * 7.5 && amount < currentTotalBet * 10)
        {
            uIManager.EnableWinPopUp(2);
            winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
            audioController.PlayWLAudio("big");

        }
        else if (amount >= currentTotalBet * 10)
        {
            uIManager.EnableWinPopUp(3);
            winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
            audioController.PlayWLAudio("mega");

        }

    }


    IEnumerator PayLineSymbolRoutine(bool oneTime)
    {
        if (SocketModel.resultGameData.symbolsToEmit.Count == 0)
            yield break;
        paylineSymbolAnimPanel.gameObject.SetActive(true);

        int loopDuration = 1;

        slotManager.StopIconAnimation();
        while (loopDuration > 0)
        {
            for (int i = 0; i < SocketModel.resultGameData.linesToEmit.Count; i++)
            {

                slotManager.PlaySymbolAnim(SocketModel.resultGameData.symbolsToEmit[i], paylineSymbolAnimPanel);
                payLineController.GeneratePayline(SocketModel.resultGameData.linesToEmit[i]);
                if (turboMode)
                    yield return new WaitForSeconds(1);
                else
                    yield return new WaitForSeconds(0.75f);
                payLineController.ResetLines();
                slotManager.StopSymbolAnim(SocketModel.resultGameData.symbolsToEmit[i]);

            }
            if (oneTime)
                loopDuration--;
            yield return null;
        }
        // if(oneTime)
        // SingleLoopAnimation();

    }

    void StopWinAnimImmediate()
    {
        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();
    }


    void StopAllWinAnimation()
    {
        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();
        uIManager.StopNormalWinAnimation();
        slotManager.ResetAllSymbols();
        payLineController.ResetLines();
        paylineSymbolAnimPanel.gameObject.SetActive(false);
    }

    IEnumerator CheckNStartFP(bool arthur, bool polly, bool thunder, bool tommy, bool initiate = true)
    {

        slotManager.ResetAllSymbols();
        if (arthur && !polly && !thunder && !tommy)
        {
            yield return arthurFP.StartFP(
            originalReel: originalReel,
            count: SocketModel.resultGameData.freeSpinCount,
            initiate: initiate);
        }
        else if (!arthur && polly && !thunder && !tommy)
        {
            yield return pollyFP.StartFP(
           count: SocketModel.resultGameData.freeSpinCount);
        }
        else if (!arthur && !polly && thunder && !tommy)
        {
            thunderFreeSpins = true;

            yield return thunderFP.StartFP(
            froxenIndeces: SocketModel.resultGameData.frozenIndices,
            count: SocketModel.resultGameData.thunderSpinCount);

            thunderFreeSpins = false;

        }

        else if (!arthur && !polly && !thunder && tommy)
        {
            yield return tommyFP.StartFP(
            count: SocketModel.resultGameData.freeSpinCount);
        }

        else
        {
            Debug.Log("More thean two is true");
            yield break;
        }

    }

}

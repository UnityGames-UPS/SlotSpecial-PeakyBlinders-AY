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
    [SerializeField] private Button infoButton;
    [SerializeField] private Transform paylineSymbolAnimPanel;
    [SerializeField] private Button StopSpinButton;
    [SerializeField] private Button TurboButton;
    [SerializeField] private GameObject turboAnim;
    [SerializeField] internal static bool ImmediateStop;
    [SerializeField] private bool turboMode;
    [SerializeField] private Sprite turboActive;
    [SerializeField] private Sprite turboInActive;

    [Header("For auto spins")]
    [SerializeField] private GameObject originalReel;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button[] AutoSpinsButtons;
    [SerializeField] private TMP_Text[] AutoSpinOptions_Text;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button AutoSpinPopup_Button;
    [SerializeField] private bool isAutoSpin;
    [SerializeField] private TMP_Text autoSpinText;
    [SerializeField] private Button autoSpinUp;
    [SerializeField] private Button autoSpinDown;
    [SerializeField] private TMP_Text autoSpinShowText;
    private int autoSpinCounter;
    private int maxAutoSpinValue = 1000;
    List<int> autoOptions = new List<int>() { 15, 20, 25, 30, 40, 100 };



    [Header("For FreeSpins")]
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
    internal static bool thunderFreeSpins;

    [SerializeField] int autoSpinLeft;

    [SerializeField] bool autoSpinShouldContinue;

    [SerializeField] private int totalLies = 20;

    internal static bool winanimRunning;
    void Start()
    {
        SetButton(SlotStart_Button, ExecuteSpin, true);
        SetButton(CancelWinAnim, () => StopWinAnimImmediate());                                  // ashu
        SetButton(TurboButton, () => ToggleTurboMode());
        SetButton(autoSpinUp, () => OnAutoSpinChange(true));
        SetButton(autoSpinDown, () => OnAutoSpinChange(false));
        SetButton(AutoSpin_Button, () =>
            {
                uIManager.ClosePopup();
                audioController.PlayButtonAudio("spin");
                ExecuteAutoSpin();
            }, true);
        for (int i = 0; i < AutoSpinsButtons.Length; i++)
        {
            int capturedIndex = i; // Capture the current value of 'i'
            AutoSpinsButtons[capturedIndex].onClick.AddListener(() =>
            {
                uIManager.ClosePopup();
                ExecuteAutoSpin(autoOptions[capturedIndex]);
                audioController.PlayButtonAudio("spin");
            });
            AutoSpinOptions_Text[i].text = autoOptions[capturedIndex].ToString();
        }

        SetButton(AutoSpinStop_Button, () =>
        {
            autoSpinShouldContinue = false;
            autoSpinLeft = 0;
            StartCoroutine(StopAutoSpinCoroutine());

        });
        autoSpinCounter = 1;
        autoSpinShowText.text = autoSpinCounter.ToString();
        // SetButton(ToatlBetMinus_Button, () => OnBetChange(false));
        // SetButton(freeSpinStartButton, () => freeSpinRoutine = StartCoroutine(FreeSpinRoutine()));



        slotManager.shuffleInitialMatrix();
        socketController.OnInit = InitGame;
        uIManager.ToggleAudio = audioController.ToggleMute;
        uIManager.playButtonAudio = audioController.PlayButtonAudio;
        uIManager.OnExit = () => socketController.CloseSocket();
        socketController.ShowDisconnectionPopup = uIManager.DisconnectionPopup;

        // for (int i = 0; i < 5; i++)
        // {
        //     for (int j = 0; j < 3; j++)
        //     {
        //         arthurFP.slotMatrix[i].slotImages[j].id = slotManager.slotMatrix[i].slotImages[j].id;
        //         arthurFP.slotMatrix[i].slotImages[j].iconImage.sprite = slotManager.slotMatrix[i].slotImages[j].iconImage.sprite;
        //     }
        // }

        // socketController.OpenSocket();


        // tommyFP.spriteRef.AddRange(slotManager.iconImages);
        tommyFP.SpinRoutine = SpinRoutine;
        tommyFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        tommyFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        tommyFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        tommyFP.thunderFP = thunderFP;
        tommyFP.FreeSpinPopUPOverlay = uIManager.OpenFreeSpinPopupOverlay;

        thunderFP.populateOriginalMatrix = slotManager.PopulateSLotMatrix;
        thunderFP.SpinRoutine = SpinRoutine;
        thunderFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        thunderFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        thunderFP.imageRef.AddRange(slotManager.iconImages);
        thunderFP.FreeSpinPopUPOverlay = uIManager.OpenFreeSpinPopupOverlay;
        thunderFP.StopAllWinAnimation = StopAllWinAnimation;                                  // ashu
        thunderFP.thunderWinPopup = ThunderWinPopups;                                  // ashu
        thunderFP.PlayStopSpinAudio = audioController.PlaySpinStopAudio;

        arthurFP.iconref.AddRange(slotManager.iconImages);
        arthurFP.populateOriginalMatrix = slotManager.PopulateSLotMatrix;
        arthurFP.SpinRoutine = SpinRoutine;
        arthurFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        arthurFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        arthurFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        arthurFP.thunderFP = thunderFP;
        arthurFP.FreeSpinPopUPOverlay = uIManager.OpenFreeSpinPopupOverlay;


        pollyFP.SpinRoutine = SpinRoutine;
        pollyFP.UpdateUI = uIManager.UpdateFreeSpinInfo;
        pollyFP.FreeSpinPopUP = uIManager.FreeSpinPopup;
        pollyFP.FreeSpinPopUpClose = uIManager.CloseFreeSpinPopup;
        pollyFP.thunderFP = thunderFP;
        pollyFP.FreeSpinPopUPOverlay = uIManager.OpenFreeSpinPopupOverlay;


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

    private void OnAutoSpinChange(bool inc)
    {

        if (audioController) audioController.PlayButtonAudio();

        if (inc)
        {
            autoSpinCounter++;
            if (autoSpinCounter > maxAutoSpinValue)
            {
                autoSpinCounter = 1;
            }
        }
        else
        {
            autoSpinCounter--;
            if (autoSpinCounter < 1)
            {
                autoSpinCounter = maxAutoSpinValue;

            }
        }

        autoSpinShowText.text = autoSpinCounter.ToString();


    }
    void ToggleTurboMode()
    {
        turboMode = !turboMode;
        if (turboMode)
        {
            // TurboButton.image.sprite = turboActive;
            turboAnim.SetActive(true);

        }
        else
        {
            // TurboButton.image.sprite = turboInActive;
            turboAnim.SetActive(false);


        }


    }
    void InitGame()
    {
        if (!initiated)
        {
            initiated = true;
            betCounter = 0;
            totalLies = socketController.initGameData.lines.Count;
            currentTotalBet = socketController.initGameData.bets[betCounter] * totalLies;
            currentBalance = socketController.playerData.balance;
            if (currentBalance < currentTotalBet)
            {
                uIManager.LowBalPopup();
            }
            payLineController.paylines.AddRange(socketController.initGameData.lines);
            if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
            uIManager.UpdatePlayerInfo(socketController.playerData);                                    //ashu
            uIManager.PopulateSymbolsPayout(socketController.uIData, socketController.initGameData.bets[betCounter]);
            uIManager.PopulateBets(socketController.initGameData.bets, totalLies, OnBetChange);
            // Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
        }
        else
        {
            uIManager.PopulateSymbolsPayout(socketController.uIData, totalLies);
        }


    }


    void ExecuteSpin() => StartCoroutine(SpinRoutine());

    void ExecuteAutoSpin(int noOfSPin = 0)
    {
        if (noOfSPin <= 0)
            noOfSPin = autoSpinCounter;

        Debug.Log(noOfSPin);
        if (!isSpinning && noOfSPin > 0)
        {
            if (StopSpinButton.gameObject.activeSelf)
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
        if (StopSpinButton.gameObject.activeSelf)
            StopSpinButton.gameObject.SetActive(false);
        // uIManager.CloseFreeSpinPopup();
        isFreeSpin = true;
        for (int i = 0; i < 5; i++)
        {
            slotManager.RespectMask(i);
        }

        Debug.Log("Dev Freespin :" + socketController.resultGameData.features.isArthurSpin);
        yield return CheckNStartFP(                                        //ashu
            arthur: socketController.resultGameData.features.isArthurSpin,
            tommy: socketController.resultGameData.features.isTomSpin,
            polly: socketController.resultGameData.features.isPollySpin,
            thunder: socketController.resultGameData.features.isThunderSpin,
            initiate: initiate
        );
        uIManager.ToggleFreeSpinPanel(false);

        StopAllWinAnimation();                                  // ashu
        audioController.playBgAudio();
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

            if (socketController.resultGameData.payload.currentWinning > 0)                           //Ashu
            {
                if (TurboButton)
                    yield return new WaitForSeconds(2.5f);
                else
                    yield return new WaitForSeconds(3f);
            }
            else
            {
                yield return new WaitForSeconds(1f);

            }

        }

        StopAllWinAnimation();                                  // ashu
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
            uIManager.DeductBalanceAnim(socketController.playerData.balance - currentTotalBet, socketController.playerData.balance);

        yield return OnSpin(OnSpinAnimStart, OnSpinAnimStop, playBeforeStart, playBeforeEnd, delay, delay1);
        yield return OnSpinEnd();                                  // ashu

        if (socketController.resultGameData.features.freeSpin.freeSpinCount > 0 && !isFreeSpin)             //ashu
        {
            Debug.Log("Dev Test : Enterd Freespin");
            if (autoSpinRoutine != null)
            {
                yield return StopAutoSpinCoroutine(true);
                if (autoSpinLeft > 0)
                    autoSpinShouldContinue = true;
            }
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = socketController.resultGameData.features.freeSpin.freeSpinCount;
            uIManager.UpdateFreeSpinInfo(freeSpinCount);
            // uIManager.FreeSpinPopup(freeSpinCount, true);
            if (turboMode)
                yield return new WaitForSeconds(0.5f);
            else
                yield return new WaitForSeconds(1f);

            paylineSymbolAnimPanel.gameObject.SetActive(false);
            // uIManager.CloseFreeSpinPopup();
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            audioController.playBgAudio("Bonus");

            yield break;
        }
        else if (socketController.resultGameData.features.bonus.thunderSpinCount > 0 && !thunderFreeSpins && !isFreeSpin)
        {
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = socketController.resultGameData.features.bonus.thunderSpinCount;
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
            audioController.playBgAudio("Bonus");
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
        audioController.StopWLAaudio();
        paylineSymbolAnimPanel.gameObject.SetActive(false);
        if (symbolAnim != null)
            StopCoroutine(symbolAnim);

        StopAllWinAnimation();                                  // ashu
        if (currentBalance < currentTotalBet && !isFreeSpin)
        {
            uIManager.LowBalPopup();
            return false;
        }


        if (isFreeSpin || thunderFreeSpins)
            uIManager.UpdateFreeSpinInfo(winnings: 0);

        uIManager.ResetWinning();
        ToggleButtonGrp(false);
        uIManager.ClosePopup();
        return true;


    }

    internal IEnumerator OnSpin(Action OnSpinStart, Action OnSpinStop, bool playBeforeStart, bool playBeforeEnd, float delay1, float delay2)
    {
        if (!isAutoSpin && !isFreeSpin)
            StopSpinButton.gameObject.SetActive(true);
        var spinData = new { data = new { currentBet = betCounter, currentLines = 20, spins = 1 }, id = "SPIN" };
        socketController.AccumulateResult(betCounter);

        yield return slotManager.StartSpin(turboMode, ImmediateStop);
        slotManager.shuffleInitialMatrix();
        slotManager.CLearAllCoins();
        yield return new WaitUntil(() => SocketController.isResultdone);
        currentBalance = socketController.playerData.balance;
        //  Helper.ConvertPayload(socketController.resultGameData.payload,socketController.initGameData.lines);


        if (!playBeforeStart)
        {
            OnSpinStart?.Invoke();
            if (delay1 > 0)
                yield return new WaitForSeconds(delay1);
        }


        if (!turboMode)
            yield return new WaitForSeconds(0.45f);
        else
            yield return new WaitForSeconds(0.35f);


        slotManager.PopulateSLotMatrix(Helper.ConvertStringMatrixToIntMatrix(socketController.resultGameData.matrix), socketController.resultGameData.features.bonus.frozenIndices);           //ashu

        if (playBeforeEnd)
        {
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);
        }

        yield return slotManager.StopSpin(ignore: !thunderFreeSpins,
        playStopSound: audioController.PlaySpinStopAudio,
        isFreeSpin: isFreeSpin,
         turboMode: turboMode);

        if (!playBeforeEnd)
        {
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);

        }

        if (StopSpinButton.gameObject.activeSelf)
            StopSpinButton.gameObject.SetActive(false);


    }
    IEnumerator OnSpinEnd()                     //ashu
    {
        // audioController.StopSpinAudio();
        SingleLoopAnimation(true);

        uIManager.UpdatePlayerInfo(socketController.playerData, socketController.resultGameData.payload.currentWinning);
        if (socketController.resultGameData.features.freeSpin.freeSpinCount > 0 || socketController.resultGameData.features.bonus.frozenIndices.Count > 0 || socketController.resultGameData.payload.lineWins.Count > 0) //ashu
            yield return new WaitForSeconds(1f);                  //ashu

        audioController.StopWLAaudio();

        if (socketController.resultGameData.payload.currentWinning > 0 && !thunderFreeSpins)
        {
            winanimRunning = true;
            CheckWinPopups(socketController.resultGameData.payload.currentWinning);
            uIManager.NormalWinAnimation();
            yield return new WaitUntil(() => !winanimRunning);

        }
        if (isFreeSpin)
            uIManager.UpdateFreeSpinInfo(winnings: socketController.resultGameData.payload.currentWinning);

        slotManager.StopIconAnimation();

        if (thunderFreeSpins)
            yield break;

        if (isAutoSpin || isFreeSpin)
        {
            SingleLoopAnimation();
            yield break;
        }

        if (socketController.resultGameData.payload.lineWins.Count == 1)
        {
            SingleLoopAnimation();
            yield break;
        }

        if (socketController.resultGameData.payload.lineWins.Count > 1)
        {
            symbolAnim = StartCoroutine(PayLineSymbolRoutine(false));
        }
        yield return null;
    }


    private void SingleLoopAnimation(bool showall = false)
    {

        if (socketController.resultGameData.payload.lineWins.Count > 0)              //ashu
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);

            slotManager.StartIconAnimation(Helper.GetAllSymbolToEmit(socketController.resultGameData.payload, socketController.initGameData.lines), paylineSymbolAnimPanel);
        }


        if (socketController.resultGameData.payload.lineWins.Count > 0)
        {
            for (int i = 0; i < socketController.resultGameData.payload.lineWins.Count; i++)
            {
                payLineController.GeneratePayline(socketController.resultGameData.payload.lineWins[i].lineIndex);
            }
        }

        if (!showall)
            return;

        if (socketController.resultGameData.features.bonus.frozenIndices.Count > 0 && !isFreeSpin && !thunderFreeSpins)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartIconAnimation(Helper.GetAllFrozenIndexex(socketController.resultGameData.features.bonus.frozenIndices), paylineSymbolAnimPanel);
        }

        // if (socketController.resultGameData.freeSpinIndices.Count > 0)            // ashu
        // {
        //     paylineSymbolAnimPanel.gameObject.SetActive(true);
        //     slotManager.StartIconAnimation(socketController.resultGameData.freeSpinIndices, paylineSymbolAnimPanel);
        // }
    }
    // private void SingleLoopAnimation(bool showall = false)
    // {
    //     if (socketController.resultGameData.symbolsToEmit.Count > 0)              //ashu.    repaces func by new
    //     {
    //         paylineSymbolAnimPanel.gameObject.SetActive(true);
    //         slotManager.StartIconAnimation(Helper.RemoveDuplicates(socketController.resultGameData.symbolsToEmit), paylineSymbolAnimPanel);
    //     }


    //     if (socketController.resultGameData.linesToEmit.Count > 0)
    //     {
    //         for (int i = 0; i < socketController.resultGameData.linesToEmit.Count; i++)
    //         {
    //             payLineController.GeneratePayline(socketController.resultGameData.linesToEmit[i]);
    //         }
    //     }

    //     if (!showall)
    //         return;

    //     if (socketController.resultGameData.frozenIndices.Count > 0 && !isFreeSpin && !thunderFreeSpins)
    //     {
    //         paylineSymbolAnimPanel.gameObject.SetActive(true);
    //         slotManager.StartIconAnimation(Helper.ConvertFrozenIndicesToCoord(socketController.resultGameData.frozenIndices), paylineSymbolAnimPanel);
    //     }

    //     if (socketController.resultGameData.freeSpinIndices.Count > 0)
    //     {
    //         paylineSymbolAnimPanel.gameObject.SetActive(true);
    //         slotManager.StartIconAnimation(socketController.resultGameData.freeSpinIndices, paylineSymbolAnimPanel);
    //     }
    // }

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (AutoSpinPopup_Button) AutoSpinPopup_Button.interactable = toggle;
        if (Bet_Button) Bet_Button.interactable = toggle;
        uIManager.Settings_Button.interactable = toggle;
        if (infoButton) infoButton.interactable = toggle;
    }

    private void OnBetChange(int index)
    {
        if (audioController) audioController.PlayButtonAudio();

        Debug.Log(index);
        betCounter = index;
        currentTotalBet = socketController.initGameData.bets[betCounter] * totalLies;
        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
        // if (currentBalance < currentTotalBet)
        //     uIManager.LowBalPopup();
    }




    void CheckWinPopups(double amount)              //ashu
    {
        if (winAnim != null)
            StopCoroutine(winAnim);

        uIManager.ClosePopup();
        winAnim = null;
        if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
        {
            uIManager.EnableWinPopUp(1);
            winAnim = StartCoroutine(uIManager.WinTextAnim(socketController.resultGameData.payload.currentWinning));
            audioController.PlayWLAudio("big");
            Invoke(nameof(StopWinAnimImmediate), 3f);

        }
        else if (amount >= currentTotalBet * 15 && amount < currentTotalBet * 20)
        {
            uIManager.EnableWinPopUp(2);
            winAnim = StartCoroutine(uIManager.WinTextAnim(socketController.resultGameData.payload.currentWinning));
            audioController.PlayWLAudio("big");
            Invoke(nameof(StopWinAnimImmediate), 3f);

        }
        else if (amount >= currentTotalBet * 20)
        {
            uIManager.EnableWinPopUp(3);
            winAnim = StartCoroutine(uIManager.WinTextAnim(socketController.resultGameData.payload.currentWinning));
            audioController.PlayWLAudio("mega");
            Invoke(nameof(StopWinAnimImmediate), 3f);

        }
        else
        {
            winanimRunning = false;
            audioController.PlayWLAudio();

        }

    }

    void ThunderWinPopups(double amount)
    {
        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();
        winAnim = null;
        uIManager.EnableWinPopUp(4);
        winAnim = StartCoroutine(uIManager.WinTextAnim(socketController.resultGameData.payload.currentWinning));
        Invoke(nameof(StopWinAnimImmediate), 2.5f);

        if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
            audioController.PlayWLAudio("big");

        else if (amount >= currentTotalBet * 15 && amount < currentTotalBet * 20)
            audioController.PlayWLAudio("big");

        else if (amount >= currentTotalBet * 20)
            audioController.PlayWLAudio("mega");
        else
            audioController.PlayWLAudio();

    }

    IEnumerator PayLineSymbolRoutine(bool oneTime)
    {
        if (socketController.resultGameData.payload.currentWinning == 0)
            yield break;
        paylineSymbolAnimPanel.gameObject.SetActive(true);

        int loopDuration = 1;

        slotManager.StopIconAnimation();
        while (loopDuration > 0)
        {
            for (int i = 0; i < socketController.resultGameData.payload.lineWins.Count; i++)
            {

                slotManager.PlaySymbolAnim(socketController.resultGameData.payload.lineWins[i].positions, socketController.resultGameData.payload.lineWins[i].lineIndex, paylineSymbolAnimPanel);   //ashu
                payLineController.GeneratePayline(socketController.resultGameData.payload.lineWins[i].lineIndex);
                if (turboMode)
                    yield return new WaitForSeconds(1);
                else
                    yield return new WaitForSeconds(0.75f);
                payLineController.ResetLines();
                slotManager.StopSymbolAnim(socketController.resultGameData.payload.lineWins[i].positions, socketController.resultGameData.payload.lineWins[i].lineIndex);

            }
            if (oneTime)
                loopDuration--;
            yield return null;
        }
        // if(oneTime)
        SingleLoopAnimation();

    }

    void StopWinAnimImmediate()
    {
        // if(!isFreeSpin && !thunderFreeSpins){

        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();
        winanimRunning = false;
        // }

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
            count: socketController.resultGameData.features.freeSpin.freeSpinCount,
            initiate: initiate);
        }
        else if (!arthur && polly && !thunder && !tommy)
        {
            yield return pollyFP.StartFP(
           count: socketController.resultGameData.features.freeSpin.freeSpinCount);
        }
        else if (!arthur && !polly && thunder && !tommy)
        {
            thunderFreeSpins = true;

            yield return thunderFP.StartFP(
                froxenIndeces: socketController.resultGameData.features.bonus.frozenIndices,
                count: socketController.resultGameData.features.bonus.thunderSpinCount,
                ResultReel: Helper.ConvertStringMatrixToIntMatrix(socketController.resultGameData.matrix)
            );

            thunderFreeSpins = false;

        }

        else if (!arthur && !polly && !thunder && tommy)
        {
            yield return tommyFP.StartFP(
            count: socketController.resultGameData.features.freeSpin.freeSpinCount);
        }

        else
        {
            Debug.Log("More thean two is true");
            yield break;
        }

    }



}

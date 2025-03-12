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
        SetButton(CancelWinAnim, () => StopWinAnimImmediate());
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

        socketController.OpenSocket();


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
        thunderFP.StopAllWinAnimation = StopAllWinAnimation;
        thunderFP.thunderWinPopup = ThunderWinPopups;
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


        // Sets the button with the given action and optional slotButton flag
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

        // Handles the change in auto spin counter
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

        // Toggles the turbo mode
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

        // Initializes the game and removes loading screen
        void InitGame()
        {
            if (!initiated)
            {
                initiated = true;
                betCounter = 0;
                totalLies = SocketModel.initGameData.lineData.Count;
                currentTotalBet = SocketModel.initGameData.Bets[betCounter] * totalLies;
                currentBalance = SocketModel.playerData.Balance;
                if (currentBalance < currentTotalBet)
                {
                    uIManager.LowBalPopup();
                }
                payLineController.paylines.AddRange(SocketModel.initGameData.lineData);
                if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
                uIManager.UpdatePlayerInfo(SocketModel.playerData);
                uIManager.PopulateSymbolsPayout(SocketModel.uIData, SocketModel.initGameData.Bets[betCounter]);
                uIManager.PopulateBets(SocketModel.initGameData.Bets, totalLies, OnBetChange);
                Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
            }
            else
            {
                uIManager.PopulateSymbolsPayout(SocketModel.uIData, totalLies);
            }


        }


    //Executes spin
    void ExecuteSpin() => StartCoroutine(SpinRoutine());

    // Executes auto spin with the given number of spins
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

    // Coroutine for handling the free spin routine
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

        // Check and start the appropriate free spin mode
        yield return CheckNStartFP(
            arthur: SocketModel.resultGameData.isArthurBonus,
            tommy: SocketModel.resultGameData.isTomBonus,
            polly: SocketModel.resultGameData.isPollyBonus,
            thunder: SocketModel.resultGameData.isThunderSpin,
            initiate: initiate
        );

        uIManager.ToggleFreeSpinPanel(false);
        // yield return new WaitForSeconds(1f);
        StopAllWinAnimation();
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

    // Coroutine for handling the auto spin routine
    IEnumerator AutoSpinRoutine(int noOfSPin)
    {
        while (noOfSPin > 0 && isAutoSpin)
        {
            noOfSPin--;
            autoSpinLeft = noOfSPin;
            autoSpinText.text = noOfSPin.ToString();

            yield return SpinRoutine();

            if (SocketModel.playerData.currentWining > 0)
            {
                if (TurboButton)
                    yield return new WaitForSeconds(2.5f); // Delay before next spin when there is a win and turbo mode is active
                else
                    yield return new WaitForSeconds(3f); // Delay before next spin when there is a win
            }
            else
            {
                yield return new WaitForSeconds(1f); // Delay before next spin when there is no win

            }

        }

        StopAllWinAnimation();
        autoSpinText.transform.parent.gameObject.SetActive(false);
        autoSpinText.text = "0";
        isSpinning = false;
        StartCoroutine(StopAutoSpinCoroutine());
        yield return null;
    }

 
    /// <summary>
    /// Coroutine for stopping the auto spin.
    /// </summary>
    /// <param name="hard">Flag to indicate if the auto spin should stop immediately or wait for the current spin to complete.</param>
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

    // Coroutine for stopping the spin
    IEnumerator StopSpin()
    {
        // Check if auto spin, free spin, thunder free spins, or immediate stop is active
        if (isAutoSpin || isFreeSpin || thunderFreeSpins || ImmediateStop)
            yield break;

        ImmediateStop = true;
        StopSpinButton.interactable = false;
        yield return new WaitUntil(() => !isSpinning);
        ImmediateStop = false;
        // StopSpinButton.gameObject.SetActive(false);
        StopSpinButton.interactable = true;
    }


    /// <summary>
    /// Coroutine for handling the spin routine and shared with all type of free spins.
    /// </summary>
    /// <param name="OnSpinAnimStart">Action to be executed when the spin animation starts. used in <param name="OnSpin"</param>
    /// <param name="OnSpinAnimStop">Action to be executed when the spin animation stops. used in <param name="OnSpin"</param>
    /// <param name="playBeforeStart">Flag to indicate if any action should be played before the spin animation starts. used in <param name="OnSpin"</param>
    /// <param name="playBeforeEnd">Flag to indicate if any action should be played before the spin animation ends. used in <param name="OnSpin"</param>
    /// <param name="delay">Delay for <param name="OnSpinAnimStart"> action </param>
    /// <param name="delay1">Delay  for <param name="OnSpinAnimStop">Action</param>
    IEnumerator SpinRoutine(Action OnSpinAnimStart = null, Action OnSpinAnimStop = null, bool playBeforeStart = false, bool playBeforeEnd = false, float delay = 0, float delay1 = 0)
    {
        // Start the spin and check if has sufficient balance
        bool start = OnSpinStart();

        if (!start)
        {
            // If the balance not sufficent, stop spinning and handle auto spin
            isSpinning = false;
            if (isAutoSpin)
            {
                StartCoroutine(StopAutoSpinCoroutine());
            }

            ToggleButtonGrp(true);
            yield break;
        }

        // Deduct balance if not in free spin mode
        if (!isFreeSpin)
            uIManager.DeductBalanceAnim(SocketModel.playerData.Balance - currentTotalBet, SocketModel.playerData.Balance);

        // Execute the spin animation
        yield return OnSpin(OnSpinAnimStart, OnSpinAnimStop, playBeforeStart, playBeforeEnd, delay, delay1);
        yield return OnSpinEnd();

        // Check if there are free spins available
        if (SocketModel.resultGameData.freeSpinCount > 0 && !isFreeSpin)
        {
            // Stop auto spin if active
            if (autoSpinRoutine != null)
            {
                yield return StopAutoSpinCoroutine(true);
                if (autoSpinLeft > 0)
                    autoSpinShouldContinue = true;
            }

            // Update free spin count and UI
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = SocketModel.resultGameData.freeSpinCount;
            uIManager.UpdateFreeSpinInfo(freeSpinCount);

            // Start free spin routine
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            audioController.playBgAudio("Bonus");

            yield break;
        }
        // Check if there are thunder free spins available
        else if (SocketModel.resultGameData.thunderSpinCount > 0 && !thunderFreeSpins && !isFreeSpin)
        {
            // Stop auto spin if active
            if (autoSpinRoutine != null)
            {
                yield return StopAutoSpinCoroutine(true);
                if (autoSpinLeft > 0)
                    autoSpinShouldContinue = true;
            }

            // Update free spin count and UI
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = SocketModel.resultGameData.thunderSpinCount;
            uIManager.UpdateFreeSpinInfo(freeSpinCount);

            // Start free spin routine
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            audioController.playBgAudio("Bonus");
            yield break;
        }

        // If not in auto spin or free spin mode, stop spinning and enable buttons
        if (!isAutoSpin && !isFreeSpin)
        {
            isSpinning = false;
            ToggleButtonGrp(true);
        }
    }
    // Starts the spin and handles necessary actions before the spin
    bool OnSpinStart()
    {
        // audioController.PlayButtonAudio("spin");
        isSpinning = true;
        winIterationCount = 0;
        audioController.StopWLAaudio();
        paylineSymbolAnimPanel.gameObject.SetActive(false);
        if (symbolAnim != null)
            StopCoroutine(symbolAnim);

        StopAllWinAnimation();
        //check if the balance is sufficient
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

    /// <summary>
    /// Coroutine for handling the spin process.
    /// </summary>
    /// <param name="OnSpinStart">Action to be executed when the spin starts. inherited from <param name="SpinRoutine"</param>
    /// <param name="OnSpinStop">Action to be executed when the spin stops. inherited from <param name="SpinRoutine"</param>
    /// <param name="playBeforeStart">Flag to indicate if any action should be played before the spin starts. inherited from <param name="SpinRoutine"</param>
    /// <param name="playBeforeEnd">Flag to indicate if any action should be played before the spin ends. inherited from <param name="SpinRoutine"</param>
    /// <param name="delay">Delay for <param name="OnSpinAnimStart"> action </param>
    /// <param name="delay1">Delay  for <param name="OnSpinAnimStop">Action</param>
    internal IEnumerator OnSpin(Action OnSpinStart, Action OnSpinStop, bool playBeforeStart, bool playBeforeEnd, float delay1, float delay2)
    {
        if (!isAutoSpin && !isFreeSpin)
            StopSpinButton.gameObject.SetActive(true);

        // Send spin data to the server
        var spinData = new { data = new { currentBet = betCounter, currentLines = 20, spins = 1 }, id = "SPIN" };
        socketController.SendData("message", spinData);

        yield return slotManager.StartSpin(turboMode, ImmediateStop);
        slotManager.shuffleInitialMatrix();
        slotManager.CLearAllCoins();
        yield return new WaitUntil(() => SocketController.isResultdone);
        currentBalance = SocketModel.playerData.Balance;

        if (!playBeforeStart)
        {
            // Execute action after the spin starts
            OnSpinStart?.Invoke();
            if (delay1 > 0)
                yield return new WaitForSeconds(delay1);
        }

        // handle turbo mode
        if (!turboMode)
            yield return new WaitForSeconds(0.45f);
        else
            yield return new WaitForSeconds(0.35f);

        // Populate the slot matrix with the result data
        slotManager.PopulateSLotMatrix(SocketModel.resultGameData.ResultReel, SocketModel.resultGameData.frozenIndices);

        if (playBeforeEnd)
        {
            // Execute action before the spin ends
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);
        }
        // Start the spin stop animation
        yield return slotManager.StopSpin(ignore: !thunderFreeSpins,
        playStopSound: audioController.PlaySpinStopAudio,
        isFreeSpin: isFreeSpin,
        turboMode: turboMode);

        if (!playBeforeEnd)
        {
            // Execute action after the spin ends
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);
        }

        if (StopSpinButton.gameObject.activeSelf)
            StopSpinButton.gameObject.SetActive(false);
    }


    // Coroutine for handling the end of the spin.
  
    IEnumerator OnSpinEnd()
    {
        // audioController.StopSpinAudio();

        //showing all win lines and symbols animation once
        SingleLoopAnimation(true);

        uIManager.UpdatePlayerInfo(SocketModel.playerData);
        if (SocketModel.resultGameData.freeSpinIndices.Count > 0 || SocketModel.resultGameData.frozenIndices.Count > 0 || SocketModel.resultGameData.linesToEmit.Count > 0)
            yield return new WaitForSeconds(1f);

        audioController.StopWLAaudio();

        if (SocketModel.playerData.currentWining > 0 && !thunderFreeSpins)
        {
            winanimRunning = true;
            CheckWinPopups(SocketModel.playerData.currentWining);
            uIManager.NormalWinAnimation();
            yield return new WaitUntil(() => !winanimRunning);

        }
        if (isFreeSpin)
            uIManager.UpdateFreeSpinInfo(winnings: SocketModel.playerData.currentWining);

        slotManager.StopIconAnimation();

        if (thunderFreeSpins)
            yield break;

        if (isAutoSpin || isFreeSpin)
        {
            //showing all win lines animation once
            SingleLoopAnimation();
            yield break;
        }

        if (SocketModel.resultGameData.linesToEmit.Count == 1)
        {
            //showing all win lines animation once
            SingleLoopAnimation();
            yield break;
        }

        //showing all win lines animation by iteration
        if (SocketModel.resultGameData.linesToEmit.Count > 1)
        {
            symbolAnim = StartCoroutine(PayLineSymbolRoutine(false));
        }
        yield return null;
    }

    /// <summary>
    /// Executes a single loop animation for the symbols and paylines.
    /// </summary>
    /// <param name="showall">Flag to indicate if all animations should be shown.</param>
    private void SingleLoopAnimation(bool showall = false)
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

        if (!showall)
            return;

        if (SocketModel.resultGameData.frozenIndices.Count > 0 && !isFreeSpin && !thunderFreeSpins)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartIconAnimation(Helper.ConvertFrozenIndicesToCoord(SocketModel.resultGameData.frozenIndices), paylineSymbolAnimPanel);
        }

        if (SocketModel.resultGameData.freeSpinIndices.Count > 0)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartIconAnimation(SocketModel.resultGameData.freeSpinIndices, paylineSymbolAnimPanel);
        }
    }

    // Toggles the interactability of the buttons in during the spin
    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (AutoSpinPopup_Button) AutoSpinPopup_Button.interactable = toggle;
        if (Bet_Button) Bet_Button.interactable = toggle;
        uIManager.Settings_Button.interactable = toggle;
        if (infoButton) infoButton.interactable = toggle;
    }

    /// <summary>
    /// Handles the change in bet counter and updates the total bet.
    /// </summary>
    /// <param name="index">The index of the selected bet.</param>
    private void OnBetChange(int index)
    {
        if (audioController) audioController.PlayButtonAudio();

        Debug.Log(index);
        betCounter = index;
        currentTotalBet = SocketModel.initGameData.Bets[betCounter] * totalLies;
        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
        // if (currentBalance < currentTotalBet)
        //     uIManager.LowBalPopup();
    }




    /// <summary>
    /// Checks and displays win popups based on the amount won.
    /// </summary>
    /// <param name="amount">The amount won.</param>
    void CheckWinPopups(double amount)
    {
        if (winAnim != null)
            StopCoroutine(winAnim);

        uIManager.ClosePopup();
        winAnim = null;
        if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
        {
            uIManager.EnableWinPopUp(1);
            winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
            audioController.PlayWLAudio("big");
            Invoke(nameof(StopWinAnimImmediate), 3f);

        }
        else if (amount >= currentTotalBet * 15 && amount < currentTotalBet * 20)
        {
            uIManager.EnableWinPopUp(2);
            winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
            audioController.PlayWLAudio("big");
            Invoke(nameof(StopWinAnimImmediate), 3f);

        }
        else if (amount >= currentTotalBet * 20)
        {
            uIManager.EnableWinPopUp(3);
            winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
            audioController.PlayWLAudio("mega");
            Invoke(nameof(StopWinAnimImmediate), 3f);

        }
        else
        {
            winanimRunning = false;
            audioController.PlayWLAudio();

        }

    }

    // Handles win popups and audio for Thunder Free Spins
    void ThunderWinPopups(double amount)
    {
        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();
        winAnim = null;
        uIManager.EnableWinPopUp(4);
        winAnim = StartCoroutine(uIManager.WinTextAnim(SocketModel.playerData.currentWining));
        Invoke(nameof(StopWinAnimImmediate), 2.5f);

        // Play different audio based on the amount won
        if (amount >= currentTotalBet * 10 && amount < currentTotalBet * 15)
            audioController.PlayWLAudio("big");
        else if (amount >= currentTotalBet * 15 && amount < currentTotalBet * 20)
            audioController.PlayWLAudio("big");
        else if (amount >= currentTotalBet * 20)
            audioController.PlayWLAudio("mega");
        else
            audioController.PlayWLAudio();
    }

    /// <summary>
    /// Coroutine for handling the animation of payline symbols.
    /// </summary>
    /// <param name="oneTime">Flag to indicate if the iteration should be played only once.</param>
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
        SingleLoopAnimation();
    }

    // Stops the win animation immediately.
    void StopWinAnimImmediate()
    {
        // if(!isFreeSpin && !thunderFreeSpins){
        
        if (winAnim != null)
            StopCoroutine(winAnim);
        uIManager.ClosePopup();
        winanimRunning = false;
        // }
    }


    //Stops all win animations and resets the game state.
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

    /// <summary>
    /// Coroutine for checking and starting the appropriate free spin based on the selected character coming from backend.
    /// </summary>
    /// <param name="arthur">Flag to indicate if Arthur free spin should be started.</param>
    /// <param name="polly">Flag to indicate if Polly free spin should be started.</param>
    /// <param name="thunder">Flag to indicate if Thunder free spin should be started.</param>
    /// <param name="tommy">Flag to indicate if Tommy free spin should be started.</param>
    /// <param name="initiate">Flag to indicate if the free spin should be initiated.</param>
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
                count: SocketModel.resultGameData.thunderSpinCount,
                ResultReel: SocketModel.resultGameData.ResultReel
            );

            thunderFreeSpins = false;
        }
        else if (!arthur && !polly && !thunder && tommy)
        {
            yield return tommyFP.StartFP(
                count: SocketModel.resultGameData.freeSpinCount);
        }
        else
        {
            Debug.Log("More than two flags are true");
            yield break;
        }
    }



}

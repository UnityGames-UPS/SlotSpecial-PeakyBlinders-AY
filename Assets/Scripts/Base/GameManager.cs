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

    private Coroutine autoSpinRoutine;
    private Coroutine freeSpinRoutine;
    private Coroutine symbolAnim;

    private Coroutine spinRoutine;
    [SerializeField] private int winIterationCount;

    [SerializeField] private int freeSpinCount;

    [SerializeField] private bool isFreeSpin;


    private bool initiated;
    bool thunderFreeSpins;
    void Start()
    {
        SetButton(SlotStart_Button, ExecuteSpin, true);


for (int i = 0; i < AutoSpinsButtons.Length; i++)
{
    int capturedIndex = i; // Capture the current value of 'i'
    AutoSpinsButtons[capturedIndex].onClick.AddListener(() =>
    {
        ExecuteAutoSpin(capturedIndex);
        uIManager.ClosePopup();
        audioController.PlayButtonAudio("spin");
    });
}
        SetButton(AutoSpinStop_Button, () => StartCoroutine(StopAutoSpinCoroutine()));
        // SetButton(ToatlBetMinus_Button, () => OnBetChange(false));
        SetButton(freeSpinStartButton, () => freeSpinRoutine = StartCoroutine(FreeSpinRoutine()));



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

        arthurFP.iconref.AddRange(slotManager.iconImages);
        arthurFP.populateOriginalMatrix = slotManager.PopulateSLotMatrix;

        tommyFP.spriteRef.AddRange(slotManager.iconImages);
        tommyFP.SpinRoutine = SpinRoutine;
        tommyFP.UpdateUI = uIManager.UpdateFreeSpinInfo;

        thunderFP.updateUI = uIManager.UpdateFreeSpinInfo;
        thunderFP.SpinRoutine = SpinRoutine;

        arthurFP.SpinRoutine = SpinRoutine;
        arthurFP.UpdateUI = uIManager.UpdateFreeSpinInfo;

        pollyFP.SpinRoutine = SpinRoutine;
        pollyFP.UpdateUI = uIManager.UpdateFreeSpinInfo;

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


    void ExecuteAutoSpin(int index)
    {
        Debug.Log(index);
        if (!isSpinning && autoOptions[index] > 0)
        {
            autoSpinCounter=index;
            isAutoSpin = true;
            autoSpinText.text = autoOptions[index].ToString();
            autoSpinText.transform.parent.gameObject.SetActive(true);
            // AutoSpin_Button.gameObject.SetActive(false);

            AutoSpinStop_Button.gameObject.SetActive(true);
            autoSpinRoutine = StartCoroutine(AutoSpinRoutine());
        }

    }

    IEnumerator FreeSpinRoutine(bool initiate = true)
    {
        uIManager.ToggleFreeSpinPanel(true);
        uIManager.EnablePurplebar(true);
        uIManager.CloseFreeSpinPopup();
        isFreeSpin = true;
        yield return CheckNStartFP(
            arthur: SocketModel.resultGameData.isArthurBonus,
            tommy: SocketModel.resultGameData.isTomBonus,
            polly: SocketModel.resultGameData.isPollyBonus,
            thunder: SocketModel.resultGameData.isThunderSpin,
            initiate: initiate
        );
        yield return new WaitForSeconds(1f);
        slotManager.ResetAllSymbols();
        audioController.playBgAudio("FP");
        uIManager.ToggleFreeSpinPanel(false);
        ToggleButtonGrp(true);
        isSpinning = false;
        isFreeSpin = false;
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
            }
            int prevFreeSpin = freeSpinCount;
            freeSpinCount = SocketModel.resultGameData.freeSpinCount;
            uIManager.UpdateFreeSpinInfo(freeSpinCount);
            uIManager.FreeSpinPopup(freeSpinCount, true);
            paylineSymbolAnimPanel.gameObject.SetActive(false);
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
            }

            StopAllCoroutines();
            uIManager.FreeSpinPopup(freeSpinCount, true);
            paylineSymbolAnimPanel.gameObject.SetActive(false);
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
        slotManager.ResetAllSymbols();
        payLineController.ResetLines();
        paylineSymbolAnimPanel.gameObject.SetActive(false);

        ToggleButtonGrp(false);
        uIManager.ClosePopup();
        return true;


    }

    internal IEnumerator OnSpin(Action OnSpinStart, Action OnSpinStop, bool playBeforeStart, bool playBeforeEnd, float delay1, float delay2)
    {
        var spinData = new { data = new { currentBet = betCounter, currentLines = 1, spins = 1 }, id = "SPIN" };
        socketController.SendData("message", spinData);
        // if (delay2 == 0)
        // {
        //     OnSpinStart?.Invoke();
        //     yield return slotManager.StartSpin();
        // }
        // else
        // {
        //     yield return slotManager.StartSpin();
        //     OnSpinStart?.Invoke();
        //     yield return new WaitForSeconds(0.5f);
        // }
        if (playBeforeStart)
        {
            OnSpinStart?.Invoke();
            if (delay1 > 0)
                yield return new WaitForSeconds(delay1);
            yield return slotManager.StartSpin();

        }
        else
        {
            yield return slotManager.StartSpin();
            OnSpinStart?.Invoke();
            if (delay1 > 0)
                yield return new WaitForSeconds(delay1);
        }
        yield return new WaitUntil(() => SocketController.isResultdone);
        yield return new WaitForSeconds(0.35f);
        // slotManager.StopIconAnimation();
        slotManager.PopulateSLotMatrix(SocketModel.resultGameData.ResultReel, SocketModel.resultGameData.frozenIndices);
        currentBalance = SocketModel.playerData.Balance;

        if (playBeforeEnd)
        {
            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);

            ;
            yield return slotManager.StopSpin(playStopSound: audioController.PlaySpinStopAudio);


        }
        else
        {
            // audioController.PlaySpinStopAudio();
            yield return slotManager.StopSpin(playStopSound: audioController.PlaySpinStopAudio);

            OnSpinStop?.Invoke();
            if (delay2 > 0)
                yield return new WaitForSeconds(delay2);

        }
        // yield return slotManager.StopSpin();
        // OnSpinStop?.Invoke();

        // if (delay1 > 0)
        //     yield return new WaitForSeconds(delay1);

        yield return new WaitForSeconds(0.2f);
        // if (audioController) 


    }
    IEnumerator OnSpinEnd()
    {
        audioController.StopSpinAudio();
        if (SocketModel.resultGameData.symbolsToEmit.Count > 0)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartIconAnimation(Helper.RemoveDuplicates(SocketModel.resultGameData.symbolsToEmit), paylineSymbolAnimPanel);
        }

        if (SocketModel.resultGameData.frozenIndices.Count > 0 && !isFreeSpin)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartCoinAnimation(SocketModel.resultGameData.frozenIndices, paylineSymbolAnimPanel);
        }

        if (SocketModel.resultGameData.linesToEmit.Count > 0)
        {
            for (int i = 0; i < SocketModel.resultGameData.linesToEmit.Count; i++)
            {
                payLineController.GeneratePayline(SocketModel.resultGameData.linesToEmit[i]);
            }
        }
        yield return new WaitForSeconds(0.5f);
        slotManager.StopIconAnimation();
        yield return PayLineSymbolRoutine(true);
        audioController.StopWLAaudio();
        if (SocketModel.resultGameData.symbolsToEmit.Count > 0)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartIconAnimation(Helper.RemoveDuplicates(SocketModel.resultGameData.symbolsToEmit), paylineSymbolAnimPanel);
        }

        if (SocketModel.resultGameData.frozenIndices.Count > 0 && !isFreeSpin)
        {
            paylineSymbolAnimPanel.gameObject.SetActive(true);
            slotManager.StartCoinAnimation(SocketModel.resultGameData.frozenIndices, paylineSymbolAnimPanel);
        }

        if (SocketModel.resultGameData.linesToEmit.Count > 0)
        {
            for (int i = 0; i < SocketModel.resultGameData.linesToEmit.Count; i++)
            {
                payLineController.GeneratePayline(SocketModel.resultGameData.linesToEmit[i]);
            }
        }

        uIManager.UpdatePlayerInfo(SocketModel.playerData);

        if (SocketModel.playerData.currentWining > 0 && !thunderFreeSpins)
        {

            CheckWinPopups(SocketModel.playerData.currentWining);
            yield return uIManager.WinTextAnim(SocketModel.playerData.currentWining);
            audioController.StopWLAaudio();

        }
        if (isFreeSpin)
            uIManager.UpdateFreeSpinInfo(winnings: SocketModel.playerData.currentWining);

        if (!isAutoSpin && !isFreeSpin && SocketModel.resultGameData.linesToEmit.Count > 1)
        {
            slotManager.StopIconAnimation();
            symbolAnim = StartCoroutine(PayLineSymbolRoutine(false));
        }



        yield return null;
    }



    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (AutoSpinPopup_Button) AutoSpinPopup_Button.interactable = toggle;
        if (Bet_Button) Bet_Button.interactable = toggle;
        uIManager.Settings_Button.interactable = toggle;
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
        if (amount >= currentTotalBet * 5 && amount < currentTotalBet * 7.5)
        {
            uIManager.EnableWinPopUp(1);
            audioController.PlayWLAudio();

        }
        else if (amount >= currentTotalBet * 7.5 && amount < currentTotalBet * 10)
        {
            uIManager.EnableWinPopUp(2);
            audioController.PlayWLAudio("big");

        }
        else if (amount >= currentTotalBet * 10)
        {
            uIManager.EnableWinPopUp(3);
            audioController.PlayWLAudio("mega");

        }
        else
        {
            uIManager.EnableWinPopUp(0);
            audioController.PlayWLAudio();

        }
    }


    IEnumerator PayLineSymbolRoutine(bool oneTime)
    {
        if (SocketModel.resultGameData.symbolsToEmit.Count == 0)
            yield break;
        paylineSymbolAnimPanel.gameObject.SetActive(true);

        int loopDuration = 1;

        while (loopDuration > 0)
        {
            for (int i = 0; i < SocketModel.resultGameData.linesToEmit.Count; i++)
            {

                slotManager.PlaySymbolAnim(SocketModel.resultGameData.symbolsToEmit[i], paylineSymbolAnimPanel);
                payLineController.GeneratePayline(SocketModel.resultGameData.linesToEmit[i]);
                yield return new WaitForSeconds(1);
                payLineController.ResetLines();
                slotManager.StopSymbolAnim(SocketModel.resultGameData.symbolsToEmit[i]);

            }
            if (oneTime)
                loopDuration--;
            yield return null;
        }


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

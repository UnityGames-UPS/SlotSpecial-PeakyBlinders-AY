using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using DG.Tweening;


public class SocketController : MonoBehaviour
{

    public Player playerData = new Player();
    public UiData uIData = new UiData();

    public GameData initGameData = new GameData();

    public Root resultGameData = new Root();



    // internal SocketModel socketModel = new SocketModel();
    internal GameData InitialData = null;
    internal UiData UIData = null;
    internal Root ResultData = null;
    internal Player PlayerData = null;

    //WebSocket currentSocket = null;
    internal static bool isResultdone = false;

    private SocketManager manager;
    [SerializeField] private UIManager uiManager;

    // protected string nameSpace="game"; //BackendChange
    private Socket gameSocket; //BackendChanges
    [SerializeField] internal JSFunctCalls JSManager;

    //[SerializeField]
    //private string SocketURI;

    protected string SocketURI = null;
    // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
    // protected string TestSocketURI = "https://7p68wzhv-5000.inc1.devtunnels.ms/";
    [SerializeField] protected string TestSocketURI = "http://localhost:5000/";
    //protected string SocketURI = "http://localhost:5000";

    [SerializeField]
    private string testToken;

    protected string gameID = "SL-PB";
    // protected string gameID = "";

    internal bool isLoading;
    internal bool SetInit = false;
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

    internal Action OnInit;
    internal Action ShowDisconnectionPopup;
    protected string nameSpace = "playground"; //BackendChanges

    private bool isConnected = false; //Back2 Start
    private bool hasEverConnected = false;
    private const int MaxReconnectAttempts = 5;
    private const float ReconnectDelaySeconds = 2f;

    private float lastPongTime = 0f;
    private float pingInterval = 2f;
    private float pongTimeout = 3f;
    private bool waitingForPong = false;
    private int missedPongs = 0;
    private const int MaxMissedPongs = 5;
    private Coroutine PingRoutine; //Back2 end

    // protected string nameSpace = "game";
    private void Start()
    {
        // Debug.unityLogger.logEnabled = false;
        OpenSocket();
    }

    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received data: " + jsonData);
        // Do something with the authToken
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        nameSpace = data.nameSpace;
    }

    string myAuth = null;

    internal bool isLoaded = false;

    private void Awake()
    {
        isLoaded = false;
    }

    private void OpenSocket()
    {
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
            JSManager.SendCustomMessage("authToken");
            StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = testToken
            };
        };
        options.Auth = authFunction;
        // Proceed with connecting to the server
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            Debug.Log("My Auth is null");
            yield return null;
        }
        while (SocketURI == null)
        {
            Debug.Log("My Socket is null");
            yield return null;
        }

        Debug.Log("My Auth is not null");
        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = myAuth
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }

    private void SetupSocketManager(SocketOptions options)
    {
#if UNITY_EDITOR
        // Create and setup SocketManager for Testing
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        // Create and setup SocketManager
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif
        if (string.IsNullOrEmpty(nameSpace) | string.IsNullOrWhiteSpace(nameSpace))
        {
            gameSocket = this.manager.Socket;
        }
        else
        {
            Debug.Log("Namespace used :" + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }
        // Set subscriptions
        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
        gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnResult);
        //gameSocket.On<string>("gamble:result", OnGameResult);
        //gameSocket.On<string>("bonus:result", OnBonusResult);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived); //Back2 Start
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
        manager.Open();
    }

    void OnBonusResult(string data)
    {
        // Handle the game result here
        Debug.Log("Bonus Result: " + data);

        ParseResponse(data);

    }
    // Connected event handler implementation
    void OnConnected(ConnectResponse resp) //Back2 Start
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            uiManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    } //Back2 end
    private void OnError(Error err)
    {
        Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }
    private void OnDisconnected() //Back2 Start
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        uiManager.DisconnectionPopup();
        isConnected = false;
        ResetPingRoutine();
    } //Back2 end
    private void OnPongReceived(string data) //Back2 Start
    {
        Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end

    private void OnError(string response)
    {
        Debug.LogError("Error: " + response);
    }

    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }
    void OnResult(string data)
    {
        print(data);
        ParseResponse(data);
    }
    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
            //InitRequest("AUTH");
        }
    }

    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        uiManager.ADfunction();
    }

    private void SendPing() //Back2 Start
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }
    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private void AliveRequest()
    {
        SendDataWithNamespace("YES I AM ALIVE");
    }
    private IEnumerator PingCheck()
    {
        while (true)
        {
            Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                uiManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    uiManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    uiManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end


    public void ExtractUrlAndToken(string fullUrl)
    {
        Uri uri = new Uri(fullUrl);
        string query = uri.Query; // Gets the query part, e.g., "?url=http://localhost:5000&token=e5ffa84216be4972a85fff1d266d36d0"

        Dictionary<string, string> queryParams = new Dictionary<string, string>();
        string[] pairs = query.TrimStart('?').Split('&');

        foreach (string pair in pairs)
        {
            string[] kv = pair.Split('=');
            if (kv.Length == 2)
            {
                queryParams[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }
        }

        if (queryParams.TryGetValue("url", out string extractedUrl) &&
            queryParams.TryGetValue("token", out string token))
        {
            Debug.Log("Extracted URL: " + extractedUrl);
            Debug.Log("Extracted Token: " + token);
            testToken = token;
            SocketURI = extractedUrl;
        }
        else
        {
            Debug.LogError("URL or token not found in query parameters.");
        }
    }
    private void SendDataWithNamespace(string eventName, string json = null)
    {
        // Send the message
        if (gameSocket != null && gameSocket.IsOpen)
        {
            if (json != null)
            {
                gameSocket.Emit(eventName, json);
                Debug.Log("JSON data sent: " + json);
            }
            else
            {
                gameSocket.Emit(eventName);
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }
    //     private void PopulateSlotSocket(List<string> LineIds)
    //     {
    //         slotManager.shuffleInitialMatrix();
    //         for (int i = 0; i < LineIds.Count; i++)
    //         {
    //             slotManager.FetchLines(LineIds[i], i);
    //         }

    //         slotManager.SetInitialUI();

    //         isLoaded = true;
    // #if UNITY_WEBGL && !UNITY_EDITOR
    //         JSManager.SendCustomMessage("OnEnter");
    // #endif
    //         uiManager.RaycastBlocker.SetActive(false);
    //     }

    internal IEnumerator CloseSocket() //Back2 Start
    {
        uiManager.RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    } //Back2 end

    internal void closeSocketReactnativeCall()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnExit");
#endif
    }
    private void CloseSocketMesssage(string eventName)
    {
        SendDataWithNamespace("EXIT");
    }
    private void ParseResponse(string jsonObject)
    {
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;
        // if (resp["message"].Type == JTokenType.Object && resp["message"]["PlayerData"] != null)
        // {
        //     SocketModel.playerData = resp["message"]["PlayerData"].ToObject<PlayerData>();
        // }

        switch (id)
        {
            case "initData":
                {
                    uIData = myData.uiData;
                    //  SocketModel.uIData.specialBonusSymbolMulipliers = resp["message"]["GameData"]["specialBonusSymbolMulipliers"].ToObject<List<SpecialSymbol>>();
                    initGameData = myData.gameData;
                    //initGameData.lines = myData.gameData.lines;
                    playerData = myData.player;
                    OnInit?.Invoke();
                    //     Debug.Log("init data" + JsonConvert.SerializeObject(initGameData));
                    uiManager.RaycastBlocker.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif
                    break;
                }
            case "ResultData":
                {

                    resultGameData = myData;
                    playerData = myData.player;
                    //  Debug.Log("result data" + JsonConvert.SerializeObject(resultGameData));
                    isResultdone = true;
                    break;
                }

            case "ExitUser":
                {
                    if (this.manager != null)
                    {
                        Debug.Log("Dispose my Socket");
                        this.manager.Close();
                    }
                    // Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("OnExit");
#endif
                    break;
                }
        }

    }

    internal void SendData(string eventName, object message = null)
    {

        if (gameSocket == null || !gameSocket.IsOpen)
        {
            Debug.LogWarning("Socket is not connected.");
            return;
        }
        if (message == null)
        {
            gameSocket.Emit(eventName);
            return;
        }
        isResultdone = false;
        string json = JsonConvert.SerializeObject(message);
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);

    }
    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.payload = new SentDeta();
        message.type = "SPIN";
        // Debug.Log(slotManager.BetCounter);
        message.payload.betIndex = currBet;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }


    //     internal void closeSocketReactnativeCall()
    //     {
    // #if UNITY_WEBGL && !UNITY_EDITOR
    //     JSManager.SendCustomMessage("onExit");
    // #endif
    //     }





}





//===================================================================



[Serializable]
public class MessageData
{
    public string type;

    public SentDeta payload;

}

[Serializable]
public class SentDeta
{
    public int betIndex;
    public string Event;
    public double lastWinning;
    public int index;
}
public class Features
{
    public FreeSpin freeSpin { get; set; }
    public Bonus bonus { get; set; }
    public int miniMultiplier { get; set; }
    public int majorMultiplier { get; set; }
    public int megaMultiplier { get; set; }
    public int grandMultiplier { get; set; }




    public List<CoinsValue> coinsValues { get; set; }
    public bool isArthurSpin { get; set; }
    public bool isTomSpin { get; set; }
    public bool isPollySpin { get; set; }
    public bool isThunderSpin { get; set; }
    public bool isGrandPrize { get; set; }
}
public class CoinsValue
{
    public List<int> position { get; set; }
    public int coinsvalue { get; set; }
    public string symbol { get; set; }
}
public class FreeSpin
{
    public int freeSpinCount { get; set; }
    public bool isFreeSpin { get; set; }
    public bool freeSpinAdded { get; set; }
    public List<object> freeSpinIndices { get; set; }
}

public class GameData
{
    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }
}

public class Paylines
{
    public List<Symbol> symbols { get; set; }
}

public class Player
{
    public double balance { get; set; }
}
public class Symbol
{
    public int id { get; set; }
    public string name { get; set; }
    public List<int> multiplier { get; set; }
    public string description { get; set; }
}

public class UiData
{
    public Paylines paylines { get; set; }
}


public class Root
{
    public string id { get; set; }
    public GameData gameData { get; set; }
    public Features features { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }

    //result

    public bool success { get; set; }
    public List<List<string>> matrix { get; set; }
    public Payload payload { get; set; }


    // // modify
    // public List<int> linesToEmit { get; set; }
    // public List<List<string>> symbolsToEmit { get; set; }
}
public class Payload
{
    public double currentWinning { get; set; }
    public List<LineWin> lineWins { get; set; }
}
public class LineWin
{
    public int lineIndex { get; set; }
    public List<int> positions { get; set; }
}


public class Bonus
{
    public bool enabled { get; set; }
    public int numberofcoinsforbonus { get; set; }
    public int thunderIncrementCount { get; set; }


    public int thunderSpinCount { get; set; }
    public bool thunderSpinAdded { get; set; }
    public List<List<string>> reels { get; set; }
    public List<FrozenIndex> frozenIndices { get; set; }
}


public class FrozenIndex
{
    public List<int> position { get; set; }
    public int coinsvalue { get; set; }
    public string symbol { get; set; }
}




[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
    public string nameSpace; //BackendChanges
}

//===================================================================
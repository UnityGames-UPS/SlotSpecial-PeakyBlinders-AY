using System.Collections.Generic;
using System;


public static class SocketModel
{
    public static PlayerData playerData= new PlayerData();
    public static UIData uIData= new UIData();

    public static InitGameData initGameData= new InitGameData();

    public static ResultGameData resultGameData= new ResultGameData();

    public static int currentBetIndex = 0;


}



[Serializable]
public class ResultGameData
{
    public List<List<int>> ResultReel { get; set; }
    public List<int> linesToEmit { get; set; }
    public bool isFreeSpin { get; set; }
    public int freeSpinCount { get; set; }
    public List<List<string>> symbolsToEmit { get; set; }
    public bool isArthurBonus {get; set;}
    public bool isTomBonus {get; set;}
    public bool isPollyBonus {get; set;}
    public bool isThunderSpin {get; set;}
    public bool freeSpinAdded {get; set;}

    public int thunderSpinCount {get; set;}

    public bool thunderSpinAdded {get; set;}

    public bool isGrandPrize{get; set;}
    public List<List<double>> frozenIndices {get; set;}


}


[Serializable]
public class InitGameData
{
    // public List<List<int>> Lines { get; set; }
    public List<double> Bets { get; set; }
    public bool canSwitchLines { get; set; }
    public List<int> LinesCount { get; set; }
    public List<List<int>> lineData { get; set; }
    public double freeSpinCount { get; set; }
}


[Serializable]
public class UIData
{
    public List<Symbol> symbols { get; set; }
    public List<SpecialSymbol> specialBonusSymbolMulipliers {get; set;}

}

[Serializable]
public class SpecialSymbol{
    public string name {get; set;}
    public double value {get; set;}
}

[Serializable]
public class BetData
{
    public double currentBet;
    public double currentLines;
    public double spins;
    //public double TotalLines;
}

[Serializable]
public class AuthData
{
    public string GameID;
    //public double TotalLines;
}

[Serializable]
public class MessageData
{
    public BetData data;
    public string id;
}

[Serializable]
public class InitData
{
    public AuthData Data;
    public string id;
}

[Serializable]
public class AbtLogo
{
    public string logoSprite { get; set; }
    public string link { get; set; }
}



[Serializable]
public class Symbol
{
    public int ID { get; set; }
    public string Name { get; set; }
    public List<List<double>> Multiplier { get; set; }
    public object defaultAmount { get; set; }
    public object symbolsCount { get; set; }
    public object increaseValue { get; set; }
    public object description { get; set; }
    public int freeSpin { get; set; }
}



[Serializable]
public class PlayerData
{
    public double Balance { get; set; }
    public double haveWon { get; set; }
    public double currentWining { get; set; }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
}

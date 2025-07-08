using BattleTech;
using BattleTech.Framework;

namespace JwTweaks.Features;

public class TravelContractFixer
{
    private static TravelContractFixer _instance;

    public static TravelContractFixer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TravelContractFixer();
            }
            return _instance;
        }
    }
    
    public void FixIsTravelContract(Contract contract, SimGameState sim, bool forced)
    {
        var sys = contract.GameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
        bool isTravelContract = sys.ID != sim.CurSystem.ID;
        
        // because HBS never thought to preserve this data, it gets lost on save/load breaking travel contracts...fml
        if (isTravelContract || forced)
        {
            contract.Override.travelOnly = true;
            
            // yes, these are magic strings; why did HBS do this? Who the fuck knows, but it's dumb...
            var employer = contract.GetTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
            var employersAlly = contract.GetTeamFaction("70af7e7f-39a8-4e81-87c2-bd01dcb01b5e");
            var target = contract.GetTeamFaction("be77cadd-e245-4240-a93e-b99cc98902a5");
            var targetsAlly = contract.GetTeamFaction("31151ed6-cfc2-467e-98c4-9ae5bea784cf");
            var neutralToAll = contract.GetTeamFaction("61612bb3-abf9-4586-952a-0559fa9dcd75");
            var hostileToAll = contract.GetTeamFaction("3c9f3a20-ab03-4bcb-8ab6-b1ef0442bbf0");
            
            var travelSeed = sim.NetworkRandom.Int(max: int.MaxValue);
            
            SimGameResultAction gameResultAction = new SimGameResultAction();
            gameResultAction.Type = SimGameResultAction.ActionType.System_StartNonProceduralContract;
            gameResultAction.value = contract.mapName;
            gameResultAction.additionalValues = new string[14];
            gameResultAction.additionalValues[0] = sys.ID;
            gameResultAction.additionalValues[1] = contract.mapPath;
            gameResultAction.additionalValues[2] = contract.encounterObjectGuid;
            gameResultAction.additionalValues[3] = contract.Override.ID;
            gameResultAction.additionalValues[4] = false.ToString();
            gameResultAction.additionalValues[5] = employer.Name;
            gameResultAction.additionalValues[6] = target.Name;
            gameResultAction.additionalValues[7] = contract.Difficulty.ToString();
            gameResultAction.additionalValues[8] = "true";
            gameResultAction.additionalValues[9] = targetsAlly.Name;
            gameResultAction.additionalValues[10] = travelSeed.ToString();
            gameResultAction.additionalValues[11] = employersAlly.Name;
            gameResultAction.additionalValues[12] = neutralToAll.Name;
            gameResultAction.additionalValues[13] = hostileToAll.Name;
            
            SimGameEventResult simGameEventResult = new SimGameEventResult();
            simGameEventResult.Actions = new SimGameResultAction[1];
            simGameEventResult.Actions[0] = gameResultAction;
            
            // remove previous results just in case we already processed this contract
            contract.Override.OnContractSuccessResults.Clear();
            contract.Override.OnContractSuccessResults.Add(simGameEventResult);
            contract.Override.travelSeed = travelSeed;
        }
        
    }
    
    public static void LogContract(Contract contract)
    {
        if (!JTCore.settings.Debug) return;
        
        var properties = typeof(Contract).GetProperties();
        var fields = typeof(Contract).GetFields();
        JTCore.modLog.Info?.Write("Writing Contract Properties:");
        foreach (var property in properties)
        {
            JTCore.modLog.Info?.Write($"{property.Name}: {property.GetValue(contract)}");
        }
        foreach (var property in fields)
        {
            JTCore.modLog.Info?.Write($"{property.Name}: {property.GetValue(contract)}");
        }
        
        var overrideProperties = typeof(ContractOverride).GetProperties();
        var overrideFields = typeof(ContractOverride).GetFields();
        JTCore.modLog.Info?.Write("Writing ContractOverride Properties:");
        foreach (var property in overrideProperties)
        {
            JTCore.modLog.Info?.Write($"{property.Name}: {property.GetValue(contract.Override)}");
        }
        foreach (var property in overrideFields)
        {
            JTCore.modLog.Info?.Write($"{property.Name}: {property.GetValue(contract.Override)}");
        }
    }
}
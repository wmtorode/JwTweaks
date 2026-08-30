using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using JwTweaks.Data;

namespace JwTweaks.Features;

public class ContractRequirementsEnhancer
{
    private const string IS_OWNER = "IsOwner";
    private const string AND_IS_OWNER = "AndIsOwner";
    private const string IS_GREAT_HOUSE = "IsGreatHouse";
    private const string AND_IS_GREAT_HOUSE = "AndIsGreatHouse";
    private const string IS_CLAN = "IsClan";
    private const string AND_IS_CLAN = "AndIsClan";
    private const string IS_PIRATE = "IsPirate";
    private const string AND_IS_PIRATE = "AndIsPirate";
    
    private List<String> SPECIAL_COMPARISONS = new List<String>
    {
        IS_OWNER, 
        AND_IS_OWNER, 
        IS_GREAT_HOUSE, 
        AND_IS_GREAT_HOUSE, 
        IS_CLAN, 
        IS_PIRATE,
        AND_IS_CLAN,
        AND_IS_PIRATE
    };
    
    private static ContractRequirementsEnhancer _instance;

    public static ContractRequirementsEnhancer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ContractRequirementsEnhancer();
            }
            return _instance;
        }
    }
    
    public List<string> GetPotentialFactions(
        StarSystem system,
        List<string> sourceFactions,
        List<ComparisonDef> comparisons,
        SimGameState simGameState)
    {
        List<string> listToModify = new List<string>();
        SimGameState.FilteredComparisonResults whiteListComparisons = simGameState.GetWhiteListComparisons(comparisons);
        SimGameState.FilteredComparisonResults blackListResult = simGameState.GetBlackListComparisons(comparisons);
        if (whiteListComparisons.IsEmpty)
        {
            listToModify.AddRange(sourceFactions);
        }
        else
        {
            FactionValue invalidUnset = FactionEnumeration.GetInvalidUnsetFactionValue();
            listToModify = whiteListComparisons.Factions.Where( f => f != invalidUnset.Name 
                                                                     && sourceFactions.Contains(f)).ToList();
            if (whiteListComparisons.Strings.Contains(IS_GREAT_HOUSE))
                AddOrRemoveGreatHouses(EFilterType.Add, listToModify, sourceFactions);
            if (whiteListComparisons.Strings.Contains(IS_CLAN))
                AddOrRemoveClans(EFilterType.Add, listToModify, sourceFactions);
            if (whiteListComparisons.Strings.Contains(IS_PIRATE))
                AddOrRemovePirates(EFilterType.Add, listToModify, sourceFactions);
            if (whiteListComparisons.Strings.Contains(IS_OWNER))
                AddOrRemoveSystemOwner(EFilterType.Add, listToModify, system, sourceFactions);
            if (whiteListComparisons.Strings.Contains(AND_IS_GREAT_HOUSE))
                AddOrRemoveGreatHouses(EFilterType.Exclusive, listToModify, sourceFactions);
            if (whiteListComparisons.Strings.Contains(AND_IS_CLAN))
                AddOrRemoveClans(EFilterType.Exclusive, listToModify, sourceFactions);
            if (whiteListComparisons.Strings.Contains(AND_IS_PIRATE))
                AddOrRemovePirates(EFilterType.Exclusive, listToModify, sourceFactions);
            if (whiteListComparisons.Strings.Contains(AND_IS_OWNER))
                AddOrRemoveSystemOwner(EFilterType.Exclusive, listToModify, system, sourceFactions);
        }
        if (!blackListResult.IsEmpty)
        {
            listToModify.RemoveAll(e => blackListResult.Factions.Contains(e));
            if (blackListResult.Strings.Contains(IS_GREAT_HOUSE))
                AddOrRemoveGreatHouses(EFilterType.Remove, listToModify, sourceFactions);
            if (blackListResult.Strings.Contains(IS_CLAN))
                AddOrRemoveClans(EFilterType.Remove, listToModify, sourceFactions);
            if (blackListResult.Strings.Contains(IS_PIRATE))
                AddOrRemovePirates(EFilterType.Remove, listToModify, sourceFactions);
            if (blackListResult.Strings.Contains(IS_OWNER))
                AddOrRemoveSystemOwner(EFilterType.Remove, listToModify, system, sourceFactions);
            if (blackListResult.Strings.Contains(AND_IS_GREAT_HOUSE))
                AddOrRemoveGreatHouses(EFilterType.Exclude, listToModify, sourceFactions);
            if (blackListResult.Strings.Contains(AND_IS_CLAN))
                AddOrRemoveClans(EFilterType.Exclude, listToModify, sourceFactions);
            if (blackListResult.Strings.Contains(AND_IS_PIRATE))
                AddOrRemovePirates(EFilterType.Exclude, listToModify, sourceFactions);
            if (blackListResult.Strings.Contains(AND_IS_OWNER))
                AddOrRemoveSystemOwner(EFilterType.Exclude, listToModify, system, sourceFactions);
            if (blackListResult.Strings.Contains("IsGreatHouse"))
                simGameState.AddOrRemoveGreatHousesToList(false, listToModify);
            if (blackListResult.Strings.Contains("IsOwner"))
                listToModify.RemoveAll(e => e == system.OwnerValue.Name);
        }
        return listToModify;
    }
    
    public SimGameState.FilteredComparisonResults GetFilteredComparisons(
        IEnumerable<ComparisonDef> comparisons,
        Func<ComparisonDef, bool> ComparisonFunc,
        SimGameState simGameState)
    {
        IEnumerable<string> strings = comparisons.Where( 
            c => ComparisonFunc(c)).Select(
            c => c.obj.Split('.')).Where(
            s => simGameState.IsProperLength(s)).Select( s => s[1]);
        List<string> stringList = new List<string>();
        List<string> factions = new List<string>();
        foreach (string factionID in strings)
        {
            stringList.Add(factionID);
            if (SPECIAL_COMPARISONS.Contains(factionID))
            {
                factions.Add(FactionEnumeration.GetInvalidUnsetFactionValue().Name);
            }
            else
            {
                factions.Add(simGameState.GetFactionValueFromString(factionID).Name);
            }
        }
        return new SimGameState.FilteredComparisonResults(stringList, factions);
    }

    private void AddOrRemoveClans(EFilterType filterType, List<string> listToModify, IEnumerable<string> whiteList = null)
    {
        var factions = FactionEnumeration.FactionList.FindAll(faction => faction.IsClan);
        AddRemoveOrFilterFactions(filterType, listToModify, factions, whiteList);
        
    }
    
    private void AddOrRemovePirates(EFilterType filterType, List<string> listToModify, IEnumerable<string> whiteList = null)
    {
        var factions = FactionEnumeration.FactionList.FindAll(faction => faction.IsPirate);
        AddRemoveOrFilterFactions(filterType, listToModify, factions, whiteList);
        
    }
    
    private void AddOrRemoveGreatHouses(EFilterType filterType, List<string> listToModify, IEnumerable<string> whiteList = null)
    {
        var factions = FactionEnumeration.FactionList.FindAll(faction => faction.IsGreatHouse);
        AddRemoveOrFilterFactions(filterType, listToModify, factions, whiteList);
        
    }
    
    private void AddOrRemoveSystemOwner(EFilterType filterType, List<string> listToModify, StarSystem starSystem,  IEnumerable<string> whiteList = null)
    {
        var factions = new List<FactionValue>
        {
            starSystem.OwnerValue
        };
        AddRemoveOrFilterFactions(filterType, listToModify, factions, whiteList);
        
    }

    private void AddRemoveOrFilterFactions(EFilterType filterType, List<string> listToModify, List<FactionValue> factions,
        IEnumerable<string> whiteList = null)
    {
        switch (filterType)
        {
            case EFilterType.Exclusive:
            case EFilterType.Exclude:
                FilterExclusiveFactions(filterType, listToModify, factions);
                break;
            default:
                AddOrRemoveFactionsToList(filterType == EFilterType.Add, listToModify, factions, whiteList);
                break;
        }
    }
    
    private void FilterExclusiveFactions(EFilterType op, List<string> listToModify, IEnumerable<FactionValue> factions)
    {
        var factionsToRemove = new List<string>();
        var factionNames = factions.Select(faction => faction.Name).ToList();
        
        foreach (var factionId in listToModify)
        {
            switch (op)
            {
                case EFilterType.Exclusive:
                    if (!factionNames.Contains(factionId))
                        factionsToRemove.Add(factionId);
                    break;
                case EFilterType.Exclude:
                    if (factionNames.Contains(factionId))
                        factionsToRemove.Add(factionId);
                    break;
                
            }
            
        }

        foreach (var factionId in factionsToRemove)
        {
            listToModify.RemoveAll(e=> e == factionId);
        }
    }
    
    private void AddOrRemoveFactionsToList(
        bool add,
        List<string> listToModify,
        List<FactionValue> factionValues,
        IEnumerable<string> whiteList = null)
    {
        if (whiteList == null)
            whiteList = new List<string>();
        foreach (FactionValue factionValue in factionValues)
        {
            if (add)
            {
                if (whiteList.Contains(factionValue.Name))
                    listToModify.Add(factionValue.Name);
            }
            else if (listToModify.Contains(factionValue.Name))
                listToModify.RemoveAll(e => e ==factionValue.Name);
        }
    }
}
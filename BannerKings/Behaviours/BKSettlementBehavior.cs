﻿using BannerKings.Managers.Buildings;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Models.Vanilla;
using BannerKings.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;
using BannerKings.Components;
using BannerKings.Extensions;
using BannerKings.Managers.Populations.Villages;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions;
using static BannerKings.Managers.PopulationManager;
using System.Reflection;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace BannerKings.Behaviours
{
    public class BKSettlementBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, OnSiegeAftermath);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void TickSettlementData(Settlement settlement)
        {
            UpdateSettlementPops(settlement);
            BannerKingsConfig.Instance.PolicyManager.InitializeSettlement(settlement);
        }

        private void OnSiegeAftermath(MobileParty attackerParty, Settlement settlement,
            SiegeAftermathAction.SiegeAftermath aftermathType,
            Clan previousSettlementOwner,
            Dictionary<MobileParty, float> partyContributions)
        {
            if (settlement?.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null)
            {
                return;
            }

            float stabilityLoss = 0f;
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (aftermathType == SiegeAftermathAction.SiegeAftermath.ShowMercy)
            {
                stabilityLoss = 0.1f;
                if (attackerParty.LeaderHero != null)
                {
                    Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(attackerParty.LeaderHero);

                    if (rel != null && settlement.Culture.StringId == BannerKingsConfig.EmpireCulture &&
                        rel.HasDoctrine(DefaultDoctrines.Instance.RenovatioImperi))
                    {
                        stabilityLoss = 0f;
                    }
                }
            }
            else
            {
                float shareToKill = aftermathType == SiegeAftermathAction.SiegeAftermath.Pillage ?
                    MBRandom.RandomFloatRanged(0.1f, 0.16f) :
                    MBRandom.RandomFloatRanged(0.16f, 0.24f);
                stabilityLoss = aftermathType == SiegeAftermathAction.SiegeAftermath.Pillage ? 0.25f : 0.45f;
                int killTotal = (int)(data.TotalPop * shareToKill);
                var weights = GetDesiredPopTypes(settlement).Select(pair => new ValueTuple<PopType, float>(pair.Key, pair.Value[0])).ToList();

                if (killTotal <= 0)
                {
                    return;
                }

                while (killTotal > 0)
                {
                    var random = MBRandom.RandomInt(10, 20);
                    var target = MBRandom.ChooseWeighted(weights);
                    var finalNum = MBMath.ClampInt(random, 0, data.GetTypeCount(target));
                    data.UpdatePopType(target, -finalNum);
                    killTotal -= finalNum;
                }

                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=ocT5sL1n}{NUMBER} people have been killed in the siege aftermath of {SETTLEMENT}.")
                        .SetTextVariable("NUMBER", killTotal)
                        .SetTextVariable("SETTLEMENT", settlement.Name)
                        .ToString()));
            }

            data.Stability -= stabilityLoss;
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || settlement.StringId.Contains("tutorial") || settlement.StringId.Contains("Ruin"))
            {
                return;
            }

            TickSettlementData(settlement);
            TickRotting(settlement);
            TickManagement(settlement);
            TickTown(settlement);
            TickCastle(settlement);
            TickVillage(settlement);
        }

        private void HandleIssues(Settlement settlement)
        {
            if (settlement.Town == null)
            {
                return;
            }

            Hero governor = settlement.Town.Governor;
            if (governor == null)
            {
                return;
            }

            RunWeekly(() =>
            {
                IssueBase issue = null;
                foreach (var notable in settlement.Notables)
                {
                    if (notable.Issue != null)
                    {
                        issue = notable.Issue;
                        break;
                    }
                }

                if (issue != null && MBRandom.RandomFloat < (governor.GetSkillValue(DefaultSkills.Steward) / 1000f))
                {
                    FinishIssue(issue, governor, settlement.OwnerClan.Leader);
                }
            },
            GetType().Name,
            false);
        }

        private void FinishIssue(IssueBase issue, Hero handler, Hero clanLeader)
        {
            handler.AddSkillXp(DefaultSkills.Steward, 1000f * TaleWorlds.CampaignSystem.Campaign.Current.Models.IssueModel.GetIssueDifficultyMultiplier());

            if (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(clanLeader.Clan,
                DefaultCouncilTasks.Instance.ManageDemesne, out var competence))
            {
                issue.CompleteIssueWithLordSolutionWithRefuseCounterOffer();
            }
            else
            {
                issue.CompleteIssueWithAiLord(null);
            }
        }

        private void TickTown(Settlement settlement)
        {
            if (settlement.Town != null)
            {
                var town = settlement.Town;
                HandleItemAvailability(town);
                //HandleExcessWorkforce(data, town);
                //HandleExcessFood(town);
                HandleMarketGold(town);
                HandleGarrison(town);
            }
        }

        private void TickManagement(Settlement target)
        {
            RunWeekly(() =>
            {
                if (target == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target))
                {
                    return;
                }

                var owner = target.IsVillage ? target.Village.GetActualOwner() : target.OwnerClan.Leader;
                if (owner.Clan == Clan.PlayerClan)
                {
                    return;
                }

                var kingdom = target.OwnerClan.Kingdom;
                var currentDecisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(target);
                var changedDecisions = new List<BannerKingsDecision>();

                var town = target.Town;
                if (town is { Governor: { } })
                {
                    if (town.FoodStocks < town.FoodStocksUpperLimit() * 0.2f && town.FoodChange < 0f)
                    {
                        var rationDecision =
                            (BKRationDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                        rationDecision.Enabled = true;
                        changedDecisions.Add(rationDecision);
                    }
                    else
                    {
                        var rationDecision =
                            (BKRationDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                        rationDecision.Enabled = false;
                        changedDecisions.Add(rationDecision);
                    }

                    var garrison = town.GarrisonParty;
                    if (garrison != null)
                    {
                        float wage = garrison.TotalWage;
                        var income = TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town).ResultNumber;
                        if (wage >= income * 0.5f)
                        {
                            BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                                new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Dischargement, target));
                        }
                        else if (wage <= income * 0.2f)
                        {
                            BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                                new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Enlistment, target));
                        }
                        else
                        {
                            BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                                new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Standard, target));
                        }
                    }

                    if (town.LoyaltyChange < 0)
                    {
                        UpdateTaxPolicy(1, target);
                    }
                    else
                    {
                        UpdateTaxPolicy(-1, target);
                    }

                    if (kingdom != null)
                    {
                        var enemies = FactionManager.GetEnemyKingdoms(kingdom);
                        var atWar = enemies.Any();

                        if (target.Owner.GetTraitLevel(DefaultTraits.Calculating) > 0)
                        {
                            var subsidizeMilitiaDecision =
                                (BKSubsidizeMilitiaDecision)currentDecisions.FirstOrDefault(x =>
                                    x.GetIdentifier() == "decision_militia_subsidize");
                            subsidizeMilitiaDecision.Enabled = atWar ? true : false;
                            changedDecisions.Add(subsidizeMilitiaDecision);
                        }
                    }

                    var criminal = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "criminal");
                    var mercy = target.Owner.GetTraitLevel(DefaultTraits.Mercy);
                    var targetCriminal = mercy switch
                    {
                        > 0 => new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Forgiveness, target),
                        < 0 => new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Execution, target),
                        _ => new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Enslavement, target)
                    };

                    if (targetCriminal.Policy != criminal.Policy)
                    {
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, targetCriminal);
                    }

                    var taxSlavesDecision =
                        (BKTaxSlavesDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                    if (target.Owner.GetTraitLevel(DefaultTraits.Authoritarian) > 0)
                    {
                        taxSlavesDecision.Enabled = true;
                    }
                    else if (target.Owner.GetTraitLevel(DefaultTraits.Egalitarian) > 0)
                    {
                        taxSlavesDecision.Enabled = false;
                    }

                    changedDecisions.Add(taxSlavesDecision);

                    var workforce = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "workforce");
                    var workforcePolicies = new List<ValueTuple<WorkforcePolicy, float>> { (WorkforcePolicy.None, 1f) };
                    var saturation = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).LandData
                        .WorkforceSaturation;
                    if (saturation > 1f)
                    {
                        workforcePolicies.Add((WorkforcePolicy.Land_Expansion, 2f));
                    }

                    if (town.Security < 20f)
                    {
                        workforcePolicies.Add((WorkforcePolicy.Martial_Law, 2f));
                    }

                    BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target,
                        new BKWorkforcePolicy(MBRandom.ChooseWeighted(workforcePolicies), target));

                    foreach (var dec in changedDecisions)
                    {
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementDecision(target, dec);
                    }
                }
                else if (target.IsVillage && target.Village.Bound.Town.Governor != null)
                {
                    var villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).VillageData;
                    villageData.StartRandomProject();
                    var hearths = target.Village.Hearth;
                    switch (hearths)
                    {
                        case < 300f:
                            UpdateTaxPolicy(-1, target);
                            break;
                        case > 1000f:
                            UpdateTaxPolicy(1, target);
                            break;
                    }
                }
            },
            GetType().Name,
            false);
        }

        private static void UpdateTaxPolicy(int value, Settlement settlement)
        {
            var tax = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            var taxType = tax.Policy;
            if ((value == 1 && taxType != TaxType.Low) || (value == -1 && taxType != TaxType.High))
            {
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement,
                    new BKTaxPolicy(taxType + value, settlement));
            }
        }

        private void HandleGarrison(Town town)
        {
            if (town.IsUnderSiege)
            {
                return;
            }

            /*var parties = new Mobile();
            var list = parties.GetPartiesAroundPosition(town.Settlement.GatePosition, 10f);

            MobileParty party = list.FirstOrDefault(x => x.Party.TotalStrength > 25f && (x.IsBandit ||
               (x.MapFaction.IsKingdomFaction && x.IsLordParty) && x.MapFaction != null &&
               town.MapFaction.GetStanceWith(x.MapFaction).IsAtWar));

            if (party != null)
            {
                EvaluateSendGarrison(town.Settlement, party);
            }*/
        }

        private void EvaluateSendGarrison(Settlement origin, MobileParty target)
        {
            if (origin == null || target == null)
            {
                return;
            }

            var distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(target, origin);
            if (distance > 10f)
            {
                return;
            }

            var garrison = origin.Town.GarrisonParty;
            if (origin.IsUnderSiege || garrison == null || garrison.MemberRoster.TotalHealthyCount < 100)
            {
                return;
            }

            MobileParty garrisonParty = GarrisonPartyComponent.CreateParty(origin, target);
            if (garrisonParty != null)
            {
                (garrisonParty.PartyComponent as GarrisonPartyComponent).TickHourly();
            }
        }

        private void HandleMarketGold(Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (!town.IsTown)
                {
                    return;
                }

                if (town.Gold < 50000)
                {
                    var notable = town.Settlement.Notables.FirstOrDefault(x => x.Gold >= 30000);
                    if (notable != null)
                    {
                        town.ChangeGold(1000);
                        notable.ChangeHeroGold(-1000);
                        notable.AddPower(10f);
                    }
                }
                else if (town.Gold > 100000)
                {
                    town.ChangeGold((int)(town.Gold * -0.01f));
                }
            }, GetType().Name);
        }

        private void HandleItemAvailability(Town town)
        {
            RunWeekly(() =>
            {
                if (!town.IsTown)
                {
                    return;
                }

                Dictionary<ItemCategory, int> desiredAmounts = new Dictionary<ItemCategory, int>();
                ItemRoster itemRoster = town.Owner.ItemRoster;

                desiredAmounts.Add(DefaultItemCategories.UltraArmor, (int)(town.Prosperity / 750f));
                desiredAmounts.Add(DefaultItemCategories.HeavyArmor, (int)(town.Prosperity / 400f));
                desiredAmounts.Add(DefaultItemCategories.MeleeWeapons5, (int)(town.Prosperity / 750f));
                desiredAmounts.Add(DefaultItemCategories.MeleeWeapons4, (int)(town.Prosperity / 400f));
                desiredAmounts.Add(DefaultItemCategories.RangedWeapons5, (int)(town.Prosperity / 1500f));
                desiredAmounts.Add(DefaultItemCategories.RangedWeapons4, (int)(town.Prosperity / 1000f));

                var behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<WorkshopsCampaignBehavior>();
                var getItem = AccessTools.Method(behavior.GetType(), "GetRandomItemAux", new Type[] { typeof(ItemCategory), typeof(Town) });

                foreach (var pair in desiredAmounts)
                {
                    var quantity = 0;
                    foreach (ItemRosterElement element in itemRoster)
                    {
                        var category = element.EquipmentElement.Item.ItemCategory;
                        if (category == pair.Key)
                        {
                            quantity++;
                        }
                    }

                    if (quantity < desiredAmounts[pair.Key])
                    {
                        EquipmentElement item = (EquipmentElement)getItem.Invoke(behavior, new object[] { pair.Key, town });
                        if (item.Item != null)
                        {
                            itemRoster.AddToCounts(item, 1);
                        }
                    }
                }
            },
            GetType().Name,
            false);
        }

        private void HandleExcessWorkforce(LandData data, Town town)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (data.WorkforceSaturation > 1f)
                {
                    var workers = data.AvailableWorkForce * (data.WorkforceSaturation - 1f);
                    var items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                    {
                        foreach (var tuple in from vil in town.Villages select BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement) into popData from tuple in BannerKingsConfig.Instance.PopulationManager.GetProductions(popData) where tuple.Item1.IsTradeGood && !tuple.Item1.IsFood select tuple)
                        {
                            items.Add(tuple.Item1);
                        }
                    }

                    if (items.Count > 0)
                    {
                        var random = items.GetRandomElementInefficiently();
                        var itemCount = (int)(workers * 0.01f);
                        BuyOutput(town, random, itemCount, town.GetItemPrice(random));
                    }
                }
            }, GetType().Name);
        }

        private void HandleExcessFood(Town town)
        {
            RunWeekly(() =>
            {
                if (town.FoodStocks >= town.FoodStocksUpperLimit() - 10)
                {
                    var items = new HashSet<ItemObject>();
                    if (town.Villages.Count > 0)
                    {
                        foreach (var vil in town.Villages)
                        {
                            var villagePopData = BannerKingsConfig.Instance.PopulationManager.GetPopData(vil.Settlement);
                            if (villagePopData != null && villagePopData.VillageData != null)
                            {
                                foreach (var tuple in BannerKingsConfig.Instance.PopulationManager.GetProductions(villagePopData))
                                {
                                    items.Add(tuple.Item1);
                                }
                            }
                        }
                    }
                    if (TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementFoodModel is not BKFoodModel)
                    {
                        return;
                    }

                    var foodModel = (BKFoodModel)TaleWorlds.CampaignSystem.Campaign.Current.Models.SettlementFoodModel;
                    var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                    if (popData == null)
                    {
                        return;
                    }

                    var data = popData.LandData;
                    var excess = foodModel.GetPopulationFoodProduction(popData, town).ResultNumber - 10 - foodModel.GetPopulationFoodConsumption(popData).ResultNumber;
                    //float pasturePorportion = data.Pastureland / data.Acreage;

                    var farmFood = MBMath.ClampFloat(data.Farmland * data.GetAcreOutput("farmland"), 0f, excess);
                    if (town.IsCastle)
                    {
                        farmFood *= 0.1f;
                    }

                    while (farmFood > 1f)
                    {
                        foreach (var item in items)
                        {
                            if (!item.IsFood)
                            {
                                continue;
                            }

                            var count = farmFood > 10f
                                ? (int)MBMath.ClampFloat(farmFood * MBRandom.RandomFloat, 0f, farmFood)
                                : (int)farmFood;
                            if (count == 0)
                            {
                                break;
                            }

                            BuyOutput(town, item, count, town.GetItemPrice(item));
                            farmFood -= count;
                        }
                    }
                }
            },
            GetType().Name,
            false);
        }

        private void TickCastle(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                ItemConsumptionBehavior itemBehavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<ItemConsumptionBehavior>();
                itemBehavior.GetType().GetMethod("MakeConsumptionInTown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(itemBehavior, new object[] { settlement.Town, new Dictionary<ItemCategory, int>(10) });

                WorkshopsCampaignBehavior workshopBehavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<WorkshopsCampaignBehavior>();
                MethodInfo runWk = workshopBehavior.GetType().GetMethod("RunTownWorkshop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (Workshop wk in settlement.Town.Workshops)
                {
                    runWk.Invoke(workshopBehavior, new object[] { settlement.Town, wk });
                }

                if (settlement.Town?.GarrisonParty == null)
                {
                    return;
                }

                Building barracks = settlement.Town.Buildings.FirstOrDefault(x => x.BuildingType.StringId == BKBuildings.Instance.CastleRetinue.StringId);
                if (barracks != null && barracks.CurrentLevel > 0)
                {
                    float max = 20 * barracks.CurrentLevel;
                    int current = 0;

                    MobileParty garrison = settlement.Town.GarrisonParty;
                    foreach (var element in garrison.MemberRoster.GetTroopRoster())
                    {
                        if (Utils.Helpers.IsRetinueTroop(element.Character))
                        {
                            current += element.Number;
                        }
                    }

                    if (current < max)
                    {
                        garrison.MemberRoster.AddToCounts(settlement.Culture.EliteBasicTroop, 1);
                    }
                }
                if (settlement.Town.FoodStocks <= settlement.Town.FoodStocksUpperLimit() * 0.05f &&
                    settlement.Town.Settlement.Stash != null)
                {
                    ConsumeStash(settlement);
                }
            }
        }

        private void TickVillage(Settlement settlement)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (settlement.IsVillage)
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    if (data == null)
                    {
                        return;
                    }

                    var villageData = data.VillageData;
                    if (villageData == null)
                    {
                        return;
                    }

                    float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                    if (!(manor > 0))
                    {
                        return;
                    }

                    var toRemove = new List<MobileParty>();
                    foreach (var party in settlement.Parties)
                    {
                        if (party.PartyComponent is RetinueComponent)
                        {
                            toRemove.Add(party);
                        }
                    }

                    if (toRemove.Count > 0)
                    {
                        toRemove.RemoveAt(0);
                        foreach (var party in toRemove)
                        {
                            DestroyPartyAction.Apply(null, party);
                        }
                    }

                    var retinue = RetinueComponent.CreateRetinue(settlement);
                    if (retinue != null)
                    {
                        (retinue.PartyComponent as RetinueComponent).DailyTick(manor);
                    }
                }
            }, this.GetType().Name);
        }

        private void BuyOutput(Town town, ItemObject item, int count, int price)
        {
            var itemFinalPrice = (int)(price * (float)count);
            if (town.IsTown)
            {
                town.Owner.ItemRoster.AddToCounts(item, count);
                town.ChangeGold(-itemFinalPrice);
            }
            else
            {
                town.Settlement.Stash.AddToCounts(item, count);
                town.OwnerClan.Leader.ChangeHeroGold(-itemFinalPrice);
                if (town.OwnerClan.Leader == Hero.MainHero)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=OeCpEGzz}You have been charged {GOLD} for the excess production of {ITEM}, now in your stash at {CASTLE}.")
                            .SetTextVariable("GOLD", $"{itemFinalPrice:n0}")
                            .SetTextVariable("ITEM", item.Name)
                            .SetTextVariable("CASTLE", town.Name)
                            .ToString()));
                }
            }
        }

        private void TickRotting(Settlement settlement)
        {
            RunWeekly(() =>
            {
                var party = settlement.Party;
                var roster = party?.ItemRoster;
                if (roster == null)
                {
                    return;
                }

                var maxStorage = 1000f;
                if (settlement.Town != null)
                {
                    maxStorage += settlement.Town.Buildings.Where(b => b.BuildingType == DefaultBuildingTypes.CastleGranary || b.BuildingType == DefaultBuildingTypes.SettlementGranary).Sum(b => b.CurrentLevel * 5000f);
                }

                RotRosterFood(roster, maxStorage);
                if (settlement.Stash != null)
                {
                    RotRosterFood(settlement.Stash, settlement.IsCastle ? maxStorage : 1000f);
                }
            },
            GetType().Name,
            false);
        }

        private void RotRosterFood(ItemRoster roster, float maxStorage)
        {
            if (!(roster.TotalFood > maxStorage))
            {
                return;
            }

            var toRot = (int)(roster.TotalFood * 0.01f);
            foreach (var element in roster.ToList().FindAll(x => x.EquipmentElement.Item != null &&
                                                                 x.EquipmentElement.Item.ItemCategory.Properties ==
                                                                 ItemCategory.Property.BonusToFoodStores))
            {
                if (toRot <= 0)
                {
                    break;
                }

                var result = (int)MathF.Min(MBRandom.RandomFloatRanged(10f, toRot), (float)element.Amount);
                roster.AddToCounts(element.EquipmentElement, -result);
                toRot -= result;
            }
        }

        private void ConsumeStash(Settlement settlement)
        {
            var elements = settlement.Stash.Where(element => element.EquipmentElement.Item != null && element.EquipmentElement.Item.CanBeConsumedAsFood()).ToList();

            float food = 0;
            foreach (var element in elements)
            {
                food += element.Amount * (float)element.EquipmentElement.Item.GetItemFoodValue();
                settlement.Stash.Remove(element);
            }

            if (food > 0)
            {
                settlement.Town.FoodStocks += (int)food;
            }
        }
    }
}

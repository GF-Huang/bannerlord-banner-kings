using System;
using System.Linq;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.Core.ItemCategory;

namespace BannerKings.Utils
{
    public static class Helpers
    {
        public static void ApplyFeat(FeatObject feat, PartyBase party, ref ExplainedNumber result)
        {
            if (PartyBaseHelper.HasFeat(party, feat))
            {
                if (feat.IncrementType == FeatObject.AdditionType.Add)
                    result.Add(feat.EffectBonus, feat.Name);
                else result.AddFactor(feat.EffectBonus, feat.Name);
            }
        }

        public static void AddTraitLevel(Hero hero, TraitObject trait, int level, float chance = 1f)
        {
            int current = hero.GetTraitLevel(trait);
            int final = current + level;
            if ((final >= trait.MinValue || final <= trait.MaxValue) && MBRandom.RandomFloat < chance)
            {
                hero.SetTraitLevel(trait, final);
                if (hero == Hero.MainHero)
                {
                    string value = GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), 
                        (level + MathF.Abs(trait.MinValue)).ToString())
                        .ToString();

                    if (BKTraits.Instance.PersonalityTraits.Contains(trait))
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=fmoNZHeO}Your sense of {TRAIT} is now perceived by others as {LEVEL}.")
                            .SetTextVariable("TRAIT", trait.Name)
                            .SetTextVariable("LEVEL", value).ToString(),
                            Color.UIntToColorString(TextHelper.COLOR_LIGHT_YELLOW)));
                    }
                    else if (BKTraits.Instance.MedicalTraits.Contains(trait))
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=fmoNZHeO}Your sense of {TRAIT} is now perceived by others as {LEVEL}.")
                            .SetTextVariable("TRAIT", trait.Name)
                            .SetTextVariable("LEVEL", value).ToString(),
                            Color.UIntToColorString(TextHelper.COLOR_LIGHT_YELLOW)));
                    }
                }
            }
        }

        public static ItemModifierGroup GetItemModifierGroup(ItemObject item)
        {
            ItemModifierGroup modifierGroup = null;
            if (item.ArmorComponent != null)
            {
                modifierGroup = item.ArmorComponent.ItemModifierGroup;
            }
            else if (item.WeaponComponent != null)
            {
                modifierGroup = item.WeaponComponent.ItemModifierGroup;
            }
            else if (item.IsFood)
            {
                modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("consumables");
            }
            else if (item.IsAnimal)
            {
                modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("animals");
            }
            else if (!item.HasHorseComponent && item.ItemCategory != DefaultItemCategories.Iron)
            {
                modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("goods");
            }

            return modifierGroup;
        }

        public static void SetAlliance(IFaction faction1, IFaction faction2)
        {
            var stance = Clan.PlayerClan.GetStanceWith(Hero.OneToOneConversationHero.Clan);
            if (stance.IsNeutral)
            {
                stance.IsAllied = true;
                if (faction1 == Hero.MainHero.MapFaction || faction2 == Hero.MainHero.MapFaction)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=gc8D4iH4}The {FACTION1} and {FACTION2} are now allies.")
                        .SetTextVariable("FACTION1", faction1.Name)
                        .SetTextVariable("FACTION2", faction2.Name),
                        100,
                        null,
                        GetKingdomDecisionSound());
                }
            }
        }

        internal static string GetRelationDecisionSound() => "event:/ui/notification/relation";
        internal static string GetKingdomDecisionSound() => "event:/ui/notification/kingdom_decision";

        public static void AddCharacterToKeep(Hero hero, Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(hero.CharacterObject, 0));
            var locCharacter = new LocationCharacter(agent, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
        }

        public static void AddCharacterToKeep(CharacterObject character, Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(character, 0));
            var locCharacter = new LocationCharacter(agent, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
        }

        public static void AddNotableToKeep(Hero notable, Settlement settlement)
        {
            if (notable != null && settlement != null)
            {
                var town = settlement.Town;

                LocationCharacter locCharacter = town.Settlement.LocationComplex.GetLocationCharacterOfHero(notable);
                if (locCharacter != null)
                {
                    locCharacter.SpecialTargetTag = null;

                    Location characterLocation = town.Settlement.LocationComplex.GetLocationOfCharacter(notable);
                    if (characterLocation.StringId != "lordshall")
                    {
                        town.Settlement.LocationComplex.ChangeLocation(locCharacter, characterLocation,
                                            settlement.LocationComplex.GetLocationWithId("lordshall"));
                    }
                }
            }
        }

        public static void AddMusicianToKeep(Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(settlement.Culture.Musician, 0));
            var locCharacter = new LocationCharacter(agent,
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "musician", 
                true,
                LocationCharacter.CharacterRelations.Neutral, 
                ActionSetCode.GenerateActionSetNameWithSuffix(agent.AgentMonster, agent.AgentIsFemale, "_musician"),
                true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; },
                settlement.Culture, 
                LocationCharacter.CharacterRelations.Neutral, 1);

            var townsmanSuffix = FaceGen.GetMonsterWithSuffix(settlement.Culture.Townsman.Race, "_settlement");
            var tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(townsmanSuffix, false, "_villager"), townsmanSuffix);
            var townsman = new LocationCharacter(new AgentData(
                new SimpleAgentOrigin(settlement.Culture.Townsman, -1, null, default(UniqueTroopDescriptor)))
                .Monster(tuple.Item2)
                .Age(MBRandom.RandomInt(30, 60)), 
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), 
                null, 
                false, 
                LocationCharacter.CharacterRelations.Friendly, 
                tuple.Item1, 
                true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
               .AddLocationCharacters(delegate { return townsman; },
               settlement.Culture,
               LocationCharacter.CharacterRelations.Friendly, 10);
        }


        public static bool IsClanLeader(Hero hero)
        {
            return hero.Clan != null && hero.Clan.Leader == hero;
        }

        public static bool IsCloseFamily(Hero hero, Hero family)
        {
            return hero.Father == family || hero.Mother == family || hero.Children.Contains(family) ||
                   hero.Siblings.Contains(family) || hero.Spouse == family;
        }

        public static int GetRosterCount(TroopRoster roster, string filter = null)
        {
            var rosters = roster.GetTroopRoster();
            var count = 0;

            rosters.ForEach(rosterElement =>
            {
                if (filter == null)
                {
                    if (!rosterElement.Character.IsHero)
                    {
                        count += rosterElement.Number + rosterElement.WoundedNumber;
                    }
                }
                else if (!rosterElement.Character.IsHero && rosterElement.Character.StringId.Contains(filter))
                {
                    count += rosterElement.Number + rosterElement.WoundedNumber;
                }
            });

            return count;
        }

        public static TextObject GetClassName(PopType type, CultureObject culture) => 
            DefaultPopulationNames.Instance.GetPopulationName(culture, type).Name;

        public static string GetConsumptionHint(ConsumptionType type)
        {
            return type switch
            {
                ConsumptionType.Luxury => new TextObject("{=2wwjFQ2A}Satisfaction over availability of products such as jewelry, velvets and fur.").ToString(),
                ConsumptionType.Industrial => new TextObject("{=irOAqrdy}Satisfaction over availability of manufacturing products such as leather, clay and tools.").ToString(),
                ConsumptionType.General => new TextObject("{=NENnF6oJ}Satisfaction over availability of various products, including military equipment and horses.").ToString(),
                _ => new TextObject("{=QJ1pjKxw}Satisfaction over availability of food types.").ToString()
            };
        }

        public static bool IsRetinueTroop(CharacterObject character)
        {
            var nobleRecruit = character?.Culture?.EliteBasicTroop;
            bool result = false;

            if (nobleRecruit == null || nobleRecruit.UpgradeTargets == null)
            {
                return false;
            }

            ExceptionUtils.TryCatch(() =>
            {
                while (nobleRecruit.UpgradeTargets != null && nobleRecruit.UpgradeTargets.Count() > 0)
                {
                    result = character == nobleRecruit || nobleRecruit.UpgradeTargets.Contains(character);
                    if (result)
                    {
                        break;
                    }
                    else
                    {
                        nobleRecruit = nobleRecruit.UpgradeTargets[0];
                    }
                }
            }, 
            Type.GetType("BannerKings.Utils.Helpers").Name,
            false);

            return result;
        }

        public static CultureObject GetCulture(string id)
        {
            var culture = MBObjectManager.Instance.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == id);
            return culture;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemCategory item)
        {
            var id = item.StringId;
            if (item.Properties == Property.BonusToFoodStores)
            {
                return ConsumptionType.Food;
            }

            if (id is "silver" or "jewelry" or "spice" or "velvet" or "war_horse" ||
                id.EndsWith("4") || id.EndsWith("5"))
            {
                return ConsumptionType.Luxury;
            }

            if (id is "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools" 
                || id.EndsWith("3") || id.Contains("horse"))
            {
                return ConsumptionType.Industrial;
            }

            return ConsumptionType.General;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemObject item)
        {
            var id = item.StringId;
            switch (id)
            {
                case "silver" or "jewelry" or "spice" or "velvet" or "fur":
                    return ConsumptionType.Luxury;
                case "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools":
                    return ConsumptionType.Industrial;
            }

            if (item.IsFood)
            {
                return ConsumptionType.Food;
            }

            if (item.IsInitialized && !item.IsBannerItem &&
                (item.HasArmorComponent || item.HasWeaponComponent || item.IsAnimal ||
                 item.IsTradeGood || item.HasHorseComponent) && item.StringId != "undefined")
            {
                return ConsumptionType.General;
            }

            return ConsumptionType.None;
        }
    }
}
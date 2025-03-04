﻿using System.Collections.Generic;
using BannerKings.Behaviours.Criminality;
using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Behaviours.Feasts;
using BannerKings.Behaviours.Marriage;
using BannerKings.Behaviours.PartyNeeds;
using BannerKings.Behaviours.Mercenary;
using BannerKings.Behaviours.Workshops;
using BannerKings.Components;
using BannerKings.Managers;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Grace;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Duties;
using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Innovations.Eras;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using BannerKings.Managers.Institutions.Religions.Faiths.Battania;
using BannerKings.Managers.Institutions.Religions.Faiths.Empire;
using BannerKings.Managers.Institutions.Religions.Faiths.Northern;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Institutions.Religions.Faiths.Vlandia;
using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Kingdoms.Council;
using BannerKings.Managers.Kingdoms.Peerage;
using BannerKings.Managers.Kingdoms.Succession;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Populations.Tournament;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.Kingdoms.Council.BKCouncilPositionDecision;
using static BannerKings.Behaviours.Diplomacy.Groups.InterestGroup;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKDraftPolicy;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;
using static BannerKings.Managers.Policies.BKMilitiaPolicy;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Populations.Estates.Estate;
using CasusBelli = BannerKings.Behaviours.Diplomacy.Wars.CasusBelli;
using BannerKings.Managers.Institutions.Religions.Faiths.Eastern;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Goals;
using BannerKings.Behaviours.Shipping;
using BannerKings.Campaign;

namespace BannerKings
{
    internal class SaveDefiner : SaveableTypeDefiner
    {
        public SaveDefiner() : base(82818189)
        {
        }

        protected override void DefineClassTypes()
        {
            AddEnumDefinition(typeof(PopType), 1);
            AddClassDefinition(typeof(PopulationClass), 2);
            AddClassDefinition(typeof(MilitaryData), 3);
            AddClassDefinition(typeof(CultureData), 4);
            AddClassDefinition(typeof(EconomicData), 5);
            AddClassDefinition(typeof(LandData), 6);
            AddClassDefinition(typeof(PopulationData), 7);
            AddClassDefinition(typeof(BannerKingsDecision), 8);
            AddClassDefinition(typeof(BannerKingsPolicy), 9);
            AddEnumDefinition(typeof(TaxType), 10);
            AddEnumDefinition(typeof(MilitiaPolicy), 11);
            AddEnumDefinition(typeof(WorkforcePolicy), 12);
            AddClassDefinition(typeof(PopulationManager), 13);
            AddClassDefinition(typeof(PolicyManager), 14);
            AddClassDefinition(typeof(PopulationPartyComponent), 15);
            AddClassDefinition(typeof(MilitiaComponent), 16);
            AddEnumDefinition(typeof(GarrisonPolicy), 17);
            AddEnumDefinition(typeof(CriminalPolicy), 18);
            AddClassDefinition(typeof(TournamentData), 19);
            AddClassDefinition(typeof(VillageData), 20);
            AddClassDefinition(typeof(VillageBuilding), 21);
            AddClassDefinition(typeof(CultureDataClass), 22);
            AddClassDefinition(typeof(FeudalTitle), 23);
            AddClassDefinition(typeof(FeudalContract), 24);
            AddEnumDefinition(typeof(TitleType), 25);
            AddEnumDefinition(typeof(FeudalDuties), 26);
            AddEnumDefinition(typeof(FeudalRights), 27);

            AddClassDefinition(typeof(TitleManager), 32);
            AddClassDefinition(typeof(CouncilMember), 34);
            AddClassDefinition(typeof(CouncilData), 35);
            AddClassDefinition(typeof(CourtManager), 36);
            AddEnumDefinition(typeof(DraftPolicy), 37);
            AddClassDefinition(typeof(BKCriminalPolicy), 38);
            AddClassDefinition(typeof(BKDraftPolicy), 39);
            AddClassDefinition(typeof(BKGarrisonPolicy), 40);
            AddClassDefinition(typeof(BKMilitiaPolicy), 41);
            AddClassDefinition(typeof(BKTaxPolicy), 42);
            AddClassDefinition(typeof(BKWorkforcePolicy), 43);
            AddClassDefinition(typeof(BKRationDecision), 44);
            AddClassDefinition(typeof(BKExportSlavesDecision), 45);
            AddClassDefinition(typeof(BKTaxSlavesDecision), 46);
            AddClassDefinition(typeof(BKEncourageMilitiaDecision), 47);
            AddClassDefinition(typeof(BKSubsidizeMilitiaDecision), 48);
            AddClassDefinition(typeof(BKExemptTariffDecision), 49);
            AddClassDefinition(typeof(BKEncourageMercantilism), 50);
            AddClassDefinition(typeof(BannerKingsDuty), 51);
            AddClassDefinition(typeof(AuxiliumDuty), 52);
            AddClassDefinition(typeof(RansomDuty), 53);
            AddClassDefinition(typeof(BannerKingsTournament), 54);
            AddClassDefinition(typeof(BKContractDecision), 55);

            AddClassDefinition(typeof(RepublicElectionDecision), 60);
            AddClassDefinition(typeof(BKSettlementClaimantDecision), 61);
            AddClassDefinition(typeof(BKKingElectionDecision), 62);
            AddClassDefinition(typeof(TitleData), 63);
            AddEnumDefinition(typeof(ClaimType), 64);
            AddClassDefinition(typeof(RetinueComponent), 65);
            AddClassDefinition(typeof(ReligionsManager), 66);
            AddClassDefinition(typeof(Religion), 67);
            AddClassDefinition(typeof(Faith), 68);
            AddEnumDefinition(typeof(FaithStance), 69);
            AddClassDefinition(typeof(FaithfulData), 70);
            AddClassDefinition(typeof(Divinity), 71);
            AddClassDefinition(typeof(ReligionData), 72);
            AddClassDefinition(typeof(Clergyman), 73);
            AddClassDefinition(typeof(PolytheisticFaith), 74);
            AddClassDefinition(typeof(MonotheisticFaith), 75);
            AddClassDefinition(typeof(AseraFaith), 76);
            AddClassDefinition(typeof(AmraFaith), 77);
            AddClassDefinition(typeof(DarusosianFaith), 78);


            AddClassDefinition(typeof(CanticlesFaith), 86);
            AddEnumDefinition(typeof(RiteType), 87);
            AddClassDefinition(typeof(EducationData), 88);
            AddClassDefinition(typeof(BookType), 89);
            AddClassDefinition(typeof(Language), 90);
            AddClassDefinition(typeof(Lifestyle), 91);
            AddClassDefinition(typeof(EducationManager), 92);
            AddClassDefinition(typeof(Innovation), 93);
            AddClassDefinition(typeof(InnovationData), 94);
            AddClassDefinition(typeof(InnovationsManager), 95);
            AddClassDefinition(typeof(BannerKingsObject), 96);
            AddClassDefinition(typeof(StartOption), 97);
            AddClassDefinition(typeof(GoalManager), 98);
            AddEnumDefinition(typeof(MineralType), 99);
            AddClassDefinition(typeof(MineralData), 100);
            AddEnumDefinition(typeof(MineralRichness), 101);
            AddClassDefinition(typeof(DemesneLaw), 102);
            AddClassDefinition(typeof(Estate), 103);
            AddClassDefinition(typeof(EstateData), 104);
            AddEnumDefinition(typeof(EstateDuty), 105);
            AddEnumDefinition(typeof(EstateTask), 106);
            AddClassDefinition(typeof(BKDemesneLawDecision), 107);
            AddClassDefinition(typeof(Peerage), 108);
            AddClassDefinition(typeof(Feast), 109);
            AddClassDefinition(typeof(MarriageContract), 110);
            AddClassDefinition(typeof(PeerageKingdomDecision), 111);
            AddClassDefinition(typeof(BannerKingsComponent), 112);
            AddClassDefinition(typeof(GarrisonPartyComponent), 113);
            AddClassDefinition(typeof(WorkshopData), 114);
            AddClassDefinition(typeof(TreeloreFaith), 115);
            AddClassDefinition(typeof(CouncilTask), 116);
            AddClassDefinition(typeof(TargetedCouncilTask<>), 117);     
            AddClassDefinition(typeof(OverseeSanitation), 118);
            AddClassDefinition(typeof(KingdomDiplomacy), 119);
            AddClassDefinition(typeof(InterestGroup), 120);
            AddClassDefinition(typeof(DemandOutcome), 121);
            AddClassDefinition(typeof(Demand), 122);
            AddClassDefinition(typeof(CouncilPositionDemand), 123);
            AddClassDefinition(typeof(BanditHeroComponent), 124);
            AddClassDefinition(typeof(PolicyChangeDemand), 125);
            AddClassDefinition(typeof(DemesneLawChangeDemand), 126);
            AddClassDefinition(typeof(War), 127);
            AddClassDefinition(typeof(CasusBelli), 128);
            AddClassDefinition(typeof(Crime), 129);
            AddClassDefinition(typeof(MercenaryCareer), 1000);
            AddClassDefinition(typeof(MercenaryPrivilege), 1001);
            AddClassDefinition(typeof(CustomTroop), 1002);
            AddClassDefinition(typeof(CustomTroopPreset), 1003);

            AddClassDefinition(typeof(DiplomacyGroup), 130);
            AddClassDefinition(typeof(AssumeFaithDemand), 131);
            AddClassDefinition(typeof(TitleDemand), 132);
            AddClassDefinition(typeof(BKDeclareWarDecision), 133);
            AddClassDefinition(typeof(BKTournamentManager), 134);
            AddClassDefinition(typeof(EstateComponent), 135);
            AddClassDefinition(typeof(BKCouncilPositionDecision), 140);
            AddClassDefinition(typeof(PartySupplies), 141);
            AddClassDefinition(typeof(CourtGrace), 142);
            AddClassDefinition(typeof(CourtExpense), 143);
            AddClassDefinition(typeof(Osfeyd), 144);
            AddClassDefinition(typeof(Era), 145);
            AddClassDefinition(typeof(SixWinds), 146);
            AddClassDefinition(typeof(Government), 147);
            AddClassDefinition(typeof(Succession), 148);
            AddClassDefinition(typeof(Inheritance), 149);
            AddClassDefinition(typeof(GenderLaw), 150);
            AddClassDefinition(typeof(ContractAspect), 151);
            AddClassDefinition(typeof(Goal), 152);
            AddClassDefinition(typeof(ContractDuty), 153);
            AddClassDefinition(typeof(ContractRight), 154);
            AddClassDefinition(typeof(BKContractChangeDecision), 155);
            AddClassDefinition(typeof(Travel), 156);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<FeudalTitle, Kingdom>));
            ConstructContainerDefinition(typeof(List<PopulationClass>));
            ConstructContainerDefinition(typeof(List<VillageBuilding>));
            ConstructContainerDefinition(typeof(List<CultureDataClass>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, PopulationData>));
            ConstructContainerDefinition(typeof(List<BannerKingsDecision>));
            ConstructContainerDefinition(typeof(List<BannerKingsPolicy>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, List<BannerKingsPolicy>>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, List<BannerKingsDecision>>));
            ConstructContainerDefinition(typeof(Dictionary<FeudalTitle, Hero>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, FeudalTitle>));
            ConstructContainerDefinition(typeof(List<FeudalTitle>));
            ConstructContainerDefinition(typeof(Dictionary<FeudalDuties, float>));
            ConstructContainerDefinition(typeof(List<FeudalRights>));
            ConstructContainerDefinition(typeof(Dictionary<Clan, CouncilData>));
            ConstructContainerDefinition(typeof(List<CouncilMember>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, ClaimType>));
            ConstructContainerDefinition(typeof(Dictionary<FeudalTitle, float>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, List<Clan>>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, Clergyman>));
            ConstructContainerDefinition(typeof(List<Divinity>));
            ConstructContainerDefinition(typeof(Dictionary<Faith, FaithStance>));
            ConstructContainerDefinition(typeof(Dictionary<TraitObject, bool>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, FaithfulData>));
            ConstructContainerDefinition(typeof(Dictionary<RiteType, CampaignTime>));
            ConstructContainerDefinition(typeof(Dictionary<Religion, Dictionary<Hero, FaithfulData>>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, EducationData>));
            ConstructContainerDefinition(typeof(Dictionary<BookType, float>));
            ConstructContainerDefinition(typeof(Dictionary<Language, float>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, ItemRoster>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, float>));
            ConstructContainerDefinition(typeof(List<Innovation>));
            ConstructContainerDefinition(typeof(Dictionary<CultureObject, InnovationData>));
            ConstructContainerDefinition(typeof(Dictionary<Religion, float>));
            ConstructContainerDefinition(typeof(Dictionary<Town, CampaignTime>));
            ConstructContainerDefinition(typeof(Dictionary<MineralType, float>));
            ConstructContainerDefinition(typeof(Dictionary<Town, int>));
            ConstructContainerDefinition(typeof(List<DemesneLaw>));
            ConstructContainerDefinition(typeof(List<Estate>));
            ConstructContainerDefinition(typeof(Dictionary<PopType, float>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, Town>));
            ConstructContainerDefinition(typeof(Dictionary<Town, Feast>)); 
            ConstructContainerDefinition(typeof(Dictionary<Hero, List<Estate>>));
            ConstructContainerDefinition(typeof(Dictionary<Workshop, WorkshopData>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, MobileParty>)); 
            ConstructContainerDefinition(typeof(List<InterestGroup>));
            ConstructContainerDefinition(typeof(List<Demand>));
            ConstructContainerDefinition(typeof(List<DemandOutcome>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, KingdomDiplomacy>));
            ConstructContainerDefinition(typeof(List<War>));
            ConstructContainerDefinition(typeof(List<Crime>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, List<Crime>>));
            ConstructContainerDefinition(typeof(Dictionary<MobileParty, PartySupplies>));
            ConstructContainerDefinition(typeof(List<CourtExpense>));

            ConstructContainerDefinition(typeof(List<MercenaryPrivilege>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, List<MercenaryPrivilege>>));
            ConstructContainerDefinition(typeof(Dictionary<Clan, MercenaryCareer>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, float>));
            ConstructContainerDefinition(typeof(Dictionary<CultureObject, CustomTroop>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, CampaignTime>));
            ConstructContainerDefinition(typeof(Dictionary<Goal, CampaignTime>));
            ConstructContainerDefinition(typeof(Dictionary<Hero, Dictionary<Goal, CampaignTime>>));
            ConstructContainerDefinition(typeof(Dictionary<ContractDuty, CampaignTime>));
            ConstructContainerDefinition(typeof(List<ContractAspect>));
            ConstructContainerDefinition(typeof(Dictionary<MobileParty, Travel>)); 
        }
    }
}
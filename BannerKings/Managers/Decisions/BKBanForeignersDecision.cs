using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKBanForeignersDecision : BannerKingsDecision
    {
        public BKBanForeignersDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        {
            return new TextObject("{=qqkVbNWD1}Foreigners that refuse to assimilate will be gradually forced to leave the settlement").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_foreigner_ban";
        }

        public override string GetName()
        {
            return new TextObject("{=reXk9jrPS}Ban foreigners").ToString();
        }
    }
}
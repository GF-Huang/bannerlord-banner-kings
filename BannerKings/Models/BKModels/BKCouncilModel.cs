using System;
using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKCouncilModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }


        public (bool, string) IsCouncilRoyal(Clan clan)
        {
            var explanation = new TextObject("{=1SAad587X}Legal crown council.");

            var kingdom = clan.Kingdom;
            if (kingdom == null)
            {
                explanation = new TextObject("{=0zwi5UCsM}No kingdom.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            if (clan.Kingdom.RulingClan != clan)
            {
                explanation = new TextObject("{=wduX1azHP}Not the ruling clan.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (sovereign == null)
            {
                explanation = new TextObject("{=3gAhpivmz}Does not hold faction's sovereign title.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            return new ValueTuple<bool, string>(true, explanation.ToString());
        }

        public bool WillAcceptAction(CouncilAction action, Hero hero)
        {
            if (action.Type != CouncilActionType.REQUEST)
            {
                return true;
            }

            return action.Possible;
        }


        public CouncilAction GetAction(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null,
            bool appointed = false)
        {
            return type switch
            {
                CouncilActionType.REQUEST => GetRequest(type, council, requester, targetPosition, currentPosition,
                    appointed),
                CouncilActionType.RELINQUISH => GetRelinquish(type, council, requester, currentPosition, targetPosition,
                    appointed),
                _ => GetSwap(type, council, requester, targetPosition, currentPosition, appointed)
            };
        }


        private CouncilAction GetSwap(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null, bool appointed = false)
        {
            var action = new CouncilAction(type, requester, targetPosition, currentPosition, council)
            {
                Influence = GetInfluenceCost(type, targetPosition)
            };

            if (currentPosition == null || currentPosition.Member != requester)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=yDYJ4V22C}Not part of the council.");
                return action;
            }

            if (!targetPosition.IsValidCandidate(requester))
            {
                action.Possible = false;
                action.Reason = new TextObject("{=Ms3eTQeEO}Not a valid candidate.");
                return action;
            }

            if (requester.Clan != null && requester.Clan.Influence < action.Influence)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=jcM4ELukB}Not enough influence.");
                return action;
            }

            if (targetPosition.IsCorePosition(targetPosition.Position))
            {
                if (requester.Clan != null && !requester.Clan.Kingdom.Leader.IsFriend(requester))
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=MVdyQkO4s}Not trustworthy enough for this position.");
                    return action;
                }

                if (council.GetCompetence(requester, targetPosition.Position) < 0.5f)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=67u5iGH5Q}Not competent enough for this position.");
                    return action;
                }
            }

            if (targetPosition.Member != null)
            {
                var candidateDesire = GetDesirability(requester, council, targetPosition);
                var currentDesire = GetDesirability(targetPosition.Member, council, targetPosition);
                if (currentDesire > candidateDesire)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=X5ke3u6Cr}Not a better candidate than current councillor.");
                    return action;
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=xmJ3aM1W9}Action can be taken.");
            return action;
        }

        private CouncilAction GetRelinquish(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember currentPosition, CouncilMember targetPosition = null, bool appointed = false)
        {
            var action = new CouncilAction(type, requester, targetPosition, currentPosition, council)
            {
                Influence = GetInfluenceCost(type, targetPosition)
            };

            if (requester != null)
            {
                if (targetPosition == null)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=wEPVpBdQK}No position to be relinquished.");
                    return action;
                }

                if (targetPosition.Member != requester)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=cevv9hFxa}Not current councilman of the position.");
                    return action;
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=xmJ3aM1W9}Action can be taken.");
            return action;
        }

        private CouncilAction GetRequest(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null, bool appointed = false)
        {
            var action = new CouncilAction(type, requester, targetPosition, currentPosition, council)
            {
                Influence = appointed ? 0f : GetInfluenceCost(type, targetPosition)
            };

            if (currentPosition != null && currentPosition.Member == requester)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=eS4wQ7Ne7}Already part of the council.");
                return action;
            }

            if (!targetPosition.IsValidCandidate(requester))
            {
                action.Possible = false;
                action.Reason = new TextObject("{=Ms3eTQeEO}Not a valid candidate.");
                return action;
            }

            if (requester.Clan != null && requester.Clan.Influence < action.Influence)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=jcM4ELukB}Not enough influence.");
                return action;
            }


            if (!appointed)
            {
                if (targetPosition.IsCorePosition(targetPosition.Position))
                {
                    if (requester.Clan != null && !requester.Clan.Kingdom.Leader.IsFriend(requester))
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=MVdyQkO4s}Not trustworthy enough for this position.");
                        return action;
                    }

                    if (council.GetCompetence(requester, targetPosition.Position) < 0.5f)
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=67u5iGH5Q}Not competent enough for this position.");
                        return action;
                    }
                }

                if (targetPosition.Member != null)
                {
                    var candidateDesire = GetDesirability(requester, council, targetPosition);
                    var currentDesire = GetDesirability(targetPosition.Member, council, targetPosition);
                    if (currentDesire > candidateDesire)
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=X5ke3u6Cr}Not a better candidate than current councillor.");
                        return action;
                    }
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=xmJ3aM1W9}Action can be taken.");
            return action;
        }

        public float GetDesirability(Hero candidate, CouncilData council, CouncilMember position)
        {
            float titleWeight = 0;
            var competence = council.GetCompetence(candidate, position.Position);
            var relation = council.Owner.GetRelation(candidate) * 0.01f;
            if (candidate.Clan == council.Owner.Clan)
            {
                relation -= 0.2f;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(candidate);
            if (title != null)
            {
                titleWeight = 4 - (int) title.type;
            }

            return (titleWeight + competence + relation) / 3f;
        }

        public int GetInfluenceCost(CouncilActionType type, CouncilMember targetPosition)
        {
            switch (type)
            {
                case CouncilActionType.REQUEST when targetPosition.Member != null:
                    return 100;
                case CouncilActionType.REQUEST:
                    return 50;
                case CouncilActionType.RELINQUISH:
                    return 0;
            }

            if (targetPosition.Member != null)
            {
                return 50;
            }

            return 10;
        }
    }
}
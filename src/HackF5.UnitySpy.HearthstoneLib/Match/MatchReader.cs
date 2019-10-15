﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HackF5.UnitySpy.HearthstoneLib.Match
{
    class MatchReader
    {
        private HearthstoneImage image;

        public MatchReader(HearthstoneImage image)
        {
            this.image = image;
        }

        public MatchInfo GetMatchInfo()
        {
            var matchInfo = new MatchInfo();
            var gameState = image["GameState"]["s_instance"];
            image.GetService("NetCache");
            var netCacheValues = image.GetService("NetCache")?["m_netCache"]?["valueSlots"];
            if (gameState != null)
            {
                var playerIds = gameState["m_playerMap"]["keySlots"];
                var players = gameState["m_playerMap"]["valueSlots"];
                for (var i = 0; i < playerIds.Length; i++)
                {
                    if (players[i]?.TypeDefinition.Name != "Player")
                    {
                        continue;
                    }
                    var medalInfo = players[i]["m_medalInfo"];
                    var standardMedalInfo = medalInfo?["m_currMedalInfo"];
                    var wildMedalInfo = medalInfo?["m_currWildMedalInfo"];
                    var playerName = players[i]["m_name"];
                    var standardRank = standardMedalInfo != null ? GetRankValue(standardMedalInfo) : 0;
                    var standardLegendRank = standardMedalInfo?["legendIndex"] ?? 0;
                    var wildRank = wildMedalInfo != null ? GetRankValue(wildMedalInfo) : 0;
                    var wildLegendRank = wildMedalInfo?["legendIndex"] ?? 0;
                    var cardBack = players[i]["m_cardBackId"];
                    var playerId = playerIds[i];
                    var side = (Side)players[i]["m_side"];
                    var accountId = players[i]["m_gameAccountId"];
                    var account = new Account { Hi = accountId?["m_hi"] ?? 0, Lo = accountId?["m_lo"] ?? 0 };
                    var battleTag = GetBattleTag(account);
                    if (side == Side.FRIENDLY)
                    {
                        dynamic netCacheMedalInfo = null;
                        if (netCacheValues != null)
                        {
                            foreach (var netCache in netCacheValues)
                            {
                                if (netCache?.TypeDefinition.Name != "NetCacheMedalInfo")
                                {
                                    continue;
                                }
                                netCacheMedalInfo = netCache;
                                break;
                            }
                        }
                        var standardStars = netCacheMedalInfo?["<Standard>k__BackingField"]["<Stars>k__BackingField"];
                        var wildStars = netCacheMedalInfo?["<Wild>k__BackingField"]["<Stars>k__BackingField"];
                        matchInfo.LocalPlayer = new Player {
                            Id = playerId,
                            Name = playerName,
                            StandardRank = standardRank,
                            StandardLegendRank = standardLegendRank,
                            StandardStars = standardStars,
                            WildRank = wildRank,
                            WildLegendRank = wildLegendRank,
                            WildStars = wildStars, 
                            CardBackId = cardBack,
                            Account = account,
                            BattleTag = battleTag,
                        };
                    }
                    else if (side == Side.OPPOSING)
                    {
                        matchInfo.OpposingPlayer = new Player
                        {
                            Id = playerId,
                            Name = playerName,
                            StandardRank = standardRank,
                            StandardLegendRank = standardLegendRank,
                            StandardStars = 0,
                            WildRank = wildRank,
                            WildLegendRank = wildLegendRank,
                            WildStars = 0,
                            CardBackId = cardBack,
                            Account = account,
                            BattleTag = battleTag,
                        };
                    }
                }
            }
            if (matchInfo.LocalPlayer == null || matchInfo.OpposingPlayer == null)
            {
                return null;
            }
            var gameMgr = image.GetService("GameMgr");
            if (gameMgr != null)
            {
                matchInfo.MissionId = gameMgr["m_missionId"];
                matchInfo.GameType = gameMgr["m_gameType"];
                matchInfo.FormatType = gameMgr["m_formatType"];
            }
            if (netCacheValues != null)
            {
                foreach (var netCache in netCacheValues)
                {
                    if (netCache?.TypeDefinition.Name != "NetCacheRewardProgress")
                    {
                        continue;
                    }
                    matchInfo.RankedSeasonId = netCache["<Season>k__BackingField"];
                    break;
                }
            }
            return matchInfo;
        }

        private int GetRankValue(dynamic medalInfo)
        {
            var leagueId = medalInfo["leagueId"];
            var starLevel = medalInfo["starLevel"];
            var leagueRankRecord = GetLeagueRankRecord(leagueId, starLevel);
            if (leagueRankRecord == null)
            {
                return 0;
            }
            var locValues = leagueRankRecord["m_medalText"]["m_locValues"]["_items"];
            foreach (var value in locValues)
            {
                if (value == null)
                {
                    continue;
                }
                if (int.TryParse(value, out int rank))
                {
                    return rank;
                }
            }
            return 0;
        }

        private dynamic GetLeagueRankRecord(int leagueId, int starLevel)
        {
            var rankManager = image["RankMgr"]["s_instance"];
            if (rankManager == null)
            {
                return null;
            }
            var rankConfig = rankManager["m_rankConfigByLeagueAndStarLevel"];
            if (rankConfig == null)
            {
                return null;
            }
            var leagueKeys = rankConfig["keySlots"];
            var leagueValues = rankConfig["valueSlots"];
            for (var i = 0; i < leagueKeys.Length; i++)
            {
                if (leagueKeys[i] != leagueId)
                {
                     continue;
                }
                var starLevelMap = leagueValues[i];
                if (starLevelMap == null)
                {
                    return null;
                }
                var starLevelKeys = starLevelMap["keySlots"];
                var starLevelValues = starLevelMap["valueSlots"];
                for (var j = 0; j < starLevelKeys.Length; j++)
                {
                    if (starLevelKeys[j] != starLevel)
                    {
                        continue;
                    }
                    return starLevelValues[j];
                }
            }
            return null;
        }

        private BattleTag GetBattleTag(Account account)
        {
            var gameAccounts = image["BnetPresenceMgr"]["s_instance"]?["m_gameAccounts"];
            if (gameAccounts == null)
            {
                return null;
            }
            var keys = gameAccounts["keySlots"];
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i]?["m_hi"] != account.Hi || keys[i]?["m_lo"] != account.Lo)
                {
                    continue;
                }
                var bTag = gameAccounts["valueSlots"][i]["m_battleTag"];
                return new BattleTag
                {
                    Name = bTag["m_name"],
                    Number = bTag["m_number"]
                };
            }
            return null;
        }
    }
}

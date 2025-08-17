using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StarResonanceDpsAnalysis.Plugin;

namespace StarResonanceDpsAnalysis.Plugin.Database
{
    public record PlayerDto(ulong Uid, string? Nickname, string? Profession, int? CombatPower);

    public class PlayerDbClient
    {
        private readonly string _apiUrl;

        public PlayerDbClient() : this("https://star-player-dps-2gkqwtyt7fa40b00-1252172690.ap-shanghai.app.tcloudbase.com/star_resonance_dps") { }
        public PlayerDbClient(string apiUrl) => _apiUrl = apiUrl.TrimEnd('/');

        public Task EnsureSchemaAsync() => Task.CompletedTask;

        private static bool IsUnknown(string? s) => string.IsNullOrWhiteSpace(s)
            || s == "未知" || s == "未知昵称" || s == "未知职业" || s == "Unknown";

        public async Task UpsertAsync(PlayerDto player)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    ["action"] = "upsert",
                    ["uid"] = player.Uid
                };

                if (!IsUnknown(player.Nickname)) payload["nickname"] = player.Nickname!;
                if (!IsUnknown(player.Profession)) payload["profession"] = player.Profession!;
                if (player.CombatPower.HasValue && player.CombatPower.Value > 0) payload["combat_power"] = player.CombatPower.Value;

                var resp = await Common.RequestPost(_apiUrl, payload);

                var code = resp?[(string)"code"]?.ToObject<int?>();
                if (code.HasValue && code != 0 && code != 200)
                    Console.WriteLine($"[API-Upsert] non-success code={code}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API-Upsert] exception: {ex.Message}");
            }
        }

        public async Task<PlayerDto?> GetByUidAsync(ulong uid)
        {
            try
            {
                var resp = await Common.RequestPost(_apiUrl, new { action = "get", uid });
                if (resp == null) return null;

                var code = resp["code"]?.ToObject<int?>();
                if (code.HasValue && code != 0 && code != 200) return null;

                JToken data = resp["data"] ?? resp;
                if (data is JArray arr && arr.Count > 0) data = arr[0];
                if (data is JObject obj && obj["player"] is JObject p) data = p;
                if (data == null || data.Type == JTokenType.Null) return null;

                ulong u = data.Value<ulong?>("uid") ?? 0UL;
                if (u == 0)
                {
                    var uidStr = data.Value<string>("uid");
                    if (!string.IsNullOrWhiteSpace(uidStr) && ulong.TryParse(uidStr, out var parsed)) u = parsed;
                }
                if (u == 0 && uid != 0) u = uid;

                var nickname = (data.Value<string>("nickname") ?? data.Value<string>("nickName") ?? string.Empty).Trim();
                var profession = (data.Value<string>("profession") ?? data.Value<string>("professional") ?? string.Empty).Trim();

                int power = data.Value<int?>("combat_power") ?? data.Value<int?>("combatPower") ?? 0;
                if (power == 0)
                {
                    var powerStr = data.Value<string>("combat_power") ?? data.Value<string>("combatPower");
                    if (!string.IsNullOrWhiteSpace(powerStr) && int.TryParse(powerStr, out var parsedPower)) power = parsedPower;
                }

                if (string.IsNullOrWhiteSpace(nickname) && string.IsNullOrWhiteSpace(profession) && power <= 0) return null;

                return new PlayerDto(u, nickname, profession, power);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API-Get] exception: {ex.Message}");
                return null;
            }
        }

        // 新增：获取战力排行榜（按战力从高到低，默认前20名）
        public async Task<List<PlayerDto>> GetTopCombatPowerAsync(int limit = 20)
        {
            var result = new List<PlayerDto>();
            try
            {
                var resp = await Common.RequestPost(_apiUrl, new { action = "get_top_power", limit });
                if (resp == null) return result;

                var code = resp["code"]?.ToObject<int?>();
                if (code.HasValue && code != 0 && code != 200) return result;

                // 可能的数据结构：
                // { code: 200, data: [ { uid, nickname, profession, combat_power }, ... ] }
                // 或 { players: [...] }
                JToken data = resp["data"] ?? resp["players"] ?? resp;
                if (data is JObject obj && obj["players"] is JArray arrPlayers) data = arrPlayers;
                if (data is not JArray arr) return result;

                foreach (var item in arr)
                {
                    if (item == null || item.Type == JTokenType.Null) continue;

                    ulong u = item.Value<ulong?>("uid") ?? 0UL;
                    if (u == 0)
                    {
                        var uidStr = item.Value<string>("uid");
                        if (!string.IsNullOrWhiteSpace(uidStr) && ulong.TryParse(uidStr, out var parsed)) u = parsed;
                    }

                    var nickname = (item.Value<string>("nickname") ?? item.Value<string>("nickName") ?? string.Empty).Trim();
                    var profession = (item.Value<string>("profession") ?? item.Value<string>("professional") ?? string.Empty).Trim();

                    int power = item.Value<int?>("combat_power") ?? item.Value<int?>("combatPower") ?? 0;
                    if (power == 0)
                    {
                        var powerStr = item.Value<string>("combat_power") ?? item.Value<string>("combatPower");
                        if (!string.IsNullOrWhiteSpace(powerStr) && int.TryParse(powerStr, out var parsedPower)) power = parsedPower;
                    }

                    if (u == 0) continue;
                    result.Add(new PlayerDto(u, nickname, profession, power));
                }

                // 保险：客户端再按战力降序并取前 N
                result = result
                    .Where(p => (p.CombatPower ?? 0) > 0)
                    .OrderByDescending(p => p.CombatPower ?? 0)
                    .Take(Math.Max(1, limit))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API-TopPower] exception: {ex.Message}");
            }
            return result;
        }

        // 新增：按职业获取战力排行榜（按战力从高到低，默认前20名）
        public async Task<List<PlayerDto>> GetTopCombatPowerByProfessionAsync(string profession, int limit = 20)
        {
            var result = new List<PlayerDto>();
            try
            {
                // 发送 profession 过滤参数；若后端不支持，则在客户端做兜底过滤
                var resp = await Common.RequestPost(_apiUrl, new { action = "get_top_power", limit, profession });
                if (resp == null) return result;

                var code = resp["code"]?.ToObject<int?>();
                if (code.HasValue && code != 0 && code != 200) return result;

                JToken data = resp["data"] ?? resp["players"] ?? resp;
                if (data is JObject obj && obj["players"] is JArray arrPlayers) data = arrPlayers;
                if (data is not JArray arr) return result;

                foreach (var item in arr)
                {
                    if (item == null || item.Type == JTokenType.Null) continue;

                    // 读取职业并先做客户端过滤（兼容后端不支持）
                    var prof = (item.Value<string>("profession") ?? item.Value<string>("professional") ?? string.Empty).Trim();
                    if (!string.IsNullOrEmpty(profession) && !string.IsNullOrEmpty(prof) && !string.Equals(prof, profession, StringComparison.Ordinal))
                        continue;

                    ulong u = item.Value<ulong?>("uid") ?? 0UL;
                    if (u == 0)
                    {
                        var uidStr = item.Value<string>("uid");
                        if (!string.IsNullOrWhiteSpace(uidStr) && ulong.TryParse(uidStr, out var parsed)) u = parsed;
                    }

                    var nickname = (item.Value<string>("nickname") ?? item.Value<string>("nickName") ?? string.Empty).Trim();
                    int power = item.Value<int?>("combat_power") ?? item.Value<int?>("combatPower") ?? 0;
                    if (power == 0)
                    {
                        var powerStr = item.Value<string>("combat_power") ?? item.Value<string>("combatPower");
                        if (!string.IsNullOrWhiteSpace(powerStr) && int.TryParse(powerStr, out var parsedPower)) power = parsedPower;
                    }

                    if (u == 0) continue;
                    result.Add(new PlayerDto(u, nickname, prof, power));
                }

                result = result
                    .Where(p => (p.CombatPower ?? 0) > 0)
                    .OrderByDescending(p => p.CombatPower ?? 0)
                    .Take(Math.Max(1, limit))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API-TopPower-Pro] exception: {ex.Message}");
            }
            return result;
        }
    }
}

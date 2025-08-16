using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StarResonanceDpsAnalysis.Plugin;

namespace StarResonanceDpsAnalysis.Plugin.Database
{
    public record PlayerDto(ulong Uid, string Nickname, string Profession, int CombatPower);

    public class PlayerDbClient
    {
        private readonly string _apiUrl;

        public PlayerDbClient() : this("https://star-player-dps-2gkqwtyt7fa40b00-1252172690.ap-shanghai.app.tcloudbase.com/star_resonance_dps") { }
        public PlayerDbClient(string apiUrl) => _apiUrl = apiUrl.TrimEnd('/');

        public Task EnsureSchemaAsync() => Task.CompletedTask;

        public async Task UpsertAsync(PlayerDto player)
        {
            try
            {
                var resp = await Common.RequestPost(_apiUrl, new
                {
                    action = "upsert",
                    uid = player.Uid,
                    nickname = player.Nickname ?? string.Empty,
                    profession = player.Profession ?? string.Empty,
                    combat_power = player.CombatPower
                });

                var code = resp?["code"]?.ToObject<int?>();
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
    }
}

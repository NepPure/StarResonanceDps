using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Forms; // 为回填后触发UI刷新

namespace StarResonanceDpsAnalysis.Plugin.Database
{
    /// <summary>
    /// API 同步：本地优先，未知时再回填，解析后回写。
    /// </summary>
    public static class PlayerDbSyncService
    {
        private static readonly PlayerDbClient Client = new();
        private static readonly ConcurrentDictionary<ulong, byte> _dbFillQueued = new(); // 避免对同一UID重复并发请求

        private static bool IsUnknown(string? s) => string.IsNullOrWhiteSpace(s)
            || s == "未知" || s == "未知昵称" || s == "未知职业" || s == "Unknown";

        /// <summary>
        /// 只对同一 UID 调用一次回填（本地未知时才会真正请求 API）。
        /// 用于在多处触发（附近实体/伤害事件）时防止重复调用。
        /// </summary>
        public static void TryFillFromDbOnce(ulong uid)
        {
            if (uid == 0) return;
            if (!_dbFillQueued.TryAdd(uid, 1)) return; // 已经排队/执行过
            _ = Task.Run(() => TryFillFromDbAsync(uid));
        }

        /// <summary>
        /// 若本地为未知，按 UID 从 API 回填（只填未知字段，不覆盖已知）。
        /// </summary>
        public static async Task TryFillFromDbAsync(ulong uid)
        {
            try
            {
                if (uid == 0) return;
                var (localName, localPower, localProf) = StatisticData._manager.GetPlayerBasicInfo(uid);
                bool needName = IsUnknown(localName);
                bool needProf = IsUnknown(localProf);
                bool needPower = localPower <= 0;
                if (!(needName || needProf || needPower)) return; // 本地已知，不请求 API

                var dto = await Client.GetByUidAsync(uid);
                if (dto == null) return;

                bool changed = false;
                if (needName && !string.IsNullOrWhiteSpace(dto.Nickname))
                { StatisticData._manager.SetNickname(uid, dto.Nickname); if (uid == AppConfig.Uid) AppConfig.NickName = dto.Nickname; changed = true; }
                if (needProf && !string.IsNullOrWhiteSpace(dto.Profession))
                { StatisticData._manager.SetProfession(uid, dto.Profession); if (uid == AppConfig.Uid) AppConfig.Profession = dto.Profession; changed = true; }
                if (needPower && (dto.CombatPower ?? 0) > 0)
                { StatisticData._manager.SetCombatPower(uid, dto.CombatPower!.Value); if (uid == AppConfig.Uid) AppConfig.CombatPower = dto.CombatPower.Value; changed = true; }

                if (changed)
                {
                    // 1) 刷新主榜单（职业图标依赖 Profession 文本）
                    DpsStatisticsForm.RequestActiveViewRefresh();

                    // 2) 如果技能详情窗体正在展示该玩家，立即刷新顶部头像/职业背景
                    var f = FormManager.skillDetailForm;
                    if (f != null && !f.IsDisposed && f.Uid == uid)
                    {
                        var info = StatisticData._manager.GetPlayerBasicInfo(uid);
                        void UpdateDetail()
                        {
                            try { f.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession); }
                            catch { }
                        }
                        if (f.InvokeRequired) f.BeginInvoke((Action)UpdateDetail); else UpdateDetail();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB-FILL] uid={uid} ex: {ex.Message}");
            }
        }

        /// <summary>
        /// 将本地解析出的最新值写回服务器（MessageAnalyzer 中解析到 updated 时机调用）。
        /// 未知字符串会被清空后写入，避免覆盖有效数据。
        /// </summary>
        public static async Task UpsertCurrentAsync(ulong uid)
        {
            try
            {
                if (uid == 0) return;
                var (nickname, combatPower, profession) = StatisticData._manager.GetPlayerBasicInfo(uid);

                // 空或未知不覆盖，借助 Upsert 端的选择性字段上传
                string? safeNickname = IsUnknown(nickname) ? null : nickname;
                string? safeProfession = IsUnknown(profession) ? null : profession;
                int? safePower = combatPower > 0 ? combatPower : (int?)null;

                await Client.UpsertAsync(new PlayerDto(uid, safeNickname, safeProfession, safePower));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB-Upsert] uid={uid} ex: {ex.Message}");
            }
        }
    }
}

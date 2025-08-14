using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System.ComponentModel;

namespace StarResonanceDpsAnalysis.Forms
{
    /// <summary>
    /// 技能释放循环监测窗口
    /// </summary>
    public partial class SkillRotationMonitorForm : BorderlessForm
    {
        #region 私有字段

        private readonly System.Windows.Forms.Timer _refreshTimer;
        private readonly System.Windows.Forms.Timer _playerListTimer; // 新增：玩家列表自动检测定时器
        private ulong _selectedPlayerId = 0;
        private readonly List<SkillRotationData> _skillRotationHistory = new();
        private readonly Dictionary<ulong, DateTime> _lastSkillUsage = new();
        private const int MAX_HISTORY_COUNT = 200; // 最多保留200条历史记录，显示更多技能
        private FlowLayoutPanel? _skillFlowPanel; // FlowLayoutPanel实现自适应布局
        
        // 精确的卡片尺寸定义
        private const int SKILL_CARD_WIDTH = 120; // 技能卡片实际宽度
        private const int SKILL_CARD_HEIGHT = 80; // 技能卡片实际高度
        
        // FlowLayoutPanel布局控制
        private const int FLOW_PANEL_LEFT_PADDING = 8; // FlowLayoutPanel左边距
        private const int FLOW_PANEL_RIGHT_PADDING = 8; // FlowLayoutPanel右边距  
        private const int FLOW_PANEL_TOP_PADDING = 5; // FlowLayoutPanel上边距
        private const int FLOW_PANEL_BOTTOM_PADDING = 5; // FlowLayoutPanel下边距

        #endregion

        #region 构造函数

        public SkillRotationMonitorForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);

            // 初始化刷新定时器
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 500, // 500ms 刷新一次
                Enabled = false
            };
            _refreshTimer.Tick += RefreshTimer_Tick;

            // 初始化玩家列表自动检测定时器
            _playerListTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000, // 每2秒检测一次玩家列表变化
                Enabled = true // 默认启用自动检测
            };
            _playerListTimer.Tick += PlayerListTimer_Tick;

            // 加载玩家列表
            LoadPlayerList();
        }

        #endregion

        #region 窗体事件

        private void SkillRotationMonitorForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);
            
            // 重新加载玩家列表以确保数据最新
            LoadPlayerList();
            
            // 初始化技能显示区域
            InitializeSkillDisplayArea();
            
            // 确保选择框显示正确的初始状态
            EnsurePlayerSelectionDisplay();

            // 注释掉自动开始监控，改为手动控制
            // StartMonitoring();
        }

        /// <summary>
        /// 更新VirtualPanel主题色彩
        /// </summary>
        private void UpdateVirtualPanelTheme()
        {
            try
            {
                if (panel_SkillRotation != null)
                {
                    panel_SkillRotation.BackColor = AppConfig.IsLight ? Color.FromArgb(245, 245, 245) : Color.FromArgb(30, 30, 30);
                    Console.WriteLine($"VirtualPanel主题已更新 - 浅色模式: {AppConfig.IsLight}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新VirtualPanel主题时出错: {ex.Message}");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopMonitoring();
            _playerListTimer?.Stop();
            _playerListTimer?.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// 确保玩家选择框显示正确的选中状态
        /// </summary>
        private void EnsurePlayerSelectionDisplay()
        {
            try
            {
                if (dropdown_PlayerSelect.Items.Count > 0 && !dropdown_PlayerSelect.Items[0].ToString().Contains("暂无"))
                {
                    if (_selectedPlayerId == 0 || dropdown_PlayerSelect.SelectedValue == null)
                    {
                        var firstItem = dropdown_PlayerSelect.Items[0].ToString();
                        dropdown_PlayerSelect.SelectedValue = firstItem;
                        dropdown_PlayerSelect.Text = firstItem;
                        
                        if (dropdown_PlayerSelect.Tag is Dictionary<string, ulong> playerMap && 
                            playerMap.TryGetValue(firstItem, out ulong playerId))
                        {
                            _selectedPlayerId = playerId;
                            UpdatePlayerStats();
                        }
                    }
                    else
                    {
                        var playerMap = dropdown_PlayerSelect.Tag as Dictionary<string, ulong>;
                        if (playerMap != null)
                        {
                            var currentPlayerItem = playerMap.FirstOrDefault(x => x.Value == _selectedPlayerId);
                            if (!string.IsNullOrEmpty(currentPlayerItem.Key))
                            {
                                dropdown_PlayerSelect.SelectedValue = currentPlayerItem.Key;
                                dropdown_PlayerSelect.Text = currentPlayerItem.Key;
                            }
                        }
                    }
                }

                dropdown_PlayerSelect.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"确保玩家选择显示时出错: {ex.Message}");
            }
        }

        #endregion

        #region 界面初始化

        /// <summary>
        /// 初始化技能显示区域
        /// </summary>
        private void InitializeSkillDisplayArea()
        {
            panel_SkillRotation.Controls.Clear();
            panel_SkillRotation.BackColor = AppConfig.IsLight ? Color.FromArgb(245, 245, 245) : Color.FromArgb(30, 30, 30);
            
            _skillFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(FLOW_PANEL_LEFT_PADDING, FLOW_PANEL_TOP_PADDING,
                                       FLOW_PANEL_RIGHT_PADDING, FLOW_PANEL_BOTTOM_PADDING),
                BackColor = Color.Transparent
            };
            
            panel_SkillRotation.Controls.Add(_skillFlowPanel);
            
            // 初始添加占位
            AddNoDataPlaceholder();
            AddInstructionToStatsPanel();
        }

        /// <summary>
        /// 添加说明文字到统计面板
        /// </summary>
        private void AddInstructionToStatsPanel()
        {
            // 在统计面板顶部添加一行说明文字，使用彩色文本
            var instructionText = "说明：#数字 表示技能释放顺序，+时间s 表示与前一个技能的间隔时间";
            
            // 创建富文本标签以支持彩色文本
            var instructionLabel = new RichTextBox
            {
                Text = instructionText,
                Location = new Point(0, -2),
                Size = new Size(880, 22),
                Font = new Font("Microsoft YaHei", 8, FontStyle.Regular),
                BackColor = panel_Stats.BackColor,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ScrollBars = RichTextBoxScrollBars.None
            };
            
            // 设置彩色文本
            SetInstructionTextColors(instructionLabel);
            
            // 向下调整现有标签的位置，为说明文字腾出空间
            label_PlayerName.Location = new Point(0, 20);
            label_TotalSkills.Location = new Point(220, 20);
            label_LastSkillTime.Location = new Point(440, 20);
            label_AvgInterval.Location = new Point(660, 20);
            
            // 添加说明标签到统计面板
            panel_Stats.Controls.Add(instructionLabel);
        }

        /// <summary>
        /// 设置说明文字的颜色，与技能卡片颜色对应
        /// </summary>
        private void SetInstructionTextColors(RichTextBox richTextBox)
        {
            try
            {
                // 设置 "#数字" 部分的颜色为蓝色（与技能卡片中的序号颜色一致）"
                var hashIndex = richTextBox.Text.IndexOf("#数字");
                if (hashIndex >= 0)
                {
                    richTextBox.Select(hashIndex, 3);
                    richTextBox.SelectionColor = Color.Blue;
                }

                // 设置 "+时间s" 部分的颜色为橙色（与技能卡片中的时间间隔颜色一致）
                var timeIndex = richTextBox.Text.IndexOf("+时间s");
                if (timeIndex >= 0)
                {
                    richTextBox.Select(timeIndex, 4);
                    richTextBox.SelectionColor = Color.Orange;
                }

                // 恢复选择到开头
                richTextBox.Select(0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置说明文字颜色时出错: {ex.Message}");
            }
        }

        #endregion

        #region 监控控制

        /// <summary>
        /// 开始监控
        /// </summary>
        private void StartMonitoring()
        {
            if (_refreshTimer.Enabled) return;

            // 每次开始监控视为新一轮检测，重置状态与UI
            ResetDetectionState(rebuildUi: true);

            _refreshTimer.Enabled = true;
            button_StartStop.Text = "停止监控";
            button_StartStop.Type = AntdUI.TTypeMini.Error;
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        private void StopMonitoring()
        {
            if (!_refreshTimer.Enabled) return;

            _refreshTimer.Enabled = false;
            button_StartStop.Text = "开始监控";
            button_StartStop.Type = AntdUI.TTypeMini.Primary;
        }

        /// <summary>
        /// 重置本次检测的内存状态及可选的UI
        /// </summary>
        private void ResetDetectionState(bool rebuildUi)
        {
            _skillRotationHistory.Clear();
            _lastSkillUsage.Clear();

            if (rebuildUi)
            {
                RebuildAllSkillCards();
                UpdatePlayerStats();
            }
        }

        #endregion

        #region 数据更新

        /// <summary>
        /// 玩家列表自动检测定时器事件
        /// </summary>
        private void PlayerListTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 获取当前玩家列表
                var currentPlayers = StatisticData._manager.GetPlayersWithCombatData().ToList();
                var currentPlayerMap = dropdown_PlayerSelect.Tag as Dictionary<string, ulong>;
                
                // 如果玩家数量发生变化，重新加载列表
                if (currentPlayerMap == null || currentPlayers.Count != currentPlayerMap.Count)
                {
                    Console.WriteLine("检测到玩家列表变化，自动更新...");
                    
                    // 保存当前选择状态
                    var previousSelection = dropdown_PlayerSelect.SelectedValue?.ToString();
                    
                    // 重新加载列表
                    LoadPlayerList();
                    
                    // 确保选择状态正确显示
                    EnsurePlayerSelectionDisplay();
                    
                    // 如果选择发生了变化，输出日志
                    var newSelection = dropdown_PlayerSelect.SelectedValue?.ToString();
                    if (previousSelection != newSelection)
                    {
                        Console.WriteLine($"玩家选择已更新: '{previousSelection}' -> '{newSelection}'");
                    }
                }
                else if (dropdown_PlayerSelect.SelectedValue == null && _selectedPlayerId != 0)
                {
                    // 处理选择框显示为空但内部有选择玩家的情况
                    EnsurePlayerSelectionDisplay();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"玩家列表自动检测时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新定时器事件（核心：只处理当前选中玩家的新增技能）
        /// </summary>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_selectedPlayerId == 0) return;

            try
            {
                var playerData = StatisticData._manager.GetOrCreate(_selectedPlayerId);
                var skillSummaries = playerData.GetSkillSummaries(
                    topN: null,
                    orderByTotalDesc: false,
                    filterType: StarResonanceDpsAnalysis.Core.SkillType.Damage);

                // 增量更新
                CheckAndAddNewSkills(skillSummaries);

                // 只更新顶栏统计
                UpdatePlayerStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"技能循环监控刷新时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 将skillSummaries中未出现过或时间更新过的技能依序加入卡片
        /// </summary>
        private void CheckAndAddNewSkills(List<SkillSummary> skillSummaries)
        {
            if (_skillFlowPanel == null || skillSummaries == null || skillSummaries.Count == 0) return;

            foreach (var skill in skillSummaries)
            {
                if (!skill.LastTime.HasValue) continue;

                var id = skill.SkillId;
                var lastTime = skill.LastTime.Value;

                // 若该技能是第一次出现或lastTime更新，则记为一次使用
                if (!_lastSkillUsage.TryGetValue(id, out var prev) || lastTime > prev)
                {
                    _lastSkillUsage[id] = lastTime;

                    var data = new SkillRotationData
                    {
                        SkillId = id,
                        SkillName = skill.SkillName,
                        UseTime = lastTime,
                        Damage = skill.Total,
                        HitCount = skill.HitCount,
                        SequenceNumber = _skillRotationHistory.Count + 1
                    };

                    _skillRotationHistory.Add(data);

                    // 控制历史长度并维护UI
                    if (_skillRotationHistory.Count > MAX_HISTORY_COUNT)
                    {
                        RemoveOldestSkill();
                        UpdateAllCardSequenceNumbers();
                    }

                    AddNewSkillCard(data);
                }
            }
        }
        /// <summary>
        /// 移除最老的技能记录和卡片
        /// </summary>
        private void RemoveOldestSkill()
        {
            if (_skillRotationHistory.Count == 0 || _skillFlowPanel == null) return;
            
            // 移除最老的记录
            _skillRotationHistory.RemoveAt(0);
            
            // 重新编号
            for (int i = 0; i < _skillRotationHistory.Count; i++)
            {
                _skillRotationHistory[i].SequenceNumber = i + 1;
            }

            // 移除最老的技能卡片
            if (_skillFlowPanel.Controls.Count > 0)
            {
                var oldestCard = _skillFlowPanel.Controls[0];
                _skillFlowPanel.Controls.Remove(oldestCard);
                oldestCard.Dispose();
            }
        }

        /// <summary>
        /// 增量添加新技能卡片
        /// </summary>
        private void AddNewSkillCard(SkillRotationData skill)
        {
            if (_skillFlowPanel == null) return;
            
            var skillCard = CreateSkillCard(skill);
            
            this.BeginInvoke(new Action(() =>
            {
                // 如果还存在“暂无技能释放记录”的占位标签，先移除
                RemoveNoDataPlaceholderIfPresent();
                
                _skillFlowPanel.SuspendLayout();
                try
                {
                    _skillFlowPanel.Controls.Add(skillCard);
                }
                finally
                {
                    _skillFlowPanel.ResumeLayout(true);
                }
                
                // 自动滚动到最新添加的技能卡片
                if (_skillFlowPanel.AutoScroll)
                {
                    _skillFlowPanel.ScrollControlIntoView(skillCard);
                }
            }));
        }

        /// <summary>
        /// 如果存在“暂无技能释放记录”的占位标签，则将其移除
        /// </summary>
        private void RemoveNoDataPlaceholderIfPresent()
        {
            if (_skillFlowPanel == null || _skillFlowPanel.Controls.Count == 0) return;

            // 查找占位标签（按 Dock = Fill 和文本匹配，避免误删）
            var placeholders = _skillFlowPanel.Controls
                .OfType<AntdUI.Label>()
                .Where(l => (l.Dock == DockStyle.Fill) || string.Equals(l.Text, "暂无技能释放记录", StringComparison.Ordinal))
                .ToList();

            foreach (var ph in placeholders)
            {
                _skillFlowPanel.Controls.Remove(ph);
                ph.Dispose();
            }
        }

        /// <summary>
        /// 更新所有现有卡片的序号
        /// </summary>
        private void UpdateAllCardSequenceNumbers()
        {
            if (_skillFlowPanel == null) return;
            
            var cards = _skillFlowPanel.Controls.OfType<System.Windows.Forms.Panel>().ToList();
            
            for (int i = 0; i < cards.Count && i < _skillRotationHistory.Count; i++)
            {
                var card = cards[i];
                var skill = _skillRotationHistory[i];
                
                // 更新卡片上的序号标签
                var sequenceLabel = card.Controls.OfType<AntdUI.Label>()
                    .FirstOrDefault(l => l.Text.StartsWith("#"));
                
                if (sequenceLabel != null)
                {
                    sequenceLabel.Text = $"#{skill.SequenceNumber}";
                }
            }
        }

        /// <summary>
        /// 重建所有技能卡片（统一的重建方法）
        /// </summary>
        private void RebuildAllSkillCards()
        {
            if (_skillFlowPanel == null) return;
            
            try
            {
                // 清空现有控件
                foreach (System.Windows.Forms.Control control in _skillFlowPanel.Controls)
                {
                    control?.Dispose();
                }
                _skillFlowPanel.Controls.Clear();

                if (_skillRotationHistory.Count == 0)
                {
                    AddNoDataPlaceholder();
                    return;
                }

                // 批量创建并添加所有技能卡片
                var cardsToAdd = _skillRotationHistory.Select(CreateSkillCard).ToArray();
                _skillFlowPanel.Controls.AddRange(cardsToAdd);

                Console.WriteLine($"重建技能卡片完成，数量: {cardsToAdd.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重建技能卡片时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加“暂无技能释放记录”的占位标签
        /// </summary>
        private void AddNoDataPlaceholder()
        {
            if (_skillFlowPanel == null) return;
            var noDataLabel = new AntdUI.Label
            {
                Text = "暂无技能释放记录",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            _skillFlowPanel.Controls.Add(noDataLabel);
        }

        /// <summary>
        /// 创建技能卡片
        /// </summary>
        private System.Windows.Forms.Panel CreateSkillCard(SkillRotationData skill)
        {
            var card = new System.Windows.Forms.Panel
            {
                Size = new Size(SKILL_CARD_WIDTH, SKILL_CARD_HEIGHT),
                Margin = new Padding(2, 2, 2, 2),
                BackColor = AppConfig.IsLight ? Color.FromArgb(250, 250, 250) : Color.FromArgb(45, 45, 45),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = skill.SkillId.ToString()
            };

            // 技能名称
            var nameLabel = new AntdUI.Label
            {
                Text = skill.SkillName.Length > 8 ? skill.SkillName.Substring(0, 8) + "..." : skill.SkillName,
                Location = new Point(5, 5),
                Size = new Size(SKILL_CARD_WIDTH - 10, 20),
                Font = new Font("Microsoft YaHei", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.TopCenter
            };

            // 使用时间
            var timeLabel = new AntdUI.Label
            {
                Text = skill.UseTime.ToString("HH:mm:ss"),
                Location = new Point(5, 25),
                Size = new Size(SKILL_CARD_WIDTH - 10, 15),
                Font = new Font("Microsoft YaHei", 7),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter
            };

            // 序号
            var sequenceLabel = new AntdUI.Label
            {
                Text = $"#{skill.SequenceNumber}",
                Location = new Point(5, 45),
                Size = new Size(SKILL_CARD_WIDTH - 10, 15),
                Font = new Font("Microsoft YaHei", 7),
                ForeColor = Color.Blue,
                TextAlign = ContentAlignment.TopCenter
            };

            card.Controls.AddRange(new System.Windows.Forms.Control[] { nameLabel, timeLabel, sequenceLabel });

            // 间隔时间（如果不是第一个技能）
            if (skill.SequenceNumber > 1)
            {
                var prevSkill = _skillRotationHistory.FirstOrDefault(s => s.SequenceNumber == skill.SequenceNumber - 1);
                if (prevSkill != null)
                {
                    var interval = (skill.UseTime - prevSkill.UseTime).TotalSeconds;
                    var intervalLabel = new AntdUI.Label
                    {
                        Text = $"+{interval:F1}s",
                        Location = new Point(5, 60),
                        Size = new Size(SKILL_CARD_WIDTH - 10, 15),
                        Font = new Font("Microsoft YaHei", 6),
                        ForeColor = Color.Orange,
                        TextAlign = ContentAlignment.TopCenter
                    };
                    card.Controls.Add(intervalLabel);
                }
            }

            return card;
        }

        /// <summary>
        /// 更新玩家统计信息
        /// </summary>
        private void UpdatePlayerStats()
        {
            if (_selectedPlayerId == 0) return;

            try
            {
                var playerData = StatisticData._manager.GetOrCreate(_selectedPlayerId);
                var playerInfo = StatisticData._manager.GetPlayerBasicInfo(_selectedPlayerId);

                label_PlayerName.Text = $"玩家: {playerInfo.Nickname}";
                label_TotalSkills.Text = $"技能总数: {_skillRotationHistory.Count}";
                label_LastSkillTime.Text = _skillRotationHistory.Count > 0 
                    ? $"最后技能: {_skillRotationHistory.Last().UseTime:HH:mm:ss}"
                    : "最后技能: 无";

                // 计算平均技能间隔
                if (_skillRotationHistory.Count > 1)
                {
                    var intervals = new List<double>();
                    for (int i = 1; i < _skillRotationHistory.Count; i++)
                    {
                        var interval = (_skillRotationHistory[i].UseTime - _skillRotationHistory[i - 1].UseTime).TotalSeconds;
                        intervals.Add(interval);
                    }
                    var avgInterval = intervals.Average();
                    label_AvgInterval.Text = $"平均间隔: {avgInterval:F1}s";
                }
                else
                {
                    label_AvgInterval.Text = "平均间隔: 无";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新玩家统计信息时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载玩家列表
        /// </summary>
        private void LoadPlayerList()
        {
            try
            {
                var currentSelectedValue = dropdown_PlayerSelect.SelectedValue?.ToString();
                var currentSelectedPlayerId = _selectedPlayerId;
                
                dropdown_PlayerSelect.Items.Clear();
                dropdown_PlayerSelect.Tag = new Dictionary<string, ulong>();

                var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
                
                if (players.Count == 0)
                {
                    const string noData = "暂无玩家数据";
                    dropdown_PlayerSelect.Items.Add(noData);
                    dropdown_PlayerSelect.SelectedValue = noData;
                    dropdown_PlayerSelect.Text = noData;
                    return;
                }

                var playerMap = (Dictionary<string, ulong>)dropdown_PlayerSelect.Tag;
                string? itemToSelect = null;

                foreach (var player in players)
                {
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(player.Uid);
                    var displayText = $"{playerInfo.Nickname} (UID: {player.Uid})";
                    
                    dropdown_PlayerSelect.Items.Add(displayText);
                    playerMap[displayText] = player.Uid;
                    
                    if (currentSelectedPlayerId != 0 && player.Uid == currentSelectedPlayerId)
                    {
                        itemToSelect = displayText;
                    }
                    else if (string.IsNullOrEmpty(itemToSelect) && !string.IsNullOrEmpty(currentSelectedValue) && displayText == currentSelectedValue)
                    {
                        itemToSelect = displayText;
                    }
                }

                if (!string.IsNullOrEmpty(itemToSelect))
                {
                    dropdown_PlayerSelect.SelectedValue = itemToSelect;
                    dropdown_PlayerSelect.Text = itemToSelect;
                }
                else if (dropdown_PlayerSelect.Items.Count > 0)
                {
                    var firstItem = dropdown_PlayerSelect.Items[0].ToString();
                    dropdown_PlayerSelect.SelectedValue = firstItem;
                    dropdown_PlayerSelect.Text = firstItem;
                    
                    if (playerMap.TryGetValue(firstItem, out ulong firstPlayerId))
                    {
                        _selectedPlayerId = firstPlayerId;
                    }
                }

                dropdown_PlayerSelect.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载玩家列表时出错: {ex.Message}");
            }
        }

        #endregion

        #region 控件事件

        /// <summary>
        /// 玩家选择改变事件（切换视角即视为新一轮检测）
        /// </summary>
        private void dropdown_PlayerSelect_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            try
            {
                var selectedText = dropdown_PlayerSelect.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(selectedText)) return;
                
                // 同步显示文本，修复选中后不显示的问题
                if (dropdown_PlayerSelect.Text != selectedText)
                {
                    dropdown_PlayerSelect.Text = selectedText;
                }

                if (dropdown_PlayerSelect.Tag is not Dictionary<string, ulong> playerMap) return;
                if (!playerMap.TryGetValue(selectedText, out var playerId)) return;

                if (_selectedPlayerId == playerId) { UpdatePlayerStats(); return; }

                _selectedPlayerId = playerId;
                ResetDetectionState(rebuildUi: true);
                UpdatePlayerStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"玩家选择改变时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 开始/停止监控按钮点击事件
        /// </summary>
        private void button_StartStop_Click(object sender, EventArgs e)
        {
            if (_refreshTimer.Enabled)
            {
                StopMonitoring();
            }
            else
            {
                StartMonitoring();
            }
        }

        /// <summary>
        /// 清空数据（不改变选中玩家）
        /// </summary>
        private void button_Clear_Click(object sender, EventArgs e)
        {
            ResetDetectionState(rebuildUi: true);
            Console.WriteLine("清空数据完成");
        }

        /// <summary>
        /// 刷新玩家列表按钮点击事件
        /// </summary>
        private void button_RefreshPlayers_Click(object sender, EventArgs e)
        {
            LoadPlayerList();
        }

        /// <summary>
        /// 关闭窗口按钮点击事件
        /// </summary>
        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 窗口拖拽
        /// </summary>
        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        #endregion

        #region 数据模型

        /// <summary>
        /// 技能循环数据
        /// </summary>
        private class SkillRotationData
        {
            public ulong SkillId { get; set; }
            public string SkillName { get; set; } = "";
            public DateTime UseTime { get; set; }
            public ulong Damage { get; set; }
            public int HitCount { get; set; }
            public int SequenceNumber { get; set; }
        }

        #endregion

        #region 重置卡片布局和重新创建控件

        /// <summary>
        /// 重置卡片布局并重新创建所有控件
        /// </summary>
        private void ResetCardLayoutAndRecreateControls()
        {
            if (_skillFlowPanel == null) return;
            
            try
            {
                // 暂停布局计算
                _skillFlowPanel.SuspendLayout();
                if (panel_SkillRotation is System.Windows.Forms.Control control)
                {
                    control.SuspendLayout();
                }
                
                // 重置FlowLayoutPanel状态
                _skillFlowPanel.AutoScrollPosition = new Point(0, 0);
                _skillFlowPanel.HorizontalScroll.Value = 0;
                _skillFlowPanel.VerticalScroll.Value = 0;
                
                // 重建所有卡片
                RebuildAllSkillCards();
                
                // 恢复布局计算
                _skillFlowPanel.ResumeLayout(true);
                if (panel_SkillRotation is System.Windows.Forms.Control resumeControl)
                {
                    resumeControl.ResumeLayout(true);
                }
                
                Console.WriteLine($"卡片布局重置完成 - 技能数量: {_skillRotationHistory.Count}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置卡片布局时出错: {ex.Message}");
                
                // 出错时确保布局恢复
                _skillFlowPanel?.ResumeLayout(true);
                if (panel_SkillRotation is System.Windows.Forms.Control errorControl)
                {
                    errorControl.ResumeLayout(true);
                }
            }
        }

        #endregion
    }
}
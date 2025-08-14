using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis
{
    /// <summary>
    /// 鼠标穿透工具类：支持指定窗体或句柄，开启/关闭“真正的鼠标穿透”
    /// 说明：
    /// 1) 开启穿透后：窗口加入 WS_EX_TRANSPARENT（鼠标事件穿透）和 WS_EX_LAYERED（分层窗口）
    /// 2) 同时去掉可拉伸相关样式（WS_THICKFRAME/WS_SIZEBOX、WS_MAXIMIZEBOX），避免在穿透状态下还能拉伸窗口
    /// 3) 关闭穿透时：按记录恢复被移除的样式
    /// </summary>
    public static class MousePenetrationHelper
    {
        #region ========== Win32 常量/导入 ==========

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        // 普通窗口样式（是否可拉伸/最大化）
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_THICKFRAME = 0x00040000; // = WS_SIZEBOX

        // 扩展窗口样式（鼠标穿透/分层）
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        // 分层窗口属性
        private const int LWA_ALPHA = 0x2;

        // 64/32 位兼容的 Get/SetWindowLong(Ptr)
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
            => IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
            => IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));

        #endregion

        #region ========== 内部状态记录（用于恢复） ==========

        // 用于记录在“开启穿透”时，被强制移除的普通样式（拉伸、最大化），以便关闭时恢复
        private static int? _backupStyle;    // GWL_STYLE 的备份（仅保存被修改前的）
        private static int? _backupExStyle;  // GWL_EXSTYLE 的备份

        #endregion

        #region ========== 对外 API：指定 Form 入口 ==========

        /// <summary>
        /// 设置指定 Form 的鼠标穿透状态
        /// </summary>
        /// <param name="form">目标窗体（你来填）</param>
        /// <param name="enable">true=开启穿透；false=关闭穿透</param>
        /// <param name="alpha">
        /// 可选：不透明度(0~255)。默认 255 完全不透明，只是鼠标穿透；
        /// 低于 255 会半透明显示（仍然鼠标穿透）。
        /// </param>
        public static void SetMousePenetrate(Form form, bool enable, byte alpha = 255)
        {
            if (form == null || form.IsDisposed) return;
            SetMousePenetrate(form.Handle, enable, alpha);

            // 额外保险：在 Form 级别同步限制拉伸（避免用户在穿透状态下通过其他方式改变大小）
            if (enable)
            {
                form.MaximumSize = form.Size;
                form.MinimumSize = form.Size;
            }
            else
            {
                // 恢复自由（可按需要自定义，默认不限制）
                form.MinimumSize = System.Drawing.Size.Empty;
                form.MaximumSize = System.Drawing.Size.Empty;
            }
        }

        #endregion

        #region ========== 对外 API：指定句柄入口（核心实现） ==========

        /// <summary>
        /// 设置指定窗口句柄的鼠标穿透状态（核心）
        /// </summary>
        /// <param name="hWnd">目标窗口句柄（你来填）</param>
        /// <param name="enable">true=开启穿透；false=关闭穿透</param>
        /// <param name="alpha">0~255，不透明度（默认 255 不透明）</param>
        public static void SetMousePenetrate(IntPtr hWnd, bool enable, byte alpha = 255)
        {
            if (hWnd == IntPtr.Zero || !IsWindow(hWnd)) return;

            // 读取当前样式
            var style = GetWindowLongPtr(hWnd, GWL_STYLE).ToInt32();
            var exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt32();

            if (enable)
            {
                // 首次开启时备份
                if (_backupStyle == null) _backupStyle = style;
                if (_backupExStyle == null) _backupExStyle = exStyle;

                // 1) 添加分层与穿透：WS_EX_LAYERED | WS_EX_TRANSPARENT
                exStyle |= WS_EX_LAYERED;
                exStyle |= WS_EX_TRANSPARENT;

                // 2) 禁止拉伸/最大化：移除 WS_THICKFRAME(=WS_SIZEBOX) 与 WS_MAXIMIZEBOX
                style &= ~WS_THICKFRAME;
                style &= ~WS_MAXIMIZEBOX;

                // 应用样式
                SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));
                SetWindowLongPtr(hWnd, GWL_STYLE, new IntPtr(style));

                // 应用分层不透明度（即便 255 也调用一次，确保属性一致）
                SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
            }
            else
            {
                // 关闭穿透：恢复备份（若存在）
                if (_backupExStyle.HasValue)
                {
                    exStyle = _backupExStyle.Value;
                    SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));
                    _backupExStyle = null;
                }
                else
                {
                    // 若无备份，仅移除穿透标志
                    exStyle &= ~WS_EX_TRANSPARENT;
                    SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));
                }

                if (_backupStyle.HasValue)
                {
                    style = _backupStyle.Value;
                    SetWindowLongPtr(hWnd, GWL_STYLE, new IntPtr(style));
                    _backupStyle = null;
                }
                else
                {
                    // 若无备份，不强行恢复；你也可以按需添加默认恢复逻辑
                }

                // 关闭时，保持当前分层透明度为 255（不强制要求）
                SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA);
            }
        }

        #endregion

        #region ========== 便捷状态查询 ==========

        /// <summary>
        /// 当前窗口是否已启用鼠标穿透
        /// </summary>
        public static bool IsPenetrating(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero || !IsWindow(hWnd)) return false;
            var ex = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt32();
            return (ex & WS_EX_TRANSPARENT) == WS_EX_TRANSPARENT;
        }

        #endregion
    }
}

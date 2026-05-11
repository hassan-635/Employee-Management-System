using System.Drawing;
using System.Drawing.Drawing2D;

namespace SmartEPS.WinForms
{
    /// <summary>Warm light luxury palette — shared by main and list forms.</summary>
    internal static class UiTheme
    {
        public static readonly Color FormBack     = Color.FromArgb(246, 244, 239);
        public static readonly Color HeaderBg     = Color.FromArgb(237, 233, 224);
        public static readonly Color CardSurface  = Color.FromArgb(255, 254, 252);
        public static readonly Color CardBorder   = Color.FromArgb(222, 215, 202);
        public static readonly Color AccentGold   = Color.FromArgb(168, 132, 72);
        public static readonly Color AccentSoft   = Color.FromArgb(195, 168, 120);
        public static readonly Color TextPrimary  = Color.FromArgb(42, 40, 38);
        public static readonly Color TextMuted    = Color.FromArgb(115, 108, 98);
        public static readonly Color TextSubtle   = Color.FromArgb(150, 142, 130);

        public static readonly Color InputFill    = Color.FromArgb(255, 255, 255);
        public static readonly Color InputBorder  = Color.FromArgb(210, 204, 192);
        public static readonly Color InputOk      = Color.FromArgb(255, 255, 255);
        public static readonly Color InputError   = Color.FromArgb(255, 236, 232);

        public static readonly Color StatusBusy   = Color.FromArgb(176, 132, 48);
        public static readonly Color StatusOk    = Color.FromArgb(88, 130, 88);

        public static readonly Font FontTitle     = new("Cambria", 20f, FontStyle.Regular);
        public static readonly Font FontSection   = new("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FontBody      = new("Segoe UI", 9f);
        public static readonly Font FontBodySmall = new("Segoe UI", 8.5f);
        public static readonly Font FontButton    = new("Segoe UI", 9.5f, FontStyle.Bold);

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void StyleCardPanel(Panel p, int cornerRadius = 14)
        {
            void ApplyRegion()
            {
                using var path = RoundedRect(new Rectangle(0, 0, p.Width, p.Height), cornerRadius);
                p.Region?.Dispose();
                p.Region = new Region(path);
            }
            ApplyRegion();
            p.Resize += (_, _) => ApplyRegion();

            p.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using var path = RoundedRect(rect, cornerRadius);
                using var border = new Pen(CardBorder, 1f);
                g.DrawPath(border, path);
            };
        }
    }
}

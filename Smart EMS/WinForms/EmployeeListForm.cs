// ============================================================
//  PART 5 — Second Form: Employee List Display
//  Receives data passed from MainForm
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartEPS.ML;
using static SmartEPS.WinForms.UiTheme;

namespace SmartEPS.WinForms
{
    // ── Data transfer record ─────────────────────────────────
    public record EmployeeRecord
    {
        public string         Name              { get; init; } = "";
        public string         Department        { get; init; } = "";
        public string         EmployeeId        { get; init; } = "";
        public EmployeeData   Data              { get; init; } = new();
        public string         PredictedCategory { get; init; } = "";
        public float          Confidence        { get; init; }
    }

    // ── Second Form ─────────────────────────────────────────
    public class EmployeeListForm : Form
    {
        // Data passed from MainForm (Part 5 requirement)
        private readonly List<EmployeeRecord> _employees;

        private DataGridView dgv = null!;
        private Label lblTitle   = null!;
        private Label lblCount   = null!;
        private Button btnExport = null!;
        private Button btnClose  = null!;
        private Panel  pnlTop    = null!;
        private Panel  pnlBot    = null!;

        public EmployeeListForm(List<EmployeeRecord> employees)
        {
            _employees = employees;
            BuildUI();
            PopulateGrid();
        }

        private void BuildUI()
        {
            this.Text            = "All Employees — Performance Summary";
            this.Size            = new Size(1100, 650);
            this.BackColor       = FormBack;
            this.Font            = FontBody;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;

            // Header
            pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 72,
                BackColor = HeaderBg
            };
            pnlTop.Paint += (_, e) =>
            {
                using var line = new Pen(Color.FromArgb(218, 198, 168), 1);
                e.Graphics.DrawLine(line, 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            };
            this.Controls.Add(pnlTop);

            lblTitle = new Label
            {
                Text      = "Employee performance registry",
                Font      = new Font("Cambria", 18f, FontStyle.Regular),
                ForeColor = TextPrimary,
                AutoSize  = true,
                Location  = new Point(22, 12)
            };
            pnlTop.Controls.Add(lblTitle);

            lblCount = new Label
            {
                Text      = $"Total records: {_employees.Count}",
                Font      = FontBodySmall,
                ForeColor = TextMuted,
                AutoSize  = true,
                Location  = new Point(24, 44)
            };
            pnlTop.Controls.Add(lblCount);

            // DataGridView
            dgv = new DataGridView
            {
                Location               = new Point(14, 82),
                Size                   = new Size(1060, 488),
                BackgroundColor        = CardSurface,
                GridColor              = CardBorder,
                BorderStyle            = BorderStyle.None,
                RowHeadersVisible      = false,
                AllowUserToAddRows     = false,
                AllowUserToDeleteRows  = false,
                ReadOnly               = true,
                SelectionMode          = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill,
                Font                   = FontBody,
                CellBorderStyle        = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false
            };

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = CardSurface,
                ForeColor          = TextPrimary,
                SelectionBackColor = Color.FromArgb(232, 224, 208),
                SelectionForeColor = TextPrimary,
                Padding            = new Padding(6, 4, 6, 4)
            };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(232, 224, 210),
                ForeColor = TextPrimary,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding   = new Padding(6, 6, 6, 6)
            };
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(252, 250, 246)
            };

            // Row color by rating
            dgv.CellFormatting += Dgv_CellFormatting;

            this.Controls.Add(dgv);

            // Bottom buttons
            pnlBot = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 58,
                BackColor = HeaderBg
            };
            pnlBot.Paint += (_, e) =>
            {
                using var line = new Pen(Color.FromArgb(218, 198, 168), 1);
                e.Graphics.DrawLine(line, 0, 0, pnlBot.Width, 0);
            };
            this.Controls.Add(pnlBot);

            btnExport = new Button
            {
                Text      = "Export CSV",
                Location  = new Point(18, 14),
                Size      = new Size(150, 34),
                BackColor = Color.FromArgb(70, 110, 90),
                ForeColor = Color.FromArgb(252, 250, 247),
                FlatStyle = FlatStyle.Flat,
                Font      = FontButton,
                Cursor    = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatAppearance.MouseOverBackColor = ControlPaint.Light(btnExport.BackColor, 0.12f);
            btnExport.Click += BtnExport_Click;
            pnlBot.Controls.Add(btnExport);

            btnClose = new Button
            {
                Text      = "Close",
                Location  = new Point(928, 14),
                Size      = new Size(130, 34),
                BackColor = Color.FromArgb(48, 58, 78),
                ForeColor = Color.FromArgb(252, 250, 247),
                FlatStyle = FlatStyle.Flat,
                Font      = FontButton,
                Cursor    = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = ControlPaint.Light(btnClose.BackColor, 0.12f);
            btnClose.Click += (s, e) => this.Close();
            pnlBot.Controls.Add(btnClose);
        }

        private void PopulateGrid()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add("EmpId",    "Employee ID");
            dgv.Columns.Add("Name",     "Name");
            dgv.Columns.Add("Dept",     "Department");
            dgv.Columns.Add("Task",     "Task % ");
            dgv.Columns.Add("Quality",  "Quality");
            dgv.Columns.Add("Attend",   "Attend %");
            dgv.Columns.Add("Training", "Training h");
            dgv.Columns.Add("Projects", "Projects");
            dgv.Columns.Add("CSAT",     "Cust. Sat.");
            dgv.Columns.Add("Collab",   "Collab");
            dgv.Columns.Add("OT",       "Overtime h");
            dgv.Columns.Add("Rating",   "Predicted Rating");
            dgv.Columns.Add("Conf",     "Confidence");

            foreach (var emp in _employees)
            {
                dgv.Rows.Add(
                    emp.EmployeeId,
                    emp.Name,
                    emp.Department,
                    $"{emp.Data.TaskCompletionRate:F1}",
                    $"{emp.Data.QualityScore:F1}",
                    $"{emp.Data.AttendanceRate:F1}",
                    $"{emp.Data.TrainingHours:F0}",
                    $"{emp.Data.ProjectsCompleted:F0}",
                    $"{emp.Data.CustomerSatisfaction:F1}",
                    $"{emp.Data.TeamCollaborationScore:F1}",
                    $"{emp.Data.OvertimeHours:F0}",
                    emp.PredictedCategory,
                    $"{emp.Confidence:F1}%"
                );
            }

            if (_employees.Count == 0)
                dgv.Rows.Add("—", "No employees added yet", "", "", "", "", "", "", "", "", "", "", "");
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count) return;
            var ratingCell = dgv.Rows[e.RowIndex].Cells["Rating"];
            if (ratingCell?.Value == null) return;
            string rating = ratingCell.Value.ToString() ?? "";

            Color rowColor = rating.Contains("Excellent") ? Color.FromArgb(232, 244, 236)
                           : rating.Contains("Good")      ? Color.FromArgb(228, 236, 244)
                           : rating.Contains("Average")   ? Color.FromArgb(244, 238, 226)
                           : rating.Contains("Poor")      ? Color.FromArgb(250, 228, 228)
                           : CardSurface;

            if (e.CellStyle != null)
                e.CellStyle.BackColor = rowColor;
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter   = "CSV Files (*.csv)|*.csv",
                FileName = $"employees_{DateTime.Now:yyyyMMdd}.csv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var lines = new System.Text.StringBuilder();
            lines.AppendLine("EmployeeId,Name,Department,TaskCompletion,Quality," +
                             "Attendance,TrainingHours,Projects,CSAT,Collaboration," +
                             "Overtime,PredictedRating,Confidence");

            foreach (var emp in _employees)
                lines.AppendLine($"{emp.EmployeeId},{emp.Name},{emp.Department}," +
                                 $"{emp.Data.TaskCompletionRate:F1},{emp.Data.QualityScore:F1}," +
                                 $"{emp.Data.AttendanceRate:F1},{emp.Data.TrainingHours:F0}," +
                                 $"{emp.Data.ProjectsCompleted:F0},{emp.Data.CustomerSatisfaction:F1}," +
                                 $"{emp.Data.TeamCollaborationScore:F1},{emp.Data.OvertimeHours:F0}," +
                                 $"{emp.PredictedCategory},{emp.Confidence:F1}%");

            System.IO.File.WriteAllText(dlg.FileName, lines.ToString());
            MessageBox.Show($"Exported {_employees.Count} records to:\n{dlg.FileName}",
                            "Export Complete", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
    }
}
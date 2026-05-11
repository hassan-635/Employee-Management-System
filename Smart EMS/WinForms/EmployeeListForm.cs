// ============================================================
//  PART 5 — Second Form: Employee List Display
//  Receives data passed from MainForm
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartEPS.ML;

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
            this.BackColor       = Color.FromArgb(15, 15, 30);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;

            // Header
            pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 65,
                BackColor = Color.FromArgb(10, 10, 22)
            };
            this.Controls.Add(pnlTop);

            lblTitle = new Label
            {
                Text      = "📋 Employee Performance Registry",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255),
                AutoSize  = true,
                Location  = new Point(15, 10)
            };
            pnlTop.Controls.Add(lblTitle);

            lblCount = new Label
            {
                Text      = $"Total Records: {_employees.Count}",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.Silver,
                AutoSize  = true,
                Location  = new Point(17, 42)
            };
            pnlTop.Controls.Add(lblCount);

            // DataGridView
            dgv = new DataGridView
            {
                Location               = new Point(10, 75),
                Size                   = new Size(1065, 490),
                BackgroundColor        = Color.FromArgb(18, 18, 35),
                GridColor              = Color.FromArgb(40, 50, 70),
                BorderStyle            = BorderStyle.None,
                RowHeadersVisible      = false,
                AllowUserToAddRows     = false,
                AllowUserToDeleteRows  = false,
                ReadOnly               = true,
                SelectionMode          = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill,
                Font                   = new Font("Segoe UI", 9f),
                CellBorderStyle        = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false
            };

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor     = Color.FromArgb(22, 22, 42),
                ForeColor     = Color.FromArgb(200, 210, 230),
                SelectionBackColor = Color.FromArgb(0, 80, 150),
                SelectionForeColor = Color.White,
                Padding       = new Padding(4)
            };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 30, 60),
                ForeColor = Color.FromArgb(0, 212, 255),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding   = new Padding(4)
            };
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(26, 26, 48)
            };

            // Row color by rating
            dgv.CellFormatting += Dgv_CellFormatting;

            this.Controls.Add(dgv);

            // Bottom buttons
            pnlBot = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 55,
                BackColor = Color.FromArgb(10, 10, 22)
            };
            this.Controls.Add(pnlBot);

            btnExport = new Button
            {
                Text      = "📥 Export CSV",
                Location  = new Point(15, 12),
                Size      = new Size(140, 32),
                BackColor = Color.FromArgb(0, 100, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            pnlBot.Controls.Add(btnExport);

            btnClose = new Button
            {
                Text      = "✖ Close",
                Location  = new Point(940, 12),
                Size      = new Size(120, 32),
                BackColor = Color.FromArgb(140, 30, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
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

            Color rowColor = rating.Contains("Excellent") ? Color.FromArgb(0, 60, 30)
                           : rating.Contains("Good")      ? Color.FromArgb(0, 40, 80)
                           : rating.Contains("Average")   ? Color.FromArgb(60, 50, 0)
                           : rating.Contains("Poor")      ? Color.FromArgb(70, 10, 10)
                           : Color.FromArgb(22, 22, 42);

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
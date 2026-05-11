// ============================================================
//  PARTS 3 & 4 — Windows Forms Main Form
//  Employee KPI Entry + ML.NET Prediction Display
//  Parts 3-4: Labels, TextBoxes, 2+ Buttons, Event Handling
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartEPS.ML;

namespace SmartEPS.WinForms
{
    public partial class MainForm : Form
    {
        // ── State ─────────────────────────────────────────────
        private readonly PerformancePredictor _predictor;
        private readonly List<EmployeeRecord> _employees = new();
        private bool _modelReady = false;

        // ── UI Controls ──────────────────────────────────────
        // Labels + TextBoxes for KPI input (assigned in BuildUI; null! satisfies nullable analysis)
        private Label   lblTitle = null!, lblStatus = null!, lblResult = null!, lblConfidence = null!;
        private Label   lblName = null!, lblDept = null!, lblEmpId = null!;
        private Label   lblTask = null!, lblQuality = null!, lblAttendance = null!;
        private Label   lblTraining = null!, lblProjects = null!, lblCsat = null!, lblCollab = null!, lblOvertime = null!;
        private TextBox txtName = null!, txtDept = null!, txtEmpId = null!;
        private TextBox txtTask = null!, txtQuality = null!, txtAttendance = null!;
        private TextBox txtTraining = null!, txtProjects = null!, txtCsat = null!, txtCollab = null!, txtOvertime = null!;

        // Buttons (2+ required)
        private Button btnPredict = null!, btnAddEmployee = null!, btnViewAll = null!, btnClear = null!, btnTrainModel = null!;

        // Result panel
        private Panel   pnlResult = null!, pnlHeader = null!, pnlLeft = null!, pnlRight = null!;
        private ProgressBar pgConfidence = null!;
        private PictureBox  pbRatingIcon = null!;

        public MainForm()
        {
            _predictor = new PerformancePredictor();
            InitializeComponent();
            BuildUI();
            WireEvents();
            TryAutoTrain();
        }

        // ── Auto-train model on startup ───────────────────────
        private void TryAutoTrain()
        {
            lblStatus.Text = "🔄 Training ML model, please wait...";
            lblStatus.ForeColor = Color.FromArgb(255, 193, 7);

            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                var (acc, _) = _predictor.TrainModel();
                e.Result = acc;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                _modelReady = true;
                double acc = (double)(e.Result ?? 0.0);
                lblStatus.Text = $"✅ ML Model Ready  |  Accuracy: {acc * 100:F1}%";
                lblStatus.ForeColor = Color.FromArgb(40, 200, 120);
                btnPredict.Enabled = true;
                btnAddEmployee.Enabled = true;
            };
            worker.RunWorkerAsync();
        }

        // ─────────────────────────────────────────────────────
        //  BUILD UI  (Parts 3 — Labels, TextBoxes, Buttons)
        // ─────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text            = "Smart Employee Performance Evaluation System";
            this.Size            = new Size(1050, 760);
            this.MinimumSize     = new Size(1000, 720);
            this.BackColor       = Color.FromArgb(15, 15, 30);
            this.Font            = new Font("Segoe UI", 9f);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;

            // ── Header panel ──────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = Color.FromArgb(10, 10, 22)
            };
            this.Controls.Add(pnlHeader);

            lblTitle = new Label
            {
                Text      = "⚡ Smart Employee Performance System",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255),
                AutoSize  = true,
                Location  = new Point(20, 18)
            };
            pnlHeader.Controls.Add(lblTitle);

            lblStatus = new Label
            {
                Text      = "⏳ Initialising...",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(22, 56)
            };
            pnlHeader.Controls.Add(lblStatus);

            // ── Left panel (Employee Info) ────────────────────
            pnlLeft = new Panel
            {
                Location  = new Point(15, 90),
                Size      = new Size(310, 620),
                BackColor = Color.FromArgb(20, 20, 40)
            };
            RoundCorners(pnlLeft);
            this.Controls.Add(pnlLeft);

            AddSectionHeader(pnlLeft, "👤 Employee Information", 12);

            lblName  = MakeLabel(pnlLeft, "Full Name",    45);
            txtName  = MakeTextBox(pnlLeft, 68, "e.g. Alice Johnson");
            lblDept  = MakeLabel(pnlLeft, "Department",  100);
            txtDept  = MakeTextBox(pnlLeft, 122, "e.g. Engineering");
            lblEmpId = MakeLabel(pnlLeft, "Employee ID", 155);
            txtEmpId = MakeTextBox(pnlLeft, 177, "e.g. EMP-001");

            AddSectionHeader(pnlLeft, "📊 KPI Metrics", 215);

            lblTask       = MakeLabel(pnlLeft, "Task Completion Rate (%)", 248);
            txtTask       = MakeTextBox(pnlLeft, 268, "0–100");
            lblQuality    = MakeLabel(pnlLeft, "Work Quality Score (0–10)", 300);
            txtQuality    = MakeTextBox(pnlLeft, 320, "0.0–10.0");
            lblAttendance = MakeLabel(pnlLeft, "Attendance Rate (%)",       352);
            txtAttendance = MakeTextBox(pnlLeft, 372, "0–100");
            lblTraining   = MakeLabel(pnlLeft, "Training Hours / Year",     404);
            txtTraining   = MakeTextBox(pnlLeft, 424, "e.g. 40");
            lblProjects   = MakeLabel(pnlLeft, "Projects Completed",        456);
            txtProjects   = MakeTextBox(pnlLeft, 476, "e.g. 8");
            lblCsat       = MakeLabel(pnlLeft, "Customer Satisfaction (0–10)", 508);
            txtCsat       = MakeTextBox(pnlLeft, 528, "0.0–10.0");

            // ── Right panel (More KPIs + Action Buttons) ──────
            pnlRight = new Panel
            {
                Location  = new Point(340, 90),
                Size      = new Size(310, 620),
                BackColor = Color.FromArgb(20, 20, 40)
            };
            RoundCorners(pnlRight);
            this.Controls.Add(pnlRight);

            AddSectionHeader(pnlRight, "📈 Additional KPIs", 12);

            lblCollab  = MakeLabel(pnlRight, "Team Collaboration (0–10)", 45);
            txtCollab  = MakeTextBox(pnlRight, 65, "0.0–10.0");
            lblOvertime= MakeLabel(pnlRight, "Overtime Hours / Month",    97);
            txtOvertime= MakeTextBox(pnlRight, 117, "e.g. 15");

            // Buttons — Part 3 requires at least 2
            btnPredict = MakeButton(pnlRight, "🤖 Predict Performance",
                                    Color.FromArgb(0, 120, 215), 165);
            btnPredict.Enabled = false;

            btnAddEmployee = MakeButton(pnlRight, "➕ Add Employee",
                                         Color.FromArgb(0, 153, 76), 215);
            btnAddEmployee.Enabled = false;

            btnViewAll = MakeButton(pnlRight, "📋 View All Employees",
                                     Color.FromArgb(100, 60, 180), 265);

            btnClear = MakeButton(pnlRight, "🗑️  Clear Form",
                                   Color.FromArgb(160, 40, 40), 315);

            btnTrainModel = MakeButton(pnlRight, "🔁 Re-Train Model",
                                        Color.FromArgb(60, 100, 140), 365);

            // Quick-fill demo data
            var btnDemo = MakeButton(pnlRight, "💡 Load Demo Data",
                                      Color.FromArgb(80, 80, 100), 415);
            btnDemo.Click += BtnDemo_Click;

            // ── Result panel ──────────────────────────────────
            pnlResult = new Panel
            {
                Location  = new Point(665, 90),
                Size      = new Size(360, 620),
                BackColor = Color.FromArgb(20, 20, 40)
            };
            RoundCorners(pnlResult);
            this.Controls.Add(pnlResult);

            AddSectionHeader(pnlResult, "🎯 Prediction Result", 12);

            pbRatingIcon = new PictureBox
            {
                Location  = new Point(130, 55),
                Size      = new Size(100, 100),
                BackColor = Color.Transparent
            };
            pnlResult.Controls.Add(pbRatingIcon);
            pbRatingIcon.Paint += PbRatingIcon_Paint;

            lblResult = new Label
            {
                Text      = "Run a prediction to see results",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.Gray,
                Size      = new Size(320, 50),
                Location  = new Point(20, 175),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlResult.Controls.Add(lblResult);

            var lblConfLbl = new Label
            {
                Text      = "Confidence",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(20, 240)
            };
            pnlResult.Controls.Add(lblConfLbl);

            pgConfidence = new ProgressBar
            {
                Location = new Point(20, 262),
                Size     = new Size(320, 22),
                Minimum  = 0,
                Maximum  = 100,
                Value    = 0,
                Style    = ProgressBarStyle.Continuous
            };
            pnlResult.Controls.Add(pgConfidence);

            lblConfidence = new Label
            {
                Text      = "0%",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(20, 292)
            };
            pnlResult.Controls.Add(lblConfidence);

            // Score breakdown label (appears after prediction)
            var lblBreakLbl = new Label
            {
                Name      = "lblBreakdown",
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 160, 200),
                Size      = new Size(320, 280),
                Location  = new Point(20, 325),
                TextAlign = ContentAlignment.TopLeft
            };
            pnlResult.Controls.Add(lblBreakLbl);
        }

        // ─────────────────────────────────────────────────────
        //  WIRE EVENTS  (Part 4 — Event Handling)
        // ─────────────────────────────────────────────────────
        private void WireEvents()
        {
            // Part 4: Predict button → display prediction result
            btnPredict.Click     += BtnPredict_Click;

            // Part 4: Add employee event
            btnAddEmployee.Click += BtnAddEmployee_Click;

            // Part 5: View all → open second form
            btnViewAll.Click     += BtnViewAll_Click;

            btnClear.Click       += BtnClear_Click;
            btnTrainModel.Click  += BtnTrainModel_Click;

            // Live input validation
            foreach (var tb in new[] { txtTask, txtQuality, txtAttendance,
                                        txtTraining, txtProjects, txtCsat,
                                        txtCollab, txtOvertime })
            {
                tb.TextChanged += TxtKpi_TextChanged;
            }
        }

        // ── Event: Predict ────────────────────────────────────
        private void BtnPredict_Click(object? sender, EventArgs e)
        {
            if (!_modelReady) { ShowMsg("Model not ready yet."); return; }
            if (!TryGetInput(out var data, out string err))
            {
                ShowMsg("Input Error: " + err);
                return;
            }
            var result = _predictor.Predict(data);
            DisplayPrediction(result, data);
        }

        // ── Event: Add Employee ───────────────────────────────
        private void BtnAddEmployee_Click(object? sender, EventArgs e)
        {
            if (!_modelReady) { ShowMsg("Model not ready yet."); return; }
            if (!TryGetInput(out var data, out string err))
            {
                ShowMsg("Input Error: " + err);
                return;
            }

            var prediction = _predictor.Predict(data);
            var rec = new EmployeeRecord
            {
                Name                   = txtName.Text.Trim(),
                Department             = txtDept.Text.Trim(),
                EmployeeId             = txtEmpId.Text.Trim(),
                Data                   = data,
                PredictedCategory      = prediction.CategoryName,
                Confidence             = prediction.Confidence
            };
            _employees.Add(rec);
            DisplayPrediction(prediction, data);
            ShowMsg($"✅ Employee '{rec.Name}' added! Total: {_employees.Count}");
        }

        // ── Event: View All (Part 5 — Multiple Form Interaction)
        private void BtnViewAll_Click(object? sender, EventArgs e)
        {
            // Pass data to second form
            using var form2 = new EmployeeListForm(_employees);
            form2.ShowDialog(this);
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            foreach (Control c in new Control[] {
                txtName, txtDept, txtEmpId, txtTask, txtQuality,
                txtAttendance, txtTraining, txtProjects, txtCsat,
                txtCollab, txtOvertime })
                if (c is TextBox tb) tb.Clear();

            lblResult.Text      = "Run a prediction to see results";
            lblResult.ForeColor = Color.Gray;
            pgConfidence.Value  = 0;
            lblConfidence.Text  = "0%";
            pbRatingIcon.Invalidate();
            GetBreakdownLabel().Text = "";
        }

        private void BtnTrainModel_Click(object? sender, EventArgs e)
        {
            _modelReady = false;
            btnPredict.Enabled = false;
            btnAddEmployee.Enabled = false;
            lblStatus.Text = "🔄 Re-training model...";
            lblStatus.ForeColor = Color.FromArgb(255, 193, 7);
            TryAutoTrain();
        }

        private void BtnDemo_Click(object? sender, EventArgs e)
        {
            txtName.Text       = "Alice Johnson";
            txtDept.Text       = "Engineering";
            txtEmpId.Text      = "EMP-001";
            txtTask.Text       = "92";
            txtQuality.Text    = "8.5";
            txtAttendance.Text = "96";
            txtTraining.Text   = "55";
            txtProjects.Text   = "12";
            txtCsat.Text       = "9.1";
            txtCollab.Text     = "8.8";
            txtOvertime.Text   = "22";
        }

        private void TxtKpi_TextChanged(object? sender, EventArgs e)
        {
            // Real-time input border highlight
            if (sender is TextBox tb)
            {
                tb.BackColor = float.TryParse(tb.Text, out _)
                    ? Color.FromArgb(25, 35, 55)
                    : Color.FromArgb(60, 20, 20);
            }
        }

        // ─────────────────────────────────────────────────────
        //  DISPLAY RESULT
        // ─────────────────────────────────────────────────────
        private string _currentCategory = "";

        private void DisplayPrediction(EmployeePrediction pred, EmployeeData data)
        {
            _currentCategory = pred.PredictedCategory.ToString();

            Color catColor = pred.PredictedCategory switch
            {
                3 => Color.FromArgb(0, 230, 120),
                2 => Color.FromArgb(60, 180, 255),
                1 => Color.FromArgb(255, 180, 0),
                _ => Color.FromArgb(255, 80, 80)
            };

            lblResult.Text      = pred.CategoryName;
            lblResult.ForeColor = catColor;

            int conf = (int)Math.Round(pred.Confidence);
            pgConfidence.Value  = Math.Min(conf, 100);
            lblConfidence.Text  = $"Confidence: {conf}%";

            pbRatingIcon.Invalidate();

            // Score breakdown
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("── KPI Summary ──────────────────");
            sb.AppendLine($"  Task Completion     : {data.TaskCompletionRate:F1}%");
            sb.AppendLine($"  Quality Score       : {data.QualityScore:F1}/10");
            sb.AppendLine($"  Attendance Rate     : {data.AttendanceRate:F1}%");
            sb.AppendLine($"  Training Hours      : {data.TrainingHours:F0} hrs/yr");
            sb.AppendLine($"  Projects Completed  : {data.ProjectsCompleted:F0}");
            sb.AppendLine($"  Customer Sat.       : {data.CustomerSatisfaction:F1}/10");
            sb.AppendLine($"  Team Collaboration  : {data.TeamCollaborationScore:F1}/10");
            sb.AppendLine($"  Overtime Hours      : {data.OvertimeHours:F0} hrs/mo");
            if (pred.Score?.Length >= 4)
            {
                sb.AppendLine();
                sb.AppendLine("── Class Probabilities ──────────");
                sb.AppendLine($"  ❌ Poor      : {pred.Score[0]*100:F1}%");
                sb.AppendLine($"  ⚠️  Average  : {pred.Score[1]*100:F1}%");
                sb.AppendLine($"  ✅ Good      : {pred.Score[2]*100:F1}%");
                sb.AppendLine($"  ⭐ Excellent : {pred.Score[3]*100:F1}%");
            }
            GetBreakdownLabel().Text = sb.ToString();
        }

        private void PbRatingIcon_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int r = 40;
            var rect = new Rectangle(10, 10, r*2, r*2);

            Color fill = _currentCategory switch
            {
                "3" => Color.FromArgb(0, 180, 100),
                "2" => Color.FromArgb(0, 140, 220),
                "1" => Color.FromArgb(200, 150, 0),
                "0" => Color.FromArgb(200, 50, 50),
                _   => Color.FromArgb(60, 60, 80)
            };

            using var brush = new SolidBrush(fill);
            g.FillEllipse(brush, rect);

            string icon = _currentCategory switch
            {
                "3" => "A", "2" => "B", "1" => "C", "0" => "D", _ => "?"
            };
            using var font = new Font("Segoe UI", 28f, FontStyle.Bold);
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(icon, font, Brushes.White,
                new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
        }

        // ─────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────
        private bool TryGetInput(out EmployeeData data, out string err)
        {
            data = new EmployeeData();
            err  = "";

            bool ok = true;
            ok &= ParseFloat(txtTask,       0, 100,  v => data.TaskCompletionRate     = v, "Task Completion", ref err);
            ok &= ParseFloat(txtQuality,    0, 10,   v => data.QualityScore           = v, "Quality Score",   ref err);
            ok &= ParseFloat(txtAttendance, 0, 100,  v => data.AttendanceRate         = v, "Attendance Rate", ref err);
            ok &= ParseFloat(txtTraining,   0, 5000, v => data.TrainingHours          = v, "Training Hours",  ref err);
            ok &= ParseFloat(txtProjects,   0, 1000, v => data.ProjectsCompleted      = v, "Projects",        ref err);
            ok &= ParseFloat(txtCsat,       0, 10,   v => data.CustomerSatisfaction   = v, "CSAT",            ref err);
            ok &= ParseFloat(txtCollab,     0, 10,   v => data.TeamCollaborationScore = v, "Collaboration",   ref err);
            ok &= ParseFloat(txtOvertime,   0, 744,  v => data.OvertimeHours          = v, "Overtime",        ref err);
            return ok;
        }

        private static bool ParseFloat(TextBox tb, float min, float max,
                                        Action<float> assign, string name,
                                        ref string err)
        {
            if (!float.TryParse(tb.Text.Trim(), out float v) || v < min || v > max)
            {
                err = $"{name} must be {min}–{max}.";
                tb.BackColor = Color.FromArgb(80, 20, 20);
                return false;
            }
            assign(v);
            return true;
        }

        private Label GetBreakdownLabel()
        {
            foreach (Control c in pnlResult.Controls)
                if (c.Name == "lblBreakdown") return (Label)c;
            return new Label();
        }

        private static void ShowMsg(string msg) =>
            MessageBox.Show(msg, "Smart EPS", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

        // ── UI factory helpers ────────────────────────────────
        private static Label MakeLabel(Panel p, string text, int y)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(150, 160, 180),
                AutoSize  = true,
                Location  = new Point(12, y)
            };
            p.Controls.Add(lbl);
            return lbl;
        }

        private static TextBox MakeTextBox(Panel p, int y, string placeholder)
        {
            var tb = new TextBox
            {
                Location    = new Point(12, y),
                Size        = new Size(284, 24),
                BackColor   = Color.FromArgb(25, 35, 55),
                ForeColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9.5f),
                PlaceholderText = placeholder
            };
            p.Controls.Add(tb);
            return tb;
        }

        private static Button MakeButton(Panel p, string text, Color color, int y)
        {
            var btn = new Button
            {
                Text      = text,
                Location  = new Point(12, y),
                Size      = new Size(284, 38),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            p.Controls.Add(btn);
            return btn;
        }

        private static void AddSectionHeader(Panel p, string text, int y)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255),
                AutoSize  = true,
                Location  = new Point(12, y)
            };
            p.Controls.Add(lbl);
        }

        private static void RoundCorners(Panel p)
        {
            // Visual refinement via region clipping
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(40, 60, 100), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
        }

        // Required for partial class designer compatibility
        private void InitializeComponent() { }
    }
}
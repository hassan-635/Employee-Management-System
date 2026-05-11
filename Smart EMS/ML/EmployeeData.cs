// ============================================================
//  PART — ML.NET Data Model
//  EmployeeData (input features) + EmployeePrediction (output)
// ============================================================
using Microsoft.ML.Data;

namespace SmartEPS.ML
{
    /// <summary>
    /// Input schema for ML.NET model.
    /// Features: KPI metrics used to predict performance category.
    /// </summary>
    public class EmployeeData
    {
        // ── Feature columns ──────────────────────────────────
        [LoadColumn(0)]
        public float TaskCompletionRate { get; set; }   // 0–100 %

        [LoadColumn(1)]
        public float QualityScore { get; set; }         // 0–10

        [LoadColumn(2)]
        public float AttendanceRate { get; set; }        // 0–100 %

        [LoadColumn(3)]
        public float TrainingHours { get; set; }         // hours/year

        [LoadColumn(4)]
        public float ProjectsCompleted { get; set; }     // count

        [LoadColumn(5)]
        public float CustomerSatisfaction { get; set; }  // 0–10

        [LoadColumn(6)]
        public float TeamCollaborationScore { get; set; } // 0–10

        [LoadColumn(7)]
        public float OvertimeHours { get; set; }          // hours/month

        // ── Label (what we predict) ───────────────────────────
        // 0 = Poor, 1 = Average, 2 = Good, 3 = Excellent
        [LoadColumn(8), ColumnName("Label")]
        public uint PerformanceCategory { get; set; }
    }

    /// <summary>
    /// Output schema — what ML.NET returns after prediction.
    /// </summary>
    public class EmployeePrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedCategory { get; set; }

        public float[] Score { get; set; } = Array.Empty<float>();

        // Helper: human-readable category name
        public string CategoryName => PredictedCategory switch
        {
            0 => "❌ Poor",
            1 => "⚠️  Average",
            2 => "✅ Good",
            3 => "⭐ Excellent",
            _ => "Unknown"
        };

        // Confidence of the winning class
        public float Confidence =>
            Score != null && Score.Length > PredictedCategory
                ? Score[PredictedCategory] * 100f
                : 0f;
    }
}
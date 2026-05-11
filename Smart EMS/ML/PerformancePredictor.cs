// ============================================================
//  ML.NET Model — Training + Prediction Engine
//  Algorithm: SDCA Multi-Class Classification
// ============================================================
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartEPS.ML
{
    public class PerformancePredictor
    {
        private readonly MLContext   _mlContext;
        private ITransformer?        _model;
        private PredictionEngine<EmployeeData, EmployeePrediction>? _predEngine;
        private readonly string      _modelPath;

        public bool IsModelTrained => _model != null;

        public PerformancePredictor(string modelPath = "performance_model.zip")
        {
            _mlContext = new MLContext(seed: 42);
            _modelPath = modelPath;

            // Auto-load if model already exists (e.g., in Docker image)
            if (File.Exists(_modelPath))
                LoadModel();
        }

        // ── Training ──────────────────────────────────────────
        public (double accuracy, double macroF1) TrainModel()
        {
            var trainingData = GenerateSyntheticData(800);
            var testData     = GenerateSyntheticData(200);

            var trainView = _mlContext.Data.LoadFromEnumerable(trainingData);
            var testView  = _mlContext.Data.LoadFromEnumerable(testData);

            // Pipeline: normalise → convert label → SDCA classifier
            var pipeline = _mlContext.Transforms
                .Concatenate("Features",
                    nameof(EmployeeData.TaskCompletionRate),
                    nameof(EmployeeData.QualityScore),
                    nameof(EmployeeData.AttendanceRate),
                    nameof(EmployeeData.TrainingHours),
                    nameof(EmployeeData.ProjectsCompleted),
                    nameof(EmployeeData.CustomerSatisfaction),
                    nameof(EmployeeData.TeamCollaborationScore),
                    nameof(EmployeeData.OvertimeHours))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Transforms.Conversion
                    .MapValueToKey("Label"))
                .Append(_mlContext.MulticlassClassification.Trainers
                    .SdcaMaximumEntropy(
                        labelColumnName:   "Label",
                        featureColumnName: "Features",
                        maximumNumberOfIterations: 100))
                .Append(_mlContext.Transforms.Conversion
                    .MapKeyToValue("PredictedLabel"));

            _model = pipeline.Fit(trainView);

            // Evaluate
            var predictions = _model.Transform(testView);
            var metrics = _mlContext.MulticlassClassification
                .Evaluate(predictions, labelColumnName: "Label");

            // Persist model
            _mlContext.Model.Save(_model, trainView.Schema, _modelPath);

            // Rebuild prediction engine
            _predEngine = _mlContext.Model
                .CreatePredictionEngine<EmployeeData, EmployeePrediction>(_model);

            return (metrics.MacroAccuracy, metrics.MacroAccuracy);
        }

        // ── Load pre-trained model ────────────────────────────
        public void LoadModel()
        {
            _model = _mlContext.Model.Load(_modelPath, out _);
            _predEngine = _mlContext.Model
                .CreatePredictionEngine<EmployeeData, EmployeePrediction>(_model);
        }

        // ── Predict single employee ───────────────────────────
        public EmployeePrediction Predict(EmployeeData input)
        {
            if (_predEngine == null)
                throw new InvalidOperationException(
                    "Model not trained. Call TrainModel() first.");
            return _predEngine.Predict(input);
        }

        // ── Synthetic training data generator ────────────────
        private static IEnumerable<EmployeeData> GenerateSyntheticData(int count)
        {
            var rng  = new Random(99);
            var data = new List<EmployeeData>(count);

            for (int i = 0; i < count; i++)
            {
                // Generate realistic correlated KPI values
                uint category;
                double roll = rng.NextDouble();

                float task, quality, attendance, training,
                      projects, csat, collab, overtime;

                if (roll < 0.20)       // Poor
                {
                    category   = 0;
                    task       = (float)Rnd(rng, 20, 54);
                    quality    = (float)Rnd(rng, 2,  5);
                    attendance = (float)Rnd(rng, 40, 65);
                    training   = (float)Rnd(rng, 0,  15);
                    projects   = (float)Rnd(rng, 0,  3);
                    csat       = (float)Rnd(rng, 2,  5);
                    collab     = (float)Rnd(rng, 2,  5);
                    overtime   = (float)Rnd(rng, 0,  5);
                }
                else if (roll < 0.45)  // Average
                {
                    category   = 1;
                    task       = (float)Rnd(rng, 55, 69);
                    quality    = (float)Rnd(rng, 5,  6.9);
                    attendance = (float)Rnd(rng, 66, 78);
                    training   = (float)Rnd(rng, 10, 30);
                    projects   = (float)Rnd(rng, 3,  6);
                    csat       = (float)Rnd(rng, 5,  6.9);
                    collab     = (float)Rnd(rng, 5,  6.9);
                    overtime   = (float)Rnd(rng, 5,  15);
                }
                else if (roll < 0.75)  // Good
                {
                    category   = 2;
                    task       = (float)Rnd(rng, 70, 84);
                    quality    = (float)Rnd(rng, 7,  8.4);
                    attendance = (float)Rnd(rng, 79, 91);
                    training   = (float)Rnd(rng, 25, 50);
                    projects   = (float)Rnd(rng, 6,  10);
                    csat       = (float)Rnd(rng, 7,  8.4);
                    collab     = (float)Rnd(rng, 7,  8.4);
                    overtime   = (float)Rnd(rng, 10, 25);
                }
                else                   // Excellent
                {
                    category   = 3;
                    task       = (float)Rnd(rng, 85, 100);
                    quality    = (float)Rnd(rng, 8.5, 10);
                    attendance = (float)Rnd(rng, 92, 100);
                    training   = (float)Rnd(rng, 40, 80);
                    projects   = (float)Rnd(rng, 10, 20);
                    csat       = (float)Rnd(rng, 8.5, 10);
                    collab     = (float)Rnd(rng, 8.5, 10);
                    overtime   = (float)Rnd(rng, 20, 40);
                }

                data.Add(new EmployeeData
                {
                    TaskCompletionRate    = task,
                    QualityScore          = quality,
                    AttendanceRate        = attendance,
                    TrainingHours         = training,
                    ProjectsCompleted     = projects,
                    CustomerSatisfaction  = csat,
                    TeamCollaborationScore= collab,
                    OvertimeHours         = overtime,
                    PerformanceCategory   = category
                });
            }
            return data;
        }

        private static double Rnd(Random r, double min, double max) =>
            min + r.NextDouble() * (max - min);
    }
}
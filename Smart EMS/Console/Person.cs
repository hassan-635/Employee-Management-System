// ============================================================
//  PART 1 & 2 — Console Prototype
//  Person base class  ➜  Employee / Manager / Staff derived
// ============================================================
using System;
using System.Collections.Generic;

namespace SmartEPS.Console
{
    // ── PART 1 ─────────────────────────────────────────────
    // Person class with constructor + result-calculation method
    public class Person
    {
        // Data members
        private string _name;
        private int    _age;
        private string _id;

        // Constructor
        public Person(string name, int age, string id)
        {
            _name = name;
            _age  = age;
            _id   = id;
        }

        // Properties (Encapsulation — Part 2)
        public string Name { get => _name; set => _name = value; }
        public int    Age  { get => _age;  set => _age  = value; }
        public string Id   { get => _id;   set => _id   = value; }

        // Method for result calculation (Part 1 requirement)
        public virtual string CalculateResult()
        {
            return $"[Person] {_name} (ID: {_id}) — base evaluation not defined.";
        }

        public override string ToString() =>
            $"Name: {_name} | Age: {_age} | ID: {_id}";
    }

    // ── PART 2 ─────────────────────────────────────────────
    // Employee class — Encapsulation via Properties
    // Inheritance from Person
    public abstract class Employee : Person
    {
        private double _taskCompletionRate;   // 0–100
        private double _qualityScore;         // 0–10
        private double _attendanceRate;       // 0–100
        private string _department;

        public Employee(string name, int age, string id,
                        string department,
                        double taskCompletion,
                        double qualityScore,
                        double attendanceRate)
            : base(name, age, id)
        {
            _department         = department;
            _taskCompletionRate = taskCompletion;
            _qualityScore       = qualityScore;
            _attendanceRate     = attendanceRate;
        }

        // Encapsulated properties
        public string Department
        {
            get => _department;
            set => _department = value;
        }
        public double TaskCompletionRate
        {
            get => _taskCompletionRate;
            set => _taskCompletionRate = Math.Clamp(value, 0, 100);
        }
        public double QualityScore
        {
            get => _qualityScore;
            set => _qualityScore = Math.Clamp(value, 0, 10);
        }
        public double AttendanceRate
        {
            get => _attendanceRate;
            set => _attendanceRate = Math.Clamp(value, 0, 100);
        }

        // Base performance score calculation
        protected double ComputeBaseScore() =>
            (_taskCompletionRate * 0.4)
          + (_qualityScore * 10 * 0.35)
          + (_attendanceRate * 0.25);

        // Polymorphism — overridden in subclasses
        public override string CalculateResult()
        {
            double score = ComputeBaseScore();
            string rating = score >= 85 ? "⭐ Excellent"
                          : score >= 70 ? "✅ Good"
                          : score >= 55 ? "⚠️  Average"
                          :               "❌ Poor";
            return $"{ToString()} | Score: {score:F1} | Rating: {rating}";
        }

        public override string ToString() =>
            base.ToString() + $" | Dept: {_department}";
    }

    // ── Staff — standard weight evaluation
    public class Staff : Employee
    {
        public Staff(string name, int age, string id,
                     string department,
                     double taskCompletion,
                     double qualityScore,
                     double attendanceRate)
            : base(name, age, id, department,
                   taskCompletion, qualityScore, attendanceRate) { }

        public override string CalculateResult()
        {
            double score = ComputeBaseScore();
            string rating = score >= 85 ? "⭐ Excellent"
                          : score >= 70 ? "✅ Good"
                          : score >= 55 ? "⚠️  Average"
                          :               "❌ Poor";
            return $"[STAFF]   {ToString()} | Score: {score:F1} | Rating: {rating}";
        }
    }

    // ── Manager — leadership bonus applied (Polymorphism / Method Overriding)
    public class Manager : Employee
    {
        private double _leadershipScore; // 0–10

        public double LeadershipScore
        {
            get => _leadershipScore;
            set => _leadershipScore = Math.Clamp(value, 0, 10);
        }

        public Manager(string name, int age, string id,
                       string department,
                       double taskCompletion,
                       double qualityScore,
                       double attendanceRate,
                       double leadershipScore)
            : base(name, age, id, department,
                   taskCompletion, qualityScore, attendanceRate)
        {
            _leadershipScore = leadershipScore;
        }

        // OVERRIDDEN rating calculation — Manager bonus
        public override string CalculateResult()
        {
            double baseScore    = ComputeBaseScore();
            double managerScore = baseScore * 0.8 + (_leadershipScore * 10) * 0.2;
            string rating = managerScore >= 85 ? "⭐ Excellent"
                          : managerScore >= 70 ? "✅ Good"
                          : managerScore >= 55 ? "⚠️  Average"
                          :                      "❌ Poor";
            return $"[MANAGER] {ToString()} | Score: {managerScore:F1} | Leadership: {_leadershipScore}/10 | Rating: {rating}";
        }
    }

    // ── Console Entry Point ─────────────────────────────────
    public class ConsoleApp
    {
        public static void Run()
        {
            System.Console.Clear();
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔══════════════════════════════════════════════════╗");
            System.Console.WriteLine("║   Smart Employee Performance Evaluation System   ║");
            System.Console.WriteLine("║            Console Prototype — Parts 1 & 2       ║");
            System.Console.WriteLine("╚══════════════════════════════════════════════════╝");
            System.Console.ResetColor();

            var employees = new List<Employee>
            {
                new Staff("Alice Johnson",   28, "EMP001", "Engineering",  92, 8.5, 95),
                new Staff("Bob Martinez",    35, "EMP002", "Marketing",    61, 6.0, 78),
                new Staff("Carol White",     30, "EMP003", "HR",           45, 5.2, 60),
                new Manager("David Lee",     42, "MGR001", "Engineering",  88, 9.0, 97, 9.2),
                new Manager("Eva Kim",       38, "MGR002", "Operations",   70, 7.5, 85, 7.8),
            };

            System.Console.WriteLine("\n📋  Employee Performance Evaluation Results:\n");
            System.Console.WriteLine(new string('─', 90));

            foreach (var emp in employees)
            {
                // Part 1 — calling CalculateResult()
                string result = emp.CalculateResult();
                System.Console.ForegroundColor = result.Contains("Excellent") ? ConsoleColor.Green
                                               : result.Contains("Good")      ? ConsoleColor.Yellow
                                               : result.Contains("Average")   ? ConsoleColor.DarkYellow
                                               :                                ConsoleColor.Red;
                System.Console.WriteLine(result);
                System.Console.ResetColor();
            }

            System.Console.WriteLine(new string('─', 90));
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }
    }
}
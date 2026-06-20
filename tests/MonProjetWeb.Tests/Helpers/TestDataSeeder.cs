using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Domain.Enums;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Tests.Helpers;

public static class TestDataSeeder
{
    public static User CreateUser(ApplicationDbContext db, string email = "test@test.com")
    {
        var user = new User
        {
            FirstName    = "Jean",
            LastName     = "Dupont",
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role         = UserRole.Admin,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static GcpAccount CreateGcpAccount(ApplicationDbContext db, int userId)
    {
        var account = new GcpAccount
        {
            UserId           = userId,
            AccountName      = "Test Account",
            ProjectId        = "test-project-123",
            BillingAccountId = "AAAAAA-BBBBBB-CCCCCC",
            IsActive         = true,
            CreatedAt        = DateTime.UtcNow
        };
        db.GcpAccounts.Add(account);
        db.SaveChanges();
        return account;
    }

    public static List<CostRecord> CreateCostRecords(
        ApplicationDbContext db, int gcpAccountId, int count = 5)
    {
        var records = new List<CostRecord>();
        var services = new[]
        {
            ("Compute Engine", "compute.googleapis.com"),
            ("Cloud Storage",  "storage.googleapis.com"),
            ("BigQuery",       "bigquery.googleapis.com"),
            ("Cloud Run",      "run.googleapis.com"),
            ("Pub/Sub",        "pubsub.googleapis.com"),
        };

        var now   = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);

        for (int i = 0; i < Math.Min(count, services.Length); i++)
        {
            var record = new CostRecord
            {
                GcpAccountId = gcpAccountId,
                ServiceName  = services[i].Item1,
                ServiceId    = services[i].Item2,
                Amount       = (i + 1) * 100m,
                Currency     = "USD",
                PeriodStart  = start,
                PeriodEnd    = now,
                Region       = "europe-west1",
                ResourceName = $"projects/{services[i].Item2}",
                CreatedAt    = DateTime.UtcNow
            };
            records.Add(record);
        }

        db.CostRecords.AddRange(records);
        db.SaveChanges();
        return records;
    }

    public static Budget CreateBudget(
        ApplicationDbContext db, int gcpAccountId,
        decimal amount = 1000m, decimal spent = 0m)
    {
        var budget = new Budget
        {
            GcpAccountId   = gcpAccountId,
            Name           = "Budget Test",
            Amount         = amount,
            SpentAmount    = spent,
            Currency       = "USD",
            PeriodStart    = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            PeriodEnd      = DateTime.UtcNow.AddMonths(1),
            AlertThreshold = 80,
            CreatedAt      = DateTime.UtcNow
        };
        db.Budgets.Add(budget);
        db.SaveChanges();
        return budget;
    }

    public static Alert CreateAlert(
        ApplicationDbContext db, int gcpAccountId,
        AlertSeverity severity = AlertSeverity.Medium,
        AlertStatus status = AlertStatus.Active)
    {
        var alert = new Alert
        {
            GcpAccountId = gcpAccountId,
            Title        = "Alerte Test",
            Message      = "Ceci est une alerte de test.",
            Severity     = severity,
            Status       = status,
            CreatedAt    = DateTime.UtcNow
        };
        db.Alerts.Add(alert);
        db.SaveChanges();
        return alert;
    }
}
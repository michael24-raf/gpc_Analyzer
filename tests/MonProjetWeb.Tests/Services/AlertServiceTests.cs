using FluentAssertions;
using MonProjetWeb.Application.Services;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Repositories;
using MonProjetWeb.Tests.Helpers;

namespace MonProjetWeb.Tests.Services;

public class AlertServiceTests
{
    [Fact]
    public async Task CreateAlert_ShouldPersistToDatabase()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);

        var alertRepo  = new AlertRepository(db);
        var budgetRepo = new BudgetRepository(db);
        var service    = new AlertService(alertRepo, budgetRepo);

        // Act
        var alert = await service.CreateAlertAsync(
            account.Id, "Test Alert", "Message test",
            AlertSeverity.High);

        // Assert
        alert.Should().NotBeNull();
        alert.Id.Should().BeGreaterThan(0);
        alert.Status.Should().Be(AlertStatus.Active);
        alert.Severity.Should().Be(AlertSeverity.High);
    }

    [Fact]
    public async Task AcknowledgeAlert_ShouldChangeStatus()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);
        var alert      = TestDataSeeder.CreateAlert(db, account.Id);

        var alertRepo  = new AlertRepository(db);
        var budgetRepo = new BudgetRepository(db);
        var service    = new AlertService(alertRepo, budgetRepo);

        // Act
        await service.AcknowledgeAlertAsync(alert.Id);

        // Assert
        var updated = await alertRepo.GetByIdAsync(alert.Id);
        updated!.Status.Should().Be(AlertStatus.Acknowledged);
    }

    [Fact]
    public async Task ResolveAlert_ShouldChangeStatus()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);
        var alert      = TestDataSeeder.CreateAlert(db, account.Id);

        var alertRepo  = new AlertRepository(db);
        var budgetRepo = new BudgetRepository(db);
        var service    = new AlertService(alertRepo, budgetRepo);

        // Act
        await service.ResolveAlertAsync(alert.Id);

        // Assert
        var updated = await alertRepo.GetByIdAsync(alert.Id);
        updated!.Status.Should().Be(AlertStatus.Resolved);
    }

    [Fact]
    public async Task GetActiveAlerts_ShouldReturnOnlyActive()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);

        TestDataSeeder.CreateAlert(db, account.Id, AlertSeverity.High,   AlertStatus.Active);
        TestDataSeeder.CreateAlert(db, account.Id, AlertSeverity.Medium, AlertStatus.Resolved);
        TestDataSeeder.CreateAlert(db, account.Id, AlertSeverity.Low,    AlertStatus.Acknowledged);

        var alertRepo  = new AlertRepository(db);
        var budgetRepo = new BudgetRepository(db);
        var service    = new AlertService(alertRepo, budgetRepo);

        // Act
        var actives = await service.GetActiveAlertsAsync(account.Id);

        // Assert
        actives.Should().HaveCount(1);
        actives.First().Severity.Should().Be(AlertSeverity.High);
    }

    [Fact]
    public async Task CheckAndGenerateBudgetAlerts_ShouldCreateAlertForExceededBudget()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);

        // Budget dépassé à 90%
        TestDataSeeder.CreateBudget(db, account.Id, 1000m, 900m);

        var alertRepo  = new AlertRepository(db);
        var budgetRepo = new BudgetRepository(db);
        var service    = new AlertService(alertRepo, budgetRepo);

        // Act
        await service.CheckAndGenerateBudgetAlertsAsync(account.Id);

        // Assert
        var alerts = await service.GetActiveAlertsAsync(account.Id);
        alerts.Should().HaveCountGreaterThan(0);
        alerts.First().Title.Should().Contain("Budget dépassé");
    }
}
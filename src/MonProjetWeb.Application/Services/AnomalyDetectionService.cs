using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Services;

public class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly ICostRepository  _costRepo;
    private readonly IAlertRepository _alertRepo;

    // Seuil : au-delà de X fois la moyenne = anomalie
    private const decimal AnomalyMultiplier = 2.0m;

    public AnomalyDetectionService(ICostRepository costRepo, IAlertRepository alertRepo)
    {
        _costRepo  = costRepo;
        _alertRepo = alertRepo;
    }

    public async Task<IEnumerable<Alert>> DetectAnomaliesAsync(int gcpAccountId)
    {
        var now          = DateTime.UtcNow;
        var currentStart = new DateTime(now.Year, now.Month, 1);

        // Coûts du mois en cours par service
        var currentCosts = await _costRepo.GetCostGroupedByServiceAsync(
            gcpAccountId, currentStart, now);

        var generatedAlerts = new List<Alert>();

        foreach (var (service, currentAmount) in currentCosts)
        {
            var isAnomaly = await IsAnomalyAsync(gcpAccountId, service, currentAmount);
            if (!isAnomaly) continue;

            var average = await GetAverageSpendAsync(gcpAccountId, service);

            // Éviter les doublons
            var exists = await _alertRepo.AnyAsync(a =>
                a.GcpAccountId == gcpAccountId &&
                a.Title.Contains(service)      &&
                a.Status == AlertStatus.Active  &&
                a.CreatedAt >= currentStart);

            if (exists) continue;

            var alert = new Alert
            {
                GcpAccountId   = gcpAccountId,
                Title          = $"Anomalie détectée : {service}",
                Message        = $"Le service '{service}' a dépensé {currentAmount:F2} USD ce mois-ci, soit {(currentAmount / average):F1}x la moyenne historique ({average:F2} USD).",
                Severity       = currentAmount > average * 3
                    ? AlertSeverity.Critical
                    : AlertSeverity.High,
                Status         = AlertStatus.Active,
                ThresholdValue = (decimal)(average * AnomalyMultiplier),
                ActualValue    = currentAmount,
                CreatedAt      = DateTime.UtcNow
            };

            await _alertRepo.AddAsync(alert);
            generatedAlerts.Add(alert);
        }

        if (generatedAlerts.Any())
            await _alertRepo.SaveChangesAsync();

        return generatedAlerts;
    }

    public async Task<bool> IsAnomalyAsync(
        int gcpAccountId, string serviceId, decimal currentAmount)
    {
        var average = await GetAverageSpendAsync(gcpAccountId, serviceId);
        if (average == 0) return false;
        return (currentAmount / average) > AnomalyMultiplier;
    }

    public async Task<decimal> GetAverageSpendAsync(
        int gcpAccountId, string serviceId, int historicalDays = 30)
    {
        var to   = DateTime.UtcNow.AddMonths(-1); // exclure le mois en cours
        var from = to.AddDays(-historicalDays);

        var records = await _costRepo.GetByPeriodAsync(gcpAccountId, from, to);
        var filtered = records.Where(c =>
            c.ServiceName == serviceId || c.ServiceId == serviceId).ToList();

        return filtered.Any() ? filtered.Average(c => c.Amount) : 0;
    }
}
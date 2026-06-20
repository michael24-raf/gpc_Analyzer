using Microsoft.AspNetCore.Mvc;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GcpAccountController : ControllerBase
{
    private readonly IGcpAccountRepository _accountRepo;

    public GcpAccountController(IGcpAccountRepository accountRepo)
    {
        _accountRepo = accountRepo;
    }

    /// <summary>Comptes GCP d'un utilisateur</summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var accounts = await _accountRepo.GetByUserAsync(userId);
        return Ok(accounts);
    }

    /// <summary>Détail d'un compte GCP avec toutes ses données</summary>
    [HttpGet("{accountId:int}")]
    public async Task<IActionResult> GetAccount(int accountId)
    {
        var account = await _accountRepo.GetWithDetailsAsync(accountId);
        if (account is null) return NotFound(new { message = "Compte GCP introuvable." });
        return Ok(account);
    }

    /// <summary>Créer un compte GCP</summary>
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateGcpAccountRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var account = new GcpAccount
        {
            UserId           = request.UserId,
            AccountName      = request.AccountName,
            ProjectId        = request.ProjectId,
            BillingAccountId = request.BillingAccountId,
            IsActive         = true,
            CreatedAt        = DateTime.UtcNow
        };

        await _accountRepo.AddAsync(account);
        await _accountRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { accountId = account.Id }, account);
    }

    /// <summary>Désactiver un compte GCP</summary>
    [HttpPatch("{accountId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account is null) return NotFound(new { message = "Compte GCP introuvable." });

        account.IsActive  = false;
        account.UpdatedAt = DateTime.UtcNow;

        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        return Ok(new { message = "Compte désactivé." });
    }

    /// <summary>Supprimer un compte GCP</summary>
    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> DeleteAccount(int accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account is null) return NotFound(new { message = "Compte GCP introuvable." });

        _accountRepo.Remove(account);
        await _accountRepo.SaveChangesAsync();

        return NoContent();
    }
}

// ── Request DTO ──────────────────────────────────────────────────────────────

public record CreateGcpAccountRequest(
    int    UserId,
    string AccountName,
    string ProjectId,
    string BillingAccountId);
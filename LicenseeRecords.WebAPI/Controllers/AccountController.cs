using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly DataService _dataService;

    // Constructor with dependency injection
    public AccountController(DataService dataService)
    {
        _dataService = dataService;
    }

    // GET: api/account
    [HttpGet]
    public ActionResult<List<Account>> GetAccounts()
    {
        var accounts = _dataService.GetAccounts();
        if (accounts == null || !accounts.Any())
        {
            return NotFound("No accounts found.");
        }
        return Ok(accounts);
    }

    // GET: api/account/{id}
    [HttpGet("{id}")]
    public ActionResult<Account> GetAccount(int id)
    {
        var account = _dataService.GetAccounts().FirstOrDefault(a => a.AccountId == id);
        if (account == null)
            return NotFound($"Account with ID {id} not found.");
        return Ok(account);
    }

    // POST: api/account
    [HttpPost]
    public ActionResult<Account> CreateAccount([FromBody] Account account)
    {
        if (account == null || string.IsNullOrWhiteSpace(account.AccountName))
        {
            return BadRequest("Invalid account data.");
        }

        var accounts = _dataService.GetAccounts();
        account.AccountId = accounts.Max(a => a.AccountId) + 1; // Generate new ID

        var licences = _dataService.GetLicences();
        account.ProductLicence[0].LicenceId = licences.Max(a => a.LicenceId) + 1; // Generate new ID

        accounts.Add(account);
        
        try
        {
            _dataService.SaveAccounts(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return CreatedAtAction(nameof(GetAccount), new { id = account.AccountId }, account);
    }

    // PUT: api/account/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateAccount(int id, [FromBody] Account updatedAccount)
    {
        if (updatedAccount == null || string.IsNullOrWhiteSpace(updatedAccount.AccountName))
        {
            return BadRequest("Invalid account data.");
        }

        var accounts = _dataService.GetAccounts();
        var index = accounts.FindIndex(a => a.AccountId == id);
        
        if (index == -1)
            return NotFound($"Account with ID {id} not found.");

        updatedAccount.AccountId = id; // Preserve the ID
        accounts[index] = updatedAccount;

        try
        {
            _dataService.SaveAccounts(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return NoContent();
    }

    // DELETE: api/account/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteAccount(int id)
    {
        var accounts = _dataService.GetAccounts();
        var accountToRemove = accounts.FirstOrDefault(a => a.AccountId == id);

        if (accountToRemove == null)
            return NotFound($"Account with ID {id} not found.");

        accounts.Remove(accountToRemove);

        try
        {
            _dataService.SaveAccounts(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return NoContent();
    }


    // PUT: api/account/{accountId}/productlicence/{licenceId}
    [HttpPut("{accountId}/productlicence/{licenceId}")]
    public IActionResult UpdateProductLicence(int accountId, int licenceId, [FromBody] ProductLicence updatedLicence)
    {
        if (updatedLicence == null || string.IsNullOrWhiteSpace(updatedLicence.LicenceStatus))
        {
            return BadRequest("Invalid product license data.");
        }

        var account = _dataService.GetAccountById(accountId); // Fetch the account by ID
        if (account == null)
            return NotFound($"Account with ID {accountId} not found.");

        var licenceIndex = account.ProductLicence.FindIndex(l => l.LicenceId == licenceId);
        if (licenceIndex == -1)
            return NotFound($"Product Licence with ID {licenceId} not found in account {accountId}.");

        // Update the license details
        updatedLicence.LicenceId = licenceId; // Preserve the ID
        account.ProductLicence[licenceIndex] = updatedLicence;

        var accounts = _dataService.GetAccounts();
        var index = accounts.FindIndex(a => a.AccountId == accountId);
        
        if (index == -1)
            return NotFound($"Account with ID {accountId} not found.");

        account.AccountId = accountId; // Preserve the ID
        accounts[index] = account;

        try
        {
            _dataService.SaveAccounts(accounts); // Save changes to the data source
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return NoContent(); // Return 204 No Content on success
    }


    // DELETE: api/account/{accountId}/productlicence/{licenceId}
    [HttpDelete("{accountId}/productlicence/{licenceId}")]
    public IActionResult DeleteProductLicence(int accountId, int licenceId)
    {
        // Fetch the account by ID
        var account = _dataService.GetAccountById(accountId); // Fetch the account by ID
        if (account == null)
        {
            return NotFound($"Account with ID {accountId} not found.");
        }

        // Find the index of the ProductLicence with the specified LicenceId
        var licenceIndex = account.ProductLicence.FindIndex(l => l.LicenceId == licenceId);
        if (licenceIndex == -1)
        {
            return NotFound($"Product Licence with ID {licenceId} not found in account {accountId}.");
        }

        // Remove the ProductLicence from the account
        account.ProductLicence.RemoveAt(licenceIndex);

        // Get the list of accounts
        var accounts = _dataService.GetAccounts();
        var accountIndex = accounts.FindIndex(a => a.AccountId == accountId);

        if (accountIndex == -1)
        {
            return NotFound($"Account with ID {accountId} not found in the list of accounts.");
        }

        // Update the account in the list of accounts
        accounts[accountIndex] = account;

        try
        {
            _dataService.SaveAccounts(accounts);
        }
        catch (Exception ex)
        {
        
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        // Return No Content status (204)
        return NoContent();
    }


    // POST: api/account/{accountId}
    [HttpPost("{accountId}")]
    public ActionResult<Account> CreateLicence(int accountId,[FromBody] ProductLicence newLicence)
    {
        if (newLicence == null )
        {
            return BadRequest("Invalid account data.");
        }
        var account = _dataService.GetAccountById(accountId); // Fetch the account by ID
        if (account == null)
        {
            return NotFound($"Account with ID {accountId} not found.");
        }

        var licences = _dataService.GetLicences();
        newLicence.LicenceId = licences.Max(a => a.LicenceId) + 1; // Generate new ID
        account.ProductLicence.Add(newLicence);
        // Get the list of accounts
        var accounts = _dataService.GetAccounts();
        var accountIndex = accounts.FindIndex(a => a.AccountId == accountId);

        if (accountIndex == -1)
        {
            return NotFound($"Account with ID {accountId} not found in the list of accounts.");
        }

        // Update the account in the list of accounts
        accounts[accountIndex] = account;
        
        try
        {
            _dataService.SaveAccounts(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return CreatedAtAction(nameof(GetAccount), new { id = account.AccountId }, account);
    }

}
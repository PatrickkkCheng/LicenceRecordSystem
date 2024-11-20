using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(HttpClient httpClient, ILogger<AccountController> logger)
    {
        _httpClient = httpClient;
        _logger = logger; // Injecting the logger
    }

    // GET: /Account
    public async Task<IActionResult> Index()
    {
        var accounts = await _httpClient.GetFromJsonAsync<List<Account>>("http://localhost:5267/api/account");
        return View(accounts);
    }

    // GET: /Account/Create
    public async Task<IActionResult> CreateAccount()
    {
        var products = await _httpClient.GetFromJsonAsync<List<Product>>("http://localhost:5267/api/product");
        return View(products);
    }

    // GET: /Account/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var account = await _httpClient.GetFromJsonAsync<Account>($"http://localhost:5267/api/account/{id}");

        if (account == null)
            return NotFound();

        // Log the ProductLicence for debugging
        if (account.ProductLicence != null && account.ProductLicence.Any())
        {
            _logger.LogInformation("Product Licenses for Account ID {AccountId}: {@ProductLicence}", id, account.ProductLicence);
        }
        else
        {
            _logger.LogInformation("No Product Licenses found for Account ID {AccountId}", id);
        }

        return View(account);
    }

    // GET: /Account/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var account = await _httpClient.GetFromJsonAsync<Account>($"http://localhost:5267/api/account/{id}");

        if (account == null)
            return NotFound();

        return View(account); // Return the account model to the edit view
    }

    // POST: /Account/Create
    [HttpPost]
    public async Task<IActionResult> CreateAccount( Account viewModel)
    {
        // if (!ModelState.IsValid)
        // {
        //     return View(viewModel);
        // }
        var productList = await _httpClient.GetFromJsonAsync<List<Product>>("http://localhost:5267/api/product");
        // _logger.LogInformation($"{JsonSerializer.Serialize(productList)}");
        // _logger.LogInformation($"{JsonSerializer.Serialize(viewModel)}");
        var account = new Account
        {
            AccountId = 0, // Will be assigned when saved to DB or storage
            AccountName = viewModel.AccountName,
            AccountStatus = viewModel.AccountStatus,
            ProductLicence = viewModel.ProductLicence.Select(licence => new ProductLicence
            {
                LicenceId = 0, // Will be assigned later
                LicenceStatus = licence.LicenceStatus,
                LicenceFromDate = licence.LicenceFromDate,
                LicenceToDate = licence.LicenceToDate,
                Product = new Product
                {
                    ProductId = licence.Product.ProductId,
                    ProductName = productList.FirstOrDefault(p => p.ProductId == licence.Product.ProductId)?.ProductName
                }
            }).ToList()
        };
        _logger.LogInformation($"{JsonSerializer.Serialize(account)}");
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5267/api/account", account);
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Product License with ID  was successfully updated.");
            return RedirectToAction(nameof(Index)); // Redirect to the index view after creation
        } 
        else
        {
            // Log the failure and add an error message
            var errorMessage = await response.Content.ReadAsStringAsync(); // Optional: Capture error message from response
            _logger.LogError($"Failed to update Product License with ID  . Status Code: {response.StatusCode}, Error: {errorMessage}");

            ModelState.AddModelError(string.Empty, "An error occurred while updating the product license.");
            // return View(productlicence); // Return to view with error message
            return RedirectToAction(nameof(Index)); // Return to view with error message
        }
    }



    // POST: /Account/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Account account)
    {
        account.AccountId = id;
        _logger.LogInformation($"{JsonSerializer.Serialize(account)}");
        // if (true)
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogInformation($"Model validation error: {error.ErrorMessage}");
            }
            return View(account); // Return to view with validation errors
        }
        // Perform the PUT request to update the account
        var response = await _httpClient.PutAsJsonAsync($"http://localhost:5267/api/account/{id}", account);

        if (response.IsSuccessStatusCode)
        {
            // Log success and redirect to the Index page
            _logger.LogInformation($"Account with ID {id} was successfully updated.");
            return RedirectToAction(nameof(Index)); // Redirect to index after successful update
        }
        else
        {
            // Log the failure and add an error message
            var errorMessage = await response.Content.ReadAsStringAsync(); // Optional: Capture error message from response
            _logger.LogError($"Failed to update account with ID {id}. Status Code: {response.StatusCode}, Error: {errorMessage}");
            
            ModelState.AddModelError(string.Empty, "An error occurred while updating the account.");
            return View(account); // Return to view with error message
        }
    }


    // DELETE: /Account/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _httpClient.DeleteAsync($"http://localhost:5267/api/account/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index)); // Redirect back to index after successful deletion
        }

        ModelState.AddModelError(string.Empty, "An error occurred while deleting the account.");
        return RedirectToAction(nameof(Index)); // Redirect back even on error
    }

    // DELETE: /DeleteLicence/{id}
    [HttpPost]
    public async Task<IActionResult> DeleteLicence(int id,int accountId)
    {
        var response = await _httpClient.DeleteAsync($"http://localhost:5267/api/account/{accountId}/productlicence/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index)); // Redirect back to index after successful deletion
        }

        ModelState.AddModelError(string.Empty, "An error occurred while deleting the account.");
        return RedirectToAction(nameof(Index)); // Redirect back even on error
    }


    // GET: /Account/EditLicense/{id}
    public async Task<IActionResult> EditLicense(int id)
    {
        // Fetch the product license by ID from your API or data source
        var accounts = await _httpClient.GetFromJsonAsync<List<Account>>($"http://localhost:5267/api/account/");

        if (accounts == null || !accounts.Any())
            return NotFound();

        var productLicense = accounts
        .SelectMany(account => account.ProductLicence) // Flatten all licences into one sequence
        .FirstOrDefault(licence => licence.LicenceId == id);


        var accountWithLicenceId = accounts
        .FirstOrDefault(account => account.ProductLicence
            .Any(licence => licence.LicenceId == id));

        var products = await _httpClient.GetFromJsonAsync<List<Product>>("http://localhost:5267/api/product");

        _logger.LogInformation($"{JsonSerializer.Serialize(accountWithLicenceId.AccountId)}");
        var viewModel = new EditProductLicenseViewModel
        {
            ProductLicence = productLicense,
            Products = products,
            AccountId= accountWithLicenceId.AccountId
        };
        return View(viewModel); // Return view for editing
    }
    

    // POST: /Account/UpdateLicense/{id}
    [HttpPost]
    public async Task<IActionResult> UpdateLicense(int id, int accountId,ProductLicence productlicence)
    {
        productlicence.LicenceId = id;
        var product = await _httpClient.GetFromJsonAsync<Product>($"http://localhost:5267/api/product/{productlicence.Product.ProductId}");
        productlicence.Product.ProductName = product.ProductName;

        _logger.LogInformation($"{JsonSerializer.Serialize(productlicence)}");
        _logger.LogInformation($"{JsonSerializer.Serialize(productlicence.Product.ProductId)}");
        // if (true)
        // if (!ModelState.IsValid)
        // {
        //     foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        //     {
        //         _logger.LogInformation($"Model validation error: {error.ErrorMessage}");
        //     }
        //     return RedirectToAction(nameof(Index)); // Return to view with validation errors
        // }
        // Perform the PUT request to update the account
        var response = await _httpClient.PutAsJsonAsync($"http://localhost:5267/api/account/{accountId}/productlicence/{id}", productlicence);

        if (response.IsSuccessStatusCode)
        {
            // Log success and redirect to the Index page
            _logger.LogInformation($"Product License with ID {id} for Account ID  was successfully updated.");
            return RedirectToAction(nameof(Index)); // Redirect to index after successful update
        }
        else
        {
            // Log the failure and add an error message
            var errorMessage = await response.Content.ReadAsStringAsync(); // Optional: Capture error message from response
            _logger.LogError($"Failed to update Product License with ID {id} . Status Code: {response.StatusCode}, Error: {errorMessage}");

            ModelState.AddModelError(string.Empty, "An error occurred while updating the product license.");
            // return View(productlicence); // Return to view with error message
            return RedirectToAction(nameof(Index)); // Return to view with error message
        }
    }


    // GET: /Account/CreateLicense
    public async Task<IActionResult> CreateLicense(int id)
    {
        var products = await _httpClient.GetFromJsonAsync<List<Product>>("http://localhost:5267/api/product");
        var viewModel = new CreateProductLicenseViewModel
        {
            Products = products,
            AccountId= id
        };
        return View(viewModel);
    }

    // POST: /Account/CreateLicense
    [HttpPost]
    public async Task<IActionResult> CreateNewLicense(int accountId,ProductLicence productlicence)
    {
        var product = await _httpClient.GetFromJsonAsync<Product>($"http://localhost:5267/api/product/{productlicence.Product.ProductId}");
        productlicence.Product.ProductName = product.ProductName;
        _logger.LogInformation($"{JsonSerializer.Serialize(accountId)}");
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5267/api/account/{accountId}", productlicence);
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Product License with ID {accountId} for Account ID  was successfully updated.");
            return RedirectToAction(nameof(Index)); // Redirect to the index view after creation
        } 
        else
        {
            // Log the failure and add an error message
            var errorMessage = await response.Content.ReadAsStringAsync(); // Optional: Capture error message from response
            _logger.LogError($"Failed to update Product License with ID {accountId} . Status Code: {response.StatusCode}, Error: {errorMessage}");

            ModelState.AddModelError(string.Empty, "An error occurred while updating the product license.");
            // return View(productlicence); // Return to view with error message
            return RedirectToAction(nameof(Index)); // Return to view with error message
        }
    }

}
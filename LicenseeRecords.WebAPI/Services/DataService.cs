using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

public class DataService
{
    private readonly string _accountsFilePath;
    private readonly string _productsFilePath;


    public DataService(IHostEnvironment hostEnvironment)
    {
        // Assuming Data folder is at the same level as your project folder
        var rootPath = Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", "Data"));
        
        _accountsFilePath = Path.Combine(rootPath, "Accounts.json");
        _productsFilePath = Path.Combine(rootPath, "Products.json");
    }


    public List<Account> GetAccounts()
    {
        var jsonString = File.ReadAllText(_accountsFilePath);
        return JsonSerializer.Deserialize<List<Account>>(jsonString);
    }

    public Account GetAccountById(int id)
    {
        var accounts = GetAccounts(); // Fetch all accounts
        return accounts.FirstOrDefault(a => a.AccountId == id); // Find and return the account with the specified ID
    }

    public void SaveAccounts(List<Account> accounts)
    {
        var jsonString = JsonSerializer.Serialize(accounts);
        File.WriteAllText(_accountsFilePath, jsonString);
    }

    public List<Product> GetProducts()
    {
        var jsonString = File.ReadAllText(_productsFilePath);
        return JsonSerializer.Deserialize<List<Product>>(jsonString);
    }

        public void SaveProducts(List<Product> products)
    {
        var jsonString = JsonSerializer.Serialize(products);
        File.WriteAllText(_productsFilePath, jsonString);
    }

    public List<ProductLicence> GetLicences()
    {
        // Read the JSON file into a string
        var jsonString = File.ReadAllText(_accountsFilePath);

        // Deserialize the JSON string into a list of Account objects
        var accounts = JsonSerializer.Deserialize<List<Account>>(jsonString);

        if (accounts == null)
        {
            // Return an empty list if deserialization fails or the file is empty
            return new List<ProductLicence>();
        }

        // Use LINQ to extract all ProductLicences into a single list
        var allLicences = accounts.SelectMany(account => account.ProductLicence).ToList();

        return allLicences;
    }

}
public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
}

public class EditProductLicenseViewModel
{
    public ProductLicence ProductLicence { get; set; }
    public List<Product> Products { get; set; } // Assuming you have a Product model
    public int AccountId { get; set; }
}

public class CreateProductLicenseViewModel
{
    public List<Product> Products { get; set; } // Assuming you have a Product model
    public int AccountId { get; set; }
}



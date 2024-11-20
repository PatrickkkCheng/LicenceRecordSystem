using System;
using System.Collections.Generic;

public class Account
{
    public int AccountId { get; set; }
    public string AccountName { get; set; }
    public string AccountStatus { get; set; }
    public List<ProductLicence> ProductLicence { get; set; }
}

public class ProductLicence
{
    public int LicenceId { get; set; }
    public string LicenceStatus { get; set; }
    public DateTime LicenceFromDate { get; set; }
    public DateTime? LicenceToDate { get; set; }
    public Product Product { get; set; }
}

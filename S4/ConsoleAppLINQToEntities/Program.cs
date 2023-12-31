﻿using ConsoleAppLINQToEntities.Models;

NorthwindContext context = new NorthwindContext();

Console.WriteLine("=================== 1) - Recherche clients par ville =========== ");


    IList<Customer> custs = (from c in context.Customers
                             where c.City == "Bruxelles"
                             select c).ToList();

    Console.WriteLine("Matching customers: " + custs.Count);

    foreach (Customer customer in custs)
    {
        Console.WriteLine(customer.ContactName);
    }

Console.WriteLine("============= 2) Exercice Lazy Loading. ===================");
IList<Product> products = context.Products.ToList();


Console.WriteLine("Catégorie : Beverages");

foreach (Product p in products)
{
    if (p.Category?.CategoryName == "Beverages") Console.WriteLine(p.ProductName);
}

Console.WriteLine("Catégorie : Condiments");
foreach (Product p in products)
{
    if (p.Category?.CategoryName == "Condiments") Console.WriteLine(p.ProductName);
}

Console.WriteLine("============= 3) Exercice Eager Loading. ===================");
IQueryable<Product> products1 = (from p in context.Products.Include("Category")
                                                                     select p);
Console.WriteLine("Catégorie : Beverages");
foreach (Product p in products1)
{
    if (p.Category?.CategoryName == "Beverages") Console.WriteLine(p.ProductName);
}

Console.WriteLine("Catégorie : Condiments");
foreach (Product p in products1)
{
    if (p.Category?.CategoryName == "Condiments") Console.WriteLine(p.ProductName);
}


Console.WriteLine("============= 4) Exercice Propriété de Navigation. ===================");
Console.WriteLine("Entrez le nom d'un client (ID):");

string? clientID = Console.ReadLine().Trim();

var orders = (from ord in context.Orders
                           where (ord.CustomerId == clientID) && (ord.ShippedDate != null)
                           orderby ord.OrderDate descending
                           select new { ord.CustomerId, ord.OrderDate, ord.ShippedDate });

foreach (var order in orders)
{
    Console.WriteLine("CustomerID : " + order.CustomerId + " OrderDate : " + order.OrderDate + " ShippedDate : " + order.ShippedDate);
}
	
Console.WriteLine("============= 5) Afficher le total des ventes par produit (ID  produit -> Total) trié par ordre de numéro produit. ===================");
var productSales = context.Products
                .Join(
                    context.OrderDetails,
                    product => product.ProductId,
                    orderDetail => orderDetail.ProductId,
                    (product, orderDetail) => new
                    {
                        product.ProductId,
                        TotalSales = orderDetail.Quantity * orderDetail.UnitPrice
                    }
                )
                .GroupBy(result => new { result.ProductId })
                .Select(group => new
                {
                    group.Key.ProductId,
                    TotalSales = group.Sum(result => result.TotalSales)
                })
                .OrderBy(o => o.ProductId)
                .ToList();

foreach (var productSale in productSales)
{
    Console.WriteLine($"Product ID: {productSale.ProductId}, Total Sales: {productSale.TotalSales}");
}

Console.WriteLine("============= 6) Afficher tous les employés (leur nom) qui ont sous leur responsabilité la région « Western ». ===================");

var matchingEmployees = context.Employees.Where(emp =>
    emp.Territories.Any(territory => // utiliser ANY quand je cherche un certain item dans une collection
        territory.Region != null &&
        territory.Region.RegionDescription == "Western"
    )
).ToList();

foreach (var employee in matchingEmployees)
{
    Console.WriteLine(employee.FirstName + " " + employee.LastName);
}


Console.WriteLine("============== 7) 7.Quels sont les territoires gérés par le supérieur de « Suyama Michael » ==========================");
var matchingTerritories = context.Territories
    .Where(territory => territory.Employees
    .Any(e => e.InverseReportsToNavigation
    .Any(e => e.LastName == "Suyama")))
    .ToList();

foreach(var territory in matchingTerritories)
{
    Console.WriteLine(territory.TerritoryDescription);
}

Console.WriteLine(" ============ UPDATES : Mettre en majuscule le nom de tous les clients ===================");
using (NorthwindContext ctx = new NorthwindContext()) {
    var customers = from e in ctx.Employees select e;

    foreach (var customer in customers)
    {
        customer.LastName = customer.LastName.ToUpper();
    }

    try
    {
        ctx.SaveChanges();
    } catch (Exception e)
    {
        Console.WriteLine($"{e.Message}");
    }

    Console.WriteLine("Done");
    foreach (var customer in customers)
    {
        Console.WriteLine(customer.LastName);
    }

}

Console.WriteLine(" ============ INSERT : Ajoutez une catégorie à partir d’un nom saisi au clavier ===================");
using (NorthwindContext ctx = new NorthwindContext())
{
    Console.WriteLine("Entrez une description de catégorie :");
    string? catName = Console.ReadLine().Trim();
    if (ctx.Categories.Where(e => e.CategoryName == catName).Count() > 0)
    {
        Console.WriteLine("Nom de catégorie existe déjà.");
    } else
    {
        Category cat = new Category
           {
               CategoryName = catName
           };
        try
            {
                ctx.Categories.Add(cat);
                ctx.SaveChanges();

    
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        Console.WriteLine("Done");

        
    }
    var c = ctx.Categories.Where(e => e.CategoryName.Contains(catName));
    Console.WriteLine(c.First().CategoryName);
}


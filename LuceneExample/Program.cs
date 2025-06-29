using Bogus;
using LuceneExample.Services;
using Microsoft.EntityFrameworkCore;

namespace LuceneExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Lucene example!");
            var dbContext = new AppDbContext();
            dbContext.Database.MigrateAsync();

            // Step 1: Rebuild the Lucene index
            var indexService = new LuceneIndexService();
            List<Product> products;

            if (!dbContext.Products.Any())
            {
                products = new();
                var faker = new Faker();
                for (int i = 0; i < 1000; i++)
                {
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = faker.Commerce.ProductName(),
                        Description = faker.Lorem.Paragraph(2),
                    });
                }
                dbContext.AddRange(products);
                dbContext.SaveChanges();
            }
            else
            {
                products = dbContext.Products.ToList();
            }
            Console.WriteLine("Database created.");
            indexService.BuildIndexIfNotExists(products);
            Console.WriteLine("Lucene indexed.");
            var searchService = new LuceneSearchService();
            var productService = new ProductService(dbContext, indexService, searchService);
            Search(productService);
        }
        static void Search(ProductService productService)
        {
            Console.WriteLine("Write search text:");
            var text = Console.ReadLine();
            var query = $""" Name:"{text}" or Description:"{text}" """;
            var searchResults = productService.SearchProducts(query, 10);

            for (int i = 1; i <= searchResults.Count; i++)
            {
                Console.WriteLine($"{i}-Id: {searchResults[i].Id}, Name: {searchResults[i].Name}, Description: {searchResults[i].Description}");
            }

            if (!searchResults?.Any() == true)
            {
                Console.WriteLine("Not exists your product");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
            Search(productService);
        }
    }
}

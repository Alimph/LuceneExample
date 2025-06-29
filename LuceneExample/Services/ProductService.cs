namespace LuceneExample.Services
{
    public class ProductService
    {
        private readonly AppDbContext _dbContext;
        private readonly LuceneIndexService _indexService;
        private readonly LuceneSearchService _searchService;

        public ProductService(AppDbContext dbContext, LuceneIndexService indexService, LuceneSearchService searchService)
        {
            _dbContext = dbContext;
            _indexService = indexService;
            _searchService = searchService;
        }

        // Rebuild the Lucene index
        public void RebuildIndex()
        {
            var products = _dbContext.Products.ToList();
            _indexService.BuildIndex(products);
        }

        // Search for products
        public List<Product> SearchProducts(string searchText, int count)
        {
            var productIds = _searchService.SearchProducts(searchText, count);
            return _dbContext.Products.Where(p => productIds.Contains(p.Id)).ToList();
        }
    }
}

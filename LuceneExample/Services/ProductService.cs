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
            var products = _dbContext.TaxProduct.ToList();
            _indexService.BuildIndex(products);
        }

        // Search for products
        public List<Product> SearchProducts(string searchText)
        {
            var productIds = _searchService.SearchProducts(searchText);
            return _dbContext.TaxProduct.Where(p => productIds.Contains(p.Id)).ToList();
        }
    }
}

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Document = Lucene.Net.Documents.Document;

namespace LuceneExample.Services
{
    public class LuceneIndexService
    {
        private static readonly string _luceneIndexDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "LuceneIndex");
        private static readonly LuceneVersion _appLuceneVersion = LuceneVersion.LUCENE_48;

        public void BuildIndexIfNotExists(IEnumerable<Product> products)
        {
            if(HasIndexFile())
                return; // Index already exists

            var directory = FSDirectory.Open(_luceneIndexDirectory);

            using var analyzer = new StandardAnalyzer(_appLuceneVersion);
            var indexConfig = new IndexWriterConfig(_appLuceneVersion, analyzer);
            using var writer = new IndexWriter(directory, indexConfig);

            foreach (var product in products)
            {
                var doc = new Document
                {
                    new StringField("Id", product.Id.ToString(), Field.Store.YES),
                    new TextField("Name", product.Name, Field.Store.YES),
                    new StringField("Description", product.Description, Field.Store.YES),
                };
                writer.AddDocument(doc);
            }
            writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        public List<int> SearchProducts(string searchText)
        {
            var directory = FSDirectory.Open(_luceneIndexDirectory);
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);

            using var analyzer = new StandardAnalyzer(_appLuceneVersion);
            var parser = new MultiFieldQueryParser(_appLuceneVersion, new[] { "Name", "Description" }, analyzer);

            var query = parser.Parse(searchText);
            var hits = searcher.Search(query, 10).ScoreDocs;

            return hits.Select(hit => int.Parse(searcher.Doc(hit.Doc).Get("Id"))).ToList();
        }

        public bool IsProductIndexed(string indexPath, Guid productId)
        {
            using var directory = FSDirectory.Open(indexPath);
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);

            var query = new TermQuery(new Term("Id", productId.ToString()));
            var hits = searcher.Search(query, 1).ScoreDocs;

            return hits.Length > 0; // Returns true if the product is indexed
        }

        public async Task UpdateIndexAsync(string indexPath, Product updatedProduct)
        {
            using var directory = FSDirectory.Open(indexPath);
            var analyzer = new StandardAnalyzer(_appLuceneVersion);
            using var indexWriter = new IndexWriter(directory, new IndexWriterConfig(_appLuceneVersion, analyzer));

            // Check if the product is already indexed
            if (IsProductIndexed(indexPath, updatedProduct.Id))
            {
                // Delete the old document
                indexWriter.DeleteDocuments(new Term("Id", updatedProduct.Id.ToString()));
            }

            // Create a new document for the updated product
            var doc = new Document
            {
                new StringField("Id", updatedProduct.Id.ToString(), Field.Store.YES),
                new TextField("Name", updatedProduct.Name, Field.Store.YES),
                new TextField("Description", updatedProduct.Description, Field.Store.YES)
            };

            // Add the new document to the index
            indexWriter.AddDocument(doc);
            indexWriter.Commit();
        }

        bool HasIndexFile()
        {
            return System.IO.Directory.Exists(_luceneIndexDirectory);
        }
    }
}

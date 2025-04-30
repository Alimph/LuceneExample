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
        private static readonly string LuceneIndexDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "LuceneIndex");
        private static readonly LuceneVersion _appLuceneVersion = LuceneVersion.LUCENE_48;

        public void BuildIndex(IEnumerable<Product> products)
        {
            var directory = FSDirectory.Open(LuceneIndexDirectory);

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
            var directory = FSDirectory.Open(LuceneIndexDirectory);
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);

            using var analyzer = new StandardAnalyzer(_appLuceneVersion);
            var parser = new MultiFieldQueryParser(_appLuceneVersion, new[] { "Name", "Description" }, analyzer);

            var query = parser.Parse(searchText);
            var hits = searcher.Search(query, 10).ScoreDocs;

            return hits.Select(hit => int.Parse(searcher.Doc(hit.Doc).Get("Id"))).ToList();
        }
    }
}

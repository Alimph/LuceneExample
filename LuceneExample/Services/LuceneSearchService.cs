using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace LuceneExample.Services
{
    public class LuceneSearchService
    {
        private static readonly string LuceneIndexDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "LuceneIndex");
        private static readonly LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        public List<Guid> SearchProducts(string searchText)
        {
            var directory = FSDirectory.Open(LuceneIndexDirectory);
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);

            using var analyzer = new StandardAnalyzer(AppLuceneVersion);
            var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "Title", "TaxRate" }, analyzer);

            var query = parser.Parse(searchText);
            var hits = searcher.Search(query, 500).ScoreDocs;

            return hits.Select(hit => Guid.Parse(searcher.Doc(hit.Doc).Get("Id"))).ToList();
        }
    }
}

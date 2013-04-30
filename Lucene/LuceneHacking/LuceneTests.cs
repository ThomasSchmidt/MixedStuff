using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Version = Lucene.Net.Util.Version;

namespace LuceneHacking
{
	[TestFixture]
    public class LuceneTests
    {
		[Test]
		public void CanCreateLuceneIndexOnDisk()
		{
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lucene_index"));
			using (Lucene.Net.Store.Directory directory = Lucene.Net.Store.FSDirectory.Open(di))
			using (Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
			{
				string test2 = "Lorem Ipsum è un testo segnaposto .....";
				using (Lucene.Net.Index.IndexWriter ixw = new Lucene.Net.Index.IndexWriter(directory, analyzer, true, new IndexWriter.MaxFieldLength(4096)))
				{
					Document document = new Document();
					document.Add(new Field("Id","<a title = \"test\" href = \"http://www.codewrecks.com/blog/index.php/2007/09/03/test/\"> test </a >.", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
					document.Add(new Field("content", "test", Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
					ixw.AddDocument(document);

					document = new Document();
					document.Add(new Field("Id", test2.GetHashCode().ToString(CultureInfo.InvariantCulture), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
					document.Add(new Field("content", test2, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
					ixw.AddDocument(document);
					ixw.Commit();
				}
			}
		}

		[Test]
		public void CanQueryLuceneIndexCreatedOnDisk()
		{
			CanCreateLuceneIndexOnDisk();

			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(System.IO.Path.GetTempPath());
			using (Lucene.Net.Store.Directory directory = Lucene.Net.Store.FSDirectory.Open(di))
			{
				Lucene.Net.Index.IndexReader ir = Lucene.Net.Index.IndexReader.Open(directory, true);
				Lucene.Net.Search.Searcher searcher = new Lucene.Net.Search.IndexSearcher(ir);
				using (Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
				{
					Lucene.Net.QueryParsers.QueryParser parser = new Lucene.Net.QueryParsers.QueryParser(Version.LUCENE_30, "content", analyzer);
					Lucene.Net.Search.Query query = parser.Parse("lorem");
					Lucene.Net.Search.TopScoreDocCollector collector = Lucene.Net.Search.TopScoreDocCollector.Create(100, true);
					searcher.Search(query, collector);
					Lucene.Net.Search.ScoreDoc[] docs = collector.TopDocs().ScoreDocs;

					foreach (Lucene.Net.Search.ScoreDoc scoreDoc in docs)
					{
						//Get the document that represents the search result.
						Document document = searcher.Doc(scoreDoc.Doc);

						var id = document.Get("Id");
						var content = document.Get("content");
					}
				}
			}
			
		}
    }
}

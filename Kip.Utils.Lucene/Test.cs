using LuceneNet = Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Tokenattributes;

namespace Kip.Utils.Lucene
{
    public class Test
    {
        private string[] ids = new string[] { "1", "2" };
        private string[] unindexed = new string[] { "Netherlands", "Italy" };
        private string[] unstored = new string[] { "Haha balabala Amsterdam", "balabala Venice" };
        private string[] text = new string[] { "Amsterdam", "Venice" };

        private Directory dir;

        public Test()
        {
            this.Setup();
        }

        private void Setup()
        {
            dir = new RAMDirectory();

            using (IndexWriter writer = Tools.getWriter(dir))
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    Document doc = new Document();
                    doc.Add(new Field("id", ids[i], Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new NumericField("id_int").SetIntValue(int.Parse(ids[i])));
                    doc.Add(new Field("country", unindexed[i], Field.Store.YES, Field.Index.NO));
                    doc.Add(new Field("contents", unstored[i], Field.Store.NO, Field.Index.ANALYZED));
                    doc.Add(new Field("city", text[i], Field.Store.YES, Field.Index.NOT_ANALYZED));
                    writer.AddDocument(doc);
                }
            }
        }

        public void TestIndexWriter()
        {
            using (IndexWriter writer = Tools.getWriter(dir))
            {
                Console.WriteLine("writer.NumDocs: {0}", writer.NumDocs());
            }
        }

        public void TestIndexReader()
        {
            using (IndexReader reader = IndexReader.Open(dir, true))
            {
                Console.WriteLine("reader.MaxDoc: {0}", reader.MaxDoc);
                Console.WriteLine("reader.NumDocs: {0}", reader.NumDocs());
            }
        }

        public void TestWriterLock()
        {
             new Thread(() =>
            {
                using (IndexWriter writer = Tools.getWriter(dir))
                {
                    Console.WriteLine("t1 start at {0}", DateTime.Now.ToString("mm:ss:fff"));
                    Thread.Sleep(1000);
                    Console.WriteLine("t1 end at {0}", DateTime.Now.ToString("mm:ss:fff"));
                }
            }).Start(); ;

            new Thread(() =>
            {
                while (IndexWriter.IsLocked(dir))
                {
                    Console.WriteLine("IndexWriter Is Locked");
                    Thread.Sleep(200);
                }

                using (IndexWriter writer = Tools.getWriter(dir))
                {
                    Console.WriteLine("t2 start at {0}", DateTime.Now.ToString("mm:ss:fff"));
                    Thread.Sleep(1000);
                    Console.WriteLine("t2 end at {0}", DateTime.Now.ToString("mm:ss:fff"));
                }
            }).Start();

        }

        public void TestIndexSearch()
        {
            using (var searcher = new IndexSearcher(dir))
            {
                // 项搜索
                Term t = new Term("city", "Amsterdam");
                Query query = new TermQuery(t);
                TopDocs docs = searcher.Search(query, 10);
                Console.WriteLine("TermQuery: {0}", docs.TotalHits);

                // 指定数字范围搜索
                NumericRangeQuery<int> nQuery = NumericRangeQuery.NewIntRange("id_int", 0, 1, true, true);
                docs = searcher.Search(nQuery, 10);
                Console.WriteLine("NumericRangeQuery: {0}", docs.TotalHits);

                // 短语搜索
                PhraseQuery pQuery = new PhraseQuery();
                // pQuery.Slop = 1; //default is 0
                pQuery.Add(new Term("contents", "balabala"));
                pQuery.Add(new Term("contents", "Amsterdam"));
                docs = searcher.Search(pQuery, 10);
                Console.WriteLine("PhraseQuery: {0}", docs.TotalHits);

                // 通配符搜索
                WildcardQuery wQuery = new WildcardQuery(new Term("contents", "Amsterda*"));
                docs = searcher.Search(wQuery, 10);
                Console.WriteLine("WildcardQuery: {0}", docs.TotalHits);

                MatchAllDocsQuery madQuery = new MatchAllDocsQuery("content");
                docs = searcher.Search(madQuery, 10);
                Console.WriteLine("MatchAllDocsQuery: {0}", docs.TotalHits);
            }
        }
    }

    static class Tools
    {
        /// <summary>
        /// 获取写索引
        /// </summary>
        public static IndexWriter getWriter(Directory dir)
        {
            IndexWriter writer = new IndexWriter(dir, new WhitespaceAnalyzer(), 
                IndexWriter.MaxFieldLength.UNLIMITED);

            return writer;
        }
    }
}

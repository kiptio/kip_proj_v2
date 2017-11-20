using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuceneNet = Lucene.Net;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;

namespace Kip.Utils.Lucene
{
    public class ReadMe
    {
        Directory dir;

        public ReadMe()
        {
            /** Directory
            SimpleFSDirectory：最简单的Directory子类，使用java.io.* API将文件存入到文件系统。不能很好的支持多线程操作。
            NIOFSDirectory：使用java.nio.* API将文件存入到文件系统。能很好的支持除Microsoft Windows之外的多线程操作，.NET版本目前无法使用。
            MMapDirectory：使用内存映射的I/O进行文件访问。对于64位系统或者小的索引尺寸来说是一个很好的选择。
            RAMDirectory：将所有文件都存入RAM。
            FileSwitchDirectory：使用两个文件目录，根据文件扩展名在两个目录之间切换使用。
            */
            /** 将另一个Directory中的内容拷贝到RAMDirectory
            RAMDirectory ramDir = new RAMDirectory(otherDir)
            */
            /** 在两个Directory之间进行所有文件拷贝的静态方法
            FSDirectory.Copy(Directory src, Directory dest, bool closeDirSrc)
            */
            dir = new RAMDirectory(); // 测试桩可以直接写入内存进行测试
            // dir = FSDirectory.Open("/path/to/index"); // 大写索引文件
        }

        /// <summary>
        /// 建立文档、域
        /// </summary>
        Document BuildDoc()
        {
            Document doc = new Document();

            /** 域存储选项（Field.Store.*）
            Store.YES：存储值域。该情况下，原始的字符串值全部被保存在索引中，并可以由IndexReader类恢复。如果索引的大小在搜索程序考虑之列的话，不要存储太大的域值，因为存储这些域值会消耗掉索引的存储空间。
            Store.NO：不存储域值。通常和Index.ANALYZED选项共同用来索引大的文本域值，通常这些域值不用恢复为初始格式。
            */
            /** 域索引选项（Field.Index.*）
            Index.ANALYZED：使用分析器将域值分解成独立的语汇单元流，并使语汇单元能被搜索。
            Index.NOT_ANALYZED：对域进行索引，但不对String值进行分析。该操作实际上将域值作为单一语汇单元并使之能被搜索。尤其适用于“精确匹配”搜索。
            Index.ANALYZED_NO_NORMS：这是Index.ANALYZED选项的变体，它不会再索引中存储norms信息。norms信息记录了索引中的index-time、boost（加权）信息，但是当你进行搜索时可能比较耗费内存。
            Index.NOT_ANALYZED_NO_NORMS：与Index.NOT_ANALYZED选项类似，但也是不存储norms。
            Index.NO：使对应的域值不被搜索
            */
            /** 项向量选项（Field.Store.*）
            有时索引完文档，你希望在搜索期间该文档所有的唯一项都能完全从文档域中检索。（我理解为索引项的位置信息）
            TermVector.YES：存储项向量。
            TermVector.NO：不存储项向量。
            */
            /** Field 对象
            Field(string name, TextReader reader[, TermVector termVector])：域值不被存储（域存储选项被硬编码为Store.NO），并且该域值会一直用于分析和索引（Index.ANALYZED）。当域值较大，内存保存代价较高时可以使用来初始化。
            Field(string name, TokenStream tokenStream[, TermVector termVector])：效果同上，只是域值对象为TokenStream（用于对域的预分析）。
            Field(string name, byte[] value_Renamed, Store store)：用来存储二进制域，域值不会被索引（Index.NO），也没有项向量（TermVector.NO）。其中store参数必须设置为Store.YES。
            Field(string name, byte[] value_Renamed, int offset, int length, Store store)：效果同上，区别在于该方法允许你对这个二进制的部分片段进行引用，该片段的起始位置为offset，处理长度为length字节数。
            */
            Field f = new Field("content", "something", Field.Store.NO, Field.Index.ANALYZED);

            /** 当Lucene建立起倒排索引时，默认会保存所有必要信息以实施Vector Space Model。该Model需要计算文档中出现的term数，以及它们的位置。
            但有时这些域只是在布尔搜索时用到，它们并不为相关评分做贡献，在这种情况下，可以通过调用“Field.OmitTermFreqAndPositions = true”，让Lucene跳过对该项的出现频率和出现位置的索引，单也会阻止获取这些信息。
            */
            f.OmitTermFreqAndPositions = true;

            // 多值域：如一篇文章有多个作者时，可以向这个域中写入多个不同的值：
            doc.Add(new Field("author", "张三", Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("author", "李四", Field.Store.YES, Field.Index.ANALYZED));

            // 数字值域：可以索引并能在搜索和排序中对它们进行精确匹配
            doc.Add(new NumericField("price").SetDoubleValue(199.99));

            // 域加权：使得该域比其他域值更重要。当改变域加权时，必须完全删除并创建对应的文档，或者使用updateDocument。
            // （默认的情况下，更短的域具有更高的加权，这取决于Lucene的评分算法具体实现）
            bool fileImportant = true;
            if (fileImportant) f.Boost = 1.2F;
            else f.Boost = 1.0F;

            // 文档加权：表示该文档针对索引中其他文档的重要程度。当改变文档加权时，必须完全删除并创建对应的文档，或者使用updateDocument。
            bool docImportant = true;
            if (docImportant) doc.Boost = 1.5F;
            else doc.Boost = 1.0F;

            return doc;
        }

        /// <summary>
        /// 获取写对象
        /// 
        /// IndexWriter线程安全：
        /// 对于一个索引来说，一次只能打开一个Writer。Lucene采用文件锁来提供保障，只有当IndexWriter对象呗关闭时，锁才会释放。可以在创建IndexWriter前调用IsLocked方法判断是否已被锁。
        /// </summary>
        private IndexWriter getWriter()
        {
            /** 域截取（IndexWriter.MaxFieldLength.*）
            MaxFieldLength.UNLIMITED：不采取截取策略。
            MaxFieldLength.LIMITED：只截取域中前1000个项，其后的文本则会全部忽略。
            */
            IndexWriter writer = new IndexWriter(dir, new WhitespaceAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);

            /** IndexWriter优化（会消耗大量的CPU和IO资源，建议在闲时进行，同时注意磁盘临时使用空间（大约3倍于优化数据），因为合并时会保存新段文件及保留旧的段文件直至IndexWriter.Commit。）
            Optimize()：将索引压缩至一个段，操作完成后再返回。
            Optimize(bool doWait)：效果同上，若doWait=false，调用会立即执行，但合并工作在后台运行。
            Optimize(int maxNumSegments)：部分优化，将索引压缩为最多maxNumSegments个段（合并到一个段的开销最大，建议优化到5个段）。
            Optimize(int maxNumSegments, bool doWait)：效果为上两个功能合并。
            */
            writer.Optimize(5);

            return writer;
        }

        /// <summary>
        /// 获取读对象
        /// </summary>
        private IndexReader getReader()
        {
            /** IndexReader线程安全：
            任意数量的只读属性的IndexReader类都可以同时打开一个索引。
            非只读属性的IndexReader对象在修改索引的时候会被作为Writer使用：它必须在修改内容之前成功的获取Writer锁，并在被关闭时释放该锁。
            IndexReader对象甚至可以在IndexWriter对象正在修改索引时打开。
            */
            IndexReader reader = IndexReader.Open(dir, true);

            // 创建近实时reader。IndexWriter返回的reader能够对索引中所有之前提交的变更进行搜索，还包括所有未提交的变更。返回的reader是只读的。
            // reader = this.getWriter().GetReader();

            // 在创建IndexReader时，它会搜索已有的索引快照。如果你需要搜索索引中的变更信息，那么必须打开一个新的reader。
            // 所幸的是，可以通过Reopen方法来获取一个新的IndexReader，重新的IndexReader能在耗费较少系统资源的情况下使用当前reader来获取索引中所有的变更信息。
            IndexReader newReader = reader.Reopen();
            if (reader != newReader)
            {
                reader.Dispose(); // 这里必须保证线程安全。
                reader = newReader;
            }

            return reader;
        }

        /// <summary>
        /// 获取搜索对象
        /// </summary>
        private IndexSearcher getSearcher()
        {
            // 因为打开一个IndexReader需要较大的系统开销，所以最好在所有搜索期间都重复使用同一个IndexReader实例。
            IndexSearcher searcher = new IndexSearcher(this.getReader());

            // 从索引目录直接创建时，系统会在后台建立自己私有的IndexReader，可以通过GetIndexReader来获取该实例；在关闭searcher时，同时也会关闭私有的IndexReader
            // searcher = new IndexSearcher(dir);

            return searcher;
        }

        /// <summary>
        /// 写入文档
        /// </summary>
        public void AddDoc()
        {
            // getWriter().addDocument(document[, analyzer]);
        }

        /// <summary>
        /// 更新文档，实际是先删除文档，在添加新文档操作的合并
        /// </summary>
        public void UpdateDoc()
        {
            // getWriter().updateDocument(term, document[, analyzer]);
        }

        public void Search()
        {
            using (var searcher = this.getSearcher())
            {
                // 项搜索
                TermQuery tQuery = new TermQuery(new Term("content", "test"));

                // 短语搜索
                PhraseQuery pQuery = new PhraseQuery();
                pQuery.Slop = 1; //单词间隔距离。 default is 0
                pQuery.Add(new Term("content", "test"));

                // 指定数字范围搜索
                NumericRangeQuery<int> nQuery = NumericRangeQuery.NewIntRange("price", 0, 100, true, true);

                // 字符串前缀搜索
                PrefixQuery prefQuery = new PrefixQuery(new Term("category", "/technology/computers")); // 搜索计算机类型的书籍，包括其子类，如/technology/computers/programming

                // 组合查询，如下用法表示 搜索域包含“test”且价格在 0~100 之间的书籍
                BooleanQuery bQuery = new BooleanQuery();
                bQuery.Add(pQuery, Occur.MUST);
                bQuery.Add(nQuery, Occur.MUST);

                // 通配符搜索：*代表0或多个字母，?代表0或1个字母
                // 较长的前缀（第一个通配符前面）可以减少用于查找匹配项的个数，如以通配符作为首的查询模式会强制枚举所有索引中的项用于搜索匹配。
                WildcardQuery wQuery = new WildcardQuery(new Term("content", "tes*"));

                // 模糊搜索。通过tast可以查找到相近的匹配test（相似度）
                // FuzzyQuery会尽可能地枚举出一个索引的所有项。因此，最好尽量少地使用这类查询
                FuzzyQuery fQuery = new FuzzyQuery(new Term("content", "tast"));

                // 匹配所有文档，一般用于为索引中的文档进行评分加权
                MatchAllDocsQuery madQuery = new MatchAllDocsQuery("content");

                // 解释查询表达式
                QueryParser parser = new QueryParser(LuceneNet.Util.Version.LUCENE_30,
                    "content", new SimpleAnalyzer());

                /** 短语搜索（parser.Parse）
                java：默认域包含java项的文档
                java junit || java OR junit：默认域包含java和junit中一个或两个的文档。
                +java +junit || java AND junit：默认域中同时包含java和junit的文档。
                title:ant：title域中包含ant项的文档。
                title:extreme -subject:sports || title:extreme AND NOT subject:sports：title域中包含extreme且subject域中不包含sports的文档。
                (agile OR extreme) AND methodlogy： 默认域中包含methodlogy且包含agile和extreme中的一个或两个的文档。
                title: "junit in action"：title域中为“junit in action”的文档。
                title: "junit action" ~5：title域中为“junit”和“action”之间距离小于5的文档。
                java*：包含由java开头的项的文档。
                java~：包含与java相近的项的文档。
                lastmodified: {1/1/09 TO 12/31/09}：lastmodified域值在2009/1/1和2009/12/31之间的文档。中括号表示包含两端的值，大括号表示排除两端的值
                */
                Query query = parser.Parse("+JUNIT +ANT -MOCK");
                query = parser.Parse("title:[Q TO V]");
                TopDocs docs = searcher.Search(query, 10);

                // 查看Query对象解释后的信息：
                Console.WriteLine(query.ToString());
                
            }
        }

        private void getAnalyzer()
        {
            // WhitespaceAnalyze：通过空格分割文本信息，并不对生成的语汇单元进行其他的规范化处理。
            // SimpleAnalyzer：首先通过非字母字符来分割文本信息，然后将语汇单元统一为小写形式。
            // StopAnalyzer：与SimpleAnalyzer类似。区别在于，StopAnalyzer会去除常用单词。
            // StandardAnalyzer：这是Lucene最复杂的核心分析器。它包含大量的逻辑操作来识别某些种类的语汇单元，比如公司名称、Email地址以及主机名称等。它还会将语汇单元转换成小写形式，并去除停用词和标点符号。
        }
    }
}


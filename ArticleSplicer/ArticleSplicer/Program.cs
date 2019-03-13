using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenNLP;
using OpenNLP.Tools.Chunker;
using OpenNLP.Tools.Parser;
using OpenNLP.Tools.PosTagger;
using OpenNLP.Tools.Tokenize;

namespace ArticleSplicer
{
    class Program
    {

        // Browser global variables
        private static bool browserFuncIsDone = false;  // The flag used to check if the document is ready for headline taking
        private static HtmlDocument browserHTML;        // The Document being used for headline taking

        // All the types of news websites
        private static string[] types = new string[] { "Normal", "Gaming", "Fake Australian", "Mixed" };


        /// <summary>
        /// Run a browser to harvest all their headlines.
        /// </summary>
        /// <param name="url"></param>
        private static void runBrowserThread(string url)
        {
            var th = new Thread(() => {
                var br = new WebBrowser();
                br.ScriptErrorsSuppressed = true;
                br.DocumentCompleted += browser_DocumentCompleted;
                br.Navigate(url);
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = false;
            th.Start();
        }


        /// <summary>
        /// Post to Facebook. However because Facebook has strict and fustrating api services. Just print it to console. 
        /// </summary>
        /// <param name="_headline">The headline being printed</param>
        private static void runFacebookCrawlerThread(string _headline)
        {
            
            var th = new Thread(() => {
                FacebookPoster fbc = new FacebookPoster(_headline);
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
        }


        /// <summary>
        /// When the browser has completed navigating/generating the document switch a complete signal on.
        /// </summary>
        private static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var br = sender as WebBrowser;
            if (br.Url == e.Url)
            {
                Console.WriteLine("Natigated to {0}", e.Url);
                browserHTML = br.Document;
                browserFuncIsDone = true;
            }
            browserHTML = br.Document;
            (sender as WebBrowser).Stop();
        }


        /// <summary>
        /// List headlines starts a browser thread which will be used to obtain headlines
        /// </summary>
        /// <param name="_url">The news sites url</param>
        /// <param name="_list">The headline list being iterated and added to</param>
        /// <param name="_procedure">Which procedure needed to obtain headline</param>
        /// <returns></returns>
        private static Dictionary<string, string> listHeadlines(string _url, Dictionary<string, string> _list, string _procedure)
        {

            // Start the browser and grab its HTML
            Dictionary<string, string> retString = _list;
            runBrowserThread(_url);
            while (!browserFuncIsDone)
            {
                Application.DoEvents();
            }
            browserFuncIsDone = false;
            HtmlDocument browserHTMLTemp = browserHTML;

            // Try the given procedure and grab heading links
            // Tbh its really interesting that all websites use pretty much the same html structure.
            // But because its a little different building a headline html parser would be a little difficult
            // So my use of _procedure is simply, useless. The url is enough.
            try
            {
                if (_procedure == "news")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h4"))
                    {
                        if (h.OuterHtml.Contains("heading") && !h.OuterHtml.Contains("link-text"))
                        {
                            if (h.GetElementsByTagName("a").Count > 0 && !retString.ContainsKey(h.GetElementsByTagName("a")[0].InnerHtml))
                            {
                                retString.Add(h.GetElementsByTagName("a")[0].InnerHtml, _url);
                            }
                        }
                    }
                }
                else if (_procedure == "9")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("span"))
                    {
                        if (h.OuterHtml.Contains("story__headline__text"))
                        {
                            if (h.GetElementsByTagName("a").Count > 0 && !retString.ContainsKey(h.GetElementsByTagName("a")[0].InnerHtml))
                            {
                                retString.Add(h.GetElementsByTagName("a")[0].InnerHtml, _url);
                            }
                        }
                    }
                }
                else if (_procedure == "abc")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h3"))
                    {
                        if (h.GetElementsByTagName("a").Count > 0 && !retString.ContainsKey(h.GetElementsByTagName("a")[0].InnerHtml))
                        {
                            retString.Add(h.GetElementsByTagName("a")[0].InnerHtml, _url);
                        }
                    }
                }
                else if (_procedure == "sbs")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("p"))
                    {
                        if (h.OuterHtml.Contains("headline preview__headline"))
                        {
                            if (h.GetElementsByTagName("a").Count > 0 && !retString.ContainsKey(h.GetElementsByTagName("a")[0].InnerHtml))
                            {
                                retString.Add(h.GetElementsByTagName("a")[0].InnerHtml, _url);
                            }
                        }
                    }
                }
                else if (_procedure == "spot")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h3"))
                    {
                        if (h.OuterHtml.Contains("media-title") && !retString.ContainsKey(h.InnerHtml))
                        {
                            retString.Add(h.InnerHtml, _url);
                        }
                    }
                }
                else if (_procedure == "pcgamer")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h3"))
                    {
                        if (h.OuterHtml.Contains("article-name") && !retString.ContainsKey(h.InnerHtml))
                        {
                            retString.Add(h.InnerHtml, _url);
                        }
                    }
                }
                else if (_procedure == "kotaku")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("span"))
                    {
                        if (h.OuterHtml.Contains("headline") && !retString.ContainsKey(h.InnerHtml))
                        {
                            retString.Add(h.InnerHtml, _url);
                        }
                    }
                }
                else if (_procedure == "techradar")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h3"))
                    {
                        if (h.OuterHtml.Contains("article-name") && !retString.ContainsKey(h.InnerHtml))
                        {
                            retString.Add(h.InnerHtml, _url);
                        }
                    }
                } else if (_procedure == "shovel")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h2"))
                    {
                        if (h.GetElementsByTagName("a").Count > 0 && !retString.ContainsKey(h.GetElementsByTagName("a")[0].InnerHtml))
                        {
                            retString.Add(h.GetElementsByTagName("a")[0].InnerHtml, _url);
                        }
                    }
                } else if (_procedure == "crikey")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("span"))
                    {
                        if (h.OuterHtml.Contains("underline-hover") && !retString.ContainsKey(h.InnerHtml) && CountWords(h.InnerHtml) > 6)
                        {
                            retString.Add(h.InnerHtml, _url);
                        }
                    }
                } else if (_procedure == "betoot")
                {
                    foreach (HtmlElement h in browserHTMLTemp.GetElementsByTagName("h3"))
                    {
                        if (h.OuterHtml.Contains("entry-title") && !retString.ContainsKey(h.InnerHtml))
                        {
                            if (h.GetElementsByTagName("a").Count > 0 && !retString.ContainsKey(h.GetElementsByTagName("a")[0].InnerHtml))
                            {

                                retString.Add(h.GetElementsByTagName("a")[0].InnerHtml, _url);
                            }
                        }
                    }
                }
                return retString;
            } catch (Exception e)
            {
                // If the procedure doesnt go well. Return the original headlines and tell us why.
                Console.WriteLine("Error grabbing headlines from " + _procedure + ": " +  e);
                return retString;
            }
        }
        

        /// <summary>
        /// Counts how many words are within a string
        /// </summary>
        /// <param name="s">The string being counted</param>
        /// <returns></returns>
        public static int CountWords(string s)
        {
            int c = 0;
            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i - 1]) == true)
                {
                    if (char.IsLetterOrDigit(s[i]) == true ||
                        char.IsPunctuation(s[i]))
                    {
                        c++;
                    }
                }
            }
            if (s.Length > 2)
            {
                c++;
            }
            return c;
        }


        /// <summary>
        /// Grab headlines simple sorts through each known news websites by type then groups their headlines
        /// </summary>
        /// <param name="_type">The type of news being headline being taken and spliced</param>
        /// <returns></returns>
        public static Dictionary<string, string> grabHeadlines(string _type)
        {
            Dictionary<string, string> headlines = new Dictionary<string, string>();
            if (_type == "Gaming" || _type == "Mixed")
            {
                Console.WriteLine("Gaming News Selected");
                headlines = listHeadlines("https://www.gamespot.com/news/", headlines, "spot");
                headlines = listHeadlines("https://www.pcgamer.com/news/", headlines, "pcgamer");
                //headlines = listHeadlines("https://www.kotaku.com.au/", headlines, "kotaku");
                headlines = listHeadlines("https://www.techradar.com/au/news/gaming", headlines, "techradar");
            }
            if (_type == "Normal" || _type == "Mixed")
            {
                Console.WriteLine("Normal News Selected");
                //headlines = listHeadlines("https://www.news.com.au", headlines, "news");
                headlines = listHeadlines("https://www.9news.com.au/", headlines, "9");
                headlines = listHeadlines("https://www.abc.net.au/news/justin/", headlines, "abc");
                headlines = listHeadlines("https://www.sbs.com.au/news/latest", headlines, "sbs");
            }
            if (_type == "Fake Australian" || _type == "Mixed")
            {
                Console.WriteLine("Fake Australian News Selected");
                headlines = listHeadlines("http://www.theshovel.com.au/", headlines, "shovel");
                headlines = listHeadlines("https://www.crikey.com.au/", headlines, "crikey");
                headlines = listHeadlines("https://www.betootaadvocate.com/category/breaking-news/", headlines, "betoot");
            }
            return headlines;
        }

        /// <summary>
        /// The main program that drives each function to obtain wacky headlines
        /// </summary>
        static void Main(string[] args)
        {
            // Initiate the starting type and cycle counter
            string type = types[0];
            int cycles = 0;

            while (true)
            {
                // Grab all the headlines and put it through our Article Parser
                ArticleParser ap = new ArticleParser(grabHeadlines(type));

                // Write all our language tags to a file
                // More on language tags using OpenNLP https://www.codeproject.com/Articles/12109/Statistical-parsing-of-English-sentences This has very good example on how to use OpenNLP
                string tagpath = AppDomain.CurrentDomain.BaseDirectory + '\\' + "LanguageTags.txt";
                if (!File.Exists(tagpath))
                {
                    using (StreamWriter sw = File.CreateText(tagpath))
                    { sw.WriteLine(ap.printStructures()); }
                } else
                {
                    using (StreamWriter sw = new StreamWriter(tagpath, true))
                    { sw.WriteLine(ap.printStructures()); }
                }

                // Clear and write 4 unique headlines that are at least 4 words in length
                Console.Clear();
                Console.WriteLine(type + " News: \n");
                int iter = 0;
                while (true)
                {
                    // Jumple all the parsed headlines and then generate one out of it
                    ap.jumbleParseHeadlines();
                    string oneHeadline = ap.generateHeadling();

                    // If its shorter than 4, go back.
                    if (CountWords(oneHeadline) < 4)
                    {
                        continue;
                    }

                    // Usually it would post to a facebook page
                    //runFacebookCrawlerThread("Beep Boop " + _type + " News: \n" + oneHeadline);
                    Console.WriteLine(iter.ToString() + ". " + oneHeadline);

                    // Write the new headline to a text file db and tell the console
                    string headlinepath = AppDomain.CurrentDomain.BaseDirectory + '\\' + "Headlines.txt";
                    if (!File.Exists(headlinepath))
                    {
                        using (StreamWriter sw = File.CreateText(headlinepath))
                        { sw.WriteLine(oneHeadline); }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(headlinepath, true))
                        { sw.WriteLine(oneHeadline); }
                    }

                    iter++;
                    if (iter == 4)
                    {
                        break;
                    }
                }

                // Add a cycle and change the type respectivly then wait
                cycles++;
                type = types[cycles % types.Length];
                Console.WriteLine("Waiting a minute for reading time:");
                Console.WriteLine("Next is " + type + " News:");
                Thread.Sleep(60000);

                // Clear console and collect garbage
                Console.Clear();
                GC.Collect();
            }
        }
    }
}

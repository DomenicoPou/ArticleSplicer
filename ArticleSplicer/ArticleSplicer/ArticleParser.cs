using OpenNLP.Tools.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleSplicer
{
    class ArticleParser
    {

        private static string mModelPath = AppDomain.CurrentDomain.BaseDirectory + '\\';

        Dictionary<string, string> headlines;


        List<Parse> parsedHeadline;


        List<List<string>> structureList;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_ret"></param>
        /// <param name="_parse"></param>
        /// <param name="_thresh"></param>
        /// <returns></returns>
        public List<Parse> parseHeadline(List<Parse> _ret, List<Parse> _parse, int _thresh)
        {
            for (int i = 0; i < _parse.Count(); i++)
            {
                if (_parse[i].ToString().Length > _thresh)
                {
                    foreach (Parse p in _parse[i].GetChildren())
                    {
                        _parse.Add(p);
                    }
                    _parse.RemoveAt(i);
                    parseHeadline(_ret, _parse, _thresh);
                } else
                {
                    _ret.Add(_parse[i]);
                    _parse.RemoveAt(i);
                }
            }
            return _ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_headlines"></param>
        public ArticleParser(Dictionary<string, string> _headlines)
        {
            headlines = _headlines;
            parsedHeadline = new List<Parse>();
            structureList = new List<List<string>>();
            int count = _headlines.Count();
            int i = 1;
            Console.WriteLine("Start parsing all headlines.\n");
            foreach (KeyValuePair<string, string> line in headlines)
            {
                try
                {
                    EnglishTreebankParser parser =
                               new EnglishTreebankParser(mModelPath, true, false);
                    Parse sentenceParse = parser.DoParse(line.Key);
                    List<Parse> g = sentenceParse.GetChildren()[0].GetChildren().ToList();
                    Console.WriteLine(i + "/" + count);
                    Console.WriteLine(line.Key);
                    Console.WriteLine(line.Value);
                    Console.WriteLine();
                    List<string> structure = new List<string>();
                    foreach (Parse p in g)
                    {
                        structure.Add(p.Type);
                    }
                    structureList.Add(structure);

                    int threshold = (int)Math.Floor((double)(line.Key.Length / 3));

                    List<Parse> ph = parseHeadline(new List<Parse>(), g, threshold);
                    foreach (Parse p in ph)
                    {
                        parsedHeadline.Add(p);
                    }
                    i++;
                } catch (Exception e)
                {
                    Console.WriteLine("Error when parsing headlines: " + e);
                }
            }
        }

        public void jumbleParseHeadlines ()
        {
            List<Parse> tempParsed = new List<Parse>();
            
            int[] intArray = new int[parsedHeadline.Count];
            for (int i = 0; i < intArray.Length; i++)
            {
                intArray[i] = i;
            }

            Random r = new Random();
            for (int i = intArray.Length; i > 0; i--)
            {
                int j = r.Next(i);
                int k = intArray[j];
                intArray[j] = intArray[i - 1];
                intArray[i - 1]  = k;
            }

            for (int i = 0; i < intArray.Length; i++)
            {
                tempParsed.Add(parsedHeadline[intArray[i]]);
            }

            parsedHeadline = tempParsed;
        }

        public string printStructures ()
        {
            string ret = "";
            foreach (List<string> l in structureList)
            {
                foreach (string s in l)
                {
                    ret += s + " ";
                }
                ret += '\n';
            }
            return ret;
        }


        public string generateHeadling()
        {
            List<string> structure = new List<string>() {"NP", "VP"};
            string retHeadline = "";

            // Find NP
            for (int i = 0; i < parsedHeadline.Count; i++)
            {
                if (parsedHeadline[i].Type == "NP")
                {
                    retHeadline += parsedHeadline[i].ToString() + " ";
                    parsedHeadline.RemoveAt(i);
                    //Console.WriteLine("NP");
                    break;
                }
            }


            Random rand = new Random();
            if (rand.Next(0, 2) != 0)
            {
                for (int i = 0; i < parsedHeadline.Count; i++)
                {
                    if (parsedHeadline[i].Type == "ADVP")
                    {
                        retHeadline += parsedHeadline[i].ToString() + " ";
                        parsedHeadline.RemoveAt(i);
                        //Console.WriteLine("ADVP");
                        break;
                    }
                }
            }

            bool br = false;
            while (!br)
            {
                
                if (rand.Next(0, 2) != 0)
                {
                    for (int i = 0; i < parsedHeadline.Count; i++)
                    {
                        if (parsedHeadline[i].Type == "VP")
                        {
                            retHeadline += parsedHeadline[i].ToString() + " ";
                            parsedHeadline.RemoveAt(i);
                            br = true;
                            //Console.WriteLine("VP");
                            break;
                        }

                    }
                }
                else
                {
                    for (int i = 0; i < parsedHeadline.Count; i++)
                    {
                        if (parsedHeadline[i].Type == "PP")
                        {
                            retHeadline += parsedHeadline[i].ToString() + " ";
                            parsedHeadline.RemoveAt(i);
                            br = true;
                            //Console.WriteLine("PP");
                            break;
                        }
                    }
                }

            }
            retHeadline.TrimEnd();
            return retHeadline;
        }
    }
}

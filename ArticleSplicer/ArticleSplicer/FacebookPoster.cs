using Facebook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArticleSplicer
{
    /// <summary>
    /// THIS CLASS IS DEPRETIATED (due to Facebooks annoying api enquiry system)
    /// </summary>
    class FacebookPoster
    {
        // Initialize headline
        public string headline;
        

        public FacebookPoster(string _headline)
        {
            headline = _headline;
            PostToPage(_headline, "");

        }
        

        private static void PostToPage(string message, string code)
        {
            var fb = new FacebookClient(code);
            var argList = new Dictionary<string, object>();
            dynamic parameters = new ExpandoObject();
            parameters.message = message;
            parameters.formatting = "MARKDOWN";
            fb.Post("", parameters);
        }
    }
}
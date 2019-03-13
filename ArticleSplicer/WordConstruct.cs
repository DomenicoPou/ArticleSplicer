using System;

public class WordConstruct
{
    public string word;
    public string type;
    public int count;
    public List<string> url;

    public WordConstruct(string _word, string _type)
    {
        word = _word;
        type = _type;
        count = 0;
        url = new List<string>();
    }

    public addUrl (string _url)
    {
        url.add(_url);
    }

    public incrementWord()
    {
        count++;
    }
}

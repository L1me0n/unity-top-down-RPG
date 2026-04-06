using System;

[Serializable]
public class TipPageData
{
    public string title;
    public string body;

    public TipPageData()
    {
        title = "";
        body = "";
    }

    public TipPageData(string title, string body)
    {
        this.title = title;
        this.body = body;
    }
}
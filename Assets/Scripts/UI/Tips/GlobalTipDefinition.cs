using System;

[Serializable]
public class GlobalTipDefinition
{
    public string tipId;
    public GlobalTipCategory category;
    public string title;

    [UnityEngine.TextArea(3, 8)]
    public string body;

    public bool showOnlyOnce = true;

    public GlobalTipDefinition()
    {
        tipId = "";
        category = GlobalTipCategory.General;
        title = "";
        body = "";
        showOnlyOnce = true;
    }

    public GlobalTipDefinition(
        string tipId,
        GlobalTipCategory category,
        string title,
        string body,
        bool showOnlyOnce = true)
    {
        this.tipId = tipId;
        this.category = category;
        this.title = title;
        this.body = body;
        this.showOnlyOnce = showOnlyOnce;
    }
}
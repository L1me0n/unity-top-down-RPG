using UnityEngine;

[CreateAssetMenu(
    fileName = "GhostDialogueDatabase",
    menuName = "Chief of Sin/Ghost Dialogue Database")]
public class GhostDialogueDatabase : ScriptableObject
{
    [Header("Rescue: Thanks / Gratitude")]
    [TextArea(2, 4)]
    public string[] rescueThanksLines;

    [Header("Rescue: Relief / Escape")]
    [TextArea(2, 4)]
    public string[] rescueReliefLines;

    [Header("Campfire: Idle Ambient")]
    [TextArea(2, 4)]
    public string[] campfireIdleLines;

    [Header("Campfire: Questions")]
    [TextArea(2, 4)]
    public string[] campfireQuestionLines;

    [Header("Campfire: Answers")]
    [TextArea(2, 4)]
    public string[] campfireAnswerLines;

    public string GetRandomRescueLine()
    {
        string line = GetRandomLine(rescueThanksLines);
        if (!string.IsNullOrWhiteSpace(line))
            return line;

        return GetRandomLine(rescueReliefLines);
    }

    public string GetRandomCampfireIdleLine()
    {
        return GetRandomLine(campfireIdleLines);
    }

    public string GetRandomCampfireQuestionLine()
    {
        return GetRandomLine(campfireQuestionLines);
    }

    public string GetRandomCampfireAnswerLine()
    {
        return GetRandomLine(campfireAnswerLines);
    }

    private string GetRandomLine(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return string.Empty;

        int count = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
                count++;
        }

        if (count == 0)
            return string.Empty;

        int pick = Random.Range(0, count);
        int current = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            if (current == pick)
                return lines[i].Trim();

            current++;
        }

        return string.Empty;
    }
}
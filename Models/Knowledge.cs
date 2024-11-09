using System.Collections.Generic;

namespace Isembard.Models;

public static class Knowledge
{
    public static readonly Dictionary<string,string> KnowledgeDictionary = CreateKnowledgeDictionary();

    private static Dictionary<string, string> CreateKnowledgeDictionary()
    {
        return new Dictionary<string, string>
        {
            {
                "Assertion failed: Attempted to bind new object",
                "Do you have mods loaded? If not, this is a game bug. Please report on Paradox forums."
            },
            {
                "Assertion failed: Failed to create lookup!",
                "Do you have mods loaded? If you do, it's a mod but we can't tell which one. If not, this could be a game bug. Please report on Paradox forums."
            },
            {
                "Data error in loc string",
                "This is a memory issue. Please restart your computer and try again."
            },
            {
                "Failed creating pixel buffer",
                "This is a graphical memory issue. Are you graphics drivers up to date? Please restart your computer and try again."
        },
            {
            "dx11",
            "Issue is graphics related. Are you graphics drivers up to date?"
        },
            {
                "Failed creating fence",
                "Do you have mods loaded? If not reinstall the game."
            },
            {
                "jomini_script_system",
                "Do you have mods loaded? If you don't it may be a game bug. Investigating.."
            },
        };
    }
}
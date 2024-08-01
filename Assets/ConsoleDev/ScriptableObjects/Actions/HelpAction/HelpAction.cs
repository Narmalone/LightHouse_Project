using UnityEngine;

[CreateAssetMenu(fileName = "HelpAction", menuName = "CustomConsole/Action/HelpAction")]
public class HelpAction : SO_CommandAction
{
    public override void Execute(string[] args, ChatController instance)
    {
        //GET TOUTES LES ACTIONS QUI SONT STORE DANS LE CHAT CONTROLLER
        instance.SendChatMessage("Bienvenue dans le /help", ChatTabs.Dev);
        instance.SendButtonChatMessage("<color=red> DOCUMENTATION </color=red>", () =>
        {
            Application.OpenURL("https://docs.google.com/document/d/166mtzyStWQJtFFlu1BrxTPAVpBINfyqICweZ_CQF-xw/edit#heading=h.on3oj7cv17b7");
        }, ChatTabs.Dev,
         "");
        instance.SendChatMessage("Voici les différentes commandes et leurs utilisations", ChatTabs.Dev);

        foreach (SO_Command cmd in instance.Commands)
        {
            if (string.IsNullOrEmpty(cmd.commandUsage)) continue;
            instance.SendChatMessage(cmd.commandUsage, ChatTabs.Dev);
        }
    }
}

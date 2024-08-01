using UnityEngine;

[CreateAssetMenu(fileName = "SpawnAction", menuName = "CustomConsole/Action/SpawnAction")]
public class SpawnAction : SO_CommandAction
{
    //afficher tous les objets possible de spawn dans les suggestions
    // /spawn {object} {distance} {Vector ?}
    public override void Execute(string[] args, ChatController instance)
    {
        if (args.Length == 1) // l'objet doit spawn en face du joueur
        {
            instance.TryGenerateObject(args[0]);
            instance.SendChatMessage($"Object {args[0]} generated", ChatTabs.Dev);
        }
        else if(args.Length == 2)
        {
            //si 2 distance 
            float.TryParse(args[1], out float distance);
            instance.TryGenerateObject(args[0], distance);
            instance.SendChatMessage($"Object {args[1]} generated at range {distance}", ChatTabs.Dev);
        }
    }
}

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LocalizedExtensions
{
    /// <summary>
    /// Construit une Smart String en injectant les arguments fournis.
    /// Utiliser cette méthode lorsque l'entrée de localisation contient des placeholders
    /// (ex : "Maintenez {key} pour {action}").
    /// </summary>
    /// <param name="localizedString">La LocalizedString à résoudre.</param>
    /// <param name="arguments">
    /// Les arguments utilisés par la Smart String.
    /// Généralement un objet anonyme :
    /// <code>
    /// await myString.Build(new
    /// {
    ///     key = "E",
    ///     action = "ouvrir"
    /// });
    /// </code>
    /// </param>
    public static async Task<string> Build(
        this LocalizedString localizedString,
        params object[] arguments)
    {
        // Copie de la LocalizedString afin de ne pas modifier les arguments
        // de l'instance d'origine.
        LocalizedString copy = new()
        {
            TableReference = localizedString.TableReference,
            TableEntryReference = localizedString.TableEntryReference,
            Arguments = arguments
        };

        var handle = copy.GetLocalizedStringAsync();
        await handle.Task;

        return handle.Result;
    }

    /// <summary>
    /// Résout une LocalizedString sans lui fournir d'arguments.
    /// À utiliser pour les chaînes de localisation classiques.
    /// </summary>
    public static async Task<string> Resolve(
        this LocalizedString localizedString)
    {
        var handle = localizedString.GetLocalizedStringAsync();

        await handle.Task;

        return handle.Result;
    }
}
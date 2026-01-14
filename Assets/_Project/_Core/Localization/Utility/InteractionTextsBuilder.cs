using System.Threading.Tasks;
using UnityEngine.Localization;

namespace LightHouse.Core.Localization
{
    public static class InteractionTextBuilder
    {
        public static async Task<string> Build(
            LocalizedString actionText,
            string bindKey = null,
            LocalizedString prefixSentence = null)
        {
            if (prefixSentence == null || string.IsNullOrEmpty(bindKey))
            {
                var op = actionText.GetLocalizedStringAsync();
                await op.Task;
                return op.Result;
            }

            var baseOp = actionText.GetLocalizedStringAsync();
            await baseOp.Task;
            string resolvedAction = baseOp.Result;

            // Clone safe pour éviter les conflits d’arguments partagés
            var wrapperCopy = new LocalizedString
            {
                TableReference = prefixSentence.TableReference,
                TableEntryReference = prefixSentence.TableEntryReference,
                Arguments = new object[]
                {
                new { key = bindKey, action = resolvedAction }
                }
            };

            var wrapperOp = wrapperCopy.GetLocalizedStringAsync();
            await wrapperOp.Task;
            return wrapperOp.Result;
        }
    }

}

using System.Threading.Tasks;
using UnityEngine.Localization;

namespace LightHouse.Localization
{
    public static class InteractionTextBuilder
    {
        public static async Task<string> Build(
            LocalizedString baseText,
            string bindKey = null,
            LocalizedString wrapperTemplate = null)
        {
            if (wrapperTemplate == null || string.IsNullOrEmpty(bindKey))
            {
                var op = baseText.GetLocalizedStringAsync();
                await op.Task;
                return op.Result;
            }

            var baseOp = baseText.GetLocalizedStringAsync();
            await baseOp.Task;
            string resolvedAction = baseOp.Result;

            // Clone safe pour éviter les conflits d’arguments partagés
            var wrapperCopy = new LocalizedString
            {
                TableReference = wrapperTemplate.TableReference,
                TableEntryReference = wrapperTemplate.TableEntryReference,
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

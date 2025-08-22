using System;
using System.Threading.Tasks;
using Aloha.Coconut;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aloha.Durian
{
    internal static class RequestHandler
    {
        public static async UniTask<TDto> Request<TResponse, TDto>(Task<TResponse> requestMethod, Func<TResponse, TDto> dtoGetter,
            bool showDialogueOnFailed = false, int repeatCount = 3)
        {
            try
            {
                return dtoGetter(await requestMethod);
            }
            catch (Exception e)
            {
                if (repeatCount > 0)
                {
                    Debug.LogWarning($"Exception when calling Api: {e}, Retry");
                    return await Request(requestMethod, dtoGetter, showDialogueOnFailed, repeatCount - 1);
                }
            
                Debug.LogError($"Exception when calling Api: {e}");
                Debug.LogError(e.StackTrace);
                if(showDialogueOnFailed) await SystemUI.ShowDialogue("Api Failed", $"{e.ToString().Substring(0, 100)}");
                throw;
            }
        }
    
        public static async UniTask Request(Task<RootVoid> requestMethod, bool showDialogueOnFailed = false, int repeatCount = 0)
        {
            try
            {
                await requestMethod;
            }
            catch (Exception e)
            {
                if (repeatCount > 0)
                {
                    Debug.LogWarning($"Exception when calling Api: {e}, Retry");
                    await Request(requestMethod, showDialogueOnFailed, repeatCount - 1);
                    return;
                }
            
                Debug.LogError($"Exception when calling Api: {e}");
                Debug.LogError(e.StackTrace);
                if(showDialogueOnFailed) await SystemUI.ShowDialogue("Api Failed", $"{e.ToString().Substring(0, 100)}");
                throw;
            }
        }
    }
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aloha.Coconut.UI
{
    [ExecuteAlways]
    public abstract class StyledComponent<T> : MonoBehaviour where T: Component
    {
        protected T target;

        protected virtual void OnEnable()
        {
            target = GetComponent<T>();
            if(Application.isPlaying) Fetch();
        }
        
#if UNITY_EDITOR
        void Update()
        {
            Fetch();
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);                                
            }
        }
#endif
        
        protected abstract void Fetch();

        protected StyleSheet GetStyleSheet()
        {
            return StyleSheet.Instance;
        }
    }
}
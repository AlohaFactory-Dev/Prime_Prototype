using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StartGameNamespace
{
    public class StartGame : MonoBehaviour
    {
        void Start()
        {
            _ = GlobalConainer.Get<GameManager>().Init();
        }
    }
}
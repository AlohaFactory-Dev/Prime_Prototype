using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aloha.CoconutMilk
{
    public class OverlayTutorial : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [FormerlySerializedAs("finger")] [SerializeField] private TutorialFinger tutorialFinger;
        [SerializeField] private TutorialFocusPoint pointA;
        [SerializeField] private TutorialFocusPoint pointB;

        [Header("Screen")] [SerializeField] private CanvasGroup screenCanvasGroup;

        [Header("Test")] 
        [NonSerialized, ShowInInspector] private GameObject _testTargetA;
        [NonSerialized, ShowInInspector] private GameObject _testTargetB;

        private Camera _camera;
        private Tween _alphaTween;

        void Awake()
        {
            canvas.enabled = false;
            SetCamera(Camera.main);
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
            canvas.worldCamera = _camera;
            pointA.SetCamera(_camera);
            pointB.SetCamera(_camera);
        }

        public void Point(GameObject target, bool fingerXFlipped = false, bool fingerYFlipped = false)
        {
            FocusOn(target);
            tutorialFinger.gameObject.SetActive(true);
            tutorialFinger.StartPointing(pointA, fingerXFlipped, fingerYFlipped);
        }

        private void TurnOn()
        {
            _alphaTween?.Kill();
            canvas.enabled = true;
            screenCanvasGroup.alpha = 0;
            _alphaTween = screenCanvasGroup.DOFade(1, .35f).SetUpdate(true);
        }

        public void FocusOn(GameObject target)
        {
            TurnOn();
            pointB.gameObject.SetActive(false);
            pointA.FocusOn(target);
            tutorialFinger.gameObject.SetActive(false);
        }

        public void Move(GameObject from, GameObject to)
        {
            TurnOn();

            pointA.FocusOn(from);
            pointB.FocusOn(to);
            tutorialFinger.gameObject.SetActive(true);
            tutorialFinger.StartMoving(pointA, pointB);
        }

        public void Move(GameObject from, Vector2 position)
        {
            TurnOn();

            pointA.FocusOn(from);
            pointB.FocusOn(position);
            tutorialFinger.gameObject.SetActive(true);
            tutorialFinger.StartMoving(pointA, pointB);
        }
    
        public void SetFocusPointSize(Vector2 size)
        {
            pointA.SetSize(size);
            pointB.SetSize(size);
        }

        [Button]
        public void TestPointing()
        {
            Point(_testTargetA);
        }
    
        [Button]
        public void TestFocusing()
        {
            FocusOn(_testTargetA);
        }

        [Button]
        public void TestMoving()
        {
            Move(_testTargetA, _testTargetB);
        }

        [Button]
        public void Cancel()
        {
            pointA.StopTracking();
            pointB.StopTracking();
            tutorialFinger.Cancel();

            pointB.gameObject.SetActive(true);

            _alphaTween?.Kill();
            _alphaTween = screenCanvasGroup.DOFade(0, .35f).SetUpdate(true)
                .OnComplete(() => canvas.enabled = false);
        }
    }
}
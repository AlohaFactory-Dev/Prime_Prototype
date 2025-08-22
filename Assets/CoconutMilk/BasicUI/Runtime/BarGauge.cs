using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Aloha.CoconutMilk
{
    public class BarGauge : MonoBehaviour
    {
        public float MaxValue => maxValue;
        public float Value => value;
        
        [SerializeField] private SlicedFilledImage fillImage;
        [SerializeField] private TMP_Text fillText;

        [InfoBox("디버그용으로만 에디터에서 조정, 실제로 두 값은 Initialize와 SetValue로 설정되어야 함")]
        [SerializeField, OnValueChanged(nameof(EditorOnValueChanged))] private float value;
        [SerializeField, OnValueChanged(nameof(EditorOnValueChanged))] private float maxValue;
        
        private Func<float, float, string> _stringFormatter;

        public void Initialize(float maxValue, Func<float, float, string> stringFormatter, float initialValue = 0)
        {
            this.maxValue = maxValue;
            _stringFormatter = stringFormatter;
            SetValue(initialValue);
        }

        public void SetValue(float value)
        {
            this.value = value;
            fillImage.fillAmount = this.value / maxValue;
            if(_stringFormatter != null) fillText.text = _stringFormatter(this.value, maxValue);
        }
        
        private void EditorOnValueChanged()
        {
            fillImage.fillAmount = value / maxValue;
            if(_stringFormatter != null) fillText.text = _stringFormatter(this.value, maxValue);
        }
    }
}
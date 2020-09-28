using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    [AddComponentMenu("UI/BoxSlider", 35)]
    [RequireComponent(typeof(RectTransform))]
    public class BoxSlider : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement {
        [Serializable]
        public class BoxSliderEvent : UnityEvent<Vector2> { }

        [SerializeField] private RectTransform _handleRect;
        [SerializeField] private Vector2       _value = Vector2.zero;

        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField] private BoxSliderEvent _onValueChanged = new BoxSliderEvent();

        public BoxSliderEvent OnValueChanged {
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

        // Private fields
        public void OnDrag(PointerEventData eventData) { }

        public void OnInitializePotentialDrag(PointerEventData eventData) { }

        public void Rebuild(CanvasUpdate executing) { }

        public void LayoutComplete() { }

        public void GraphicUpdateComplete() { }
    }
}
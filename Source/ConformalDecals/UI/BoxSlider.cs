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
        [SerializeField] private Vector2 _value = Vector2.zero;

        public RectTransform HandleRect {
            get => _handleRect;
            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value != _handleRect) {
                    _handleRect = value;
                    UpdateCachedReferences();
                    UpdateVisuals();
                }
            }
        }

        public Vector2 Value {
            get => _value;
            set {
                _value = value;
                _onValueChanged.Invoke(value);
                UpdateVisuals();
            }
        }

        [Space(6)]

        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField]
        private BoxSliderEvent _onValueChanged = new BoxSliderEvent();

        public BoxSliderEvent OnValueChanged {
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

        // Private fields

        private Transform     _handleTransform;
        private RectTransform _handleContainerRect;

        // The offset from handle position to mouse down position
        private Vector2 _offset = Vector2.zero;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
            if (IsActive()) {
                UpdateCachedReferences();
                // Update rects since other things might affect them even if value didn't change.
                UpdateVisuals();
            }

#if UNITY_2018_3_OR_NEWER

            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);

#else
            var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
			if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
				CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
#endif
        }
#endif // if UNITY_EDITOR

        public virtual void Rebuild(CanvasUpdate executing) {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                OnValueChanged.Invoke(Value);
#endif
        }

        public void LayoutComplete() { }

        public void GraphicUpdateComplete() { }

        protected override void OnEnable() {
            base.OnEnable();
            UpdateCachedReferences();
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        private void UpdateCachedReferences() {
            if (_handleRect) {
                _handleTransform = _handleRect.transform;
                if (_handleTransform.parent != null)
                    _handleContainerRect = _handleTransform.parent.GetComponent<RectTransform>();
            }
            else {
                _handleContainerRect = null;
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            UpdateVisuals();
        }

        // Force-update the slider. Useful if you've changed the properties and want it to update visually.
        private void UpdateVisuals() {
            if (_handleContainerRect != null) {
                _handleRect.anchorMin = _value;
                _handleRect.anchorMax = _value;
            }
        }

        // Update the slider's position based on the mouse.
        private void UpdateDrag(PointerEventData eventData, Camera cam) {
            var clickRect = _handleContainerRect;
            if (clickRect != null && clickRect.rect.size[0] > 0) {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, cam, out var localCursor))
                    return;

                var rect = clickRect.rect;
                localCursor -= rect.position;

                Vector2 newVal;
                newVal.x = Mathf.Clamp01((localCursor - _offset).x / rect.size.x);
                newVal.y = Mathf.Clamp01((localCursor - _offset).y / rect.size.y);

                Value = newVal;
            }
        }

        private bool MayDrag(PointerEventData eventData) {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);

            _offset = Vector2.zero;
            if (_handleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(_handleRect, eventData.position, eventData.enterEventCamera)) {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_handleRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                    _offset = localMousePos;

                _offset.y = -_offset.y;
            }
            else {
                // Outside the slider handle - jump to this point instead
                UpdateDrag(eventData, eventData.pressEventCamera);
            }
        }

        public virtual void OnDrag(PointerEventData eventData) {
            if (!MayDrag(eventData))
                return;

            UpdateDrag(eventData, eventData.pressEventCamera);
        }
        
        public virtual void OnInitializePotentialDrag(PointerEventData eventData) {
            eventData.useDragThreshold = false;
        }
    }
}
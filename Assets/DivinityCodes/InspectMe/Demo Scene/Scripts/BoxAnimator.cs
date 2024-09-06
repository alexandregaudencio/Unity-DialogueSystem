using DivinityCodes.InspectMe.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace DivinityCodes.InspectMe.Demo_Scene.Scripts
{
    [InspectMe]
    public class BoxAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRectTransform;
        [InspectMe] [SerializeField] private Image targetImage; 
        [SerializeField] private CanvasGroup canvasGroup; 
        
        [SerializeField] private bool update;
        [SerializeField] private float animationDelay;

        private readonly Vector2[] _anchorPositions = new Vector2[] { new (0, 0), new (0, 1), new (1, 0), new (1, 1) };
        private readonly Color[] _colors = new Color[] { new Color(1f, 0.34f, 0.49f), new Color(0.44f, 0.69f, 0.4f), new Color(0.27f, 0.54f, 1f), new Color(1f, 0.87f, 0.26f) };
        private Vector2[] _deltaSizes = new Vector2[] { new (15, 15), new (60, 60), new (35, 35), new (80, 80) };

        [InspectMe] public string Name => gameObject.name;
        private int _currentPositionIndex = 0;
        private float _timer;
        private BoxesController _controller;
        
        public void Initialize(BoxesController controller)
        {
            _controller = controller;
        }
        
        private void Update()
        {
            if (!update || targetRectTransform == null || targetImage == null) return;
            
            // Update timer
            _timer += Time.deltaTime * _controller.AnimationSpeed;

            // Calculate the next position index
            var nextPositionIndex = (_currentPositionIndex + 1) % _anchorPositions.Length;

            // Lerp anchorMin, anchorMax, pivot, deltaSize, and color
            var nextAnchorPos = _anchorPositions[nextPositionIndex];
            var nextDeltaSize = _deltaSizes[nextPositionIndex];
            var nextColor = _colors[nextPositionIndex];

            targetRectTransform.anchorMin = Vector2.Lerp(targetRectTransform.anchorMin, nextAnchorPos, _timer);
            targetRectTransform.anchorMax = Vector2.Lerp(targetRectTransform.anchorMax, nextAnchorPos, _timer);
            targetRectTransform.pivot = Vector2.Lerp(targetRectTransform.pivot, nextAnchorPos, _timer);
            targetRectTransform.sizeDelta = Vector2.Lerp(targetRectTransform.sizeDelta, nextDeltaSize, _timer);
            targetImage.color = Color.Lerp(targetImage.color, nextColor, _timer);
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0, 1, _timer);
            
            if (_timer >= 1.0f)
            {
                _currentPositionIndex = nextPositionIndex;
                _timer = 0f;
                canvasGroup.alpha = 1;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using DivinityCodes.InspectMe.Runtime.Core.Inspect;
using UnityEngine;

namespace DivinityCodes.InspectMe.Demo_Scene.Scripts
{
    public class BoxesController : MonoBehaviour
    {
        [SerializeField] private BoxAnimator boxPrefab;
        [SerializeField] private int amount;
        [SerializeField] private float animationSpeed;
        [SerializeField] private float delay;
        [SerializeField] private bool inspectOnSpawn;

        private Queue<BoxAnimator> _boxes = new Queue<BoxAnimator>();
        public float AnimationSpeed => animationSpeed;
        private WaitForSeconds _waitDelay;
        private IEnumerator Start()
        {
            this.InspectMe(x => x._boxes);
            _waitDelay ??= new WaitForSeconds(delay);
            for (var i = 0; i < amount; i++)
            {
                var box = Instantiate(boxPrefab, transform);
                box.Initialize(this);
                _boxes.Enqueue(box);
                if (inspectOnSpawn)
                {
                    box.InspectMe(box.name);
                }
                yield return _waitDelay;
            }
        }

    }
}
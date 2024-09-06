using System;
using System.Collections.Generic;
using DivinityCodes.InspectMe.Runtime.Core.Inspect;
using UnityEngine;
using UnityEngine.UI;

namespace DivinityCodes.InspectMe.Demo_Scene.Scripts
{
    public class InspectMeDemo: MonoBehaviour
    {
        [Header("Options")] 
        [SerializeField] private bool inspectOnSpawn;
        [SerializeField] private bool updateValue;
        
        [SerializeField] private DemoItem demoItemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        
        private readonly List<DemoItem> _items = new List<DemoItem>();
        private DateTime _time;

        private void Start()
        {
            _time = DateTime.Now;
            this.InspectMe(x => x._time);
            
            scrollRect.content.InspectMe(x => x.childCount, "Items Count");
        }

        public void SpawnItem()
        {
            var item = Instantiate(demoItemPrefab, scrollRect.content);
            item.SetLabel($"item {_items.Count}");
            item.transform.SetAsFirstSibling();
            if (inspectOnSpawn)
            {
                item.InspectMe(item.Label);
            }
            _items.Add(item);
        }

        public void DestroyItem()
        {
            if(_items.Count == 0) return;
            Destroy(_items[^1].gameObject);
            _items.RemoveAt(_items.Count - 1);
        }
        
        private void Update()
        {
            _time = DateTime.Now;
            if(!updateValue) return;
            foreach (var item in _items)
            {
                item.RandomLabel();
            }
        }
    }
}
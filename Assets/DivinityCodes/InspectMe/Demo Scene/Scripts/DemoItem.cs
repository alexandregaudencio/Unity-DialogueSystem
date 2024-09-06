using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DivinityCodes.InspectMe.Demo_Scene.Scripts
{
    public class DemoItem : MonoBehaviour
    {
        [SerializeField] private Text labelTxt;
        public string Label => labelTxt.text;
        
        public void SetLabel(string label)
        {
            gameObject.name = label;
            labelTxt.text = label;
        }

        public void RandomLabel()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            var randomLabel = new string(Enumerable.Repeat(chars, Random.Range(10, 20))
                .Select(s => s[random.Next(s.Length)]).ToArray());
            SetLabel(randomLabel);
        }
        
    }
}
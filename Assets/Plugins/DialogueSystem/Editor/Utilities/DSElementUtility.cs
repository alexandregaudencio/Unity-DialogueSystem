using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor.Utilities
{
    using Elements;
    using System.Collections.Generic;

    public static class DSElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };


            return button;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }

        public static DropdownField CreateDropdown(string title, List<string> options, Func<string, string> valueChange, int defaultIndex = 0)
        {
            DropdownField dropdown = new DropdownField(title, options, defaultIndex, valueChange);
            return dropdown;
        }

        public static Port CreatePort(this DSNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            port.portName = portName;

            return port;
        }

        public static TextElement CreateTextElement(string text, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {

            TextElement textElement = new TextElement();
            textElement.text = text;
            if (onValueChanged != null)
            {
                textElement.RegisterValueChangedCallback(onValueChanged);
            }
            return textElement;


        }

        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);

            textArea.multiline = true;

            return textArea;
        }
    }
}
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [Serializable]
    public class SerializedValueWrapper
    {
        public enum FieldType { Float, Int, Bool, String }
        public FieldType type;
        public float     floatValue;
        public int       intValue;
        public bool      boolValue;
        public string    stringValue;
    }

    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class ComponentFieldData : ISnapshotData
    {
        public string               fieldPath;
        public SerializedValueWrapper value = new SerializedValueWrapper();

        [NonSerialized] private FieldInfo _cachedField;
        [NonSerialized] private Type      _cachedType;

        private FieldInfo GetField(Component target)
        {
            if (target == null || string.IsNullOrEmpty(fieldPath)) return null;
            var type = target.GetType();
            if (_cachedField != null && _cachedType == type) return _cachedField;
            _cachedType  = type;
            _cachedField = type.GetField(fieldPath,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return _cachedField;
        }

        public void CaptureFrom(Component target)
        {
            var field = GetField(target);
            if (field == null) return;
            var raw = field.GetValue(target);
            if      (raw is float f)  { value.type = SerializedValueWrapper.FieldType.Float;  value.floatValue  = f; }
            else if (raw is int i)    { value.type = SerializedValueWrapper.FieldType.Int;    value.intValue    = i; }
            else if (raw is bool b)   { value.type = SerializedValueWrapper.FieldType.Bool;   value.boolValue   = b; }
            else if (raw is string s) { value.type = SerializedValueWrapper.FieldType.String; value.stringValue = s; }
        }

        public void ApplyTo(Component target, ISnapshotData from, float t)
        {
            var field = GetField(target);
            if (field == null) return;
            var f = from as ComponentFieldData;
            switch (value.type)
            {
                case SerializedValueWrapper.FieldType.Float:
                    float fromF = f?.value.floatValue ?? (float)(field.GetValue(target) ?? 0f);
                    field.SetValue(target, t < 1f ? Mathf.Lerp(fromF, value.floatValue, t) : value.floatValue);
                    break;
                case SerializedValueWrapper.FieldType.Int:
                    int fromI = f?.value.intValue ?? (int)(field.GetValue(target) ?? 0);
                    field.SetValue(target, t < 1f ? Mathf.RoundToInt(Mathf.Lerp(fromI, value.intValue, t)) : value.intValue);
                    break;
                case SerializedValueWrapper.FieldType.Bool:
                    field.SetValue(target, value.boolValue);
                    break;
                case SerializedValueWrapper.FieldType.String:
                    field.SetValue(target, value.stringValue);
                    break;
            }
        }
    }
}

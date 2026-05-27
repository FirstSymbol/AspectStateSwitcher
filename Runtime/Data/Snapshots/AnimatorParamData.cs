using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class AnimatorParamData : ISnapshotData
    {
        public enum ParamType { Float, Int, Bool }

        public string    paramName;
        public ParamType paramType;
        public float     floatValue;
        public int       intValue;
        public bool      boolValue;

        public void CaptureFrom(Component target)
        {
            if (!(target is Animator animator) || string.IsNullOrEmpty(paramName)) return;
            switch (paramType)
            {
                case ParamType.Float: floatValue = animator.GetFloat(paramName);   break;
                case ParamType.Int:   intValue   = animator.GetInteger(paramName); break;
                case ParamType.Bool:  boolValue  = animator.GetBool(paramName);    break;
            }
        }

        public void ApplyTo(Component target, ISnapshotData from, float t)
        {
            if (!(target is Animator animator) || string.IsNullOrEmpty(paramName)) return;
            var f = from as AnimatorParamData;
            switch (paramType)
            {
                case ParamType.Float:
                    float fromF = f?.floatValue ?? animator.GetFloat(paramName);
                    animator.SetFloat(paramName, t < 1f ? Mathf.Lerp(fromF, floatValue, t) : floatValue);
                    break;
                case ParamType.Int:
                    animator.SetInteger(paramName, intValue);
                    break;
                case ParamType.Bool:
                    animator.SetBool(paramName, boolValue);
                    break;
            }
        }
    }
}

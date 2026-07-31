using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class AnimatorParamData : SnapshotData
    {
        public enum ParamType { Float, Int, Bool }

        public string    paramName;
        public ParamType paramType;
        public float     floatValue;
        public int       intValue;
        public bool      boolValue;

        public override void CaptureFrom(Component target)
        {
            if (!(target is Animator animator) || string.IsNullOrEmpty(paramName)) return;
            switch (paramType)
            {
                case ParamType.Float: floatValue = animator.GetFloat(paramName);   break;
                case ParamType.Int:   intValue   = animator.GetInteger(paramName); break;
                case ParamType.Bool:  boolValue  = animator.GetBool(paramName);    break;
            }
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is Animator animator) || string.IsNullOrEmpty(paramName)) return;
            var f = previousStateData as AnimatorParamData;
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

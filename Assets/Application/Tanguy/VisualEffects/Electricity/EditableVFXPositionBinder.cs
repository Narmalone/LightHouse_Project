using UnityEngine.VFX;

namespace UnityEngine.VFX.Utility
{
    [AddComponentMenu("VFX/Property Binders/Position Binder")]
    [VFXBinder("Transform/Position")]
    public class EditableVFXPositionBinder : VFXBinderBase
    {
        public enum BinderSpace
        {
            Automatic,
            World,
            Local
        }

        [SerializeField]
        public BinderSpace Space;
        public string Property { get { return (string)m_Property; } set { m_Property = value; } }

        [VFXPropertyBinding("UnityEditor.VFX.Position", "UnityEngine.Vector3"), SerializeField]
        protected ExposedProperty m_Property = "Position";
        public Transform Target = null;

        public override bool IsValid(VisualEffect component)
        {
            return Target != null && component.HasVector3(m_Property);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            var position = ApplySpacePosition(component, m_Property, Target.transform.position);
            component.SetVector3(m_Property, position);
        }

        public override string ToString()
        {
            return string.Format("Position : '{0}' -> {1}", m_Property, Target == null ? "(null)" : Target.name);
        }

        protected Vector3 ApplySpacePosition(VisualEffect component, ExposedProperty targetProperty, Vector3 sourceWorldPosition)
        {
            var targetSpace = GetTargetSpace(component, targetProperty);
            if (targetSpace == VFXSpace.Local)
            {
                var sourceLocalPosition = component.transform.worldToLocalMatrix.MultiplyPoint(sourceWorldPosition);
                return sourceLocalPosition;
            }
            return sourceWorldPosition;
        }

        private VFXSpace GetTargetSpace(VisualEffect component, ExposedProperty targetProperty)
        {
            var targetSpace = VFXSpace.None;
            switch (Space)
            {
                case BinderSpace.Automatic: targetSpace = component.visualEffectAsset.GetExposedSpace(targetProperty); break;
                case BinderSpace.World: targetSpace = VFXSpace.World; break;
                case BinderSpace.Local: targetSpace = VFXSpace.Local; break;
            }
            return targetSpace;
        }
    }
}

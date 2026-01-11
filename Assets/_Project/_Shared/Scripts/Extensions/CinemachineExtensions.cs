using Cinemachine;
using UnityEngine;

namespace LightHouse.Core.Extensions
{
    public static class CinemachineExtensions
    {
        #region Priority

        // Modifier la priorité avec callback
        public static int SetPriority(this CinemachineVirtualCamera target, int newPriority)
        {
            if (target == null) return -1;
            target.Priority = newPriority;
            return target.Priority;
        }

        public static int GetPriority(this CinemachineVirtualCamera target)
        {
            if (target == null) return -1;
            return target.Priority;
        }

        public static void SwitchCameraPriority(this CinemachineVirtualCamera target, CinemachineVirtualCamera newCam, int oldcamPriority, int newCamPriority)
        {
            target.Priority = oldcamPriority;
            newCam.Priority = newCamPriority;
        }
        #endregion

        #region TRANSFORMS / FOLLOW & LOOK AT

        // Tranforms
        public static Transform SetFollow(this CinemachineVirtualCamera target, Transform newFollow)
        {
            return target.Follow = newFollow;
        }

        public static Transform SetLookAt(this CinemachineVirtualCamera target, Transform newLookAt)
        {
            return target.LookAt = newLookAt;
        }
        #endregion

        #region SetPosition And Rotations

        public static Vector3 SetPosition(this CinemachineVirtualCamera virtualCamera, Vector3 position)
        {
            return virtualCamera.transform.position = position;
        }

        // Définit la rotation de la caméra virtuelle
        public static Quaternion SetRotation(this CinemachineVirtualCamera virtualCamera, Quaternion rotation)
        {
            return virtualCamera.transform.rotation = rotation;
        }

        public static Quaternion SetRotation(this CinemachineVirtualCamera virtualCamera, Vector3 eulerAngles)
        {
            return virtualCamera.transform.rotation = Quaternion.Euler(eulerAngles);
        }
        public static Vector3 SetRotationGetEuler(this CinemachineVirtualCamera virtualCamera, Vector3 eulerAngles)
        {
            return virtualCamera.transform.eulerAngles = eulerAngles;
        }

        #endregion

        #region FOV AND CLIP PLANES

        // Définit le champ de vision de la caméra virtuelle
        public static float SetFieldOfView(this CinemachineVirtualCamera virtualCamera, float fieldOfView)
        {
            return virtualCamera.m_Lens.FieldOfView = fieldOfView;
        }

        // Définit la distance du plan proche de la caméra virtuelle
        public static void SetNearClipPlane(this CinemachineVirtualCamera virtualCamera, float nearClipPlane)
        {
            virtualCamera.m_Lens.NearClipPlane = nearClipPlane;
        }

        // Définit la distance du plan lointain de la caméra virtuelle
        public static void SetFarClipPlane(this CinemachineVirtualCamera virtualCamera, float farClipPlane)
        {
            virtualCamera.m_Lens.FarClipPlane = farClipPlane;
        }

        #endregion

        #region NOISE

        //Cinemachine Perlin
        public static CinemachineBasicMultiChannelPerlin AddNoise(this CinemachineVirtualCamera target, float amplitudeGain = 1f, float frequencyGain = 1f)
        {
            CinemachineBasicMultiChannelPerlin cinemachineNoiseComponent = target.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            SetNoise(cinemachineNoiseComponent, amplitudeGain, frequencyGain);
            return cinemachineNoiseComponent;
        }

        public static CinemachineBasicMultiChannelPerlin SetNoise(this CinemachineBasicMultiChannelPerlin target, float amplitudeGain = 1, float frequencyGain = 1)
        {
            target.m_AmplitudeGain = amplitudeGain;
            target.m_FrequencyGain = frequencyGain;
            return target;
        }

        #endregion

    }
}

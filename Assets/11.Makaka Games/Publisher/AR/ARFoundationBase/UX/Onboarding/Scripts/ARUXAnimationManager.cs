/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using TMPro;

using System;

namespace MakakaGames.Publisher.AR.ARFoundationBase.UX.Onboarding
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ARUXAnimationManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Instructional test for visual UI")]
        private TMP_Text m_InstructionText;

        [SerializeField]
        [Tooltip("Move device animation")]
        private VideoClip m_FindAPlaneClip;

        [SerializeField]
        [Tooltip("Tap to place animation")]
        private VideoClip m_TapToPlaceClip;

        [SerializeField]
        [Tooltip("Find Clip animation")]
        private VideoClip m_FindImageClip;
        
        [SerializeField]
        [Tooltip("Find body animation")]
        private VideoClip m_FindBodyClip;

        [SerializeField]
        [Tooltip("Find object animation")]
        private VideoClip m_FindObjectClip;

        [SerializeField]
        [Tooltip("Find face animation")]
        private VideoClip m_FindFaceClip;

        [SerializeField]
        [Tooltip("ARKit Coaching overlay reference")]
        private ARKitCoachingOverlay m_CoachingOverlay;

        [SerializeField]
        [Tooltip("Video player reference")]
        private VideoPlayer m_VideoPlayer;

        [SerializeField]
        [Tooltip("Raw image used for videoplayer reference")]
        private RawImage m_RawImage;

        [SerializeField]
        [Tooltip("time the UI takes to fade on")]
        private float m_FadeOnDuration = 1.0f;

        [SerializeField]
        [Tooltip("time the UI takes to fade off")]
        private float m_FadeOffDuration = 0.5f;

        private Color m_AlphaWhite = new(1f, 1f, 1f, 0f);
        private Color m_White = new(1f, 1f, 1f, 1f);

        private Color m_TargetColor;
        private Color m_StartColor;
        private Color m_LerpingColor;

        private bool m_FadeOn;
        private bool m_FadeOff;
        private bool m_Tweening;
        private bool m_UsingARKitCoaching;

        private float m_TweenTime;
        private float m_TweenDuration;

        private const string k_MoveDeviceText = "Move Device Slowly";
        private const string k_TapToPlaceText = "Tap to Choose the Plane";
        private const string k_FindABodyText = "Find a Body to Track";
        private const string k_FindAFaceText = "Find a Face to Track";
        private const string k_FindClipText = "Find an Image to Track";
        private const string k_FindObjectText = "Find an Object to Track";

        public static event Action OnFadeOffComplete;

        [SerializeField]
        private Texture m_Transparent;

        private RenderTexture m_RenderTexture;

        private void Start()
        {
            m_StartColor = m_AlphaWhite;
            m_TargetColor = m_White;
        }

        private void Update()
        {
            if (!m_VideoPlayer.isPrepared)
            {
                return;
            }

            if (m_FadeOff || m_FadeOn)
            {
                if (m_FadeOn)
                {
                    m_StartColor = m_AlphaWhite;
                    m_TargetColor = m_White;
                    m_TweenDuration = m_FadeOnDuration;
                    m_FadeOff = false;
                }
            
                if(m_FadeOff)
                {
                    m_StartColor = m_White;
                    m_TargetColor = m_AlphaWhite;
                    m_TweenDuration = m_FadeOffDuration;

                    m_FadeOn = false;
                }
                
                if (m_TweenTime < 1)
                {
                    m_TweenTime += Time.deltaTime / m_TweenDuration;
                    m_LerpingColor =
                        Color.Lerp(m_StartColor, m_TargetColor, m_TweenTime);

                    m_RawImage.color = m_LerpingColor;
                    m_InstructionText.color = m_LerpingColor;
                    
                    m_Tweening = true;
                }
                else
                {
                    m_TweenTime = 0;
                    m_FadeOff = false;
                    m_FadeOn = false;
                    m_Tweening = false;
    
                    // was it a fade off?
                    if (m_TargetColor == m_AlphaWhite)
                    {
                        OnFadeOffComplete?.Invoke();

                        // fix issue with render texture showing a single frame
                        // of the previous video
                        m_RenderTexture = m_VideoPlayer.targetTexture;
                        m_RenderTexture.DiscardContents();
                        m_RenderTexture.Release();

                        Graphics.Blit(m_Transparent, m_RenderTexture);
                    }
                }
            }
        }
        
        public void ShowTapToPlace()
        {
            m_VideoPlayer.clip = m_TapToPlaceClip;
            m_VideoPlayer.Play();
            m_InstructionText.text = k_TapToPlaceText;
            
            m_FadeOn = true;
        }

        public void ShowFindImage()
        {
            m_VideoPlayer.clip = m_FindImageClip;
            m_VideoPlayer.Play();
            m_InstructionText.text = k_FindClipText;
            
            m_FadeOn = true;
        }

        public void ShowFindBody()
        {
            m_VideoPlayer.clip = m_FindBodyClip;
            m_VideoPlayer.Play();
            m_InstructionText.text = k_FindABodyText;
            
            m_FadeOn = true;
        }

        public void ShowFindObject()
        {
            m_VideoPlayer.clip = m_FindObjectClip;
            m_VideoPlayer.Play();
            m_InstructionText.text = k_FindObjectText;
            
            m_FadeOn = true;
        }

        public void ShowFindFace()
        {
            m_VideoPlayer.clip = m_FindFaceClip;
            m_VideoPlayer.Play();
            m_InstructionText.text = k_FindAFaceText;
        
            m_FadeOn = true;
        }

        public void ShowCrossPlatformFindAPlane()
        {
            m_VideoPlayer.clip = m_FindAPlaneClip;
            m_VideoPlayer.Play();
            m_InstructionText.text = k_MoveDeviceText;
            
            m_FadeOn = true;
        }

        public void ShowCoachingOverlay()
        {
            if (m_CoachingOverlay)
            {
                if (m_CoachingOverlay.Supported)
                {
                    m_CoachingOverlay.ActivateCoaching(true);
                    m_VideoPlayer.Stop();
                    m_UsingARKitCoaching = true;
                }
                else
                {
                    Debug.LogWarning(
                        "Coaching Overlay not supported on this platform");
                }
            }
        }

        public bool ARKitCoachingOverlaySupported()
        {
            if (m_CoachingOverlay)
            {
                return m_CoachingOverlay.Supported;
            }

            return false;
        }
        
        public void FadeOffCurrentUI()
        {
            // assumes coaching overlay is first in the order
            if (m_UsingARKitCoaching)
            {
                // disables it instantly rather than animating it off
                m_CoachingOverlay.DisableCoaching(false);
                m_UsingARKitCoaching = false;
                m_InstructionText.color = m_AlphaWhite;

                OnFadeOffComplete?.Invoke();

                m_FadeOff = true;
            }
            
            if (m_VideoPlayer.clip != null)
            {
                // handle exiting fade out early if currently
                // fading out another Clip
                if (m_Tweening || m_FadeOn)
                {
                    // stop tween immediately
                    m_TweenTime = 1.0f;
                    m_RawImage.color = m_AlphaWhite;
                    m_InstructionText.color = m_AlphaWhite;

                    OnFadeOffComplete?.Invoke();
                }

                m_FadeOff = true;
            }
        }
    }
}
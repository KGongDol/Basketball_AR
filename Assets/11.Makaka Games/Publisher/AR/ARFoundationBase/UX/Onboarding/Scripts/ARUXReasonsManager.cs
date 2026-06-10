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
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using TMPro;

namespace MakakaGames.Publisher.AR.ARFoundationBase.UX.Onboarding
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ARUXReasonsManager : MonoBehaviour
    {
        [SerializeField]
        private bool m_ShowNotTrackingReasons = true;

        [SerializeField]
        private TMP_Text m_ReasonDisplayText;

        [SerializeField]
        private GameObject m_ReasonParent;

        [SerializeField]
        private Image m_ReasonIcon;

        [SerializeField]
        private Sprite m_InitRelocalSprite;

        [SerializeField]
        private Sprite m_MotionSprite;

        [SerializeField]
        private Sprite m_LightSprite;

        [SerializeField]
        private Sprite m_FeaturesSprite;

        [SerializeField]
        private Sprite m_UnsupportedSprite;

        [SerializeField]
        private Sprite m_NoneSprite;

        private NotTrackingReason m_CurrentReason;

        private bool m_SessionTracking;

        private const string k_InitRelocalText = "Initializing augmented reality.";
        private const string k_MotionText = "Try moving at a slower pace.";
        private const string k_LightText = "It’s too dark. Try going to a more" +
            " well lit area.";
        private const string k_FeaturesText = "Look for more textures or details" +
            " in the area.";
        private const string k_UnsupportedText = "AR content is not supported.";
        private const string k_NoneText = "Wait for tracking to begin.";

        private void OnEnable()
        {
            ARSession.stateChanged += ARSessionOnstateChanged;

            if (!m_ShowNotTrackingReasons)
            {
                m_ReasonParent.SetActive(false);
            }
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= ARSessionOnstateChanged;
        }

        private void Update()
        {
            if (m_ShowNotTrackingReasons)
            {
                if (!m_SessionTracking)
                {
                    m_CurrentReason = ARSession.notTrackingReason;

                    ShowReason();
                }
                else
                {
                    if (m_ReasonDisplayText.gameObject.activeSelf)
                    {
                        m_ReasonParent.SetActive(false);
                    }
                }
            }
        }

        private void ARSessionOnstateChanged(ARSessionStateChangedEventArgs obj)
        {
            m_SessionTracking = obj.state == ARSessionState.SessionTracking;
        }

        private void ShowReason()
        {
            m_ReasonParent.SetActive(true);

            SetReason();
        }

        private void SetReason()
        {
            switch (m_CurrentReason)
            {
                case NotTrackingReason.Initializing:
                case NotTrackingReason.Relocalizing:
                
                    m_ReasonDisplayText.text = k_InitRelocalText;
                    
                    m_ReasonIcon.sprite = m_InitRelocalSprite;

                    break;

                case NotTrackingReason.ExcessiveMotion:
                    
                    m_ReasonDisplayText.text = k_MotionText;
                    
                    m_ReasonIcon.sprite = m_MotionSprite;

                    break;

                case NotTrackingReason.InsufficientLight:
                    
                    m_ReasonDisplayText.text = k_LightText;
                    
                    m_ReasonIcon.sprite = m_LightSprite;

                    break;

                case NotTrackingReason.InsufficientFeatures:
                    
                    m_ReasonDisplayText.text = k_FeaturesText;
                    
                    m_ReasonIcon.sprite = m_FeaturesSprite;

                    break;

                case NotTrackingReason.Unsupported:
                    
                    m_ReasonDisplayText.text = k_UnsupportedText;
                    
                    m_ReasonIcon.sprite = m_UnsupportedSprite;

                    break;

                case NotTrackingReason.None:
                    
                    m_ReasonDisplayText.text = k_NoneText;
                    
                    m_ReasonIcon.sprite = m_NoneSprite;

                    break;
            }
        }

        public void TestForceShowReason(NotTrackingReason reason)
        {
            m_CurrentReason = reason;

            m_ReasonParent.SetActive(true);

            SetReason();
        }
        
    }
}
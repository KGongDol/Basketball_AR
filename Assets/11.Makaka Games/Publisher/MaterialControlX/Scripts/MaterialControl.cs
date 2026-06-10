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

using System.Collections;

using MakakaGames.Publisher.Debugging;

#pragma warning disable 649

namespace MakakaGames.Publisher.MaterialControlX
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class MaterialControl : MonoBehaviour
    {
        [SerializeField]
        private Renderer renderer3D;

        [Tooltip("Array for Customizing Material Changing for Any Custom Game Logic"
            + " (e.g., fail, win)."
            + "\n\nSetMaterial() function is used instead of Color Changing with"
            + " SetColor() function to avoid memory allocations."
            + "\n\nSo if you plan to change the Colors of Object in the Runtime"
            + " outside the script, you need to create separate Materials with"
            + " Target Colors in advance.")]
        [SerializeField]
        private MaterialData[] materialDataCustom;

        private Material materialStandardShared;

        // To avoid Material Clones
        private Material materialStandardClone;

        [Header("Shader Parameter Changing")]
        [SerializeField]
        [Tooltip("Enables initialization of script. Allows to disable Parameter"
            + " Changing for Individual Game Object.")]
        private bool isParameterChangingOnAtStart = true;
        private bool isParameterChanging = false;
        private bool isFirstAppearance = true;

        [Tooltip("Check to understand Fading Steps. At the same time, Error Reports"
            + " and Troubleshooting Tips in the Console are thrown independently of"
            + " this flag.")]
        [SerializeField]
        private bool isDebugLogging = false;

        [Tooltip("Indicates the Name of any Float Parameter in the Shader for"
            + " changing.")]
        [SerializeField]
        private string parameterName = "_Cutoff";

        private float parameterAtStart = 1f;
        private float parameterMin = 0f;
        private float parameterMax = 1f;
        private float parameterCurrent = 0f;

        [Header("Delays (Parameter Changing must be completed before" +
            " Object's Deactivating)")]
        [Tooltip("Seconds to Start Fading In after calling Fade() function outside"
            + " the script.")]
        public float delayIn = 0f;
        
        [Tooltip("Seconds to Start Fading Out after calling Fade() function outside"
            + " the script.")]
        public float delayOut = 0f;
        
        [Header("( Fading Animation Curves )"
            + "\nX = Shader Parameter Value in the Previous Step"
            + "\nY = Delta Speed of Shader Parameter Changing")]
        [SerializeField]
        private AnimationCurve deltaSpeedIn =
            AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Tooltip("For convenient Scaling of the Curve along the Y axis without"
            + " changing the Curve itself.")]
        [SerializeField]
        private float deltaSpeedInFactor = 1f;

        private float deltaSpeedCurrent;

        [Space]
        [SerializeField]
        private AnimationCurve deltaSpeedOut =
            AnimationCurve.Linear(0f, 0.02f, 1f, 0.02f);

        [Tooltip("For convenient Scaling of the Curve along the Y axis without"
            + " changing the Curve itself.")]
        [SerializeField]
        private float deltaSpeedOutFactor = 1f;

        private void Awake()
        {
            //MaterialCounter.Print();

            materialStandardShared = renderer3D.sharedMaterial;

            //MaterialCounter.Print();
        }

        private void Start()
        {
            if (isParameterChangingOnAtStart)
            {
                materialStandardClone = new Material(materialStandardShared);

                if (isDebugLogging)
                {
                    DebugPrinter.Print($"MaterialControl:"
                        + $" {materialStandardClone.GetFloat(parameterName)} — "
                        + " Parameter Start Value in Material."
                        + "\nIf you see Unexpected Behavior, you must sync This"
                        + " Value with the Edge Value (Min X) on Fading Animation"
                        + " Curve.");
                }

                parameterAtStart = parameterCurrent = deltaSpeedIn.keys[^1].time;

                //MaterialCounter.Print();
            }
        }

        private void OnDisable()
        {
            if (isParameterChangingOnAtStart && isParameterChanging 
                // avoid cases when Loading New Active Scene or App Quitting   
                && gameObject.scene.isLoaded)
            {
                DebugPrinter.PrintError(gameObject.name + " > OnDisable():"
                    + " Parameter Changing is Not Completed! Adjust Timing or"
                    + " Fading Animation Curves to Avoid Stopping the Coroutine"
                    + " when Game Object Deactivating.");

                isParameterChanging = false;
            }
        }

        public void Fade(
            bool isFadeIn,
            bool isResetFadingInTheEnd,
            bool isResetColorToStandardInTheBeginning = false)
        {
            StartCoroutine(FadeCoroutine(
                isFadeIn,
                isResetFadingInTheEnd,
                isResetColorToStandardInTheBeginning));
        }

        public IEnumerator FadeCoroutine(
            bool isFadeIn,
            bool isResetFadingInTheEnd,
            bool isResetColorToStandardInTheBeginning = false)
        {
            if (isParameterChangingOnAtStart)
            {
                if (isFadeIn)
                {
                    if (isFirstAppearance)
                    {
                        isFirstAppearance = false;

                        Step();
                    }

                    yield return new WaitForSeconds(delayIn);
                }
                else
                {
                    yield return new WaitForSeconds(delayOut);
                }

                if (isParameterChanging)
                {
                    DebugPrinter.PrintError(gameObject.name + " > Crossfade Detected!"
                        + " Avoid Competing on the Same Game Object.");
                }
                
                isParameterChanging = true;

                if (isDebugLogging)
                {
                    if (isFadeIn)
                    {
                        DebugPrinter.Print(
                            $"MaterialControl: Fade In with Delay: {delayIn}");
                    }
                    else
                    {
                        DebugPrinter.Print(
                            $"MaterialControl: Fade Out with Delay: {delayOut}");
                    }
                }

                //MaterialCounter.Print();

                if (isResetColorToStandardInTheBeginning)
                {
                    materialStandardClone.color = materialStandardShared.color;
                }

                if (isFadeIn)
                {
                    parameterMin = deltaSpeedIn.keys[0].time;
                    parameterMax = parameterCurrent = deltaSpeedIn.keys[^1].time;

                    if (isDebugLogging)
                    {
                        DebugPrinter.Print(
                            $" MaterialControl Fade In Min X: {parameterMin}," +
                            $"\n\t\tMaterialControl Fade In Max X: {parameterMax}");
                    }

                    while (parameterCurrent > parameterMin)
                    {
                        deltaSpeedCurrent = deltaSpeedIn.Evaluate(parameterCurrent);
                        
                        if (isDebugLogging)
                        {
                            DebugPrinter.Print(
                                $" MaterialControl Fade In X: {parameterCurrent}," +
                                $"\n\t\tMaterialControl Fade In Y: {deltaSpeedCurrent}");
                        }

                        parameterCurrent -= deltaSpeedCurrent * deltaSpeedInFactor;

                        Step();

                        yield return new WaitForFixedUpdate();
                    }
                }
                else // Fade Out
                {
                    parameterMin = parameterCurrent = deltaSpeedOut.keys[0].time;
                    parameterMax = deltaSpeedOut.keys[^1].time;

                    if (isDebugLogging)
                    {
                        DebugPrinter.Print(
                            $" MaterialControl FadeOut Min X: {parameterMin}," +
                            $"\n\t\tMaterialControl FadeOut Max X: {parameterMax}");
                    }

                    while (parameterCurrent < parameterMax)
                    {
                        deltaSpeedCurrent = deltaSpeedOut.Evaluate(parameterCurrent);

                        if (isDebugLogging)
                        {
                            DebugPrinter.Print(
                                $" MaterialControl FadeOut X: {parameterCurrent}," +
                                $"\n\t\tMaterialControl FadeOut Y: {deltaSpeedCurrent}");
                        }

                        parameterCurrent += deltaSpeedCurrent * deltaSpeedOutFactor;

                        Step();

                        yield return new WaitForFixedUpdate();
                    }
                }

                if (isResetFadingInTheEnd)
                {
                    ResetFade();
                }

                isParameterChanging = false;

                if (isDebugLogging)
                {
                    //MaterialCounter.Print();

                    if (isFadeIn)
                    {
                        DebugPrinter.Print("MaterialControl: Finish of Fading In.");
                    }
                    else
                    {
                        DebugPrinter.Print("MaterialControl: Finish of Fading Out.");
                    }
                }
            }
        }

        private void Step()
        {
            if (renderer3D.sharedMaterial != materialStandardClone)
            {
                // if (gameObject.name == "Net")
                // {
                //     DebugPrinter.Print(materialStandardClone.shader.name);

                //     DebugPrinter.Print(renderer3D.sharedMaterial.shader.name);
                // }

                materialStandardClone.color = renderer3D.sharedMaterial.color;

                SetMaterial(materialStandardClone);
            }

            materialStandardClone.SetFloat(parameterName, parameterCurrent);

            //DebugPrinter.Print("alphaCurrent = " + alphaCurrent);
        }

        private void ResetFade()
        {
            if (isParameterChangingOnAtStart)
            {
                parameterCurrent = parameterAtStart;
            }
        }

        public void SetMaterial(Material material)
        {
            renderer3D.material = material;
        }

        public void SetMaterial(int index)
        {
            if (index >= 0
                && index < materialDataCustom.Length
                && materialDataCustom[index] != null)
            {
                renderer3D.material = materialDataCustom[index].material;
            }
            else
            {
                DebugPrinter.Print("Material doesn't exist at index: " + index);
            }
        }

        private void SetMaterialStandard()
        {
            renderer3D.material = materialStandardShared;
        }

        public void SetRendererEnabled(bool enabled)
        {
            if (renderer3D)
            {
                renderer3D.enabled = enabled;
            }
            else
            {
                DebugPrinter.Print("Renderer is Null!");
            }
        }

        [System.Serializable]
        public class MaterialData
        {
            public string nameForLog;

            public Material material;

            public MaterialData() { }

            public MaterialData(
                string nameForLog,
                Material material)
            {
                this.nameForLog = nameForLog;
                this.material = material;
            }
        }
    }
}
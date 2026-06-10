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
using UnityEngine.Events;
using UnityEngine.InputSystem;

using System.Collections;

using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.SceneManagement;

namespace MakakaGames.SensorCamera
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class SensorCameraControl : MonoBehaviour 
	{
		[SerializeField]
		private Transform cameraMain;

		private AttitudeSensor attitudeSensor;
		private Accelerometer accelerometerSensor;

		private bool isGyroSupportedNotInEditor = false;
		private bool isRotationWithGyro = false;
		private Quaternion rotationWithGyroFix = new(0f, 0f, 1f, 0f);

		[Tooltip("To Reset Gyro Data. If it’s “true”, then Gyro’s “Y” Rotation" +
			" is reset on Scene Closing or Reloading. Useful if you need to" +
			" Control the Start Rotation of the Camera when Restart.")]
		[SerializeField]
		private bool isGyroDisabledOnDestroy = false;

		[Header("Testing Not in Editor")]
		[SerializeField]
		private bool isGyroUnsupportedNotInEditorTest = false;

		[SerializeField]
		private bool isAccelerometerUnsupportedNotInEditorTest = false;

		/// <summary>
		/// It's intended for <see cref="ForciblySelectedSensor"/> variable.
		/// </summary>
		public enum ForcedSensorSelection
		{
			Default,
			NoSensors,
			Gyro,
			Accelerometer
		}

		/// <summary>
		/// It's intended for setting outside the Scene with Sensor Camera
		/// for testing purposes.
		/// <para>It will be set to <see cref="ForcedSensorSelection.NoSensors"/> on the 
		/// Scene with Sensor Camera when calling <see cref="OnDestroy"/>.</para>
		/// </summary>
		public static ForcedSensorSelection ForciblySelectedSensor =
			ForcedSensorSelection.Default;

	#if UNITY_EDITOR

		[Header("Testing In Editor")]
		[SerializeField]
		private bool isGyroSupportedInEditorTest = true;

		[SerializeField]
		private bool isAccelerometerSupportedInEditorTest = false;

	#endif

		[Header("Movement")]
		[SerializeField]
		private bool isMovementWithWASDQE = false;

		[SerializeField]
		private Vector3 movementWASDQESpeed = new(9f, 10f, 10f);

		[SerializeField]
		private float inputWASDQEGravity = 6f;

		[SerializeField]
		private float inputWASDQESensitivity = 1.5f;

		private Vector3 inputWASDQE;
		private Vector3 inputWASDQETarget;

		[Header("Events (in the order of execution)\nSensor Events")]
		[Space]
		[SerializeField]
		private UnityEvent OnGyroInitialized;

		[Space]
		[SerializeField]
		private UnityEvent OnGyroIsNotSupported;

		[Space]
		[SerializeField]
		private UnityEvent OnAccelerometerInitialized;

		[Space]
		[SerializeField]
		private UnityEvent OnAccelerometerIsNotSupported;

		[Header("Common Events")]
		[Space]
		[SerializeField]
		private UnityEvent OnAwake;

		[Space]
		[SerializeField]
		private UnityEvent OnInitializedNotInEditor = null;

	#if UNITY_EDITOR

		[Space]
		[SerializeField]
		private UnityEvent OnInitializedInEditor = null;

	#endif

		[Header("Accelerometer Settings")]
		[Tooltip("1f => no vibrations")]
		[Range(1f, 20f)]
		[SerializeField]
		private float accelerometerSensitivityXZ = 5f;

		[Tooltip("if > 1f => use it for smooth motion")]
		[Range(0f, 5f)]
		[SerializeField]
		private float accelerometerSmoothLimitXZ = 0.5f;

		private readonly float accelerometerRotationalAngleFactorXZ = -90f;
		private Vector3 accelerometerCurrentRotationXZ;
		private Quaternion accelerometerResultRotationXZ;

		[Range(0f, 1f)]
		[SerializeField]
		private float accelerometerSensitivityY = 0.11f;

		// Rotational Speed: Left and Right
		[SerializeField]
		private float accelerometerRotationalSpeedFactorY = 350f;

		private Vector3 accelerometerDirNormalized;

		private bool isRotationWithAccelerometer = false;
		private bool isAccelerometerSupportedNotInEditor = false;

		private void Awake()
		{
			OnAwake.Invoke();
		}

		private IEnumerator Start() 
		{
			// for cashing objects that are in the FOV
			yield return null;

	#if UNITY_EDITOR

			InitInEditor();

	#else

			InitNotInEditor();

	#endif

		}

		private void InitNotInEditor()
		{
			// Sensor Detection

			if (SceneControl.IsWebGLOnDesktop())
			{
				isGyroSupportedNotInEditor = false;
				isAccelerometerSupportedNotInEditor = false;
			}
			else
			{
				attitudeSensor = AttitudeSensor.current;

				if (attitudeSensor == null)
				{
					DebugPrinter.Print("Attitude Sensor is NOT SUPPORTED");

					isGyroSupportedNotInEditor = false;
				}
				else
				{
					DebugPrinter.Print("Attitude Sensor SUPPORTED");

					isGyroSupportedNotInEditor = true;
				}

				accelerometerSensor = Accelerometer.current;

				if (accelerometerSensor == null)
				{
					DebugPrinter.Print("Accelerometer Sensor is NOT SUPPORTED");

					isAccelerometerSupportedNotInEditor = false;
				}
				else
				{
					DebugPrinter.Print("Accelerometer Sensor SUPPORTED");

					isAccelerometerSupportedNotInEditor = true;
				}
			}

			// Set Testing Flags

			if (isGyroSupportedNotInEditor
				&& isGyroUnsupportedNotInEditorTest)
			{
				isGyroSupportedNotInEditor = false;
			}

			if (isAccelerometerSupportedNotInEditor
				&& isAccelerometerUnsupportedNotInEditorTest)
			{
				isAccelerometerSupportedNotInEditor = false;
			}

			// Set Testing Flags (High Priority) from Menu Scene

			DebugPrinter.Print("ForciblySelectedSensor: " + ForciblySelectedSensor);

			switch (ForciblySelectedSensor)
			{
				case ForcedSensorSelection.Gyro:
					
					if (attitudeSensor != null)
						isGyroSupportedNotInEditor = true;
					
					isAccelerometerSupportedNotInEditor = false;
					
					break;

				case ForcedSensorSelection.Accelerometer:
					
					isGyroSupportedNotInEditor = false;
					
					if (accelerometerSensor != null)
						isAccelerometerSupportedNotInEditor = true;
					
					break;

				case ForcedSensorSelection.NoSensors:
					isGyroSupportedNotInEditor = false;
					isAccelerometerSupportedNotInEditor = false;
					break;

				case ForcedSensorSelection.Default:
				default:
					break;
			}

			// Enable Sensors and Invoke Events

			if (isGyroSupportedNotInEditor)
			{
				isMovementWithWASDQE = false;

				cameraMain.parent.transform.rotation =
					Quaternion.Euler(90f, 180f, 0f);

				InputSystem.EnableDevice(attitudeSensor);

				OnGyroInitialized.Invoke();
			}
			else
			{
				OnGyroIsNotSupported.Invoke();

				if (isAccelerometerSupportedNotInEditor)
				{
					isMovementWithWASDQE = false;

					InputSystem.EnableDevice(accelerometerSensor);

					OnAccelerometerInitialized.Invoke();
				}
				else // no sensors
				{
					OnAccelerometerIsNotSupported.Invoke();
				}
			}

			OnInitializedNotInEditor.Invoke();
		}

	#if UNITY_EDITOR

		private void InitInEditor()
		{
			// Set Testing Flags

			bool isGyroSupportedInEditor = isGyroSupportedInEditorTest;
			bool isAccelerometerSupportedInEditor = isAccelerometerSupportedInEditorTest;

			// Set Testing Flags (High Priority) from Menu Scene

			DebugPrinter.Print("ForciblySelectedSensor: " + ForciblySelectedSensor);

			switch (ForciblySelectedSensor)
			{
				case ForcedSensorSelection.Gyro:
					isGyroSupportedInEditor = true;
					isAccelerometerSupportedInEditor = false;
					break;

				case ForcedSensorSelection.Accelerometer:
					isGyroSupportedInEditor = false;
					isAccelerometerSupportedInEditor = true;
					break;

				case ForcedSensorSelection.NoSensors:
					isGyroSupportedInEditor = false;
					isAccelerometerSupportedInEditor = false;
					break;

				case ForcedSensorSelection.Default:
				default:
					break;
			}

			// Invoke Events

			if (isGyroSupportedInEditor)
			{
				OnGyroInitialized.Invoke();
			}
			else
			{
				OnGyroIsNotSupported.Invoke();

				if (isAccelerometerSupportedInEditor)
				{
					OnAccelerometerInitialized.Invoke();
				}
				else
				{
					OnAccelerometerIsNotSupported.Invoke();
				}
			}

			OnInitializedInEditor.Invoke();
		}

	#else

		private void Update() 
		{
			UpdateNotInEditor();
		}

	#endif

		private void LateUpdate()
		{
			if (isMovementWithWASDQE)
			{
				MoveWithWASDQE();
			}
		}

		private void UpdateNotInEditor()
		{
			if (isGyroSupportedNotInEditor && isRotationWithGyro)
			{
				cameraMain.localRotation =
					attitudeSensor.attitude.ReadValue() * rotationWithGyroFix;
			}
			else if (isAccelerometerSupportedNotInEditor
				&& isRotationWithAccelerometer)
			{
				RotateYWithAccelerometer();
				RotateXZWithAccelerometer();
			}
		}

		private void RotateYWithAccelerometer()
		{
			accelerometerDirNormalized =
				accelerometerSensor.acceleration.ReadValue().normalized;

			if (accelerometerDirNormalized.x >= accelerometerSensitivityY
				|| accelerometerDirNormalized.x <= -accelerometerSensitivityY)
			{
				cameraMain.Rotate(
					0f,
					accelerometerSensor.acceleration.ReadValue().x
						* accelerometerRotationalSpeedFactorY * Time.deltaTime,
					0f);
			}
		}

		private void RotateXZWithAccelerometer()
		{
			accelerometerCurrentRotationXZ.y = cameraMain.localEulerAngles.y;

			accelerometerCurrentRotationXZ.x =
				accelerometerSensor.acceleration.ReadValue().z
				* accelerometerRotationalAngleFactorXZ;

			accelerometerCurrentRotationXZ.z =
				accelerometerSensor.acceleration.ReadValue().x
				* accelerometerRotationalAngleFactorXZ;

			accelerometerResultRotationXZ = Quaternion.Slerp(
				cameraMain.localRotation,
				Quaternion.Euler(accelerometerCurrentRotationXZ),
				accelerometerSensitivityXZ * Time.deltaTime);

			if (Quaternion.Angle(cameraMain.rotation, accelerometerResultRotationXZ)
				> accelerometerSmoothLimitXZ)
			{
				cameraMain.localRotation = accelerometerResultRotationXZ;
			}
			else
			{
				cameraMain.localRotation = Quaternion.Slerp(
					cameraMain.localRotation,
					Quaternion.Euler(accelerometerCurrentRotationXZ),
					Time.deltaTime);
			}
		}

		private void OnDestroy()
		{
			if (isGyroDisabledOnDestroy)
			{
				if (attitudeSensor != null)
				{
					InputSystem.DisableDevice(attitudeSensor);
				}
			}

			if (accelerometerSensor != null)
			{
				InputSystem.DisableDevice(accelerometerSensor);
			}

			ForciblySelectedSensor = ForcedSensorSelection.Default;

			DebugPrinter.Print("ForciblySelectedSensor: " + ForciblySelectedSensor);
		}

		/// <summary>
		/// Used in Editor to Test a Specific Data of Camera Transform.
		/// </summary>
		public void SetPositionAndRotation(Transform transform)
		{
			cameraMain.SetPositionAndRotation(
				transform.position, transform.rotation);
		}

		public void SetRotationWithGyroActive(bool isActive)
		{
			isRotationWithGyro = isActive;
		}

		public void SetRotationWithAccelerometerActive(bool isActive)
		{
			isRotationWithAccelerometer = isActive;
		}

		private void MoveWithWASDQE()
		{
			inputWASDQETarget = new Vector3(
				GetAxis(Key.D, Key.A),
				GetAxis(Key.E, Key.Q),
				GetAxis(Key.W, Key.S)
			) * inputWASDQESensitivity;

			// Smooth the input for all three axes
			inputWASDQE = Vector3.MoveTowards(inputWASDQE, inputWASDQETarget,
				inputWASDQEGravity * Time.deltaTime);

			Vector3 movementWASDQE = 
				inputWASDQE.x * movementWASDQESpeed.x * cameraMain.right +
				inputWASDQE.y * movementWASDQESpeed.y * Vector3.up +
				inputWASDQE.z * movementWASDQESpeed.z * Vector3.ProjectOnPlane(
					cameraMain.forward, Vector3.up).normalized;

			transform.position += movementWASDQE * Time.deltaTime;
		}

		private float GetAxis(Key positiveKey, Key negativeKey)
		{
			return (Keyboard.current[positiveKey].isPressed ? 1f : 0f) - 
				(Keyboard.current[negativeKey].isPressed ? 1f : 0f);
		}

	}
}
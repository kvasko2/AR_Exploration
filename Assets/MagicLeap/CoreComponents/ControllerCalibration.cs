// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2017 Magic Leap, Inc. (COMPANY) All Rights Reserved.
// Magic Leap, Inc. Confidential and Proprietary
//
//  NOTICE:  All information contained herein is, and remains the property
//  of COMPANY. The intellectual and technical concepts contained herein
//  are proprietary to COMPANY and may be covered by U.S. and Foreign
//  Patents, patents in process, and are protected by trade secret or
//  copyright law.  Dissemination of this information or reproduction of
//  this material is strictly forbidden unless prior written permission is
//  obtained from COMPANY.  Access to the source code contained herein is
//  hereby forbidden to anyone except current COMPANY employees, managers
//  or contractors who have executed Confidentiality and Non-disclosure
//  agreements explicitly covering such access.
//
//  The copyright notice above does not evidence any actual or intended
//  publication or disclosure  of  this source code, which includes
//  information that is confidential and/or proprietary, and is a trade
//  secret, of  COMPANY.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION,
//  PUBLIC  PERFORMANCE, OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS
//  SOURCE CODE  WITHOUT THE EXPRESS WRITTEN CONSENT OF COMPANY IS
//  STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE LAWS AND
//  INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE
//  CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS
//  TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE,
//  USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.
//
// %COPYRIGHT_END%
// --------------------------------------------------------------------*/
// %BANNER_END%

using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles controller calibration.
    /// </summary>
    [RequireComponent(typeof(PlaceFromCamera))]
    public class ControllerCalibration : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("The on-screen controller calibration instructions.")]
        private GameObject _calibrationText;

        private MLInputController _controller;

        private Vector3 _calibratedPosition = Vector3.zero;
        private Quaternion _calibratedOrientation = Quaternion.identity;

        private bool _isCalibrated = false;

        private PlaceFromCamera _cameraPlacement;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns calibrated controller position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }

        /// <summary>
        /// Returns calibrated controller rotation
        /// </summary>
        public Quaternion Orientation
        {
            get
            {
                return transform.rotation;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Sets up component dependencies.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error ControllerCalibration starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            _controller = MLInput.GetController(MLInput.Hand.Left);

            _cameraPlacement = GetComponent<PlaceFromCamera>();

#if UNITY_EDITOR
            // Disable instruction for calibratiion on Simulator.
            if (_calibrationText != null)
            {
                _calibrationText.SetActive(false);
            }
#endif
        }

        /// <summary>
        /// Resets calibration if necessary and registers callback.
        /// </summary>
        void OnEnable()
        {
            MLInput.OnControllerButtonUp += OnControllerButtonUp;
            if (!_isCalibrated)
            {
#if !UNITY_EDITOR // Removing calibration step from ML Remote Host builds.
                ResetTransform();
#else
                _isCalibrated = true;
#endif
            }
        }

        /// <summary>
        /// Deregisters input callbacks.
        /// </summary>
        void OnDisable()
        {
            MLInput.OnControllerButtonUp -= OnControllerButtonUp;
        }

        /// <summary>
        /// Stops the Input service.
        /// </summary>
        void OnDestroy()
        {
            MLInput.Stop();
        }

        /// <summary>
        /// Updates position and rotation of object to newest.
        /// </summary>
        void Update()
        {
            if (_isCalibrated)
            {
                transform.position = _controller.Position + _calibratedPosition;
                transform.rotation = _calibratedOrientation * _controller.Orientation;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reset controller position in front of the camera.
        /// </summary>
        private void ResetTransform()
        {
            _cameraPlacement.ResetTransform();
            _calibratedPosition = transform.position;
            _calibratedOrientation = transform.rotation;
        }
        #endregion

        #region Event Handlers
        ///<summary>
        /// Handles the event for button up.
        ///</summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnControllerButtonUp(byte controller_id, MLInputControllerButton button)
        {
#if !UNITY_EDITOR // Removing calibration step from ML Remote Host builds.
            // Reset to new calibration spot in front of view.
            if (button == MLInputControllerButton.HomeTap)
            {
                if (!_isCalibrated)
                {
                    // Calculate the calibration offsets.
                    _calibratedPosition = _calibratedPosition - _controller.Position;
                    _calibratedOrientation = _calibratedOrientation * Quaternion.Inverse(_controller.Orientation);
                }
                else
                {
                    // If already calibrated then reset to static calibration in front of view.
                    ResetTransform();
                }
                _isCalibrated = !_isCalibrated;
            }
#endif
        }
        #endregion
    }
}

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
using UnityEngine.Events;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    public class ImageCaptureExample : MonoBehaviour
    {
        [System.Serializable]
        private class ImageCaptureEvent : UnityEvent<Texture2D>
        {}

        #region Private Variables
        [SerializeField]
        private ImageCaptureEvent OnImageReceivedEvent;
        private bool _isCameraConnected = false;
        private bool _isCapturing = false;

        #endregion

        #region Unity Methods
        /// <summary>
        /// Start input API and register callbacks.
        /// </summary>
        void OnEnable()
        {
            if(!EnableMLCamera())
            {
                Debug.LogError("MLCamera failed to connect. Disabling ImageCapture component.");
                enabled = false;
                return;
            }

            if(!MLInput.Start())
            {
                Debug.LogError("Failed to start MLInput on ImageCapture component. Disabling the script.");
                enabled = false;
                return;
            }

            MLInput.OnControllerButtonDown += OnButtonDown;
            MLCamera.OnRawImageAvailable += OnCaptureRawImageComplete;
        }

        /// <summary>
        /// Unregister callbacks and stop input API.
        /// </summary>
        void OnDisable()
        {
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.Stop();

            MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
            _isCapturing = false;

            DisableMLCamera();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connects the MLCamera component and instantiates a new instance
        /// if it was never created.
        /// </summary>
        public bool EnableMLCamera()
        {
            if (MLCamera.Start())
            {
                _isCameraConnected = MLCamera.Connect();
            }
            return _isCameraConnected;
        }

        /// <summary>
        /// Disconnects the MLCamera if it was ever created or connected.
        /// </summary>
        public void DisableMLCamera()
        {
            MLCamera.Disconnect();
            // Explicitly set to false here as the disconnect was attempted.
            _isCameraConnected = false;
            MLCamera.Stop();
        }

        /// <summary>
        /// Captures a still image using the device's camera and returns
        /// the data path where it is saved.
        /// </summary>
        /// <param name="fileName">The name of the file to be saved to.</param>
        public void TriggerAsyncCapture()
        {
            if (MLCamera.IsStarted && _isCameraConnected)
            {
                if (MLCamera.CaptureRawImageAsync())
                {
                    _isCapturing = true;
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if ((MLInputControllerButton.Bumper == button || MLInputControllerButton.Move == button) && !_isCapturing)
            {
                TriggerAsyncCapture();
            }
        }

        /// <summary>
        /// Handles the event of a new image getting captured.
        /// </summary>
        /// <param name="imageData">The raw data of the image.</param>
        private void OnCaptureRawImageComplete(byte[] imageData)
        {
            _isCapturing = false;

            // Initialize to 8x8 texture so there is no discrepency
            // between uninitalized captures and error texture
            Texture2D texture = new Texture2D(8, 8);
            bool status = texture.LoadImage(imageData);

            if (status && (texture.width != 8 && texture.height != 8))
            {
                OnImageReceivedEvent.Invoke(texture);
            }
        }
        #endregion
    }
}

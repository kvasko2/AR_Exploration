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
using UnityEngine.Serialization;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Component used to hook into the HandGestures script and attach
    /// primitive game objects to it's detected keypoint positions for
    /// each hand.
    /// </summary>
    [RequireComponent(typeof(Gestures))]
    public class GesturesKeypointVisualizer : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Left Hand Key Points")]
        private Transform[] _leftHandKeyPoints;

        [SerializeField, Tooltip("Right Hand Key Points")]
        private Transform[] _rightHandKeyPoints;

        // Minimum distance between keypoints before hiding
        private const float KEYPOINT_PROXIMITY_DISTANCE_THRESHOLD = 0.02f;

        // Exact number of key points for each gesture
        private const uint NUM_KEYPOINTS = 3;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes MLHands API.
        /// </summary>
        void OnEnable()
        {
            if (!MLHands.Start())
            {
                Debug.LogError("Error GesturesKeypointVisualizer starting MLHands, disabling script.");
                enabled = false;
                return;
            }
            if (NUM_KEYPOINTS != _leftHandKeyPoints.Length)
            {
                Debug.LogError("Error GesturesKeypointVisualizer._leftHandKeyPoints has incorrect number of keypoints, disabling script.");
                enabled = false;
                return;
            }
            if (NUM_KEYPOINTS != _rightHandKeyPoints.Length)
            {
                Debug.LogError("Error GesturesKeypointVisualizer._rightHandKeyPoints has incorrect number of keypoints, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Stops the communication to the MLHands API and unregisters required events.
        /// </summary>
        void OnDisable()
        {
            MLHands.Stop();
        }

        /// <summary>
        /// Polls the Gestures API each frame and gets relevant information about
        /// the currently tracked gesture's keypoints.
        /// </summary>
        void Update()
        {
            if (MLHands.IsStarted)
            {
                if (MLHands.Left.StaticGesture != MLStaticGestureType.NoHand && MLHands.Left.KeyPoints.Length > 0)
                {
                    UpdateKeypointObjects(_leftHandKeyPoints, MLHands.Left);
                }
                else
                {
                    System.Array.ForEach(_leftHandKeyPoints, (x) => x.gameObject.SetActive(false));
                }

                if (MLHands.Right.StaticGesture != MLStaticGestureType.NoHand && MLHands.Right.KeyPoints.Length > 0)
                {
                    UpdateKeypointObjects(_rightHandKeyPoints, MLHands.Right);
                }
                else
                {
                    System.Array.ForEach(_rightHandKeyPoints, (x) => x.gameObject.SetActive(false));
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the positions of the keypoints to the latest data from the
        /// ML device.
        /// </summary>
        /// <param name="keypoints">The array of transforms to set.</param>
        /// <param name="hand">The hand to poll for the keypoint information.</param>
        private void UpdateKeypointObjects(Transform[] keypoints, MLHand hand)
        {

            keypoints[0].position = hand.KeyPoints[0];
            keypoints[1].position = hand.KeyPoints[1];
            keypoints[2].position = hand.Center;

            keypoints[0].gameObject.SetActive(true);

            if (Vector3.Distance(keypoints[0].position, keypoints[1].position) < KEYPOINT_PROXIMITY_DISTANCE_THRESHOLD)
            {
                keypoints[1].gameObject.SetActive(false);
            }
            else
            {
                keypoints[1].gameObject.SetActive(true);
            }

            if (Vector3.Distance(keypoints[0].position, keypoints[2].position) < KEYPOINT_PROXIMITY_DISTANCE_THRESHOLD ||
                Vector3.Distance(keypoints[1].position, keypoints[2].position) < KEYPOINT_PROXIMITY_DISTANCE_THRESHOLD)
            {
                keypoints[2].gameObject.SetActive(false);
            }
            else
            {
                keypoints[2].gameObject.SetActive(true);
            }
        }
        #endregion
    }
}

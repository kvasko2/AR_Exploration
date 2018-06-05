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
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles the functionality of updating the bounding box
    /// for the planes query params through input. This class also updates
    /// the UI text containing the latest useful info on the planes queries.
    /// </summary>
    [RequireComponent(typeof(Planes))]
    public class PlanesExample : MonoBehaviour
    {
        public enum PlanePlacement
        {
            Submarine,
            Free,
            Camera,
        }

        #region Private Variables
        [SerializeField, Tooltip("Transform follower component that drives planes transform.")]
        private TransformFollower _transformFollower;

        [SerializeField, Space, Tooltip("Place to center planes queries at.")]
        private PlanePlacement _planePlacement;
        private int _placementCount = System.Enum.GetNames(typeof(PlanePlacement)).Length;
        private readonly Vector3 _placementCameraScale = new Vector3(10.0f, 10.0f, 10.0f);
        private readonly Vector3 _placementFreeScale = new Vector3(3.0f, 3.0f, 3.0f);
        private readonly Vector3 _placementPlaneScale = Vector3.one;

        [Space, SerializeField, Tooltip("Text to display planes info on.")]
        private Text _statusText;

        [Space, SerializeField, Tooltip("Submarine object to place planes query center at.")]
        private GameObject _submarine;

        private Planes _planesComponent;

        private Camera _camera;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Check editor set variables for null references.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error PlanesExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_transformFollower == null)
            {
                Debug.LogError("Error PlanesExample._transformFollower is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_statusText == null)
            {
                Debug.LogError("Error PlanesExample._statusText is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_submarine == null)
            {
                Debug.LogError("Error PlanesExample._submarine is not set, disabling script.");
                enabled = false;
                return;
            }

            _camera = Camera.main;

            MLInput.OnControllerButtonUp += OnButtonUp;
        }

        /// <summary>
        /// Initializes variables.
        /// </summary>
        void Start()
        {
            _planesComponent = GetComponent<Planes>();

            UpdatePlanePlacement();
        }

        /// <summary>
        /// Stop input API and unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerButtonUp -= OnButtonUp;
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update transform follower to latest mesh placement value.
        /// </summary>
        private void UpdatePlanePlacement()
        {
            switch (_planePlacement)
            {
                case PlanePlacement.Camera:
                    _transformFollower.ObjectToFollow = _camera.gameObject.transform;
                    _transformFollower.gameObject.transform.localScale = _placementCameraScale;
                    break;
                case PlanePlacement.Free:
                    _transformFollower.ObjectToFollow = null;
                    _transformFollower.gameObject.transform.localScale = _placementFreeScale;
                    break;
                case PlanePlacement.Submarine:
                    _transformFollower.ObjectToFollow = _submarine.transform;
                    _transformFollower.gameObject.transform.localScale = _placementPlaneScale;
                    break;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Callback handler, changes text when new planes are received.
        /// </summary>
        /// <param name="planes"> Array of new planes. </param>
        public void OnPlanesUpdate(MLWorldPlane[] planes)
        {
            _statusText.text = string.Format("Number of Planes = {0}/{1}\nPlane Placement: {2}", planes.Length, _planesComponent.MaxPlaneCount, _planePlacement.ToString());
        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnButtonUp(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.HomeTap)
            {
                _planePlacement = (PlanePlacement)((int)(_planePlacement + 1) % _placementCount);
                UpdatePlanePlacement();
            }
        }
        #endregion
    }
}

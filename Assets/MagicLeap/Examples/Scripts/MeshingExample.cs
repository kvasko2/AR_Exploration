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

#if UNITY_EDITOR || PLATFORM_LUMIN

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This represents all the runtime control over meshing component in order to best visualize the
    /// affect changing parameters has over the meshing API.
    /// </summary>
    public class MeshingExample : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("The spatial mapper from which to update mesh params.")]
        private SpatialMapper _spatialMapper;

        [SerializeField, Tooltip("Visualizer for the meshing results.")]
        private MeshingVisualizer _meshingVisualizer;

        [SerializeField, Space, Tooltip("Flag specifying if mesh extents are bounded.")]
        private bool _bounded = false;

        [SerializeField, Space, Tooltip("The headpose canvas for example status text.")]
        private Text _statusLabel;

        [SerializeField, Space, Tooltip("Prefab to shoot into the scene.")]
        private GameObject _shootingPrefab;

        private MeshingVisualizer.RenderMode _renderMode = MeshingVisualizer.RenderMode.Wireframe;
        private int _renderModeCount;

        private static readonly Vector3 _boundedExtentsSize = new Vector3(2.0f, 2.0f, 2.0f);
        private static readonly Vector3 _boundlessExtentsSize = new Vector3(10.0f, 10.0f, 10.0f);

        private uint _trianglesPerBlockIndex = 0;
        private static readonly uint[] _trianglesPerBlockSettings = new uint[]
        {
            250,
            1000,
            5000,
            20000
        };

        private const float SHOOTING_FORCE = 300.0f;
        private const float MIN_BALL_SIZE = 0.2f;
        private const float MAX_BALL_SIZE = 0.5f;
        private const int BALL_LIFE_TIME = 10;

        private Camera _camera;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error MeshingExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_spatialMapper == null)
            {
                Debug.LogError("Error MeshingExample._spatialMapper is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_meshingVisualizer == null)
            {
                Debug.LogError("Error MeshingExample._meshingVisualizer is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_statusLabel == null)
            {
                Debug.LogError("Error MeshingExample._statusLabel is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_shootingPrefab == null)
            {
                Debug.LogError("Error MeshingExample._prefab is not set, disabling script.");
                enabled = false;
                return;
            }

            _renderModeCount = System.Enum.GetNames(typeof(MeshingVisualizer.RenderMode)).Length;

            _camera = Camera.main;

            MLInput.OnControllerButtonDown += OnButtonDown;
            MLInput.OnTriggerDown += OnTriggerDown;
            MLInput.OnControllerTouchpadGestureStart += OnTouchpadGestureStart;
        }

        /// <summary>
        /// Set correct render mode for meshing and update transform follower to input mesh placement.
        /// </summary>
        void Start()
        {
            _meshingVisualizer.SetRenderers(_renderMode);

            _spatialMapper.transform.position = _camera.gameObject.transform.position;
            _spatialMapper.transform.localScale = _bounded ? _boundedExtentsSize : _boundlessExtentsSize;
            _spatialMapper.MLSpatialMapper.targetNumberTrianglesPerblock = _trianglesPerBlockSettings[_trianglesPerBlockIndex];
            _spatialMapper.ClearFragments();
            _spatialMapper.ShowBounds(_bounded);

            UpdateStatusText();
        }

        /// <summary>
        /// Polls input and updates components.
        /// </summary>
        void Update()
        {
            _spatialMapper.transform.position = _camera.gameObject.transform.position;
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerTouchpadGestureStart -= OnTouchpadGestureStart;
            MLInput.OnTriggerDown -= OnTriggerDown;
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates examples status text.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusLabel.text = string.Format("Render Mode: {0}\nTriangles Per Block: {1}\nBounded Extents: {2}", _renderMode.ToString(),
                                                                                                                  _trianglesPerBlockSettings[_trianglesPerBlockIndex],
                                                                                                                  _bounded.ToString());
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down. Changes render mode if bumper is pressed or
        /// changes from bounded to boundless and viceversa if home button is pressed.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnButtonDown(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper || button == MLInputControllerButton.Move)
            {
                _renderMode = (MeshingVisualizer.RenderMode)((int)(_renderMode + 1) % _renderModeCount);
                _meshingVisualizer.SetRenderers(_renderMode);
            }
            // On MLMA, trigger is represented with the app button.
            if(button == MLInputControllerButton.App)
            {
                OnTriggerDown(0, 1.0f);
            }
            if (button == MLInputControllerButton.HomeTap)
            {
                _bounded = !_bounded;

                // Disable the bounds visual.
                _spatialMapper.ShowBounds(_bounded);
                _spatialMapper.transform.localScale = _bounded ? _boundedExtentsSize : _boundlessExtentsSize;
                _spatialMapper.ClearFragments();
            }

            UpdateStatusText();
        }

        /// <summary>
        /// Handles the event for trigger down. Throws a ball in the direction of
        /// the camera's forward vector.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnTriggerDown(byte controller_id, float value)
        {
            // TODO: Use pool object instead of instantiating new object on each trigger down.
            // Create the ball and necessary components and shoot it along raycast.
            GameObject ball = Instantiate(_shootingPrefab);

            ball.SetActive(true);
            float ballsize = Random.Range(MIN_BALL_SIZE, MAX_BALL_SIZE);
            ball.transform.localScale = new Vector3(ballsize, ballsize, ballsize);
            ball.transform.position = _camera.gameObject.transform.position;

            Rigidbody rigidBody = ball.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                rigidBody = ball.AddComponent<Rigidbody>();
            }
            rigidBody.AddForce(_camera.gameObject.transform.forward * SHOOTING_FORCE);

            Destroy(ball, BALL_LIFE_TIME);
        }

        /// <summary>
        /// Handles the event for touchpad gesture start. Changes amount of triangles per
        /// block if gesture is swipe up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="gesture">The gesture getting started.</param>
        private void OnTouchpadGestureStart(byte controller_id, MLInputControllerTouchpadGesture gesture)
        {
            if (gesture.Type == MLInputControllerTouchpadGestureType.Swipe
                && gesture.Direction == MLInputControllerTouchpadGestureDirection.Up)
            {
                _trianglesPerBlockIndex = (uint)((_trianglesPerBlockIndex + 1) % _trianglesPerBlockSettings.Length);
                _spatialMapper.MLSpatialMapper.targetNumberTrianglesPerblock = (uint)(_trianglesPerBlockSettings[_trianglesPerBlockIndex]);
                _spatialMapper.ClearFragments();

                UpdateStatusText();
            }
        }
        #endregion
    }
}

#endif

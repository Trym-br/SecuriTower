using UnityEngine;

namespace MyNameSpace
{
    public sealed class PixelPerfectPlayerFollower : MonoBehaviour
    {
        [SerializeField] int _pixelsPerUnit = 64;
        [SerializeField, Tooltip("Height(y value) of the base resolution.\n" +
            "This class assumes width and height of the screen resolution is *an* integer multiple of the base resolution")]
        int _baseResolutionHeight = 360;

        const string _playerTag = "Player";

        Transform _player;

        /// <summary>
        /// Scaled version of pixels per unit; where the scale is relative to the base resolution<br></br>
        /// This enables this transform to move pixel by pixel for all target resolutions
        /// </summary>
        int _scaledPixelsPerUnit;
        float _scaledInversePixelsPerUnit;

        void Awake()
        {
            _player = GameObject.FindWithTag(_playerTag).transform;

#if UNITY_EDITOR
            Debug.Assert(Mathf.Floor(((float)Screen.currentResolution.height) / _baseResolutionHeight) == ((float)Screen.currentResolution.height) / _baseResolutionHeight, "screen resolution must be an integer multiple of the base resolution");
#endif
            _scaledPixelsPerUnit = _pixelsPerUnit * Screen.currentResolution.height / _baseResolutionHeight;
            _scaledInversePixelsPerUnit = 1f / _scaledPixelsPerUnit;
        }

        void LateUpdate()
        {
            var pixelPerfectPlayerPosition = PixelPerfect(_player.position);
            transform.position = new Vector3(//might be faster to reuse a vector3 instead of creating a new one every late frame
                pixelPerfectPlayerPosition.x,
                pixelPerfectPlayerPosition.y,
                transform.position.z);
        }

        /// <summary>
        /// Converts a non-pixel perfect vector's x and y components to pixel perfect<br></br>
        /// You should not pass the position of this transform into this method, use an independent position(like the player's position) instead, otherwise this transform will go into negatives much faster
        /// </summary>
        Vector3 PixelPerfect(Vector3 vector)
        {
            vector.x = Mathf.FloorToInt(vector.x * _scaledPixelsPerUnit) * _scaledInversePixelsPerUnit;
            vector.y = Mathf.FloorToInt(vector.y * _scaledPixelsPerUnit) * _scaledInversePixelsPerUnit;
            return vector;
        }
    }
}
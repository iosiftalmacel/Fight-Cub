using UnityEngine;
using System.Collections;

namespace GameInput
{
    public class InputUtils
    {
        static InputUtils self = null;

        float _dpi;
        float _oodpi;
        float _sreenInches;
        float _bseScreenInches = 4.0f;

        public static float dpi
        {
            get
            {
                if (null == self)
                    self = new InputUtils();
                return self._dpi;
            }
        }

        public static float oodpi
        {
            get
            {
                if (null == self)
                    self = new InputUtils();
                return self._oodpi;
            }
        }

        public static float screenInches
        {
            get
            {
                if (null == self)
                    self = new InputUtils();
                return self._sreenInches;
            }
        }

        public static float baseScreenInches
        {
            get
            {
                if (null == self)
                    self = new InputUtils();
                return self._bseScreenInches;
            }

            set
            {
                if (null == self)
                    self = new InputUtils();
                self._bseScreenInches = value;
            }
        }

        public static float screenToUnits
        {
            get
            {
                if (null == self)
                    self = new InputUtils();
                return self._oodpi * (self._bseScreenInches / self._sreenInches);
            }
        }

        public static float unitsToScreen
        {
            get
            {
                if (null == self)
                    self = new InputUtils();
                return (self._sreenInches / self._bseScreenInches) * self._dpi;
            }
        }

        InputUtils()
        {
#if UNITY_EDITOR
            _dpi = (445.0f * Screen.height / 1920.0f);
#else
            _dpi = Screen.dpi;
#endif
            _oodpi = 1.0f / _dpi;

            float w = Screen.width, h = Screen.height;
            _sreenInches = Mathf.Sqrt(w * w + h * h) * _oodpi;
        }
    }
}
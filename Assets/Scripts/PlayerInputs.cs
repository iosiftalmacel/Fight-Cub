using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameInput
{
    public class PlayerInputs : NetworkBehaviour
    {
        #region Public members
        public bool showTrail = true;
        public bool isTouching = false;
        public bool inputsEnabled = false;
        public Camera trailCam;

        public bool isMoving;
        #endregion

        #region Private members
        int lastTouchId = -1;

        Vector2 lastTouchStart;
        Vector2 lastTouchEnd;
        Vector2 prevTouchPos;

        float lastTouchLength;
        float touchStartTime;
        float prevTouchTimestamp;


        bool startedFastSwipe;
        float startFastSwipeTime;
        Vector2 startFastSwipePos;
        float fastSwipeMaxSpeed;

        bool startedSlowSwipe;
        float startSlowSwipeTime;
        Vector2 startSlowSwipePos;
        int lastSlowSwipeDir = 0;

        private TrailRenderer touchTrail;
        private Player player;
        #endregion

        #region Private methods
        bool startTouch(Vector2 position)
        {
            isTouching = true;

            prevTouchPos = lastTouchStart = position;
            prevTouchTimestamp = touchStartTime = Time.realtimeSinceStartup;
            lastTouchLength = .0f;

            if (showTrail)
            {
                this.StopAllCoroutines();

                touchTrail.transform.position = trailCam.ScreenToWorldPoint(new Vector3(position.x, position.y, 0.3f));
                touchTrail.time = 0.5f;
            }
            return true;
        }

        void updateTouch(Vector2 position)
        {
            var time = Time.realtimeSinceStartup;
            var dt = time - prevTouchTimestamp;
            var absDelta = (position - lastTouchStart) * InputUtils.screenToUnits;

            var deltaMove = (position - prevTouchPos) * InputUtils.screenToUnits;
            var deltaMoveMag = deltaMove.magnitude;
            var deltaMoveSpeed = Mathf.Abs(deltaMoveMag) / dt;

            float angle = Mathf.Atan2(lastTouchStart.y - position.y, lastTouchStart.x - position.x) * 180 / Mathf.PI;
            float angle2 = Mathf.Atan2(startSlowSwipePos.y - position.y, startSlowSwipePos.x - position.x) * 180 / Mathf.PI;

            if(!isMoving)
                isMoving = (angle <= 180 && angle >= 160 || angle >= -180 && angle <= -160 || angle <= 20 && angle >= 0 || angle >= -20 && angle <= 0) && deltaMoveSpeed < 6f;

            if (!isMoving && startedSlowSwipe)
                isMoving = (angle2 <= 180 && angle2 >= 160 || angle2 >= -180 && angle2 <= -160 || angle2 <= 20 && angle2 >= 0 || angle2 >= -20 && angle2 <= 0) && deltaMoveSpeed < 6f;

            if (!startedFastSwipe && startedSlowSwipe && Mathf.Abs(startSlowSwipePos.x - position.x) * InputUtils.screenToUnits > 0.04f)
            {
                player.OnSlowSwipe(lastSlowSwipeDir);
                //if (startSlowSwipePos.x - position.x < 0)
                //    test.AddForce(Vector3.right * 2f );
                //else
                //    test.AddForce(-Vector3.right * 2f );
            }

            if ((deltaMoveSpeed < 3f || isMoving) && deltaMoveSpeed > 0.1f)
            {
                int currentSwipeDir = deltaMove.x > 0 ? 1 : -1;
                if (!startedFastSwipe && (!startedSlowSwipe || currentSwipeDir != lastSlowSwipeDir))
                {
                    startedSlowSwipe = true;
                    startSlowSwipePos = position;
                    startSlowSwipeTime = time;
                    lastSlowSwipeDir = currentSwipeDir;
                }
                 
                //Debug.LogError("enter + "  + (Mathf.Abs(startSlowSwipePos.x - position.x) * InputUtils.screenToUnits));
            }
            else if (deltaMoveSpeed > 3 && !startedFastSwipe && !isMoving)
            {
                startedFastSwipe = true;
                startFastSwipePos = position;
                startFastSwipeTime = time;
                fastSwipeMaxSpeed = Mathf.Max(fastSwipeMaxSpeed, deltaMoveSpeed);

                startedSlowSwipe = false;
                lastSlowSwipeDir = 0;
            }
            else if (deltaMoveSpeed < 0.1f)
            {
                if (startedFastSwipe)
                {
                    startedFastSwipe = false;
                    startedSlowSwipe = false;
                    lastSlowSwipeDir = 0;

                    Vector2 heading = (position - startFastSwipePos);
                    float mag = heading.magnitude;
                    Vector2 direction = heading / mag;

                    float force = mag * InputUtils.screenToUnits;
                    Debug.Log("mag - " + force);

                    player.OnFastSwipe(direction, force);
                }
            }


            if (showTrail)
                touchTrail.transform.position = trailCam.ScreenToWorldPoint(new Vector3(position.x, position.y, 0.33f));

            lastTouchLength = Mathf.Max(absDelta.magnitude, lastTouchLength);

            prevTouchPos = position;
            prevTouchTimestamp = time;
            isMoving = false;
        }

        void endTouch(Vector2 position)
        {
            isTouching = false;
            if (showTrail)
            {
                touchTrail.transform.position = trailCam.ScreenToWorldPoint(new Vector3(position.x, position.y, 0.33f));
                this.StartCoroutine(this.DisableTrail());
            }
            if (startedFastSwipe)
            {
                startedFastSwipe = false;
                startedSlowSwipe = false;
                lastSlowSwipeDir = 0;

                Vector2 heading = (position - startFastSwipePos);
                float mag = heading.magnitude;
                Vector2 direction = heading / mag;

                float force = mag * InputUtils.screenToUnits;
                Debug.Log("mag - " + force);

                player.OnFastSwipe(direction, force);
            }

            player.OnSwipeFinish((position - lastTouchStart).normalized);


            startedFastSwipe = false;
            startedSlowSwipe = false;
            lastSlowSwipeDir = 0;

            lastTouchEnd = position;

            var touchDir = (lastTouchEnd - lastTouchStart).normalized;
        }
        #endregion

        #region Public methods
        public void ResetInputs()
        {
            this.StopAllCoroutines();
            touchTrail.time = .0f;

            lastTouchId = -1;
        }

        public void EnableInputs()
        {
            inputsEnabled = true;
        }

        public void DisableInputs()
        {
            inputsEnabled = false;

            lastTouchId = -1;
        }

        public float GetCurrentTouchLength()
        {
            return lastTouchLength;
        }

        #endregion

        #region Coroutines
        IEnumerator DisableTrail()
        {
            float t = touchTrail.time;
            while (t > .01f)
            {
                yield return null;

                t = Mathf.Lerp(t, .0f, 7.0f * Time.deltaTime);
                touchTrail.time = t;
            }
            touchTrail.time = .0f;
        }
        #endregion

        #region Unity callbacks
        void Start()
        {
            if (!isLocalPlayer)
                return;
            player = GetComponent<Player>();
            trailCam = Instantiate(trailCam);
            touchTrail = trailCam.transform.GetChild(0).GetComponent<TrailRenderer>(); ;
            touchTrail.time = .0f;
        }

        private bool WasAButton()
        {
            UnityEngine.EventSystems.EventSystem ct
                = UnityEngine.EventSystems.EventSystem.current;

            if (!ct.IsPointerOverGameObject()) return false;
            if (!ct.currentSelectedGameObject) return false;
            if (ct.currentSelectedGameObject.GetComponent<Button>() == null)
                return false;

            return true;
        }

        private bool IsPointerOverUIObject(Vector2 t)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(t.x, t.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        void Update()
        {

            if (!inputsEnabled || !isLocalPlayer)
                return;

#if !UNITY_EDITOR && !UNITY_STANDALONE
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId < 0 || t.fingerId >= 4)
                    continue;

                switch (t.phase)
                {
                    case TouchPhase.Began:
                        //UIDebugPage.Log("TouchPhase.Began " + lastTouchId + ", " + t.fingerId);
                        if (-1 == lastTouchId && 0 == t.fingerId)
                        {
                            if (!IsPointerOverUIObject(t.position) && this.startTouch(t.position))
                            {
                                lastTouchId = t.fingerId;
                            }
                        }
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        //UIDebugPage.Log("TouchPhase.Moved or Stationary " + lastTouchId + ", " + t.fingerId);
                        if (t.fingerId == lastTouchId)
                            this.updateTouch(t.position);
                        break;
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        //UIDebugPage.Log("TouchPhase.Canceled or Ended " + lastTouchId + ", " + t.fingerId);
                        if (t.fingerId == lastTouchId)
                        {
                            this.updateTouch(t.position);
                            if (lastTouchId > -1)
                            {
                                this.endTouch(t.position);
                                lastTouchId = -1;
                            }
                        }
                        break;
                }
            }
#else
            if (lastTouchId > -1)
                this.updateTouch(Input.mousePosition);
           

            if (Input.GetMouseButtonDown(0))
            {
                if (!WasAButton() && this.startTouch(Input.mousePosition))
                {
                    lastTouchId = 0;
                }
            }

            if (Input.GetMouseButtonUp(0) && lastTouchId > -1)
            {
                this.endTouch(Input.mousePosition);
                lastTouchId = -1;
            }
#endif
        }
        #endregion
    }
}

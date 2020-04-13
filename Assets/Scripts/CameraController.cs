using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CameraController : NetworkBehaviour
{
    public float minOrto, maxOrto;
    public float maxX, maxY, minX, minY;

    Camera camera;
    GameObject[] players;

    Vector3 nextPos;
    Vector3 velocity;
    float shakeStart, shakeDuration, shakeMagnitude;
    float maxXOrto, maxYOrto, minXOrto, minYOrto;

    float maxVisibleDistance;
    float nextOrto;
    int updateCount;
    int trackingCount;


    float camHeight;
    float camWidth;

    const float translateVel = 0.2f;
    const float ortoVel = 5f;
    const float translateVelChanging = 0.9f;
    const float ortoVelChanging = 1f;

    float currTranslateVel = 0.2f;
    float currOrtoVel = 10f;

    float lastChangeTime;
    int lastChangeCount;
    int prevLastChangeCount;

    void Awake()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        lastChangeCount = 1;

        for (int i = 0; i < indicator.Length; i++)
        {
            indicator[i].parent = null;
            indicator[i].localScale = Vector3.one * 0.14f;
            indicator[i].gameObject.SetActive(false);
        }

        AudioManager.Instance.Stop("off_game");
        AudioManager.Instance.Play("in_game", 4, true);
    }
    public Transform[] indicator;
    // Update is called once per frame
    void LateUpdate()
    {
        if (!isLocalPlayer || camera.gameObject.layer == 9)
            return;

        camHeight = 2f * camera.orthographicSize;
        camWidth = camHeight * camera.aspect;

        maxVisibleDistance = 2 * maxOrto * camera.aspect / 2.2f;

        updateCount++;
        if (updateCount == 30)
        {
            updateCount = 0;
            players = GameObject.FindGameObjectsWithTag("Player");
        }

        nextPos = this.transform.position;
        trackingCount = 1;

        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != this.gameObject && Mathf.Abs(Vector3.Distance(players[i].transform.position, this.transform.position)) < maxVisibleDistance)
                {
                    trackingCount++;
                    nextPos += players[i].transform.position;
                }
            }
        }


        if (trackingCount != lastChangeCount)
        {
            lastChangeTime = Time.timeSinceLevelLoad;
            prevLastChangeCount = lastChangeCount;
            lastChangeCount = trackingCount;
        }
        if (Time.timeSinceLevelLoad - lastChangeTime < 1)
        {
            //if (trackingCount > prevLastChangeCount)
            //{
            //    currTranslateVel = (translateVel + translateVelChanging) / 2;
            //    currOrtoVel = (ortoVel + ortoVelChanging) / 2;
            //}
            //else
            {
                currTranslateVel = Mathf.Lerp(translateVelChanging, translateVel, (Time.timeSinceLevelLoad - lastChangeTime) / 1);
                currOrtoVel = Mathf.Lerp(ortoVelChanging, ortoVel, (Time.timeSinceLevelLoad - lastChangeTime) / 1);
            }
        }
        else
        {
            currTranslateVel = translateVel;
            currOrtoVel = ortoVel;
        }

        nextPos /= trackingCount;

        float distance = Mathf.Abs(Vector3.Distance(nextPos, this.transform.position));

        nextOrto = distance * 1.2f;
        nextOrto = Mathf.Clamp(nextOrto, minOrto, maxOrto);

        maxYOrto = maxY - camHeight / 2;
        maxXOrto = maxX - camWidth / 2;

        minYOrto = minY + camHeight / 2;
        minXOrto = minX + camWidth / 2;

        nextPos = new Vector3(Mathf.Clamp(nextPos.x, minXOrto, maxXOrto), Mathf.Clamp(nextPos.y, minYOrto, maxYOrto), -10);

        camera.transform.position = Vector3.Lerp(camera.transform.position, nextPos, Time.deltaTime / currTranslateVel);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, nextOrto, Time.deltaTime * currOrtoVel);

        if (Time.realtimeSinceStartup - shakeStart < shakeDuration)
        {
            camera.transform.position = camera.transform.position + (Vector3)new Vector2(Random.Range(-shakeMagnitude, shakeMagnitude), Random.Range(-shakeMagnitude, shakeMagnitude));
        }


        for (int i = 0; i < indicator.Length; i++)
        {
            if (players == null || i >= players.Length || players[i] == this.gameObject)
            {
                indicator[i].gameObject.SetActive(false);
                continue;
            }

            Renderer[] renderers = players[i].GetComponentsInChildren<Renderer>();

            if (!renderers[1].isVisible)
            {
                // This Enemy is out of Frustrum, so we want an indicator point to its direction it.
                Vector3 lookAt = (players[i].transform.position + this.transform.position) / 2;
                //lookAt.x = players[i].transform.position.x;
                Vector3 worldToViewportPoint = camera.WorldToViewportPoint(lookAt);
                // returns coming from upper left
                //worldToViewportPoint = (-0.1, 0.5, 14.8), viewportToScreenPoint =(-66.1, 361.3, 14.8)

                worldToViewportPoint += (players[i].transform.position - this.transform.position).normalized * (worldToViewportPoint.x < 0.5f ? worldToViewportPoint.x : 1 - worldToViewportPoint.x);
                Vector3 screenPosClamped = new Vector3(Mathf.Clamp(worldToViewportPoint.x, 0.0f, 1.0f), Mathf.Clamp(worldToViewportPoint.y, 0.0f, 1.0f), 0);

                Vector3 v3 = new Vector3(camera.transform.position.x + screenPosClamped.x * camWidth - (camWidth / 2), camera.transform.position.y + screenPosClamped.y * camHeight - (camHeight / 2), 0);

                //Debug.Log("enemy " + allenemies.name + " out of view at worldToViewportPoint = " + worldToViewportPoint + ", v3 =" + v3);
                indicator[i].position = v3;
                indicator[i].gameObject.SetActive(true);
                indicator[i].localEulerAngles = new Vector3(0, 0, Vector3.Angle(Vector3.up, indicator[i].transform.position - this.transform.position) * (players[i].transform.position.x > this.transform.position.x ? -1 : 1));
                indicator[i].position -= indicator[i].up * 0.1f;
            }
            else
            {
                indicator[i].gameObject.SetActive(false);
            }
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeStart = Time.realtimeSinceStartup;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

}
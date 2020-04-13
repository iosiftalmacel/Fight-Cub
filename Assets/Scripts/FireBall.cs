using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour {

    float startTime;
    Vector2 translateDir;

    // Use this for initialization
    public void Shoot (Vector2 dir) {
        startTime = Time.timeSinceLevelLoad;
        translateDir = dir;
        transform.rotation = Quaternion.Euler(Vector2.zero);
        transform.localScale = Vector3.zero;
    }

    public void Shoot(Vector2 dir, Vector2 pos)
    {
        this.transform.position = pos;
        Shoot(dir);
    }

    // Update is called once per frame
    void Update () {
        transform.Translate(translateDir * Time.deltaTime);

        if(Time.timeSinceLevelLoad - startTime > 6)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 6);
            if (transform.localScale == Vector3.zero)
            {
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 1.9f + Vector3.up * 0.01F, Time.deltaTime * 3.5f);
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Saw : NetworkBehaviour {
    public Vector3 speed;

    public Vector2 startPos;
    public Vector2 endPos;

    Vector2 nextPos;

    [SyncVar(hook = "RpcChangePos")]
    public Vector2 currentPos;

    // Use this for initialization
    void OnEnable()
    {
        nextPos = startPos;
        //AudioManager.Instance.Play("ig_loop_saw", 0.01f, true);
	}

    void RpcChangePos(Vector2 pos)
    {
        transform.localPosition = pos;
    }

    // Update is called once per frame
    void Update () {
        transform.Rotate(speed);

        if (startPos == endPos)
            return;

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, nextPos, Time.deltaTime * 5f);
        if (((Vector2)transform.localPosition - nextPos).magnitude < 0.2f)
        {
            nextPos = nextPos == startPos ? endPos : startPos;
            if (isServer)
                currentPos = transform.localPosition;
        }
	}

    public Sprite normal;
    public Sprite blody;

    void OnCollisionEnter2D(Collision2D coll)
    {
        GetComponent<SpriteRenderer>().sprite = blody;
    }
}

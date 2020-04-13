using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour {
    public Vector2 startPos;
    public Vector2 endPos;

    Vector2 nextPos;

    // Use this for initialization
    void Start () {
        nextPos = startPos;
    }
	
	// Update is called once per frame
	void Update () {
        this.transform.localPosition = Vector2.MoveTowards(transform.localPosition, nextPos, 0.008f);
        if(Mathf.Abs(Vector2.Distance(this.transform.localPosition, nextPos)) < 0.05)
        {
            nextPos = nextPos == startPos ? endPos : startPos;
        }
	}
}

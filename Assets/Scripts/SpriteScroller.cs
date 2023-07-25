using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteScroller : MonoBehaviour
{
    [SerializeField] Vector2 moveSpeed;

    Vector2 offset;
    Material material;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        offset.x = moveSpeed.x * Time.deltaTime * Mathf.Sign(Time.deltaTime) ;
        //Debug.Log(offset.x);
        Debug.Log(Mathf.Sign(Time.deltaTime));
        material.mainTextureOffset += new Vector2(offset.x, offset.y);
    }
}

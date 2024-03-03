using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType
{
    Blue=0,
    Green=1,
    Pink=2,
    Purple=3,
    Red=4,
    Yellow=5,
    empty=6
}
public class Shape : MonoBehaviour
{
    [SerializeField] public ShapeType shapeType;
    [SerializeField] private float duration = 0.5f;

    public int xIndex;
    public int yIndex;

    public bool isMatching;
    private Vector2 currentPos;
    private Vector2 targetPos;

    private bool isMoving=false;

    public Shape(int _x,int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }
    public void SetShape(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }
    public void SetShapeType(int i,Sprite sprite)
    {
        shapeType = (ShapeType)i;
        GetComponent<SpriteRenderer>().sprite = sprite;
        isMatching = false;
    }
    public void MoveToTarget(Vector2 _targetPosition)
    {
        StartCoroutine(MoveCoroutine(_targetPosition));
    }
    IEnumerator MoveCoroutine(Vector2 _targetPosition)
    {
        isMoving = true;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0;
        float t;

        while(elaspedTime<duration)
        {
            t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPosition, t);
            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPosition;
        isMoving = false;
    }
    public void ResizeShape()
    {
        StartCoroutine(ResizeCoroutine());
    }
    IEnumerator ResizeCoroutine()
    {
        float size = transform.localScale.x;

        while(size< 0.65f)
        {
            transform.localScale = new Vector3(size + 0.05f, size + 0.05f, size + 0.05f);

            yield return new WaitForSeconds(0.01f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;

public class Point_Viz : MonoBehaviour
{
    private Vector3 mOffset = Vector3.zero;

    public delegate void OnPointChange(Transform t);
    public OnPointChange mOnDragPoint;
    public OnPointChange mOnDragStart;
    public OnPointChange mOnDragStop;


    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, Vector2.zero);
        if (hit)
        {
            FullImage fullImage;
            if(hit.collider.gameObject.TryGetComponent(out fullImage))
                return;
        }
        mOffset = transform.position - Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));


        mOnDragStart?.Invoke(transform);
    }

    void OnMouseDrag()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, Vector2.zero);
        if (hit)
        {
            FullImage fullImage;
            if(hit.collider.gameObject.TryGetComponent(out fullImage))
                return;
        }
        

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + mOffset;
        transform.position = curPosition;

        mOnDragPoint?.Invoke(transform);
    }
    void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, Vector2.zero);
        if (hit)
        {
            FullImage fullImage;
            if(hit.collider.gameObject.TryGetComponent(out fullImage))
                return;
        }
        mOnDragStop?.Invoke(transform);
    }


}

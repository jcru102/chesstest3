using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class InstantTweeners : MonoBehaviour, IObjectTweeners
{
    public void MoveTo(Transform transform, Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }
}

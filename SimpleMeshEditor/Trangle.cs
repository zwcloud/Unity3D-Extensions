#if UNITY_EDITOR
//This is a editor only script
using UnityEngine;

[ExecuteInEditMode]
public class Trangle : MonoBehaviour
{
    public Vector3 Point { set; get; }
}
#endif
/*! \file		TrangleEditor.cs
 *  \author		Zou Wei, zwcloud@yeah.net
 *  \version	1.2
 *  \date		2015.7.22
 *  \remark		
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Trangle))]
public class TrangleEditor : Editor
{
    /// <summary>
    /// States of the editor
    /// </summary>
    enum TriangleEditorState
    {
        /// <summary>
        /// now selecting a triange
        /// </summary>
        Selecting,
        /// <summary>
        /// now selected a triange
        /// </summary>
        Selected
    };

    /// <summary>
    /// state of the editor
    /// </summary>
    TriangleEditorState state = TriangleEditorState.Selecting;

    /// <summary>
    /// layer mask
    /// </summary>
    int layerMask = 0xff;

    /// <summary>
    /// Transform of selected mesh
    /// </summary>
    Transform transform;


    /// <summary>
    /// selected mesh
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// MeshCollider of selected mesh
    /// </summary>
    private MeshCollider collider;


    /// <summary>
    /// copy of the trangle points
    /// </summary>
    Vector3[] p = { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };

    /// <summary>
    /// vertex index of the trangle points
    /// </summary>
    int[] vIndex = new int[3];

    /// <summary>
    /// handle scene GUI update
    /// </summary>
    void OnSceneGUI()
    {
        Event e = Event.current;
        HandleUtility.AddDefaultControl(0);
        RaycastHit raycastHit = new RaycastHit();
        Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if(state == TriangleEditorState.Selecting)
        {
            if (Physics.Raycast(r, out raycastHit, Mathf.Infinity, layerMask))
            {
                collider = raycastHit.collider as MeshCollider;
                if (collider == null || collider.sharedMesh == null)
                    return;

                mesh = collider.sharedMesh;

                int[] triangles = mesh.triangles;
                vIndex[0] = triangles[raycastHit.triangleIndex * 3];
                vIndex[1] = triangles[raycastHit.triangleIndex * 3 + 1];
                vIndex[2] = triangles[raycastHit.triangleIndex * 3 + 2];

                Vector3[] vertices = mesh.vertices;
                p[0] = vertices[vIndex[0]];
                p[1] = vertices[vIndex[1]];
                p[2] = vertices[vIndex[2]];

                transform = collider.transform;

                DrawTrangle();

                Handles.color = Color.green;
                Handles.SphereCap(0, raycastHit.point, Quaternion.identity, 0.1f);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    state = TriangleEditorState.Selected;
                }

                SceneView.RepaintAll();
            }
        }
        else
        {
            if (mesh == null)
                return;
            DrawTrangle();

            Vector3[] pWorld = new Vector3[3];

            pWorld[0] = transform.TransformPoint(p[0]);
            pWorld[1] = transform.TransformPoint(p[1]);
            pWorld[2] = transform.TransformPoint(p[2]);

            Vector3[] pWorldNew = new Vector3[3];

            pWorldNew[0] = Handles.PositionHandle(pWorld[0], Quaternion.identity);
            pWorldNew[1] = Handles.PositionHandle(pWorld[1], Quaternion.identity);
            pWorldNew[2] = Handles.PositionHandle(pWorld[2], Quaternion.identity);

            //Prevent self movement of vertex, which is caused by lack of precision of float type.
            bool changed = false;
            for (var i=0; i<3; ++i)
            {
                if ((pWorld[i] - pWorldNew[i]).sqrMagnitude > 0.0001)
                {
                    pWorld[i] = pWorldNew[i];
                    changed = true;
                }
            }
            if (changed)
            {
                Vector3[] vertices = mesh.vertices;

                p[0] = vertices[vIndex[0]] = transform.InverseTransformPoint(pWorld[0]);
                p[1] = vertices[vIndex[1]] = transform.InverseTransformPoint(pWorld[1]);
                p[2] = vertices[vIndex[2]] = transform.InverseTransformPoint(pWorld[2]);

                mesh.vertices = vertices;

                mesh.RecalculateNormals();

                //refresh sharedMesh of MeshCollider
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }
            
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                state = TriangleEditorState.Selecting;
            }
        }

    }

    #region for flash
    private float v = 0;
    bool order = false;
    #endregion

    /// <summary>
    /// Highlight the selected trangle
    /// </summary>
    void DrawTrangle()
    {
        Vector3[] pWorld = (Vector3[])p.Clone();
        //p is a local position, now transform it into a world one
        pWorld[0] = transform.TransformPoint(p[0]);
        pWorld[1] = transform.TransformPoint(p[1]);
        pWorld[2] = transform.TransformPoint(p[2]);

        #region flash the trangle
        v = Mathf.Clamp(v, 0.3f, 1.0f);
        if (v >= 1.0f || v <= 0.3f)
        {
            order = !order;
        }
        if (order)
        {
            v += 0.02f;
        }
        else
        {
            v -= 0.02f;
        }
        #endregion
        Handles.color = EditorGUIUtility.HSVToRGB(0.361f, 1.0f, v);
        Handles.DrawLine(pWorld[0], pWorld[1]);
        Handles.DrawLine(pWorld[1], pWorld[2]);
        Handles.DrawLine(pWorld[2], pWorld[0]);
    }

}

/*! \file		TrangleEditor.cs
 *  \author		Zou Wei, zwcloud@yeah.net
 *  \version	1.3
 *  \date		2015.7.29
 *  \remark		
 */

/*
# Usage
1. Put all files into your Project
2. Add component **Trangle.cs** to the mesh you want to edit.
3. Select a triangle by click on a triangle you want to edit.
4. Press Tab to switch among three vertex of the triangles.
5. Drag the handle to move the selected vertex around. <br>
   Or input position of the vertex directly.
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
        /// now selecting a triangle
        /// </summary>
        Selecting,
        /// <summary>
        /// now selecting a vertex of the triangle
        /// </summary>
        SelectingVertex,
    };

    /// <summary>
    /// state of the editor
    /// </summary>
    TriangleEditorState state = TriangleEditorState.SelectingTrangle;

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
        var trangle = target as Trangle;
        if (trangle == null) return;
        var meshFilter = trangle.GetComponent<MeshFilter>();
        if (meshFilter == null) return;
        mesh = meshFilter.sharedMesh;

        Event e = Event.current;
        Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (state == TriangleEditorState.SelectingTrangle)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(r, out raycastHit, Mathf.Infinity, layerMask))
            {
                if (raycastHit.transform.gameObject != trangle.gameObject)
                {
                    return;
                }

                collider = raycastHit.collider as MeshCollider;
                if (collider == null || collider.sharedMesh == null)
                    return;

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

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    state = TriangleEditorState.SelectingVertex;
                }

                SceneView.RepaintAll();
            }
        }
        else if (state == TriangleEditorState.SelectingVertex)
        {
            if (mesh == null)
                return;

            DrawTrangle();

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab)
            {
                SelectedVertexNumber++;
                if (SelectedVertexNumber == 3)
                {
                    SelectedVertexNumber = 0;
                }
            }

            var localPos = p[SelectedVertexNumber];
            var worldPos = transform.TransformPoint(localPos);
            trangle.Point = worldPos;

            var worldPosNew = Handles.PositionHandle(worldPos, Quaternion.identity);

            if ((worldPos - worldPosNew).sqrMagnitude > 0.0001)
            {
                Vector3[] vertices = mesh.vertices;
                p[SelectedVertexNumber] = vertices[vIndex[SelectedVertexNumber]] = transform.InverseTransformPoint(worldPosNew);
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                //Update mesh of the collider
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }
            Repaint();

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                state = TriangleEditorState.SelectingTrangle;
            }
            SceneView.RepaintAll();
        }
    }

    public TrangleEditor()
    {
        SelectedVertexNumber = 0;
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
    public override void OnInspectorGUI()
    {
        if (mesh == null) return;
        var trangle = target as Trangle;
        if (trangle == null) return;
        var newPoint = EditorGUILayout.Vector3Field("Vertex Position", trangle.Point);
        if ((newPoint - trangle.Point).sqrMagnitude > 0.0001)
        {
            trangle.Point = newPoint;
            Vector3[] vertices = mesh.vertices;
            vertices[vIndex[SelectedVertexNumber]] = transform.InverseTransformPoint(trangle.Point);
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            //Update mesh of the collider
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
        }
    }
}

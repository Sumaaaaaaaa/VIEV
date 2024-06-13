using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MatrixBlender))]
public class Camera_PerspectiveSwitcher : MonoBehaviour
{
    private Matrix4x4 ortho,
                        perspective;
    public float fov = 60f,
                        near = .3f,
                        far = 1000f,
                        orthographicSize = 50f;
    private float aspect;
    private MatrixBlender blender;
    private bool orthoOn;

    void Start()
    {
        aspect = (float)Screen.width / (float)Screen.height;
        ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, near, far);
        perspective = Matrix4x4.Perspective(fov, aspect, near, far);
        GetComponent<Camera>().projectionMatrix = perspective;//1
        orthoOn = true;
        blender = (MatrixBlender)GetComponent(typeof(MatrixBlender));
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            orthoOn = !orthoOn;
            if (orthoOn)
                blender.BlendToMatrix(ortho, 1f);
            else
                blender.BlendToMatrix(perspective, 1f);
        }
        */
    }
    public void ToPersp(float time)
    {
        blender.BlendToMatrix(perspective, time);
    }
    public void ToOrtho(float time)
    {
        blender.BlendToMatrix(ortho, time);
    }
}
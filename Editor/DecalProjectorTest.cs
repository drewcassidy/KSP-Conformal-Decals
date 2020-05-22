using UnityEngine;

[ExecuteInEditMode]
public class DecalProjectorTest : MonoBehaviour
{
    public GameObject target = null;
    public Material targetMaterial = null;
    public MeshRenderer targetRenderer;


    public float aspectRatio = 1.0f;
    public float size = 1.0f;
    public float factor = 1.0f;

    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _OrthoMatrix;

    private int _matrixID;
    private int _normalID;
    public int _tangentID;
    
    // Start is called before the first frame update
    void Awake()
    {
        _projectionMatrix = Matrix4x4.identity;
        _matrixID = Shader.PropertyToID("_ProjectionMatrix");
        _normalID = Shader.PropertyToID("_DecalNormal");
        _tangentID= Shader.PropertyToID("_DecalTangent");
        targetRenderer = target.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos =new Vector3( 0.5f ,0.5f, 0);
        Vector3 scale = new Vector3(1 / size, 1 / (aspectRatio * size), 1);
        _OrthoMatrix.SetTRS(pos, Quaternion.identity, scale);
        //Debug.Log(_OrthoMatrix);
        var targetToProjector = transform.worldToLocalMatrix * targetRenderer.localToWorldMatrix;
        var projectorToTarget = targetRenderer.worldToLocalMatrix * transform.localToWorldMatrix;
        _projectionMatrix = _OrthoMatrix * targetToProjector;
        
        targetMaterial.SetMatrix(_matrixID, _projectionMatrix);
        targetMaterial.SetVector(_normalID, projectorToTarget.MultiplyVector(Vector3.back).normalized);
        targetMaterial.SetVector(_tangentID, projectorToTarget.MultiplyVector(Vector3.right).normalized);
    }
}

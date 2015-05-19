//
// Streamline - line based particle system.
//

using UnityEngine;
using System.Collections;

namespace Kvant {

[ExecuteInEditMode]
[AddComponentMenu("Kvant/Streamline")]
public class Streamline : MonoBehaviour
{
    #region Parameters Exposed To Editor

    [SerializeField] int _maxParticles = 32768;

    [SerializeField] Vector3 _emitterPosition = Vector3.forward * 20;
    [SerializeField] Vector3 _emitterSize = Vector3.one * 40;
    [SerializeField] float _throttle = 1.0f;

    [SerializeField] Vector3 _direction = -Vector3.forward;
    [SerializeField] float _spread = 0.2f;

    [SerializeField] float _minSpeed = 5.0f;
    [SerializeField] float _maxSpeed = 10.0f;

    [SerializeField] float _noiseFrequency = 0.2f;
    [SerializeField] float _noiseSpeed = 0.1f;
    [SerializeField] float _noiseAnimation = 1.0f;

    [SerializeField] Color _color = Color.white;
    [SerializeField] float _colorAmp = 1.0f;
    [SerializeField] float _tail = 1.0f;

    [SerializeField] int _randomSeed = 0;
    [SerializeField] bool _debug;

    #endregion

    #region Public Properties

    // Returns the actual number of particles.
    public int maxParticles {
        get { return BufferWidth * BufferHeight; }
    }

    public float throttle {
        get { return _throttle; }
        set { _throttle = value; }
    }

    public Vector3 emitterPosition {
        get { return _emitterPosition; }
        set { _emitterPosition = value; }
    }
    public Vector3 emitterSize {
        get { return _emitterSize; }
        set { _emitterSize = value; }
    }

    public Vector3 direction {
        get { return _direction; }
        set { _direction = value; }
    }
    public float spread {
        get { return _spread; }
        set { _spread = value; }
    }

    public float minSpeed {
        get { return _minSpeed; }
        set { _minSpeed = value; }
    }
    public float maxSpeed {
        get { return _maxSpeed; }
        set { _maxSpeed = value; }
    }

    public float noiseFrequency {
        get { return _noiseFrequency; }
        set { _noiseFrequency = value; }
    }
    public float noiseSpeed {
        get { return _noiseSpeed; }
        set { _noiseSpeed = value; }
    }
    public float noiseAnimation {
        get { return _noiseAnimation; }
        set { _noiseAnimation = value; }
    }

    public Color color {
        get { return _color; }
        set { _color = value; }
    }
    public float colorAmp {
        get { return _colorAmp; }
        set { _colorAmp = value; }
    }
    public float tail {
        get { return _tail; }
        set { _tail = value; }
    }

    #endregion

    #region Shader And Materials

    [SerializeField] Shader _kernelShader;
    [SerializeField] Shader _lineShader;
    [SerializeField] Shader _debugShader;

    Material _kernelMaterial;
    Material _lineMaterial;
    Material _debugMaterial;

    #endregion

    #region GPGPU Buffers

    RenderTexture _positionBuffer1;
    RenderTexture _positionBuffer2;

    #endregion

    #region Private Objects

    Mesh _mesh;
    bool _needsReset = true;

    #endregion

    #region Resource Management

    public void NotifyConfigChange()
    {
        _needsReset = true;
    }

    int BufferWidth { get { return 256; } }

    int BufferHeight {
        get {
            return Mathf.Clamp(_maxParticles / BufferWidth + 1, 1, 127);
        }
    }

    Material CreateMaterial(Shader shader)
    {
        var material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        return material;
    }

    RenderTexture CreateBuffer()
    {
        var buffer = new RenderTexture(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGBFloat);
        buffer.hideFlags = HideFlags.DontSave;
        buffer.filterMode = FilterMode.Point;
        buffer.wrapMode = TextureWrapMode.Repeat;
        return buffer;
    }

    Mesh CreateMesh()
    {
        var Nx = BufferWidth;
        var Ny = BufferHeight;

        // Create vertex arrays.
        var VA = new Vector3[Nx * Ny * 2];
        var TA = new Vector2[Nx * Ny * 2];

        var Ai = 0;
        for (var x = 0; x < Nx; x++)
        {
            for (var y = 0; y < Ny; y++)
            {
                VA[Ai + 0] = new Vector3(0, 0, 0);
                VA[Ai + 1] = new Vector3(1, 0, 0);

                var u = (float)x / Nx;
                var v = (float)y / Ny;
                TA[Ai] = TA[Ai + 1] = new Vector2(u, v);

                Ai += 2;
            }
        }

        // Index array.
        var IA = new int[VA.Length];
        for (Ai = 0; Ai < VA.Length; Ai++) IA[Ai] = Ai;

        // Create a mesh object.
        var mesh = new Mesh();
        mesh.vertices = VA;
        mesh.uv = TA;
        mesh.SetIndices(IA, MeshTopology.Lines, 0);
        mesh.Optimize();

        // Avoid being culled.
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        // This only for temporary use. Don't save.
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    void ApplyKernelParameters()
    {
        _kernelMaterial.SetVector("_EmitterPos", _emitterPosition);
        _kernelMaterial.SetVector("_EmitterSize", _emitterSize);

        var dir = new Vector4(_direction.x, _direction.y, _direction.z, _spread);
        _kernelMaterial.SetVector("_Direction", dir);

        _kernelMaterial.SetVector("_SpeedParams", new Vector2(_minSpeed, _maxSpeed));

        if (_noiseSpeed > 0)
        {
            var np = new Vector3(_noiseFrequency, _noiseSpeed, _noiseAnimation);
            _kernelMaterial.SetVector("_NoiseParams", np);
            _kernelMaterial.EnableKeyword("NOISE_ON");
        }
        else
        {
            _kernelMaterial.DisableKeyword("NOISE_ON");
        }

        var life = 2.0f;
        var delta = Application.isPlaying ? Time.smoothDeltaTime : 1.0f / 30;
        _kernelMaterial.SetVector("_Config", new Vector4(_throttle, life, _randomSeed, delta));
    }

    void ResetResources()
    {
        // Mesh object.
        if (_mesh == null) _mesh = CreateMesh();

        // GPGPU buffers.
        if (_positionBuffer1) DestroyImmediate(_positionBuffer1);
        if (_positionBuffer2) DestroyImmediate(_positionBuffer2);

        _positionBuffer1 = CreateBuffer();
        _positionBuffer2 = CreateBuffer();

        // Shader materials.
        if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader );
        if (!_lineMaterial )  _lineMaterial   = CreateMaterial(_lineShader );
        if (!_debugMaterial)  _debugMaterial  = CreateMaterial(_debugShader);

        // Initialization.
        ApplyKernelParameters();
        Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);

        _needsReset = false;
    }

    #endregion

    #region MonoBehaviour Functions

    void Reset()
    {
        _needsReset = true;
    }

    void OnDestroy()
    {
        if (_mesh) DestroyImmediate(_mesh);
        if (_positionBuffer1) DestroyImmediate(_positionBuffer1);
        if (_positionBuffer2) DestroyImmediate(_positionBuffer2);
        if (_kernelMaterial)  DestroyImmediate(_kernelMaterial);
        if (_lineMaterial )   DestroyImmediate(_lineMaterial);
        if (_debugMaterial)   DestroyImmediate(_debugMaterial);
    }

    void Update()
    {
        if (_needsReset) ResetResources();

        ApplyKernelParameters();

        if (Application.isPlaying)
        {
            // Swap the buffers.
            var temp = _positionBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _positionBuffer2 = temp;

            // Apply the kernel shader.
            Graphics.Blit(_positionBuffer1, _positionBuffer2, _kernelMaterial, 1);
        }
        else
        {
            // Editor: initialize the buffer on every update.
            Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);

            // Apply the kernel shader.
            Graphics.Blit(_positionBuffer2, _positionBuffer1, _kernelMaterial, 1);
            Graphics.Blit(_positionBuffer1, _positionBuffer2, _kernelMaterial, 1);
        }

        // Draw lines.
        _lineMaterial.SetTexture("_PositionTex1", _positionBuffer1);
        _lineMaterial.SetTexture("_PositionTex2", _positionBuffer2);
        _lineMaterial.SetColor("_Color", _color);
        _lineMaterial.SetVector("_Options", new Vector2(_tail, _colorAmp));
        Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _lineMaterial, 0);
    }

    void OnGUI()
    {
        if (_debug && Event.current.type.Equals(EventType.Repaint)) {
            if (_debugMaterial && _positionBuffer2) {
                var rect = new Rect(0, 0, 256, 64);
                Graphics.DrawTexture(rect, _positionBuffer2, _debugMaterial);
            }
        }
    }

    #endregion
}

} // namespace Kvant

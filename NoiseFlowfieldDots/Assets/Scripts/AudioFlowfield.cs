using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NoiseFlowField))]
public class AudioFlowfield : MonoBehaviour
{
    NoiseFlowField _noiseFlowfield;
    public AudioPeer _audioPeer;
    [Header("Speed")]
    public bool _useSpeed;
    public Vector2 _moveSpeedMinMax, _rotateSpeedMinMax;
    [Header("Scale")]
    public bool _useScale;
    public Vector2 _scaleMinMax;
    [Header("Material")]
    public Material _material;
    private Material[] _audioMaterial;
    [Header("Color")]
    public bool _useColor1;
    public string _colorName1 = "_Color";
    public Gradient _gradient1;
    private Color[] _color1;
    [UnityEngine.Range(0, 1)]
    public float _colorThreshold1 ;
    public float _colorMultiplier1;
    [Header("Color 2")]
    public bool _useColor2;
    public string _colorName2 = "_EmissionColor";
    public Gradient _gradient2;
    private Color[] _color2;
    [UnityEngine.Range(0, 1)]
    public float _colorThreshold2 ;
    public float _colorMultiplier2;
    // Start is called before the first frame update

    private const int COLORS_NUMBER = 8;
    void Start()
    {
        _audioMaterial = new Material[COLORS_NUMBER];
        _color1 = new Color[COLORS_NUMBER];
        _color2 = new Color[COLORS_NUMBER];
        for (int i = 0; i < COLORS_NUMBER; i++)
        {
            _color1[i] = _gradient1.Evaluate(1f / COLORS_NUMBER * i);
            _color2[i] = _gradient2.Evaluate(1f / COLORS_NUMBER * i);
            _audioMaterial[i] = new Material(_material);
        }
        _noiseFlowfield = GetComponent<NoiseFlowField>();
        int countBand = 0;
        for (int i = 0; i < _noiseFlowfield._amountOfParticles; i++)
        {
            int band = countBand % 8;
            _noiseFlowfield._particleMeshRender[i].material = _audioMaterial[band];
            _noiseFlowfield._particles[i]._audioBand = band;
            countBand++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_useSpeed)
        {
            _noiseFlowfield._particleMoveSpeed = Mathf.Lerp(_moveSpeedMinMax.x, _moveSpeedMinMax.y, _audioPeer.amplitudeBuffer);
            _noiseFlowfield._particleRotateSpeed = Mathf.Lerp(_moveSpeedMinMax.x, _moveSpeedMinMax.y, _audioPeer.amplitudeBuffer);
        }

        if (_useScale)
        {
            for (var i = 0; i < _noiseFlowfield._amountOfParticles; i++)
            {
                var particle = _noiseFlowfield._particles[i];
                var ratio = _audioPeer.audioBandBuffer[particle._audioBand];
                if (!float.IsNaN(ratio))
                {
                    var scale = Mathf.Lerp(_scaleMinMax.x, _scaleMinMax.y,
                        _audioPeer.audioBandBuffer[particle._audioBand]);
                    particle.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }
        if (_useColor1)
        {
            for (var i = 0; i < COLORS_NUMBER; i++)
            {
                if (_audioPeer.audioBandBuffer[i] > _colorThreshold1)
                    _audioMaterial[i].SetColor(_colorName1, _color1[i] * _audioPeer.audioBandBuffer[i] * _colorMultiplier1);
                else
                    _audioMaterial[i].SetColor(_colorName1, _color1[i] * 0f);
            }
        }
        if (_useColor2)
        {
            for (var i = 0; i < COLORS_NUMBER; i++)
            {
                if (_audioPeer.audioBandBuffer[i] > _colorThreshold2)
                    _audioMaterial[i].SetColor(_colorName2, _color2[i] * _audioPeer.audioBand[i] * _colorMultiplier2);
                else
                    _audioMaterial[i].SetColor(_colorName2, _color2[i] * 0f);
            }
        }
    }
}

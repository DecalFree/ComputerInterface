using System.Threading.Tasks;
using ComputerInterface.Behaviors;
using ComputerInterface.Enumerations;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace ComputerInterface.Models;

public class CustomKeyboardButton : GorillaTriggerBox {
    private Color _pressedColor = new(0.5f, 0.5f, 0.5f);

    public EKeyboardButton KeyboardButton { get; private set; }
    public TextMeshPro KeyboardTextMesh { get; private set; }

    public float pressTime;

    public bool isFunctionKey;

    private bool _isOnCooldown;

    private Material _material;
    private Color _originalColor;

    private BoxCollider _collider;
    private bool _isBumped;

    private void Awake() {
        enabled = false;
        _material = GetComponent<MeshRenderer>().material;
        _originalColor = _material.color;
        _collider = GetComponent<BoxCollider>();
    }

    public void InitializeCustomButton(EKeyboardButton keyboardButton, TextMeshPro keyboardTextMesh = null) {
        KeyboardButton = keyboardButton;
        KeyboardTextMesh = keyboardTextMesh;

        if (_collider != null && !_collider.enabled)
            _collider.enabled = true;
        enabled = true;
    }

    public void InitializeCustomButton(EKeyboardButton keyboardButton, TextMeshPro keyboardTextMesh, string text) {
        InitializeCustomButton(keyboardButton, keyboardTextMesh);
        if (keyboardTextMesh != null)
            keyboardTextMesh.text = text;
    }

    public void InitializeCustomButton(EKeyboardButton keyboardButton, TextMeshPro keyboardTextMesh, string text, Color buttonColor) {
        InitializeCustomButton(keyboardButton, keyboardTextMesh, text);

        if (_material == null) {
            _originalColor = buttonColor;

            Renderer baseRenderer = GetComponent<Renderer>();
            if (baseRenderer.material == null) {
                _material = new Material(Shader.Find("Legacy Shaders/Diffuse")) {
                    color = buttonColor
                };
            }
            else {
                baseRenderer.material.color = buttonColor;
            }
        }
        else {
            _material.color = buttonColor;
            _originalColor = buttonColor;
        }

        Color.RGBToHSV(buttonColor, out float h, out float s, out float _);
        _pressedColor = Color.HSVToRGB(h, s, 0.6f);
    }

    private async void OnTriggerEnter(Collider collider) {
        if (!collider.TryGetComponent(out GorillaTriggerColliderHandIndicator colliderHandIndicator))
            return;

        if (_isOnCooldown)
            return;
        _isOnCooldown = true;

        BumpInButton();
        Main.Singleton.PressKeyboardButton(this, colliderHandIndicator.isLeftHand);
        GorillaTagger.Instance.StartVibration(colliderHandIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
        if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 66, colliderHandIndicator.isLeftHand, 0.1f);

        await Task.Delay(Constants.KeyboardButtonPressCooldown);
        _isOnCooldown = false;
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.GetComponent<GorillaTriggerColliderHandIndicator>() == null)
            return;

        BumpOutButton();
    }

    private void BumpInButton() {
        if (_isBumped)
            return;

        _isBumped = true;
        Vector3 localPosition = transform.localPosition;
        localPosition.y -= Constants.KeyboardButtonBumpAmount;
        transform.localPosition = localPosition;
        _collider.center -= new Vector3(0, 0, Constants.KeyboardButtonBumpAmount / 1.125f);

        _material.color = _pressedColor;
    }

    private void BumpOutButton() {
        if (!_isBumped)
            return;

        _isBumped = false;
        Vector3 localPosition = transform.localPosition;
        localPosition.y += Constants.KeyboardButtonBumpAmount;
        transform.localPosition = localPosition;
        _collider.center += new Vector3(0, 0, Constants.KeyboardButtonBumpAmount / 1.125f);

        _material.color = _originalColor;
    }
}
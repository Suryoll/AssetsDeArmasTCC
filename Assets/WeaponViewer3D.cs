using TMPro;
using UnityEngine;

/// <summary>
/// Mostra a arma escolhida em 3D, flutuando na tela (dentro de um RawImage).
/// Segure o botão esquerdo do mouse e arraste para girar a arma.
/// Cria sozinho um "palco" longe da cena (câmera + luzes + RenderTexture),
/// então não precisa de layers nem de câmera extra.
/// </summary>
public class WeaponViewer3D : MonoBehaviour
{

    [Header("Saída (UI)")]
    [Tooltip("RawImage do Canvas onde a arma 3D aparece.")]
    [SerializeField] private UnityEngine.UI.RawImage output;
    [Tooltip("(Opcional) Texto com o nome da arma.")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private int textureSize = 512;

    [Header("Rotação")]
    [Tooltip("Graus de rotação por pixel arrastado com o botão esquerdo.")]
    [SerializeField] private float dragSensitivity = 0.4f;
    [Tooltip("Giro lento automático enquanto você não arrasta. 0 = desligado.")]
    [SerializeField] private float idleSpinSpeed = 15f;
    [Tooltip("Duração da animação de entrada (0 = sem animação).")]
    [SerializeField] private float popInDuration = 0.25f;

    [Header("Enquadramento")]
    [Tooltip("Tamanho (maior dimensão) que toda arma terá no palco.")]
    [SerializeField] private float targetSize = 1f;
    [SerializeField] private float cameraFov = 30f;
    [Tooltip("Folga ao redor do modelo. Aumente se a arma estiver cortada.")]
    [SerializeField] private float framingMargin = 1.35f;

    [Header("Iluminação do palco")]
    [SerializeField] private float keyLightIntensity = 8f;
    [SerializeField] private float fillLightIntensity = 3f;

    [Header("Palco")]
    [Tooltip("Posição do palco no mundo. Deve ficar bem longe da cena do jogo.")]
    [SerializeField] private Vector3 stageOrigin = new Vector3(5000f, 5000f, 5000f);

    public bool IsOpen { get; private set; }

    private Transform stage;
    private Transform pivot;
    private Camera previewCamera;
    private RenderTexture renderTexture;
    private GameObject currentModel;
    private float popTime = 1f;

    private void Awake()
    {
        BuildStage();
        Hide();
    }

    private void BuildStage()
    {

        stage = new GameObject("WeaponViewerStage").transform;
        stage.position = stageOrigin;

        pivot = new GameObject("Pivot").transform;
        pivot.SetParent(stage, false);

        // RenderTexture com canal alpha para o fundo ficar transparente
        renderTexture = new RenderTexture(textureSize, textureSize, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4,
            name = "WeaponViewerRT"
        };
        renderTexture.Create();

        // Câmera do palco
        var camObj = new GameObject("ViewerCamera");
        camObj.transform.SetParent(stage, false);
        previewCamera = camObj.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        previewCamera.fieldOfView = cameraFov;
        previewCamera.nearClipPlane = 0.05f;
        previewCamera.farClipPlane = 20f;
        previewCamera.targetTexture = renderTexture;

        float distance = (targetSize * 0.5f * framingMargin) /
                         Mathf.Tan(cameraFov * 0.5f * Mathf.Deg2Rad);
        camObj.transform.localPosition = new Vector3(0f, 0f, -distance);
        camObj.transform.localRotation = Quaternion.identity;

        // Luzes pontuais de alcance curto: não iluminam a cena principal
        CreateLight("KeyLight", new Vector3(1.8f, 1.6f, -2.0f), keyLightIntensity);
        CreateLight("FillLight", new Vector3(-2.0f, 0.4f, -1.5f), fillLightIntensity);

        if (output != null)
        {
            output.texture = renderTexture;
            output.color = Color.white;
            output.raycastTarget = false;
        }
    }

    private void CreateLight(string lightName, Vector3 localPos, float intensity)
    {
        var lightObj = new GameObject(lightName);
        lightObj.transform.SetParent(stage, false);
        lightObj.transform.localPosition = localPos;

        var l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 12f;
        l.intensity = intensity;
        l.shadows = LightShadows.None;
    }

    /// <summary>Mostra a arma em 3D. Chamado pelo Player quando solta o TAB.</summary>
    public void Show(WEAPON_INFO info)
    {

        ClearModel();

        if (info.prefab3D == null)
        {
            Debug.LogWarning("[WeaponViewer3D] A arma '" + info.name + "' não tem prefab3D no WeaponData.");
            Hide();
            return;
        }

        pivot.rotation = Quaternion.identity;
        pivot.localScale = Vector3.one;

        currentModel = Instantiate(info.prefab3D, pivot);
        currentModel.name = info.name + "_Viewer";
        DisableGameplayComponents(currentModel);

        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.Euler(info.viewRotationOffset);

        FitToStage(currentModel.transform, info.viewScale > 0f ? info.viewScale : 1f);

        popTime = popInDuration > 0f ? 0f : 1f;
        pivot.localScale = Vector3.one * (popTime >= 1f ? 1f : 0.6f);

        if (nameLabel != null) nameLabel.text = info.name;
        if (output != null) output.enabled = true;
        previewCamera.enabled = true;
        IsOpen = true;
    }

    /// <summary>Esconde a arma e para de renderizar o palco.</summary>
    public void Hide()
    {
        ClearModel();
        IsOpen = false;
        if (previewCamera != null) previewCamera.enabled = false;
        if (output != null) output.enabled = false;
        if (nameLabel != null) nameLabel.text = "";
    }

    private void Update()
    {

        if (!IsOpen || currentModel == null) return;

        float dt = Time.unscaledDeltaTime;

        if (GameInput.LeftMouseHeld)
        {
            // Arrastar com o botão esquerdo gira a arma em qualquer direção
            Vector2 d = GameInput.MouseDelta * dragSensitivity;
            pivot.Rotate(Vector3.up, -d.x, Space.World);
            pivot.Rotate(Vector3.right, d.y, Space.World);
        }
        else if (idleSpinSpeed != 0f)
        {
            pivot.Rotate(Vector3.up, idleSpinSpeed * dt, Space.World);
        }

        if (popTime < 1f)
        {
            popTime = Mathf.Min(1f, popTime + dt / popInDuration);
            float eased = 1f - (1f - popTime) * (1f - popTime);
            pivot.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, eased);
        }
    }

    private void FitToStage(Transform model, float extraScale)
    {

        if (!TryGetBounds(model, out Bounds b)) return;

        float maxSize = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (maxSize < 0.0001f) return;

        model.localScale *= (targetSize / maxSize) * extraScale;

        // Recentraliza no pivô para a arma girar em torno do próprio centro
        if (TryGetBounds(model, out b))
            model.position += pivot.position - b.center;
    }

    private static bool TryGetBounds(Transform root, out Bounds bounds)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        bounds = default;
        if (renderers.Length == 0) return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return true;
    }

    private static void DisableGameplayComponents(GameObject root)
    {
        foreach (var c in root.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
    }

    private void ClearModel()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (stage != null) Destroy(stage.gameObject);
    }
}
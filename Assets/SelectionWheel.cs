using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionWheel : MonoBehaviour
{

    public Transform slotPrefab;
    public WeaponData weaponData;

    [Header("Layout")]
    [Tooltip("Distância (em pixels) das fatias até o centro da roda.")]
    [SerializeField] float distance = 380f;

    [Tooltip("Raio (em pixels) no centro da tela onde o mouse não troca de arma.")]
    [SerializeField] float deadZone = 70f;

    [Header("Ajuste ao vivo")]
    [Tooltip("Ligado: a roda fica sempre aberta e relê o WeaponList a cada frame. " +
             "Use no Play para ajustar tamanho/posição/rotação de cada ícone. Desligue depois.")]
    [SerializeField] bool tuningMode = false;

    RectTransform slotParent;
    RectTransform background;
    TextMeshProUGUI weaponName;

    List<GameObject> slots = new List<GameObject>();

    int slotsQtt;                 // vem da quantidade de armas
    float angleInterval = 0;

    public int selectedIndex { get; private set; }
    public WEAPON_INFO SelectedWeapon => weaponData.weapons[selectedIndex];

    private void Awake()
    {

        background = transform.GetChild(0).GetComponent<RectTransform>();
        slotParent = transform.GetChild(1).GetComponent<RectTransform>();
        weaponName = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        slotsQtt = weaponData.weapons.Count;
        selectedIndex = 0;

    }

    void Start()
    {

        for (int i = 0; i < slotsQtt; ++i)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent).gameObject;
            slots.Add(newSlot);
        }

        angleInterval = 360f / slotsQtt;

        RefreshIcons();
        LayoutSlots();

        weaponName.text = SelectedWeapon.name;

        gameObject.SetActive(tuningMode);

    }

    // Coloca o sprite de cada arma e usa o tamanho nativo da imagem
    void RefreshIcons()
    {

        for (int i = 0; i < slots.Count; ++i)
        {
            Image weaponImage = slots[i].transform.GetChild(0).GetComponent<Image>();

            weaponImage.sprite = weaponData.weapons[i].sprite;
            weaponImage.SetNativeSize();
        }

    }

    // Posiciona as fatias e aplica escala/posição/rotação de cada ícone
    void LayoutSlots()
    {

        for (int i = 0; i < slots.Count; ++i)
        {

            float angleDeg = angleInterval * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            RectTransform slotRect = slots[i].GetComponent<RectTransform>();
            slotRect.localPosition = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * distance;
            slotRect.localRotation = Quaternion.Euler(0, 0, angleDeg - 90);

            WEAPON_INFO info = weaponData.weapons[i];

            RectTransform iconRect = slots[i].transform.GetChild(0).GetComponent<RectTransform>();
            iconRect.localScale = Vector3.one * (info.iconScale > 0f ? info.iconScale : 1f);
            iconRect.anchoredPosition = info.iconOffset;
            iconRect.localRotation = Quaternion.Euler(0, 0, -angleDeg + 90 + info.iconRotation);

        }

    }

    public void Open()
    {
        gameObject.SetActive(true);
        weaponName.text = SelectedWeapon.name;
    }

    public void Close()
    {
        // No modo de ajuste a roda fica sempre visível
        if (!tuningMode) gameObject.SetActive(false);
    }

    void Selection()
    {

        // Com o cursor travado o mouse fica parado no centro, por isso o Player
        // destrava o cursor antes de abrir a roda. A roda deve estar no centro da tela.
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 offset = GameInput.MousePosition - center;

        // Mouse no centro = nenhuma troca (evita escolher uma fatia sem querer)
        if (offset.magnitude < deadZone) return;

        Vector2 mouseDir = offset.normalized;

        for (int i = 0; i < slots.Count; ++i)
        {

            var angle = Vector2.Angle(slots[i].transform.up, mouseDir);

            Image slotImage = slots[i].GetComponent<Image>();
            if (angle <= angleInterval / 2)
            {
                slotImage.color = Color.Lerp(slotImage.color, Color.white, 0.01f);
                weaponName.text = weaponData.weapons[i].name;
                selectedIndex = i;
            }
            else
            {
                slotImage.color = Color.Lerp(slotImage.color, Color.white * 0.5f, 0.01f);
            }

        }

    }


    void Update()
    {

        // Modo de ajuste: relê o WeaponList todo frame, então o que você
        // mudar no Inspector aparece na hora.
        if (tuningMode)
        {
            RefreshIcons();
            LayoutSlots();
        }

        Selection();

    }

}
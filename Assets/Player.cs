using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    SelectionWheel selectionWheel;
    WeaponViewer3D weaponViewer;

    bool wheelOpen;

    private void Awake()
    {

        // Include: encontra o objeto mesmo se ele estiver desativado
        selectionWheel = FindFirstObjectByType<SelectionWheel>(FindObjectsInactive.Include);
        weaponViewer = FindFirstObjectByType<WeaponViewer3D>(FindObjectsInactive.Include);

    }

    void Start()
    {

        // Jogo normal: cursor travado
        SetCursorFree(false);

    }

    void Update()
    {

        if (GameInput.TabDown)
        {
            wheelOpen = true;
            if (weaponViewer != null) weaponViewer.Hide();
            selectionWheel.Open();
            SetCursorFree(true);
        }

        if (GameInput.TabUp && wheelOpen)
        {
            wheelOpen = false;
            selectionWheel.Close();

            if (weaponViewer != null)
            {
                weaponViewer.Show(selectionWheel.SelectedWeapon);
            }
            else
            {
                // sem visualizador: trava o cursor de novo
                SetCursorFree(false);
            }
        }

        if (GameInput.EscapeDown && weaponViewer != null && weaponViewer.IsOpen)
        {
            weaponViewer.Hide();
            SetCursorFree(false);
        }

    }

    void SetCursorFree(bool free)
    {
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }

}
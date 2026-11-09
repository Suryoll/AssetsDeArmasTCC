using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WEAPON_INFO
{
    public string name;

    [Tooltip("Ícone 2D mostrado na roda de seleção.")]
    public Sprite sprite;

    [Header("Ajustes do ícone na roda")]
    [Tooltip("Multiplicador do tamanho nativo do ícone. 0 = usa 1.")]
    public float iconScale;

    [Tooltip("Deslocamento dentro da fatia. Y positivo = para fora da roda.")]
    public Vector2 iconOffset;

    [Tooltip("Rotação extra do ícone, em graus.")]
    public float iconRotation;

    [Header("Modelo 3D")]
    [Tooltip("Modelo 3D mostrado depois que a arma é selecionada.")]
    public GameObject prefab3D;

    [Tooltip("Rotação inicial do modelo (corrige modelos importados deitados/virados).")]
    public Vector3 viewRotationOffset;

    [Tooltip("Multiplicador de tamanho no visualizador. 0 = usa 1.")]
    public float viewScale;
}


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{

    public List<WEAPON_INFO> weapons;

}
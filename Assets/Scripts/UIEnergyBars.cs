using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBars : MonoBehaviour {
    public static UIEnergyBars Instance = null;

    [System.Serializable]
    public struct EnergyBarsStruct {
        public Image mask;
        public float size;
    }

    public EnergyBarsStruct[] energyBarsStructs;

    public enum EnergyBars { PlayerHealth, BossHealth };

    [SerializeField] Sprite[] energySprites;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    void Start() {
        if (Instance == null) {
            Instance = this;
        }
        
        foreach (EnergyBars energyBar in Enum.GetValues(typeof(EnergyBars))) {
            energyBarsStructs[(int)energyBar].size = energyBarsStructs[(int)energyBar].mask.rectTransform.rect.height;
        }
    }

    public void SetValue(EnergyBars energyBar, float value) {
        energyBarsStructs[(int)energyBar].mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, energyBarsStructs[(int)energyBar].size * value);
    }

    public void SetVisibility(EnergyBars energyBar, bool visible) {
        energyBarsStructs[(int)energyBar].mask.gameObject.transform.parent.GetComponent<CanvasGroup>().alpha = visible ? 1f : 0f;
    }
}
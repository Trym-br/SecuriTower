using System;
using System.Linq;
using UnityEngine;

public class GateController : MonoBehaviour
{
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private CrystalController[] Inputs;
    private bool isOpen = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        bool shouldOpen = Inputs.All(x=> x.isLaserOn == true);
        if (shouldOpen && !isOpen)
        {
            Open();
            isOpen = true;
        }
        else if(!shouldOpen && isOpen)
        {
            Close();
            isOpen = false;
        }
    }

    private void Open()
    {
        spriteRenderer.enabled = false;
        _collider2D.enabled = false;

		FMODController.PlaySoundFrom(FMODController.Sound.SFX_GateOpen, gameObject);
    }

    private void Close()
    {
        spriteRenderer.enabled = true;
        _collider2D.enabled = true;

		FMODController.PlaySoundFrom(FMODController.Sound.SFX_GateClose, gameObject);
    }
}

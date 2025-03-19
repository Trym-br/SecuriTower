using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputActions))]
public class TestPlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private InputActions _input;
    [SerializeField] private float speed = 5;

    private void Awake()
    {
        _input = GetComponent<InputActions>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
       _rigidbody2D.linearVelocity = _input.movement.normalized * speed; 
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MareGameplay
{
    public class PlaceholderElement : MonoBehaviour
    {
        public UnityAction OnSelected { get; set; }

        [SerializeField] private Sprite filledSprite;
        [SerializeField] private Collider2D collider;

        private SpriteRenderer spriteRenderer;
        
        // If element is not filled, but it still can't be used cause of game restrictions 
        private bool isAvailable;

        public bool IsAvailable
        {
            get => isAvailable;
            set => isAvailable = value;
        }

        private bool isFilled;

        private void Start()
        {
            isFilled = false;
            isAvailable = true;
        }

        private void OnValidate()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            collider = GetComponent<Collider2D>();
        }

        private void OnMouseDown()
        {
            Select();
        }

        private void Select()
        {
            if (!isFilled && isAvailable)
            {
                spriteRenderer.sprite = filledSprite;
                isFilled = true;
                
                OnSelected.Invoke();
            }
        }
    }
}
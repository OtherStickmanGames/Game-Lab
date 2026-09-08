using System;
using UnityEngine;
using DwarfClone.Core;

namespace DwarfClone.Entity.Base
{
    public enum AnimState
    {
        Idle,
        Walk,
        Action,
        Incapacitated
    }

    public class EntityAnimator : MonoBehaviour
    {
        [Header("Sprite Prefix")]
        [SerializeField] private string spriteCategory = "Characters";
        [SerializeField] private string baseSpritePrefix = "char_warrior";

        public string SpriteCategory => spriteCategory;
        public string BaseSpritePrefix => baseSpritePrefix;

        private SpriteRenderer spriteRenderer;
        private EntityMovement movement;
        private AnimState currentState = AnimState.Idle;

        private float animTimer = 0f;
        private int walkFrame = 0;
        private Sprite spriteIdle;
        private Sprite spriteWalk0;
        private Sprite spriteWalk1;
        private Sprite spriteAction;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            movement = GetComponent<EntityMovement>();
        }

        public void InitializeSprites(string category, string prefix)
        {
            this.spriteCategory = category;
            this.baseSpritePrefix = prefix;

            spriteIdle = Resources.Load<Sprite>($"Sprites/{category}/{prefix}_idle");
            spriteWalk0 = Resources.Load<Sprite>($"Sprites/{category}/{prefix}_walk_0") ?? spriteIdle;
            spriteWalk1 = Resources.Load<Sprite>($"Sprites/{category}/{prefix}_walk_1") ?? spriteIdle;
            spriteAction = Resources.Load<Sprite>($"Sprites/{category}/{prefix}_action") ?? spriteIdle;

            if (spriteIdle == null)
            {
                Texture2D tex = new Texture2D(32, 32);
                Color[] px = new Color[32 * 32];
                Color c = new Color(0.85f, 0.65f, 0.45f);
                for (int i = 0; i < px.Length; i++) px[i] = c;
                tex.SetPixels(px);
                tex.Apply();
                spriteIdle = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
                spriteWalk0 = spriteIdle;
                spriteWalk1 = spriteIdle;
                spriteAction = spriteIdle;
            }

            if (spriteRenderer != null && spriteIdle != null)
            {
                spriteRenderer.sprite = spriteIdle;
            }
        }

        private void Update()
        {
            if (currentState == AnimState.Incapacitated)
            {
                transform.rotation = Quaternion.Euler(0, 0, 90f); // Knocked down on side
                return;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            if (currentState == AnimState.Action)
            {
                if (spriteAction != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = spriteAction;
                }
                return;
            }

            // Determine if moving
            if (movement != null && movement.IsMoving)
            {
                currentState = AnimState.Walk;
                animTimer += Time.deltaTime * 6f;
                if (animTimer >= 1f)
                {
                    animTimer = 0f;
                    walkFrame = (walkFrame + 1) % 2;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = walkFrame == 0 ? spriteWalk0 : spriteWalk1;
                    }
                }
            }
            else
            {
                currentState = AnimState.Idle;
                if (spriteIdle != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = spriteIdle;
                }
            }
        }

        public void SetState(AnimState state)
        {
            currentState = state;
            if (state == AnimState.Action && spriteAction != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteAction;
            }
        }
    }
}

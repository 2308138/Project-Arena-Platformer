using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldToLoadLevel : MonoBehaviour
{
    [SerializeField] public float holdDuration = 0F;
    [SerializeField] public Image fillCircle;

    [SerializeField] public static event Action OnHoldComplete;

    [SerializeField] private float holdTimer = 0F;
    [SerializeField] private bool isHolding = false;

    private void Update()
    {
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            fillCircle.fillAmount = holdTimer / holdDuration;

            if (holdTimer >= holdDuration)
            {
                OnHoldComplete.Invoke();
                ResetHold();
            }
        }
    }

    public void OnHold(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isHolding = true;
        }
        else if (context.canceled)
        {
            ResetHold();
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0F;
        fillCircle.fillAmount = 0;
    }
}